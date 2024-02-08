using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using EmbraceSDK.Bugshake;
using UnityEngine.SceneManagement;

#if EMBRACE_ENABLE_BUGSHAKE_FORM
using EmbraceSDK.Bugshake;
using Utilities;
#endif

namespace EmbraceSDK
{
    public class Embrace : MonoBehaviour, IEmbraceUnityApi
    {
        public IEmbraceProvider provider;
        private static Embrace _instance;
        private Thread _mainThread;
        private bool _started;
        private static EmbraceSdkInfo sdkInfo;
        private UnhandledExceptionRateLimiting rateLimiter = new UnhandledExceptionRateLimiting();
        private Dictionary<string, string> emptyDictionary = new Dictionary<string, string>();

        private EmbraceScenesToViewReporter scenesToViewReporter = null;

        #if UNITY_ANDROID && EMBRACE_ENABLE_BUGSHAKE_FORM
        private bool _isBugReportFormSwapSafe
        {
            get;
            set;
        } = true; // TODO: Set to true by default for now. We need to make this stronger.
        private readonly WaitUntil _waitForSwapSafety = new WaitUntil(() => Instance._isBugReportFormSwapSafe);
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private Coroutine _bugReportFormSwapRoutine = null;
        private readonly float _bugReportFormSwapSafetyTimeout = 5.0f; // Set the bug report form swap safety timeout to 5 seconds by default for now.
        #endif

        /// <inheritdoc />
        public bool IsStarted => _started;

        public static Embrace Instance
        {
            get
            {
                // Only initialize in a built player or Play Mode in the Editor
                if (_instance != null || !Application.isPlaying)
                {
                    return _instance;
                }

                #if UNITY_2022_3_OR_NEWER
                Embrace embrace = FindAnyObjectByType<Embrace>();
                #else
                Embrace embrace = FindObjectOfType<Embrace>();
                #endif
                if (embrace == null)
                {
                    var go = new GameObject { name = "Embrace" };
                    embrace = go.AddComponent<Embrace>();
                    DontDestroyOnLoad(go);
                }

                embrace.Initialize();
                return embrace;
            }
        }

        /// <summary>
        /// An alternative to the Instance property which will not instantiate a new instance if the singleton is null.
        /// </summary>
        /// <returns>The singleton instance, or null if it does not exist</returns>
        internal static Embrace GetExistingInstance() => _instance;

        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) {
                provider.InstallUnityThreadSampler();
#if UNITY_ANDROID
                // The behaviors of the native Android SDK and the native iOS SDK have been
                // demonstrated to be different. Namely, the iOS SDK perpetuates the views
                // as expected when returning from a long-pause (long enough to create a new session)
                // However, the Android SDK does not do so and instead loses that information, instead
                // capturing the Unity activity and possibly a test view label "a_view" as well.
                // As a result, the StartView and EndView clauses here should forcibly capture
                // the view information we need for this feature.
                scenesToViewReporter?.StartViewFromScene(SceneManager.GetActiveScene());
                    
                #if EMBRACE_ENABLE_BUGSHAKE_FORM
                // We should attempt to register the shake listener whenever the app is resumed
                // Because the internal behavior of the Android SDK is such that it contains a ShakeListener singleton
                // we will not cause issues by registering it multiple times.
                StartCoroutine(RegisterShakeListener());
                #endif
#endif
            } else
            {
#if UNITY_ANDROID
                scenesToViewReporter?.EndViewFromScene(SceneManager.GetActiveScene());
#endif
            }
        }
        
        #if UNITY_ANDROID && EMBRACE_ENABLE_BUGSHAKE_FORM
        public void ShowBugReportForm()
        {
            if (_bugReportFormSwapRoutine == null) // This will implicitly debounce the coroutine requests to only allow one at a time.
                _bugReportFormSwapRoutine = StartCoroutine(BugReportFormSwapRoutine());
        }
        
        /// <summary>
        /// Embrace users should call this method when they are ready to show the bug report form.
        /// Do not call this method if you are in the middle of a scene transition or other operation that may cause a swap to
        /// the bug report form to occur at an inopportune time.
        /// Calling this after MarkBugReportFormSwapUnsafe has been called is required,
        /// otherwise the swap to the bug report form will be rendered impossible.
        /// </summary>
        public void MarkBugReportFormSwapSafe()
        {
            _isBugReportFormSwapSafe = true;
        }
        
        /// <summary>
        /// Embrace users should call this method when they are not ready to show the bug report form.
        /// This will prevent the bug report form from being shown until MarkBugReportFormSwapSafe is called.
        /// Call this if you are in the middle of a scene transition or other operation that may cause the bug report form
        /// to be shown at an inopportune time. NOT calling this can result in ANRs or other issues.
        /// </summary>
        public void MarkBugReportFormSwapUnsafe()
        {
            _isBugReportFormSwapSafe = false;
        }
        
        IEnumerator BugReportFormSwapRoutine()
        {
            var requestTimestamp = Time.realtimeSinceStartup;
            
            yield return _waitForSwapSafety;
            yield return _waitForEndOfFrame;
            
            if (Time.realtimeSinceStartup - requestTimestamp > _bugReportFormSwapSafetyTimeout)
            {
                EmbraceLogger.LogWarning("Bug report form swap request timeout exceeded. Skipping bug report form swap.");
            }
            else
            {
                provider.ShowBugReportForm();
            }
            _bugReportFormSwapRoutine = null;
        }
        
        // It is possible to break this by disabling the Embrace GameObject in the scene before we complete registration.
        // We have no good way of fixing this at the moment because of the design of the Embrace MonoBehaviour.
        IEnumerator RegisterShakeListener()
        {
            var androidProvider = provider as Embrace_Android;
            if (androidProvider == null)
            {
                // This implies that we are in the Unity Editor
                yield break;
            }
            var waitFor = new WaitUntil(() => androidProvider.IsReady);
            yield return waitFor;

            provider.setShakeListener(new UnityShakeListener());
        }
        #endif

        /// <summary>
        /// Alternative way of creating the Embrace singleton, primarily used for unit testing.
        /// Use Embrace.Instance in all other cases.
        /// </summary>
        /// <returns></returns>
        public static Embrace Create()
        {
            #if UNITY_2022_3_OR_NEWER
            var embraceInstance = FindObjectOfType<Embrace>();
            #else
            var embraceInstance = FindObjectOfType<Embrace>();
            #endif
            if (embraceInstance != null)
            {
                DestroyImmediate(embraceInstance.gameObject);
            }

            var go = new GameObject { name = "Embrace" };
            embraceInstance = go.AddComponent<Embrace>();

            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);
            embraceInstance.provider = new Embrace_Stub();
            _instance = embraceInstance;

            return embraceInstance;
        }

        /// <summary>
        /// Initializes core SDK parameters and instantiates a platform specific provider.
        /// </summary>
        private void Initialize()
        {
            _instance = this;

            _mainThread = Thread.CurrentThread;

            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);

