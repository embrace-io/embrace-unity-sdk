using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using EmbraceSDK.Editor;
using EmbraceSDK.Internal;
using EmbraceSDK.Utilities;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EmbraceSDK
{
    public class Embrace : IEmbraceUnityApi
    {
        private static readonly object providerMutex = new object();
        public IEmbraceProvider provider;
        public EmbraceUnityListener listener { get; private set; }
        
        public IEmbraceProvider Provider
        {
            get
            {
                lock (providerMutex)
                {
                    if (provider == null)
                    {
                        // Initialize the provider if it is null
                        #if UNITY_ANDROID && !UNITY_EDITOR
                        provider = new Embrace_Android();
                        #elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
                        provider = new Embrace_iOS();
                        #else
                        provider = new Embrace_Stub();
                        #endif

                        if (provider == null)
                        {
                            EmbraceLogger.LogError("Embrace provider is null after forced initialization. " +
                                                   "There has been an unknown error blocking assignment." +
                                                   "Please inform the Embrace team as soon as possible.");
                            return null;
                        }
                    }
                }
                return provider;
            }
            private set
            {
                lock (providerMutex)
                {
                    provider = value;
                }
            }
        }

        private static Embrace _instance;
        private Thread _mainThread;
        private bool _started;
        private static EmbraceSdkInfo sdkInfo;
        private UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
        private Dictionary<string, string> emptyDictionary = new Dictionary<string, string>();

        private EmbraceScenesToViewReporter scenesToViewReporter = null;

        /// <inheritdoc />
        public bool IsStarted => _started;
        
        public static Embrace Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning("Embrace instance is null. Please call Embrace.StartSDK() first.");
                }
                
                return _instance;
            }
            private set => _instance = value;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) {
                Provider?.InstallUnityThreadSampler();
#if UNITY_ANDROID
                // The behaviors of the native Android SDK and the native iOS SDK have been
                // demonstrated to be different. Namely, the iOS SDK perpetuates the views
                // as expected when returning from a long-pause (long enough to create a new session)
                // However, the Android SDK does not do so and instead loses that information, instead
                // capturing the Unity activity and possibly a test view label "a_view" as well.
                // As a result, the StartView and EndView clauses here should forcibly capture
                // the view information we need for this feature.
                scenesToViewReporter?.StartViewFromScene(SceneManager.GetActiveScene());
#endif
            } else
            {
#if UNITY_ANDROID
                scenesToViewReporter?.EndViewFromScene(SceneManager.GetActiveScene());
#endif
            }
        }

        /// <summary>
        /// Alternative way of creating the Embrace singleton, primarily used for unit testing.
        /// Use Embrace.Instance in all other cases.
        /// </summary>
        /// <returns></returns>
        private static void CreateUnityListener()
        {
            if (Instance.listener)
            {
                return;
            }
            
            var go = new GameObject { name = "Embrace" };
            Instance.listener = go.AddComponent<EmbraceUnityListener>();
            
            Instance.listener.SetOnDestroyCallback(() =>
            {
                Instance.scenesToViewReporter?.Dispose();
            });
            
            Instance.listener.SetOnApplicationPauseCallback(Instance.OnApplicationPause);
            Object.DontDestroyOnLoad(Instance.listener);
        }

        /// <summary>
        /// Initializes core SDK parameters and instantiates a platform specific provider.
        /// </summary>
        private void Initialize()
        {
            try
            {
                Instance = this;
                _mainThread = Thread.CurrentThread;
                TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
                sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);
                Provider?.InitializeSDK();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Start(EmbraceStartupArgs args = null, bool createListener = true)
        {
            if (_instance != null)
            {
                return;
            }
            
            new Embrace().StartSDK(args, createListener);
        }

        public static void Stop()
        {
            if (_instance != null)
            {
                _instance.StopSDK();
                _instance = null;
            }
        }

        /// <inheritdoc />
        public void StartSDK(EmbraceStartupArgs args = null, bool createListener = true)
        {
            if (_started)
            {
                return;
            }

            if (_instance == null)
            {
                Initialize();
            }
            
            if(createListener)
            {
                CreateUnityListener();
            }
            
            try
            {
                Provider?.StartSDK(args);
                Provider?.SetMetaData(Application.unityVersion, Application.buildGUID, sdkInfo.version);
                TimeUtil.Clean();
                TimeUtil.InitStopWatch();
                Application.logMessageReceived += Embrace_Log_Handler;

                // Scene change registration here
#if EMBRACE_AUTO_CAPTURE_ACTIVE_SCENE_AS_VIEW
            scenesToViewReporter = new EmbraceScenesToViewReporter();
            scenesToViewReporter.StartViewFromScene(SceneManager.GetActiveScene());
#endif

#if EMBRACE_USE_THREADING
                // If this directive is defined, the Embrace SDK will capture messages regardless of whether they
                // originate from the main thread or not.  For more details please see Unity documentation:
                // https://docs.unity3d.com/ScriptReference/Application-logMessageReceivedThreaded.html
                Application.logMessageReceivedThreaded += Embrace_Threaded_Log_Handler;
                Debug.LogWarning("THREADED LOGGING ENABLED");
#endif

                _started = true;
                InternalEmbrace.SetInternalInstance(Instance);
                EmbraceLogger.Log("Embrace SDK enabled. Version: " + sdkInfo.version);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void StopSDK()
        {
            Provider = null;
            Instance = null;
        }

        private bool IsMainThread()
        {
            if (_mainThread == null) return false;
            return _mainThread.Equals(Thread.CurrentThread);
        }

#if EMBRACE_USE_THREADING
        private void Embrace_Threaded_Log_Handler(string message, string stack, LogType type)
        {
            if (IsMainThread())
            {
                return;
            }

            Embrace_Log_Handler(message, stack, type);
        }
#endif

        /// <summary>
        /// Handles log messages of LogType.Exception and LogType.Assert. For internal use and testing only.
        /// </summary>
        /// <param name="message">Custom message that will be attached to this log.</param>
        /// <param name="stack">Stack trace of the message origin</param>
        /// <param name="type">Log type (see UnityEngine.LogType for more info)</param>
        public void Embrace_Log_Handler(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Assert)
            {
                UnhandledException ue = new UnhandledException(message, stack);
                if (!rateLimiter.IsAllowed(ue))
                {
                    return;
                }

                (string splitName, string splitMessage) = UnhandledExceptionUtility.SplitConcatenatedExceptionNameAndMessage(message);
                Provider?.LogUnhandledUnityException(splitName, splitMessage, stack);
            }
        }

        /// <inheritdoc />
        public LastRunEndState GetLastRunEndState()
        {
            return IsStarted ? Provider?.GetLastRunEndState() ?? LastRunEndState.Invalid : LastRunEndState.Invalid;
        }

        /// <inheritdoc />
        public void SetUserIdentifier(string identifier)
        {
            if (identifier == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user identifier"));
                return;
            }

            Provider?.SetUserIdentifier(identifier);
        }

        /// <inheritdoc />
        public void ClearUserIdentifier()
        {
            Provider?.ClearUserIdentifier();
        }

        /// <inheritdoc />
        public void SetUsername(string username)
        {
            if (username == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("username"));
                return;
            }

            Provider?.SetUsername(username);
        }

        /// <inheritdoc />
        public void ClearUsername()
        {
            Provider?.ClearUsername();
        }

        /// <inheritdoc />
        public void SetUserEmail(string email)
        {
            if (email == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user email"));
                return;
            }

            Provider?.SetUserEmail(email);
        }

        /// <inheritdoc />
        public void ClearUserEmail()
        {
            Provider?.ClearUserEmail();
        }

        /// <inheritdoc />
        public void SetUserAsPayer()
        {
            Provider?.SetUserAsPayer();
        }

        /// <inheritdoc />
        public void ClearUserAsPayer()
        {
            Provider?.ClearUserAsPayer();
        }
        
        /// <inheritdoc />
        public void AddUserPersona(string persona)
        {
            if (persona == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user persona"));
                return;
            }

            Provider?.AddUserPersona(persona);
        }

        /// <inheritdoc />
        public void ClearUserPersona(string persona)
        {
            if (persona == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user persona"));
                return;
            }

            Provider?.ClearUserPersona(persona);
        }

        /// <inheritdoc />
        public void ClearAllUserPersonas()
        {
            Provider?.ClearAllUserPersonas();
        }

        /// <inheritdoc />
        public void AddSessionProperty(string key, string value, bool permanent)
        {
            if (key == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("session property key"));
                return;
            }

            if (value == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("session property value"));
                return;
            }

            Provider?.AddSessionProperty(key, value, permanent);
        }

        /// <inheritdoc />
        public void RemoveSessionProperty(string key)
        {
            if (key == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("session property key"));
                return;
            }

            Provider?.RemoveSessionProperty(key);
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetSessionProperties()
        {
            var properties = Provider?.GetSessionProperties();
            if (properties == null)
            {
                properties = emptyDictionary;
            }

            return properties;
        }
        
        public void LogMessage(string message, EMBSeverity severity)
        {
            LogMessage(message, severity, null);
        }
        
        /// <inheritdoc />
        public void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties)
        {
            if (message == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("log message"));
                return;
            }

            if (properties == null)
            {
                properties = emptyDictionary;
            }

            Provider?.LogMessage(message, severity, properties);
        }

        #if UNITY_ANDROID
        /// <inheritdoc />
        public void LogMessage(string message, EMBSeverity severity, 
            Dictionary<string, string> properties, sbyte[] attachment)
        {
            if (message == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("log message"));
                return;
            }

            if (properties == null)
            {
                properties = emptyDictionary;
            }

            if (attachment == null || attachment.Length > 1024 * 1024) // Larger than 1 MiB
            {
                AddBreadcrumb($"Embrace Attachment failure. Attachment size too large. Message: {message}");
                return;
            }
            
            Provider?.LogMessage(message, severity, properties, attachment);
        }
        #elif UNITY_IOS
        /// <inheritdoc />
        public void LogMessage(string message, EMBSeverity severity, 
            Dictionary<string, string> properties, byte[] attachment)
        {
            if (message == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("log message"));
                return;
            }

            if (properties == null)
            {
                properties = emptyDictionary;
            }

            if (attachment == null || attachment.Length > 1024 * 1024) // Larger than 1 MiB
            {
                AddBreadcrumb($"Embrace Attachment failure. Attachment size too large. Message: {message}");
                return;
            }
            
            Provider?.LogMessage(message, severity, properties, attachment);
        }
        #endif
        
        /// <inheritdoc />
        public void LogMessage(string message, EMBSeverity severity, 
            Dictionary<string, string> properties, string attachmentId, string attachmentUrl)
        {
            if (message == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("log message"));
                return;
            }

            if (properties == null)
            {
                properties = emptyDictionary;
            }
            
            Provider?.LogMessage(message, severity, properties, attachmentId, attachmentUrl);
        }

        /// <inheritdoc />
        public void LogInfo(string message)
        {
            LogMessage(message, EMBSeverity.Info);
        }

        /// <inheritdoc />
        public void LogWarning(string message)
        {
            LogMessage(message, EMBSeverity.Warning);
        }

        /// <inheritdoc />
        public void LogError(string message)
        {
            LogMessage(message, EMBSeverity.Error);
        }

        /// <inheritdoc />
        public void AddBreadcrumb(string message)
        {
            if (message == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("breadcrumb message"));
                return;
            }

            Provider?.AddBreadcrumb(message);
        }

        /// <inheritdoc />
        public void EndSession(bool clearUserInfo = false)
        {
            Provider?.EndSession(clearUserInfo);
        }

        /// <inheritdoc />
        public string GetDeviceId()
        {
            return Provider?.GetDeviceId();
        }

        /// <inheritdoc />
        public bool StartView(string name)
        {
            if (name == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("view name"));
                return false;
            }

            return Provider?.StartView(name) ?? false;
        }

        /// <inheritdoc />
        public bool EndView(string name)
        {
            if (name == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("view name"));
                return false;
            }

            return Provider?.EndView(name) ?? false;
        }
        
        /// <inheritdoc />
        public void RecordCompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code)
        {
            if (url == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("network url"));
                return;
            }
            
            Provider?.RecordCompletedNetworkRequest(url, method, startms, endms, bytesin, bytesout, code);
        }
        
        /// <inheritdoc />
        public void RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error)
        {
            if (url == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("network url"));
                return;
            }
            
            if (error == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("network error"));
                return;
            }
            
            Provider?.RecordIncompleteNetworkRequest(url, method, startms, endms, error);
        }

        /// <inheritdoc />
        public void LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (string.IsNullOrEmpty(exceptionName))
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception name"));
                return;
            }

            Provider?.LogUnhandledUnityException(exceptionName, exceptionMessage ?? "", stack ?? "");
        }

        /// <inheritdoc />
        public void LogUnhandledUnityException(Exception exception, string stack = null)
        {
            if (exception == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception"));
                return;
            }

            Provider?.LogUnhandledUnityException(
                exceptionName: exception.GetType().Name,
                exceptionMessage: exception.Message ?? "",
                stack: stack ?? exception.StackTrace ?? "");
        }

        /// <inheritdoc />
        public void LogHandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (string.IsNullOrEmpty(exceptionName))
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception name"));
                return;
            }

            Provider?.LogHandledUnityException(exceptionName, exceptionMessage ?? "", stack ?? "");
        }

        /// <inheritdoc />
        public void LogHandledUnityException(Exception exception, string stack = null)
        {
            if (exception == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception"));
                return;
            }

            Provider?.LogHandledUnityException(
                exceptionName: exception.GetType().Name,
                exceptionMessage: exception.Message ?? "",
                stack: stack ?? exception.StackTrace ?? "");
        }
        
        /// <inheritdoc />
        public string GetCurrentSessionId()
        {
            return Provider?.GetCurrentSessionId();
        }

        /// <inheritdoc />
        public void RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            #if UNITY_IOS
            Provider?.RecordPushNotification(iosArgs);
            #else
            EmbraceLogger.LogError("Attempting to record iOS push notification on non-iOS platform");
            #endif
        }

        /// <inheritdoc />
        public void RecordPushNotification(AndroidPushNotificationArgs androidArgs)
        {
            #if UNITY_ANDROID
            Provider?.RecordPushNotification(androidArgs);
            #else
            EmbraceLogger.LogError("Attempting to record Android push notification on non-Android platform");
            #endif
        }
        
        /// <summary>
        /// Create and start a new span.
        /// </summary>
        /// <returns>Returns the spanId of the new span if both operations are successful, and null if either fails.</returns>
        public string StartSpan(string spanName, long startTimeMs, string parentSpanId = null)
        {
            return provider.StartSpan(spanName, parentSpanId, startTimeMs);
        }
        
        /// <summary>
        /// Stop an active span with the given [spanId].
        /// </summary>
        /// <returns>Returns true if the span is stopped after the method returns and false otherwise</returns>
        public bool StopSpan(string spanId, long endTimeMs, EmbraceSpanErrorCode? errorCode = null)
        {
            if (spanId == null) 
            {
                EmbraceLogger.LogError("in order to stop a span, " + EmbraceLogger.GetNullErrorMessage("spanId"));
                return false;
            }

            return provider.StopSpan(spanId, __BridgedSpanErrorCode(errorCode), endTimeMs);
        }
        
        /// <summary>
        /// Create and add a Span Event with the given parameters to an active span with the given [spanId].
        /// </summary>
        /// <returns>Returns false if the event cannot be added.</returns>
        public bool AddSpanEvent(string spanId, string spanName, long timestampMs, Dictionary<string, string> attributes = null)
        {
            return provider.AddSpanEvent(spanId, spanName, timestampMs, attributes);
        }
        
        /// <summary>
        /// Add an attribute to an active span with the given [spanId].
        /// </summary>
        /// <returns>Returns true if the attributed is added and false otherwise</returns>
        public bool AddSpanAttribute(string spanId, string key , string value)
        {
            return provider.AddSpanAttribute(spanId, key, value);
        }
        
        /// <summary>
        /// 
        /// Record a completed span with the given parameters.
        /// 
        /// </summary>
        /// <returns>Returns true if the span is record and false otherwise</returns>
        public bool RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, 
            EmbraceSpanErrorCode? errorCode = null, Dictionary<string, string> attributes = null, EmbraceSpanEvent embraceSpanEvent = null, 
            string parentSpanId = null)
        {
            return provider.RecordCompletedSpan(spanName, startTimeMs, endTimeMs, __BridgedSpanErrorCode(errorCode), parentSpanId, attributes, new EmbraceSpanEvent[] { embraceSpanEvent });
        }

        /// <summary>
        /// Converts an HTTPMethod to an int value.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static int __BridgedHTTPMethod(HTTPMethod method)
        {
            switch (method)
            {
                case HTTPMethod.GET: return 1;
                case HTTPMethod.POST: return 2;
                case HTTPMethod.PUT: return 3;
                case HTTPMethod.DELETE: return 4;
                case HTTPMethod.PATCH: return 5;
                default: return 0;
            }
        }
        
        /// <summary>
        /// Converts a SpanErrorCode to an int value.
        /// </summary>
        /// <param name="embraceSpanErrorCode"></param>
        /// <returns></returns>
        public static int __BridgedSpanErrorCode(EmbraceSpanErrorCode? embraceSpanErrorCode)
        {
            if (embraceSpanErrorCode == null) return 0;
            
            switch (embraceSpanErrorCode)
            {
                case EmbraceSpanErrorCode.FAILURE: return 1;
                case EmbraceSpanErrorCode.USER_ABANDON: return 2;
                case EmbraceSpanErrorCode.UNKNOWN: return 3;
                default: return 0;
            }
        }
    }
}
