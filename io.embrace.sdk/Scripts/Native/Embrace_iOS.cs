using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using EmbraceSDK.Internal;
using Newtonsoft.Json;
using UnityEngine.TestTools;

namespace EmbraceSDK.Internal
{
    /// <summary>
    ///  Embrace_iOS uses statically linked methods that are linked into the executable using the DllImportattribute.  For iOS, we have to use “__Internal” as the library name for the attribute [DllImport("__Internal")].  
    ///  When a user makes a call to our Embrace class this is passed on to a method from the provider interface which then calls the external method. 
    /// </summary>
#if UNITY_IOS || UNITY_TVOS
    [ExcludeFromCoverage]
    public class Embrace_iOS : IEmbraceProvider
    {
        [DllImport("__Internal")]
        private static extern void embrace_sdk_start();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_endAppStartup(string properties);

        [DllImport("__Internal")]
        private static extern int embrace_sdk_getLastRunEndState();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_setUserIdentifier(string identifier);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_clearUserIdentifier();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_setUsername(string username);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_clearUsername();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_setUserEmail(string email);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_clearUserEmail();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_setUserAsPayer();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_clearUserAsPayer();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_setUserPersona(string persona);        
        
        [DllImport("__Internal")]
        private static extern void embrace_sdk_addUserPersona(string persona);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_clearUserPersona(string persona);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_clearAllUserPersonas();

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_addSessionProperty(string key, string value, bool permanent);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_removeSessionProperty(string key);

        [DllImport("__Internal")]
        private static extern string embrace_sdk_getSessionProperties();

        [DllImport("__Internal")]
        private static extern void embrace_sdk_startMoment(string name, string identifier, bool allowScreenshot, string properties);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_endMoment(string name, string identifier, string properties);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_logMessage(string message, string severity, string properties, bool allowScreenshot);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_logBreadcrumb(string message);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_endSession(bool clearUserInfo);

        [DllImport("__Internal")]
        private static extern string embrace_sdk_getDeviceId();

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_startView(string name);

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_endView(string name);

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_crash();

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_setUnityMetaData(string unityVersion, string guid, string sdkVersion);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_logNetworkRequest(string url, int method, long startms, long endms, int bytesin, int bytesout, int code, string error);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_logUnhandledUnityException(string exceptionName, string exceptionMessage, string stacktrace);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_logHandledUnityException(string exceptionName, string exceptionMessage, string stacktrace);

        [DllImport("__Internal")]
        private static extern void embrace_sdk_logPushNotification(string title, string subtitle, string body,
            string category, int badge);
        
        [DllImport("__Internal")]
        private static extern string embrace_sdk_getCurrentSessionId();
        
        [DllImport("__Internal")]
        private static extern string embrace_sdk_start_span_with_name(string spanName, string parentSpanId);
        
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_stop_span_with_id(string spanId, int errorCode);
        
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_add_span_event_to_span_id(string spanId, string spanName, long timestampMs, Dictionary<string, string> attributes);
        
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_add_span_attribute_to_span_id(string spanId, string key, string value);
        
        [DllImport("__Internal")]
        private static extern string embrace_sdk_record_completed_span(string name, long startTimeNanos, long endTimeNanos, int errorCode, string parentSpanId, string attributes, string events);

        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log("initializing Objc objects");
        }

        void IEmbraceProvider.StartSDK(bool enableIntegrationTesting)
        {
            embrace_sdk_start();
        }

        void IEmbraceProvider.EndAppStartup(Dictionary<string, string> properties)
        {
            embrace_sdk_endAppStartup(DictionaryToJson(properties));
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            int lastRunStateValue = embrace_sdk_getLastRunEndState();

            switch (lastRunStateValue)
            {
                case (int)LastRunEndState.Crash:
                    return LastRunEndState.Crash;

                case (int)LastRunEndState.CleanExit:
                    return LastRunEndState.CleanExit;

                default:
                    return LastRunEndState.Invalid;
            }
        }