#if UNITY_ANDROID && !UNITY_EDITOR
            provider = new Embrace_Android();
#elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            provider = new Embrace_iOS();
#else
            provider = new Embrace_Stub();
#endif
                
            #if UNITY_ANDROID && EMBRACE_ENABLE_BUGSHAKE_FORM
            
            #if EMBRACE_USE_BUGSHAKE_SCENE_MANAGER_OVERRIDE
            if (SceneManagerAPI.overrideAPI == null)
            {
                SceneManagerAPI.overrideAPI = new EmbraceSceneManagerOverride(
                    MarkBugReportFormSwapUnsafe, MarkBugReportFormSwapSafe);
            }
            else
            {
                // The only ways to handle this are to either invoke reflection at runtime or use a weaver to capture the user's SceneManagerAPI override and weave into that.
                EmbraceLogger.LogWarning("User requested to use the EmbraceSceneManagerOverride, but the override API is already set. EmbraceSceneManagerOverride assignment skipped.");
            }
            #endif
            
            // We should allow the user to configure if this is enabled or not by default.
            // For now we don't have a good way to allow the user to configure this setting.
            // We could use an instance variable since this is a Monobehaviour, but we don't force
            // the user to setup the prefab in the scene at edit time.
            // As a result if the prefab is instantiated dynamically we have no good behavioral assumption.
            // For now we will enable this by default.
            StartCoroutine(RegisterShakeListener());
            #endif
            
            provider.InitializeSDK();
        }

#if UNITY_ANDROID && EMBRACE_ENABLE_BUGSHAKE_FORM
        internal void TakeShakeScreenshot()
        {
            StartCoroutine(TakeScreenshot());
        }

        private IEnumerator TakeScreenshot()
        {
            // Read the screen buffer after rendering is complete
            yield return new WaitForEndOfFrame();
            provider.saveShakeScreenshot(ScreenshotUtil.TakeScreenshot());
        }
