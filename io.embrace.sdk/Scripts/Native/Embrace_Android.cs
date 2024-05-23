using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EmbraceSDK.Internal;
using UnityEngine.TestTools;

#if EMBRACE_ENABLE_BUGSHAKE_FORM
using EmbraceSDK.Bugshake;
#endif

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
        private AndroidJavaObject applicationInstance;
        private AndroidJavaObject unityAppFramework;
        private AndroidJavaObject logInfo;
        private AndroidJavaObject logWarning;
        private AndroidJavaObject logError;
        private AndroidJavaClass embraceClass;
        
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
                    
                        if (embraceSharedInstance == null)
                        {
                            EmbraceLogger.LogError("Embrace Unity - Android SDK connection failed to initialize.");
                        }
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
                    embraceUnityInternalSharedInstance = EmbraceSharedInstance?.Call<AndroidJavaObject>(_GetUnityInternalInterfaceMethod);
                }
                return embraceUnityInternalSharedInstance;
            }
        }

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
        private const string _RecordCompletedNetworkRequestMethod = "recordCompletedNetworkRequest";
        private const string _RecordIncompleteNetworkRequestMethod = "recordIncompleteNetworkRequest";
        private const string _logUnhandledUnityExceptionMethod = "logUnhandledUnityException";
        private const string _logHandledUnityExceptionMethod = "logHandledUnityException";
        private const string _initUnityAndroidConnection = "initUnityConnection";
        private const string _installUnityThreadSampler = "installUnityThreadSampler";
        private const string _GetCurrentSessionId = "getCurrentSessionId";
        private const string _GetUnityInternalInterfaceMethod = "getUnityInternalInterface";

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
        }

        // A touch sloppy here I think
        public bool IsReady => ReadyForCalls();

        private bool ReadyForCalls()
        {
            bool result = true;
            
            if (embraceSharedInstance == null)
            {
                EmbraceLogger.LogError("Embrace Unity SDK did not initialize, ensure the prefab is added to the scene.");
                result = false;
            }
            
            if (result == true && emb_jniIsAttached() == false && AndroidJNI.AttachCurrentThread() != 0)
            {
                EmbraceLogger.LogError("Embrace Unity SDK did not initialize, the current thread did not attach to the Java (Dalvik) VM.");
                result = false;
            }
            
            return result;
        }

        private bool UnityInternalInterfaceReadyForCalls()
        {
            bool result = true;
            
            if (_embraceUnityInternalSharedInstance == null)
            {
                EmbraceLogger.LogError("Embrace Unity SDK did not initialize, the internal interface is null. " +
                                       "Check if the SDK is enabled or ensure the prefab is added to the scene.");
                result = false;
            }
            
            return result;
        }
        
        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log("Embrace Unity SDK initializing java objects");
            CacheJavaMapPointers();
            CacheJavaNativeObjectTypes();
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activityInstance = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            applicationInstance = activityInstance.Call<AndroidJavaObject>("getApplication");
            embraceClass = new AndroidJavaClass("io.embrace.android.embracesdk.Embrace");
            EmbraceSharedInstance = embraceClass.CallStatic<AndroidJavaObject>("getInstance");
            // get the app framework object
            AndroidJavaClass appFramework = new AndroidJavaClass("io.embrace.android.embracesdk.Embrace$AppFramework");
            unityAppFramework = appFramework.GetStatic<AndroidJavaObject>("UNITY");
            // get the log severity objects
            AndroidJavaClass logSeverity = new AndroidJavaClass("io.embrace.android.embracesdk.Severity");
            logInfo = logSeverity.GetStatic<AndroidJavaObject>("INFO");
            logWarning = logSeverity.GetStatic<AndroidJavaObject>("WARNING");
            logError = logSeverity.GetStatic<AndroidJavaObject>("ERROR");
        }

        void IEmbraceProvider.StartSDK(bool enableIntegrationTesting)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_StartMethod, applicationInstance, enableIntegrationTesting, unityAppFramework);
        }

        void IEmbraceProvider.EndAppStartup(Dictionary<string, string> properties)
        {
            if (!ReadyForCalls()) { return; }
            AndroidJavaObject javaMap = DictionaryToJavaMap(properties);
            EmbraceSharedInstance?.Call(_EndAppStartupMethod, javaMap);
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            if (!ReadyForCalls()) { return LastRunEndState.Invalid; }

            try
            {
                AndroidJavaObject lastRunStateObject = EmbraceSharedInstance?.Call<AndroidJavaObject>(_GetLastRunEndStateMethod);
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

        void IEmbraceProvider.InitNativeSdkConnection() { }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_SetUserIdentifierMethod, identifier);
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_ClearUserIdentifierMethod);
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_SetUsernameMethod, username);
        }
        
        #if UNITY_ANDROID && EMBRACE_ENABLE_BUGSHAKE_FORM
        void IEmbraceProvider.ShowBugReportForm()
        {
            if (!ReadyForCalls())
            {
                return;
            }

            EmbraceSharedInstance?.Call("showBugReportForm");
        }
        #endif

        void IEmbraceProvider.ClearUsername()
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_ClearUsernameMethod);
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_SetUserEmailMethod, email);
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_ClearUserEmailMethod);
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_SetUserAsPayerMethod);
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_ClearUserAsPayerMethod);
        }

        void IEmbraceProvider.SetUserPersona(string persona)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_AddUserPersonaMethod, persona);
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_AddUserPersonaMethod, persona);
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_ClearUserPersonaMethod, persona);
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_ClearAllUserPersonasMethod);
        }

        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            if (!ReadyForCalls()) { return false; }
            return EmbraceSharedInstance?.Call<bool>(_AddSessionPropertyMethod, key, value, permanent) ?? false;
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call<bool>(_RemoveSessionPropertyMethod, key);
        }

        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            if (!ReadyForCalls()) { return null; }

            AndroidJavaObject javaMap = EmbraceSharedInstance?.Call<AndroidJavaObject>(_GetSessionPropertiesMethod);

            // The Android SDK can return null if this function is called before the SDK is initialized, or if SDK
            // initialization fails. In this case, return an empty dictionary to match behavior on iOS.
            if (javaMap == null)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> dictionary = DictionaryFromJavaMap(javaMap.GetRawObject());
            return dictionary;
        }

        void IEmbraceProvider.StartMoment(string name, string identifier, bool allowScreenshot, Dictionary<string, string> properties)
        {
            if (!ReadyForCalls()) { return; }
            AndroidJavaObject javaMap = DictionaryToJavaMap(properties);
            EmbraceSharedInstance?.Call(_StartEventMethod, name, identifier, javaMap);
        }

        void IEmbraceProvider.EndMoment(string name, string identifier, Dictionary<string, string> properties)
        {
            if (!ReadyForCalls()) { return; }
            AndroidJavaObject javaMap = DictionaryToJavaMap(properties);
            EmbraceSharedInstance?.Call(_EndEventMethod, name, identifier, javaMap);
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties)
        {
            if (!ReadyForCalls()) { return; }
            AndroidJavaObject javaMap = DictionaryToJavaMap(properties);

            switch (severity)
            {
                case EMBSeverity.Info:
                    EmbraceSharedInstance?.Call(_LogMessageMethod, message, logInfo, javaMap);
                    break;
                case EMBSeverity.Warning:
                    EmbraceSharedInstance?.Call(_LogMessageMethod, message, logWarning, javaMap);
                    break;
                case EMBSeverity.Error:
                    EmbraceSharedInstance?.Call(_LogMessageMethod, message, logError, javaMap);
                    break;
            }
        }
        
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties, bool allowScreenshot)
        {
            (this as IEmbraceProvider).LogMessage(message, severity, properties);
        }

        void IEmbraceProvider.LogBreadcrumb(string message)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_AddBreadcrumbMethod, message);
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_AddBreadcrumbMethod, message);
        }

        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            if (!ReadyForCalls()) { return; }
            EmbraceSharedInstance?.Call(_EndSessionMethod, clearUserInfo);
        }

        string IEmbraceProvider.GetDeviceId()
        {
            if (!ReadyForCalls()) { return null; }
            return EmbraceSharedInstance?.Call<string>(_GetDeviceIdMethod);
        }

        bool IEmbraceProvider.StartView(string name)
        {
            if (!ReadyForCalls()) { return false; }
            return EmbraceSharedInstance?.Call<bool>(_StartFragmentMethod, name) ?? false;
        }

        bool IEmbraceProvider.EndView(string name)
        {
            if (!ReadyForCalls()) { return false; }
            return EmbraceSharedInstance?.Call<bool>(_EndFragmentMethod, name) ?? false;
        }
        
        void IEmbraceProvider.Crash()
        {
            // Removed on Android 6.+ as it is no longer supported
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            if (!ReadyForCalls()) { return; }
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            _embraceUnityInternalSharedInstance.Call(_SetUnityMetaDataMethod, unityVersion, guid, sdkVersion);
        }
        
        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code)
        {
            if (!ReadyForCalls()) { return; }
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            EmbraceLogger.Log($"Network Request: {url} method: {method} start: {startms} end: {endms} bytesin: {bytesin} bytesout: {bytesout}");
            _embraceUnityInternalSharedInstance.Call(_RecordCompletedNetworkRequestMethod, url, method.ToString(), startms, endms, bytesout, bytesin, code, null);
        }
        
        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error)
        {
            if (!ReadyForCalls()) { return; }
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            EmbraceLogger.Log($"Network Request: {url} method: {method} start: {startms} end: {endms} error: {error}");
            _embraceUnityInternalSharedInstance.Call(_RecordIncompleteNetworkRequestMethod, url, method.ToString(), startms, endms, null, error, null);
        }

        void IEmbraceProvider.InstallUnityThreadSampler()
        {
            if (!ReadyForCalls()) { return; }
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            _embraceUnityInternalSharedInstance.Call(_installUnityThreadSampler);
        }

        void IEmbraceProvider.logUnhandledUnityException(string exceptionMessage, string stack)
        {
            (this as IEmbraceProvider).LogUnhandledUnityException("", exceptionMessage, stack);
        }

        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (!ReadyForCalls()) { return; }
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            _embraceUnityInternalSharedInstance.Call(_logUnhandledUnityExceptionMethod, exceptionName, exceptionMessage, stack);
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            if (!ReadyForCalls()) { return; }
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            _embraceUnityInternalSharedInstance.Call(_logHandledUnityExceptionMethod, exceptionName, exceptionMessage, stack);
        }
        
        string IEmbraceProvider.GetCurrentSessionId()
        {
            if (!ReadyForCalls()) { return null; }
            return EmbraceSharedInstance?.Call<string>(_GetCurrentSessionId);
        }

        void IEmbraceProvider.RecordPushNotification(AndroidPushNotificationArgs androidArgs)
        {
            if (!ReadyForCalls()) { return; }

            var jNotificationPriority =
                integerClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.notificationPriority);
            var jMessageDeliveredPriority = 
                integerClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.messageDeliveredPriority);
            
            var jIsNotification = booleanClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.isNotification);
            var jHasData = booleanClass.CallStatic<AndroidJavaObject>("valueOf", androidArgs.hasData);
            
            EmbraceSharedInstance?.Call(_LogPushNotification, androidArgs.title, androidArgs.body, androidArgs.topic, androidArgs.id,
                jNotificationPriority, jMessageDeliveredPriority, jIsNotification, jHasData);
        }
        
        #if EMBRACE_ENABLE_BUGSHAKE_FORM
        void IEmbraceProvider.setShakeListener(UnityShakeListener listener)
        {
            if (!ReadyForCalls()) { return;}
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            _embraceUnityInternalSharedInstance.Call("setShakeListener", listener);
        }
        
        void IEmbraceProvider.saveShakeScreenshot(byte[] screenshot)
        {
            if (!ReadyForCalls()) { return;}
            if(!UnityInternalInterfaceReadyForCalls()) { return; }
            _embraceUnityInternalSharedInstance.Call("saveScreenshot", screenshot);
        }
        #endif

        private AndroidJavaObject DictionaryToJavaMap(Dictionary<string, string> dictionary)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
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
    }
#endif
}