        void IEmbraceProvider.InitNativeSdkConnection() { }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            embrace_sdk_setUserIdentifier(identifier);
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            embrace_sdk_clearUserIdentifier();
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            embrace_sdk_setUsername(username);
        }

        void IEmbraceProvider.ClearUsername()
        {
            embrace_sdk_clearUsername();
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            embrace_sdk_setUserEmail(email);
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            embrace_sdk_clearUserEmail();
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            embrace_sdk_setUserAsPayer();
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            embrace_sdk_clearUserAsPayer();
        }
        
        void IEmbraceProvider.SetUserPersona(string persona)
        {
            embrace_sdk_setUserPersona(persona);
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            embrace_sdk_addUserPersona(persona);
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            embrace_sdk_clearUserPersona(persona);
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            embrace_sdk_clearAllUserPersonas();
        }
            
        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            return embrace_sdk_addSessionProperty(key, value, permanent);
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            embrace_sdk_removeSessionProperty(key);
        }
        
        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            return JsonToDictionary(embrace_sdk_getSessionProperties());
        }
        
        void IEmbraceProvider.StartMoment(string name, string identifier, bool allowScreenshot, Dictionary<string, string> properties)
        {
            embrace_sdk_startMoment(name, identifier, allowScreenshot, DictionaryToJson(properties));
      
        }

        void IEmbraceProvider.EndMoment(string name, string identifier, Dictionary<string, string> properties)
        {
            embrace_sdk_endMoment(name, identifier, DictionaryToJson(properties));
        }
        
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties, bool allowScreenshot)
        {
            (this as IEmbraceProvider).LogMessage(message, severity, properties);
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties)
        {
            string severityString = "";

            switch (severity)
            {
                case EMBSeverity.Info:
                    severityString = "info";
                    break;
                case EMBSeverity.Warning:
                    severityString = "warning";
                    break;
                case EMBSeverity.Error:
                    severityString = "error";
                    break;
            }
            
            embrace_sdk_logMessage(message, severityString, DictionaryToJson(properties), false);
        }

        void IEmbraceProvider.LogBreadcrumb(string message)
        {
            embrace_sdk_logBreadcrumb(message);
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            embrace_sdk_logBreadcrumb(message);
        }

        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            embrace_sdk_endSession(clearUserInfo);
        }
            
        string IEmbraceProvider.GetDeviceId()
        {
            return embrace_sdk_getDeviceId();
        }

        bool IEmbraceProvider.StartView(string name)
        {
            return embrace_sdk_startView(name);
        }
    
        bool IEmbraceProvider.EndView(string name)
        {
            return embrace_sdk_endView(name);
        }
        
        void IEmbraceProvider.Crash()
        {
            embrace_sdk_crash();
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            embrace_sdk_setUnityMetaData(unityVersion, guid, sdkVersion);
        }
        
        private string DictionaryToJson(Dictionary<string, string> dictionary)
        {
            return JsonConvert.SerializeObject(dictionary);
        }
        
        private string DictionaryToJson(List<Dictionary<string, string>> dictionary)
        {
            return JsonConvert.SerializeObject(dictionary);
        }

        private Dictionary<string, string> JsonToDictionary(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code)
        {
            embrace_sdk_logNetworkRequest(url, Embrace.__BridgedHTTPMethod(method), startms, endms, (int)bytesin, (int)bytesout,
                code, "");
        }

        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error)
        {
            embrace_sdk_logNetworkRequest(url, Embrace.__BridgedHTTPMethod(method), startms, endms, 0, 0,0, error);
        }

        void IEmbraceProvider.InstallUnityThreadSampler()
        {
            // not supported on iOS yet
        }

        void IEmbraceProvider.logUnhandledUnityException(string exceptionMessage, string stack)
        {
            embrace_sdk_logUnhandledUnityException("", exceptionMessage, stack);
        }

        void IEmbraceProvider.RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            embrace_sdk_logPushNotification(iosArgs.title, iosArgs.subtitle, iosArgs.body, iosArgs.category, iosArgs.badge);
        }

        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stacktrace)
        {
            embrace_sdk_logUnhandledUnityException(exceptionName, exceptionMessage, stacktrace);
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stacktrace)
        {
            embrace_sdk_logHandledUnityException(exceptionName, exceptionMessage, stacktrace);
        }
        
        string IEmbraceProvider.GetCurrentSessionId()
        {
            return embrace_sdk_getCurrentSessionId();
        }
        
        string IEmbraceProvider.StartSpan(string spanName, string parentSpanId, long startTimeMs)
        {
            return embrace_sdk_start_span_with_name(spanName, parentSpanId);
        }

        bool IEmbraceProvider.StopSpan(string spanId, int errorCode, long endTimeMs)
        {
            return embrace_sdk_stop_span_with_id(spanId, errorCode);
        }

        bool IEmbraceProvider.AddSpanEvent(string spanName, string spanId, long timestampMs, Dictionary<string, string> attributes)
        {
            return embrace_sdk_add_span_event_to_span_id(spanId, spanName, timestampMs, attributes);
        }

        bool IEmbraceProvider.AddSpanAttribute(string spanId, string key, string value)
        {
            return embrace_sdk_add_span_attribute_to_span_id(spanId, key, value); 
        }
        
        bool IEmbraceProvider.RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode, string parentSpanId,
            Dictionary<string, string> attributes, EmbraceSpanEvent events)
        {
            var spanId = embrace_sdk_record_completed_span(spanName, startTimeMs, endTimeMs, errorCode ?? 0, parentSpanId, DictionaryToJson(attributes), JsonConvert.SerializeObject(events));
            return !string.IsNullOrEmpty(spanId);
        }
    }
#endif
}
