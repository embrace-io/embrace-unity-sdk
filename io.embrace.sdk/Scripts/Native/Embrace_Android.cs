using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EmbraceSDK.Internal;
using UnityEngine.TestTools;

namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Embrace_Android makes calls to our java methods in our Android SDK. It uses a list of hardcoded strings
    /// that represent the Java methods that we can call from our Android SDK.
    /// Using the provider interface to receive calls from the Embrace class, it then uses AndroidJavaObject.
    /// Call to make calls to the corresponding java method on our Android SDK by passing in the name of the method and its property values. 
    /// </summary>
#if UNITY_ANDROID
    [ExcludeFromCoverage]
    public class Embrace_Android : IEmbraceProvider
    {
        [DllImport("embrace-native")]
        private static extern bool emb_jniIsAttached();

        private AndroidJavaObject embraceSharedInstance;
        private AndroidJavaObject embraceUnityInternalSharedInstance;
        private AndroidJavaObject embraceInternalSharedInstance;
        private AndroidJavaObject applicationInstance;
        private AndroidJavaObject applicationContext;
        private AndroidJavaObject unityAppFramework;
        private AndroidJavaObject logInfo;
        private AndroidJavaObject logWarning;
        private AndroidJavaObject logError;
        private AndroidJavaClass embraceClass;
        private AndroidJavaClass embraceInternalApiClass;
        private AndroidJavaObject spanFailureCode;
        private AndroidJavaObject spanUserAbandonCode;
        private AndroidJavaObject spanUnknownCode;
        
        private static readonly object mutex = new object();
        private AndroidJavaObject EmbraceSharedInstance
        {
            get
            {
                lock (mutex)
                {
                    if (embraceSharedInstance != null)
                    {
                        embraceSharedInstance = embraceClass.CallStatic<AndroidJavaObject>("getInstance");
                    }
                }

                return embraceSharedInstance;
            }
            set
            {
                lock (mutex)
                {
                    embraceSharedInstance = value;
                }
            }
        }
        
        private AndroidJavaObject _embraceUnityInternalSharedInstance
        {
            get
            {
                if (embraceUnityInternalSharedInstance == null)
                {
                    embraceUnityInternalSharedInstance = _embraceInternalSharedInstance.Call<AndroidJavaObject>(_GetUnityInternalInterfaceMethod);
                }
                return embraceUnityInternalSharedInstance;
            }
        }
        
        private AndroidJavaObject _embraceInternalSharedInstance
        {
            get
            {
                if (embraceInternalSharedInstance == null)
                {
                    embraceInternalSharedInstance = embraceInternalApiClass.CallStatic<AndroidJavaObject>("getInstance");
                }
                return embraceInternalSharedInstance;
            }
        }

        private const string _GetUnityInternalInterfaceMethod = "getUnityInternalInterface";
        private const string _GetInternalInterfaceMethod = "getInternalInterface";
        private const string _StartMethod = "start";
        private const string _GetLastRunEndStateMethod = "getLastRunEndState";
        private const string _LastRunEndStateGetValueMethod = "getValue";
        private const string _SetUserIdentifierMethod = "setUserIdentifier";
        private const string _ClearUserIdentifierMethod = "clearUserIdentifier";
        private const string _SetUserEmailMethod = "setUserEmail";
        private const string _ClearUserEmailMethod = "clearUserEmail";
        private const string _SetUserAsPayerMethod = "setUserAsPayer";
        private const string _ClearUserAsPayerMethod = "clearUserAsPayer";
        private const string _AddUserPersonaMethod = "addUserPersona";
        private const string _ClearUserPersonaMethod = "clearUserPersona";
        private const string _ClearAllUserPersonasMethod = "clearAllUserPersonas";
        private const string _AddSessionPropertyMethod = "addSessionProperty";
        private const string _RemoveSessionPropertyMethod = "removeSessionProperty";
        private const string _GetSessionPropertiesMethod = "getSessionProperties";
        private const string _SetUsernameMethod = "setUsername";
        private const string _ClearUsernameMethod = "clearUsername";
        private const string _StartEventMethod = "startMoment";
        private const string _EndEventMethod = "endMoment";
        private const string _EndAppStartupMethod = "endAppStartup";
        private const string _LogMessageMethod = "logMessage";
        private const string _AddBreadcrumbMethod = "addBreadcrumb";
        private const string _LogPushNotification = "logPushNotification";
        private const string _EndSessionMethod = "endSession";
        private const string _GetDeviceIdMethod = "getDeviceId";
        private const string _StartFragmentMethod = "startView";
        private const string _EndFragmentMethod = "endView";
        private const string _SetUnityMetaDataMethod = "setUnityMetaData";
        private const string _logUnhandledUnityExceptionMethod = "logUnhandledUnityException";
        private const string _logHandledUnityExceptionMethod = "logHandledUnityException";
        private const string _initUnityAndroidConnection = "initUnityConnection";
        private const string _GetCurrentSessionId = "getCurrentSessionId";
        private const string _StartSpanMethod = "startSpan";
        private const string _StopSpanMethod = "stopSpan";
        private const string _AddSpanEventMethod = "addSpanEvent";
        private const string _AddSpanAttributeMethod = "addSpanAttribute";
        private const string _RecordCompleteSpanMethod = "recordCompletedSpan";

        // Java Map Reading
        IntPtr CollectionIterator;
        IntPtr MapEntrySet;
        IntPtr IteratorHasNext;
        IntPtr IteratorNext;
        IntPtr MapEntryGetKey;
        IntPtr MapEntryGetValue;
        IntPtr ObjectToString;
        
        // Java Native Object Types
        AndroidJavaObject integerClass;
        AndroidJavaObject booleanClass;
        AndroidJavaObject longClass;

        // we need some jni pointers to read java maps, it is best to grab them once and cache them
        // these are just pointers to the methods, not actual objects.
        private void CacheJavaMapPointers()
        {
            IntPtr collectionRef = AndroidJNI.FindClass("java/util/Collection");
            IntPtr CollectionClass = AndroidJNI.NewGlobalRef(collectionRef);
            IntPtr mapRef = AndroidJNI.FindClass("java/util/Map");
            IntPtr MapClass = AndroidJNI.NewGlobalRef(mapRef);
            CollectionIterator = AndroidJNI.GetMethodID(CollectionClass, "iterator", "()Ljava/util/Iterator;");
            MapEntrySet = AndroidJNI.GetMethodID(MapClass, "entrySet", "()Ljava/util/Set;");
            IntPtr iterRef = AndroidJNI.FindClass("java/util/Iterator");
            IntPtr IteratorClass = AndroidJNI.NewGlobalRef(iterRef);
            IteratorHasNext = AndroidJNI.GetMethodID(IteratorClass, "hasNext", "()Z");
            IteratorNext = AndroidJNI.GetMethodID(IteratorClass, "next", "()Ljava/lang/Object;");
            IntPtr entryRef = AndroidJNI.FindClass("java/util/Map$Entry");
            IntPtr MapEntryClass = AndroidJNI.NewGlobalRef(entryRef);
            MapEntryGetKey = AndroidJNI.GetMethodID(MapEntryClass, "getKey", "()Ljava/lang/Object;");
            MapEntryGetValue = AndroidJNI.GetMethodID(MapEntryClass, "getValue", "()Ljava/lang/Object;");
            IntPtr objectRef = AndroidJNI.FindClass("java/lang/Object");
            ObjectToString = AndroidJNI.GetMethodID(objectRef, "toString", "()Ljava/lang/String;");
        }

        private void CacheJavaNativeObjectTypes()
        {
            integerClass = new AndroidJavaClass("java.lang.Integer");
            booleanClass = new AndroidJavaClass("java.lang.Boolean");
            longClass = new AndroidJavaClass("java.lang.Long");
        }

        // A touch sloppy here I think
        public bool IsReady => ReadyForCalls();

        private bool ReadyForCalls()
        {
            if (EmbraceSharedInstance == null)
            {
                return false;
            }

            if (emb_jniIsAttached() == false)
            {
                return false;
            }

            if (AndroidJNI.AttachCurrentThread() != 0)
            {
                return false;
            }

            return true;
        }

        private bool UnityInternalInterfaceReadyForCalls()
        {
            return  _embraceUnityInternalSharedInstance != null;
        }
        
        private bool InternalInterfaceReadyForCalls()
        {
            return  _embraceInternalSharedInstance != null;
        }
        
        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log("Embrace Unity SDK initializing java objects");
            CacheJavaMapPointers();
            CacheJavaNativeObjectTypes();
            using AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            applicationInstance = activityInstance.Call<AndroidJavaObject>("getApplication");
            applicationContext = activityInstance.Call<AndroidJavaObject>("getApplicationContext");
            embraceClass = new AndroidJavaClass("io.embrace.android.embracesdk.Embrace");
            embraceInternalApiClass = new AndroidJavaClass("io.embrace.android.embracesdk.internal.EmbraceInternalApi");
            EmbraceSharedInstance = embraceClass.CallStatic<AndroidJavaObject>("getInstance");
            // get the app framework object
            using AndroidJavaClass appFramework = new AndroidJavaClass("io.embrace.android.embracesdk.AppFramework");
            unityAppFramework = appFramework.GetStatic<AndroidJavaObject>("UNITY");
            // get the log severity objects
            using AndroidJavaClass logSeverity = new AndroidJavaClass("io.embrace.android.embracesdk.Severity");
            logInfo = logSeverity.GetStatic<AndroidJavaObject>("INFO");
            logWarning = logSeverity.GetStatic<AndroidJavaObject>("WARNING");
            logError = logSeverity.GetStatic<AndroidJavaObject>("ERROR");
            AndroidJavaClass spanErrorCode = new AndroidJavaClass("io.embrace.android.embracesdk.spans.ErrorCode");
            spanFailureCode = spanErrorCode.GetStatic<AndroidJavaObject>("FAILURE");
            spanUserAbandonCode = spanErrorCode.GetStatic<AndroidJavaObject>("USER_ABANDON");
            spanUnknownCode = spanErrorCode.GetStatic<AndroidJavaObject>("UNKNOWN");
        }

        void IEmbraceProvider.StartSDK(EmbraceStartupArgs args)
        {
            if (!ReadyForCalls())
            {
                return;
            }
            
            // enableIntegrationTesting/isDevMode is no longer supported on Android
            // we hard-code to false as this resolves to a functional method call
            // TODO: Update this to the appropriate method call at a later date
            // We need to replace the applicationInstance with the Context
            EmbraceSharedInstance.Call(_StartMethod, applicationContext, unityAppFramework);
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to get last run end state, Embrace SDK not initialized");
                return LastRunEndState.Invalid;
            }

            try
            {
                using AndroidJavaObject lastRunStateObject = EmbraceSharedInstance.Call<AndroidJavaObject>(_GetLastRunEndStateMethod);
                int lastRunStateInt = lastRunStateObject.Call<int>(_LastRunEndStateGetValueMethod);

                switch (lastRunStateInt)
                {
                    case (int)LastRunEndState.Crash:
                        return LastRunEndState.Crash;

                    case (int)LastRunEndState.CleanExit:
                        return LastRunEndState.CleanExit;

                    default:
                        return LastRunEndState.Invalid;
                }
            }
            catch (Exception e)
            {
                EmbraceLogger.LogException(e);
                return LastRunEndState.Invalid;
            }
        }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to set user identifier, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_SetUserIdentifierMethod, identifier);
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to clear user identifier, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_ClearUserIdentifierMethod);
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to set username, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_SetUsernameMethod, username);
        }

        void IEmbraceProvider.ClearUsername()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to clear username, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_ClearUsernameMethod);
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to set user email, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_SetUserEmailMethod, email);
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to clear user email, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_ClearUserEmailMethod);
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            (this as IEmbraceProvider).AddUserPersona("payer");
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            (this as IEmbraceProvider).ClearUserPersona("payer");
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add user persona, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_AddUserPersonaMethod, persona);
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to clear user persona, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_ClearUserPersonaMethod, persona);
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to clear all user personas, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_ClearAllUserPersonasMethod);
        }

        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add session property, Embrace SDK not initialized");
                return false;
            }

            return EmbraceSharedInstance.Call<bool>(_AddSessionPropertyMethod, key, value, permanent);
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to remove session property, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call<bool>(_RemoveSessionPropertyMethod, key);
        }

        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to get session properties, Embrace SDK not initialized");
                return null;
            }

            using AndroidJavaObject javaMap = EmbraceSharedInstance.Call<AndroidJavaObject>(_GetSessionPropertiesMethod);

            // The Android SDK can return null if this function is called before the SDK is initialized, or if SDK
            // initialization fails. In this case, return an empty dictionary to match behavior on iOS.
            if (javaMap == null)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> dictionary = DictionaryFromJavaMap(javaMap.GetRawObject());
            return dictionary;
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log message, Embrace SDK not initialized");
                return;
            }
            
            using AndroidJavaObject javaMap = DictionaryToJavaMap(properties);

            switch (severity)
            {
                case EMBSeverity.Warning:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logWarning, javaMap);
                    break;
                case EMBSeverity.Error:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logError, javaMap);
                    break;
                default:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logInfo, javaMap);
                    break;
            }
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties, sbyte[] attachment)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log message, Embrace SDK not initialized");
                return;
            }
            
            using AndroidJavaObject javaMap = DictionaryToJavaMap(properties);
            
            switch (severity)
            {
                case EMBSeverity.Warning:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logWarning, javaMap, attachment);
                    break;
                case EMBSeverity.Error:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logError, javaMap, attachment);
                    break;
                default:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logInfo, javaMap, attachment);
                    break;
            }
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            string attachmentId, string attachmentUrl)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log message, Embrace SDK not initialized");
                return;
            }
            
            using AndroidJavaObject javaMap = DictionaryToJavaMap(properties);
            
            switch (severity)
            {
                case EMBSeverity.Warning:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logWarning, javaMap, attachmentId, attachmentUrl);
                    break;
                case EMBSeverity.Error:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logError, javaMap, attachmentId, attachmentUrl);
                    break;
                default:
                    EmbraceSharedInstance.Call(_LogMessageMethod, message, logInfo, javaMap, attachmentId, attachmentUrl);
                    break;
            }
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add breadcrumb, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_AddBreadcrumbMethod, message);
        }

        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to end session, Embrace SDK not initialized");
                return;
            }
            
            EmbraceSharedInstance.Call(_EndSessionMethod, clearUserInfo);
        }

        string IEmbraceProvider.GetDeviceId()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to get device id, Embrace SDK not initialized");
                return null;
            }
            
            return EmbraceSharedInstance.Call<string>(_GetDeviceIdMethod);
        }

        bool IEmbraceProvider.StartView(string name)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to start view, Embrace SDK not initialized");
                return false;
            }
            
            return EmbraceSharedInstance.Call<bool>(_StartFragmentMethod, name);
        }

        bool IEmbraceProvider.EndView(string name)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to end view, Embrace SDK not initialized");
                return false;
            }
            
            return EmbraceSharedInstance.Call<bool>(_EndFragmentMethod, name);
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to set meta data, Embrace SDK not initialized");
                return;
            }

            if (!UnityInternalInterfaceReadyForCalls())
            {
                return;
            }
            
            _embraceUnityInternalSharedInstance.Call(_SetUnityMetaDataMethod, unityVersion, guid, sdkVersion);
        }
        
        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record completed network request, Embrace SDK not initialized");
                return;
            }

            if (!UnityInternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record completed network request, Embrace SDK not initialized");
                return;
            }
            
            // Reference the EmbraceNetworkRequest class
            var networkRequestClass = new AndroidJavaClass("io.embrace.android.embracesdk.network.EmbraceNetworkRequest");

            // Reference the HttpMethod enum
            var httpMethodEnum = new AndroidJavaClass("io.embrace.android.embracesdk.network.http.HttpMethod");
            var httpMethod = httpMethodEnum.GetStatic<AndroidJavaObject>(method.ToString()); // or POST, PUT, etc.

            // Call the static method to get the EmbraceNetworkRequest object
            AndroidJavaObject networkRequest = networkRequestClass.CallStatic<AndroidJavaObject>(
                "fromCompletedRequest",
                url,
                httpMethod,
                startms,
                endms,
                bytesin,
                bytesout,
                code,
                null,
                null,
                null
            );

            // Pass it into your SDK method
            embraceSharedInstance.Call("recordNetworkRequest", networkRequest);
        }
        
        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record incomplete network request, Embrace SDK not initialized");
                return;
            }

            if (!UnityInternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record incomplete network request, Embrace SDK not initialized");
                return;
            }
            
            // Reference the EmbraceNetworkRequest class
            var networkRequestClass = new AndroidJavaClass("io.embrace.android.embracesdk.network.EmbraceNetworkRequest");

            // Reference the HttpMethod enum
            var httpMethodEnum = new AndroidJavaClass("io.embrace.android.embracesdk.network.http.HttpMethod");
            var httpMethod = httpMethodEnum.GetStatic<AndroidJavaObject>(method.ToString()); // or POST, PUT, etc.

            // Call the static method to get the EmbraceNetworkRequest object
            AndroidJavaObject networkRequest = networkRequestClass.CallStatic<AndroidJavaObject>(
                "fromIncompleteRequest",
                url,
                httpMethod,
                startms,
                endms,
                "",
                error,
                null,
                null,
                null
            );

            // Pass it into your SDK method
            embraceSharedInstance.Call("recordNetworkRequest", networkRequest);
        }

        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log unhandled unity exception, Embrace SDK not initialized");
                return;
            }

            if (!UnityInternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log unhandled unity exception, Embrace SDK not initialized");
                return;
            }
            
            _embraceUnityInternalSharedInstance.Call(_logUnhandledUnityExceptionMethod, exceptionName, exceptionMessage, stack);
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log handled unity exception, Embrace SDK not initialized");
                return;
            }

            if (!UnityInternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to log handled unity exception, Embrace SDK not initialized");
                return;
            }
            
            _embraceUnityInternalSharedInstance.Call(_logHandledUnityExceptionMethod, exceptionName, exceptionMessage, stack);
        }
        
        string IEmbraceProvider.GetCurrentSessionId()
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to get current session id, Embrace SDK not initialized");
                return null;
            }
            
            return EmbraceSharedInstance.Call<string>(_GetCurrentSessionId);
        }

        void IEmbraceProvider.RecordPushNotification(AndroidPushNotificationArgs androidArgs)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record push notification, Embrace SDK not initialized");
                return;
            }

            using AndroidJavaObject jNotificationPriority =
                integerClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.notificationPriority);
            using AndroidJavaObject jMessageDeliveredPriority = 
                integerClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.messageDeliveredPriority);
            
            using AndroidJavaObject jIsNotification = booleanClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.isNotification);
            using AndroidJavaObject jHasData = booleanClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.hasData);
            
            EmbraceSharedInstance.Call(_LogPushNotification, androidArgs.title, androidArgs.body, androidArgs.topic, androidArgs.id,
                jNotificationPriority, jMessageDeliveredPriority, jIsNotification, jHasData);
        }
        
        public string StartSpan(string spanName, string parentSpanId, long startTimeMs)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to start span, Embrace SDK not initialized");
                return null;
            }

            if (!InternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to start span, Embrace SDK not initialized");
                return null;
            }
            
            var startTime = longClass.CallStatic<AndroidJavaObject>("valueOf", startTimeMs);
            return _embraceUnityInternalSharedInstance.Call<string>(_StartSpanMethod, spanName, parentSpanId, startTime);
        }

        public bool StopSpan(string spanId, int errorCode, long endTimeMs)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to stop span, Embrace SDK not initialized");
                return false;
            }

            if (!InternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to stop span, Embrace SDK not initialized");
                return false;
            }
            
            var endTime = longClass.CallStatic<AndroidJavaObject>("valueOf", endTimeMs);
            return _embraceUnityInternalSharedInstance.Call<bool>(_StopSpanMethod, spanId, GetSpanErrorCode(errorCode), endTime); 
        }

        public bool AddSpanEvent(string spanId, string spanName, long timestampMs, Dictionary<string, string> attributes)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add span event, Embrace SDK not initialized");
                return false;
            }

            if (!InternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add span event, Embrace SDK not initialized");
                return false;
            }
            
            var timestamp = longClass.CallStatic<AndroidJavaObject>("valueOf", timestampMs);
            return _embraceUnityInternalSharedInstance.Call<bool>(_AddSpanEventMethod, spanId, spanName, timestamp, DictionaryToJavaMap(attributes)); }

        public bool AddSpanAttribute(string spanId, string key, string value)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add span attribute, Embrace SDK not initialized");
                return false;
            }

            if (!InternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to add span attribute, Embrace SDK not initialized");
                return false;
            }
            
            return _embraceUnityInternalSharedInstance.Call<bool>(_AddSpanAttributeMethod, spanId, key, value); }
        
        /// <summary>
        /// 
        /// The map representing a SpanEvent has the following schema:
        ///     {
        ///         "name": [String],
        ///         "timestampMs": [Long] (optional),
        ///         "timestampNanos": [Long] (deprecated and optional),
        ///         "attributes": [Dictionary<string, string>] (optional)
        ///     }
        /// 
        /// Any object passed in the list that violates that schema will be dropped and no event will be created for it. If an entry in the
        /// attributes map isn't <string, string>, it'll also be dropped. Omitting or passing in nulls for the optional fields are OK.
        /// </summary>>
        public bool RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode, string parentSpanId,
            Dictionary<string, string> attributes, EmbraceSpanEvent[] embraceSpanEvents)
        {
            if (!ReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record completed span, Embrace SDK not initialized");
                return false;
            }

            if (!InternalInterfaceReadyForCalls())
            {
                EmbraceLogger.Log("Unable to record completed span, Embrace SDK not initialized");
                return false;
            }
            
            var spanEvents = new List<Dictionary<string, object>>();
            foreach (var embraceSpanEvent in embraceSpanEvents)
            {
                if (embraceSpanEvent != null)
                {
                    spanEvents.Add(embraceSpanEvent.SpanEventToDictionary());
                }
            }

            var dict = DictionariesToJavaListOfMaps(spanEvents, out var disposables);
            var attDict = DictionaryToJavaMap(attributes);
            
            var result = _embraceUnityInternalSharedInstance.Call<bool>(_RecordCompleteSpanMethod, spanName, startTimeMs,
                endTimeMs, errorCode != null ? GetSpanErrorCode(errorCode.Value) : null, 
                parentSpanId, attDict, dict);
            
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }

            return result;
        }

        /// <summary>
        /// This method is used to convert a .NET dictionary to a Java map pointer.
        /// </summary>
        /// <param name="dictionary" of string key and string value></param>
        private static AndroidJavaObject DictionaryToJavaMap(Dictionary<string, string> dictionary)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

            if (dictionary == null)
            {
                return map;
            }

            foreach (var entry in dictionary)
            {
                AndroidJNI.CallObjectMethod(
                    map.GetRawObject(),
                    putMethod,
                    AndroidJNIHelper.CreateJNIArgArray(new object[] { entry.Key, entry.Value })
                );
            }
            
            return map;
        }
        
        /// <summary>
        /// This method is used to convert a .NET dictionary to a Java map pointer.
        /// </summary>
        /// <param name="dictionary" of string key and object value></param>
        private static AndroidJavaObject DictionaryWithObjectsToJavaMap(Dictionary<string, object> dictionary, out List<IDisposable> disposables)
        {
            disposables = new List<IDisposable>();
            
            if (dictionary == null)
            {
                return null;
            }

            var map = new AndroidJavaObject("java.util.HashMap");
            var putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

            foreach (var entry in dictionary)
            {
                if (entry.Key == null)
                {
                    continue;
                }

                var key = new AndroidJavaObject("java.lang.String", entry.Key);
                var value = CreateJavaObjectFromNetObject(entry.Value);

                if (value == null)
                {
                    continue;
                }
                            
                AndroidJNI.CallObjectMethod(
                    map.GetRawObject(),
                    putMethod,
                    AndroidJNIHelper.CreateJNIArgArray(new object[] { key, value })
                );

                value.Dispose();
            }
            
            disposables.Add(map);
            return map;
        }
        
        /// <summary>
        /// This method is used to convert a .NET dictionary to a Java list of maps pointer.
        /// </summary>
        /// <param name="dictionary" of string key and object value></param>
        private static AndroidJavaObject DictionariesToJavaListOfMaps(List<Dictionary<string, object>> dictionaries, out List<IDisposable> disposables)
        {
            disposables = new List<IDisposable>();
            
            if (dictionaries == null)
            {
                return null;
            }

            var arrayList = new AndroidJavaObject("java.util.ArrayList");
            var listAddMethod = AndroidJNIHelper.GetMethodID(arrayList.GetRawClass(), "add", "(Ljava/lang/Object;)Z");

            foreach (var dictionary in dictionaries)
            {
                var map = DictionaryWithObjectsToJavaMap(dictionary, out var inner_disposables);
                
                if (map != null)
                {
                    AndroidJNI.CallBooleanMethod(
                        arrayList.GetRawObject(), 
                        listAddMethod,
                        AndroidJNIHelper.CreateJNIArgArray(new object[] { map }));
                }
                
                disposables.AddRange(inner_disposables);
            }
            
            disposables.Add(arrayList);
            return arrayList;
        }

        /// <summary>
        /// This method is used to convert a Java pointer to a .NET dictionary.
        /// </summary>
        private Dictionary<string, string> DictionaryFromJavaMap(IntPtr source)
        {
            var dict = new Dictionary<string, string>();

            if (source == IntPtr.Zero)
            {
                return dict;
            }

            IntPtr entries = AndroidJNI.CallObjectMethod(source, MapEntrySet, new jvalue[] { });
            IntPtr iterator = AndroidJNI.CallObjectMethod(entries, CollectionIterator, new jvalue[] { });
            AndroidJNI.DeleteLocalRef(entries);

            while (AndroidJNI.CallBooleanMethod(iterator, IteratorHasNext, new jvalue[] { }))
            {
                IntPtr entry = AndroidJNI.CallObjectMethod(iterator, IteratorNext, new jvalue[] { });
                string key = AndroidJNI.CallStringMethod(entry, MapEntryGetKey, new jvalue[] { });
                IntPtr value = AndroidJNI.CallObjectMethod(entry, MapEntryGetValue, new jvalue[] { });
                AndroidJNI.DeleteLocalRef(entry);

                if (value != null && value != IntPtr.Zero)
                {
                    dict.Add(key, AndroidJNI.CallStringMethod(value, ObjectToString, new jvalue[] { }));
                }
                
                AndroidJNI.DeleteLocalRef(value);
            }
            
            AndroidJNI.DeleteLocalRef(iterator);
            return dict;
        }

        /// <summary>
        /// This method is used to convert a .NET object to a Java object.
        /// </summary>
        /// <returns>The proper Android Java Object to use as pointer</returns>
        private static AndroidJavaObject CreateJavaObjectFromNetObject(object netObject)
        {
            if (netObject == null)
            {
                return null;
            }
    
            if (netObject is string s)
            {
                return new AndroidJavaObject("java.lang.String", s);
            }
            if (netObject is int i)
            {
                return new AndroidJavaObject("java.lang.Integer", i);
            }
            if (netObject is long l)
            {
                return new AndroidJavaObject("java.lang.Long", l);
            }
            if (netObject is float f)
            {
                return new AndroidJavaObject("java.lang.Float", f);
            }
            if (netObject is double d)
            {
                return new AndroidJavaObject("java.lang.Double", d);
            }
            if (netObject is bool b)
            {
                return new AndroidJavaObject("java.lang.Boolean", b);
            }
            if (netObject is Dictionary<string, string> dict)
            {
                return DictionaryToJavaMap(dict);
            }
            
            // Add more types as needed
            EmbraceLogger.LogError($"Unsupported type: {netObject.GetType()}");
            return null;
        }
        
        private AndroidJavaObject GetSpanErrorCode(int errorCode)
        {
            switch (errorCode)
            {
                case 1:
                    return spanFailureCode;
                case 2:
                    return spanUserAbandonCode;
                case 3:
                    return spanUnknownCode;
                default: return null;
            }
        }
    }
#endif
}