#endif
        
        // Called by Unity runtime
        private void Start()
        {
            // If some other Game Object gets added to the scene that has an Embrace
            // component that doesn't match our singleton then get rid of it...
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
            }
            else if (_instance == null)
            {
                // ...otherwise if the singleton instance is null, invoke Initialize() to create it.
                // This scenario is likely to occur if a user adds the Embrace Monobehaviour to a
                // game object in a startup scene, but doesn't invoke the StartSDK() method through
                // the singleton instance until later in the application's startup process.
                Initialize();
                DontDestroyOnLoad(gameObject);
            }
        }

        // Called by Unity runtime
        private void OnDestroy()
        {
            scenesToViewReporter?.Dispose();
        }

        /// <inheritdoc />
        public void StartSDK(bool enableIntegrationTesting = false)
        {
            if (_started)
            {
                return;
            }

            if (_instance == null)
            {
                Initialize();
            }

            provider.StartSDK(enableIntegrationTesting);
            provider.SetMetaData(Application.unityVersion, Application.buildGUID, sdkInfo.version);

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
            EmbraceLogger.Log("Embrace SDK enabled. Version: " + sdkInfo.version);
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

                (string splitName, string splitMessage) = SplitConcatenatedExceptionNameAndMessage(message);
                provider.LogUnhandledUnityException(splitName, splitMessage, stack);
            }
        }

        /// <inheritdoc />
        public void EndAppStartup(Dictionary<string, string> properties = null)
        {
            if (properties == null)
            {
                properties = emptyDictionary;
            }

            provider.EndAppStartup(properties);
        }

        /// <inheritdoc />
        public LastRunEndState GetLastRunEndState()
        {
            return IsStarted ? provider.GetLastRunEndState() : LastRunEndState.Invalid;
        }

        /// <inheritdoc />
        public void SetUserIdentifier(string identifier)
        {
            if (identifier == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user identifier"));
                return;
            }

            provider.SetUserIdentifier(identifier);
        }

        /// <inheritdoc />
        public void ClearUserIdentifier()
        {
            provider.ClearUserIdentifier();
        }

        /// <inheritdoc />
        public void SetUsername(string username)
        {
            if (username == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("username"));
                return;
            }

            provider.SetUsername(username);
        }

        /// <inheritdoc />
        public void ClearUsername()
        {
            provider.ClearUsername();
        }

        /// <inheritdoc />
        public void SetUserEmail(string email)
        {
            if (email == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user email"));
                return;
            }

            provider.SetUserEmail(email);
        }

        /// <inheritdoc />
        public void ClearUserEmail()
        {
            provider.ClearUserEmail();
        }

        /// <inheritdoc />
        public void SetUserAsPayer()
        {
            provider.SetUserAsPayer();
        }

        /// <inheritdoc />
        public void ClearUserAsPayer()
        {
            provider.ClearUserAsPayer();
        }

        [System.Obsolete("Please use AddUserPersona() instead. This method will be removed in a future release.")]
        public void SetUserPersona(string persona)
        {
            AddUserPersona(persona);
        }
        
        /// <inheritdoc />
        public void AddUserPersona(string persona)
        {
            if (persona == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user persona"));
                return;
            }

            provider.AddUserPersona(persona);
        }

        /// <inheritdoc />
        public void ClearUserPersona(string persona)
        {
            if (persona == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("user persona"));
                return;
            }

            provider.ClearUserPersona(persona);
        }

        /// <inheritdoc />
        public void ClearAllUserPersonas()
        {
            provider.ClearAllUserPersonas();
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

            provider.AddSessionProperty(key, value, permanent);
        }

        /// <inheritdoc />
        public void RemoveSessionProperty(string key)
        {
            if (key == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("session property key"));
                return;
            }

            provider.RemoveSessionProperty(key);
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetSessionProperties()
        {
            var properties = provider.GetSessionProperties();
            if (properties == null)
            {
                properties = emptyDictionary;
            }

            return properties;
        }

        /// <inheritdoc />
        public void StartMoment(string name, string identifier = null, bool allowScreenshot = false, Dictionary<string, string> properties = null)
        {
            if (name == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("moment name"));
                return;
            }

            if (properties == null)
            {
                properties = emptyDictionary;
            }

            provider.StartMoment(name, identifier, allowScreenshot, properties);
        }

        /// <inheritdoc />
        public void EndMoment(string name, string identifier = null, Dictionary<string, string> properties = null)
        {
            if (name == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("moment name"));
                return;
            }

            if (properties == null)
            {
                properties = emptyDictionary;
            }

            provider.EndMoment(name, identifier, properties);
        }

        /// <inheritdoc />
        [System.Obsolete("Please use LogMessage() without the screenshot argument instead. This method will be removed in a future release.")]
        public void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties = null, bool allowScreenshot = false)
        {
            LogMessage(message, severity, properties);
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

            provider.LogMessage(message, severity, properties);
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

        [System.Obsolete("Please use AddBreadcrumb() instead. This method will be removed in a future release.")]
        public void LogBreadcrumb(string message)
        {
            AddBreadcrumb(message);
        }

        /// <inheritdoc />
        public void AddBreadcrumb(string message)
        {
            if (message == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("breadcrumb message"));
                return;
            }

            provider.AddBreadcrumb(message);
        }

        /// <inheritdoc />
        public void EndSession(bool clearUserInfo = false)
        {
            provider.EndSession(clearUserInfo);
        }

        /// <inheritdoc />
        public string GetDeviceId()
        {
            return provider.GetDeviceId();
        }

        /// <inheritdoc />
        public bool StartView(string name)
        {
            if (name == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("view name"));
                return false;
            }

            return provider.StartView(name);
        }

        /// <inheritdoc />
        public bool EndView(string name)
        {
            if (name == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("view name"));
                return false;
            }

            return provider.EndView(name);
        }

        /// <summary>
        /// Causes a crash. Use this for test purposes only.
        /// </summary>
        [System.Obsolete("This method will be removed in a future release.")]
        public void Crash()
        {
            provider.Crash();
        }

        /// <inheritdoc />
        [System.Obsolete("Please use RecordNetworkRequest() instead. This method will be removed in a future release.")]
        public void LogNetworkRequest(string url, HTTPMethod method, long startms, long endms, int bytesin, int bytesout, int code, string error)
        {
            RecordNetworkRequest(url, method, startms, endms, bytesin, bytesout, code, error);
        }
        
        /// <inheritdoc />
        [System.Obsolete("Please use RecordCompletedNetworkRequest() or RecordIncompleteNetworkRequest() instead. This method will be removed in a future release.")]

        public void RecordNetworkRequest(string url, HTTPMethod method, long startms, long endms, int bytesin, int bytesout, int code, string error = "")
        {
            if (!string.IsNullOrEmpty(error))
            {
                RecordIncompleteNetworkRequest(url, method, startms, endms, error);
                return;
            }

            provider.RecordCompletedNetworkRequest(url, method, startms, endms, bytesin, bytesout, code);
        }
        
        /// <inheritdoc />
        public void RecordCompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code)
        {
            if (url == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("network url"));
                return;
            }
            
            provider.RecordCompletedNetworkRequest(url, method, startms, endms, bytesin, bytesout, code);
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
            
            provider.RecordIncompleteNetworkRequest(url, method, startms, endms, error);
        }

        /// <inheritdoc />
        [Obsolete("Please use LogUnhandledUnityException instead. This method will be removed in a future release.")]
        public void logUnhandledUnityException(string exceptionMessage, string stack)
        {
            if (exceptionMessage == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception message"));
                return;
            }

            if (stack == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception stack"));
                return;
            }

            (string splitName, string splitMessage) = SplitConcatenatedExceptionNameAndMessage(exceptionMessage);
            provider.LogUnhandledUnityException(splitName, splitMessage, stack);
        }

        /// <inheritdoc />
        public void LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (string.IsNullOrEmpty(exceptionName))
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception name"));
                return;
            }

            provider.LogUnhandledUnityException(exceptionName, exceptionMessage ?? "", stack ?? "");
        }

        /// <inheritdoc />
        public void LogUnhandledUnityException(Exception exception, string stack = null)
        {
            if (exception == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception"));
                return;
            }

            provider.LogUnhandledUnityException(
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

            provider.LogHandledUnityException(exceptionName, exceptionMessage ?? "", stack ?? "");
        }

        /// <inheritdoc />
        public void LogHandledUnityException(Exception exception, string stack = null)
        {
            if (exception == null)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("exception"));
                return;
            }

            provider.LogHandledUnityException(
                exceptionName: exception.GetType().Name,
                exceptionMessage: exception.Message ?? "",
                stack: stack ?? exception.StackTrace ?? "");
        }
        
        /// <inheritdoc />
        public string GetCurrentSessionId()
        {
            return provider.GetCurrentSessionId();
        }

        /// <inheritdoc />
        public void RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            #if UNITY_IOS
            provider.RecordPushNotification(iosArgs);
            #else
            EmbraceLogger.LogError("Attempting to record iOS push notification on non-iOS platform");
            #endif
        }

        /// <inheritdoc />
        public void RecordPushNotification(AndroidPushNotificationArgs androidArgs)
        {
            #if UNITY_ANDROID
            provider.RecordPushNotification(androidArgs);
            #else
            EmbraceLogger.LogError("Attempting to record Android push notification on non-Android platform");
            #endif
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
        /// Splits the Unity-formatted exception type name and message into separate strings.
        /// </summary>
        /// <param name="exception">The exception string provided by Unity in the format "ExceptionType: Exception message."</param>
        internal static (string name, string message) SplitConcatenatedExceptionNameAndMessage(string exception)
        {
            if (string.IsNullOrEmpty(exception))
            {
                return ("", "");
            }

            int separatorIndex = exception.IndexOf(':');
            if(separatorIndex < 0)
            {
                return ("", exception);
            }

            string name = exception.Substring(0, separatorIndex);
            string message = exception.Substring(separatorIndex + 1);

            return (name, message);
        }
    }
}
