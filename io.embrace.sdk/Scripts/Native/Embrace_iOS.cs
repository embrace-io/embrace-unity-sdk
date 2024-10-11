using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Internal
{
    /// <summary>
    ///  Embrace_iOS uses statically linked methods that are linked into the executable using the DllImportattribute.  For iOS, we have to use “__Internal” as the library name for the attribute [DllImport("__Internal")].  
    ///  When a user makes a call to our Embrace class this is passed on to a method from the provider interface which then calls the external method. 
    /// </summary>
#if (UNITY_IOS || UNITY_TVOS)
    [ExcludeFromCoverage]
    public class Embrace_iOS : IEmbraceProvider
    {
        // The first prime greater than 255. The number of views in the dictionary managed from Unity's side is *probably* going to be 255 or less.
        // So choosing the first prime greater than that number to reduce the chance of collisions (and therefore resizing).
        private const int INITIALCAPACITY = 257;
        
        // The ConcurrencyLevel (first argument) is recommended by the example docs for ConcurrentDictionary.
        ConcurrentDictionary<string, string> _viewDictionary = new ConcurrentDictionary<string, string>(Environment.ProcessorCount * 2, INITIALCAPACITY);
        
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_start_native(string appId, int config, string appGroupId, string baseUrl,
            string devBaseUrl, string configBaseUrl);

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_is_started();

        [DllImport("__Internal")]
        private static extern IntPtr embrace_ios_sdk_version();

        [DllImport("__Internal")]
        private static extern void embrace_crash();
        
        [DllImport("__Internal")]
        private static extern void embrace_set_unity_metadata(string unityVersion, string guid, string sdkVersion);
        
        [DllImport("__Internal")]
        private static extern void embrace_end_session();

        [DllImport("__Internal")]
        private static extern IntPtr embrace_get_device_id();

        [DllImport("__Internal")]
        private static extern IntPtr embrace_get_session_id();

        [DllImport("__Internal")]
        private static extern int embrace_get_last_run_end_state();

        [DllImport("__Internal")]
        private static extern void embrace_set_user_identifier(string userIdentifier);

        [DllImport("__Internal")]
        private static extern void embrace_clear_user_identifier();

        [DllImport("__Internal")]
        private static extern void embrace_add_breadcrumb(string @event);

        [DllImport("__Internal")]
        private static extern void embrace_set_username(string username);

        [DllImport("__Internal")]
        private static extern void embrace_clear_username();

        [DllImport("__Internal")]
        private static extern void embrace_set_user_email(string email);

        [DllImport("__Internal")]
        private static extern void embrace_clear_user_email();

        [DllImport("__Internal")]
        private static extern void embrace_add_user_persona(string persona);

        [DllImport("__Internal")]
        private static extern void embrace_clear_user_persona(string persona);

        [DllImport("__Internal")]
        private static extern void embrace_clear_all_user_personas();

        [DllImport("__Internal")]
        private static extern bool embrace_add_session_property(string key, string value, bool permanent);

        [DllImport("__Internal")]
        private static extern void embrace_remove_session_property(string key);

        [DllImport("__Internal")]
        private static extern void embrace_log_message_with_severity_and_properties(string message, string severity,
            string propsJson);

        [DllImport("__Internal")]
        private static extern void embrace_set_user_as_payer();

        [DllImport("__Internal")]
        private static extern void embrace_clear_user_as_payer();

        [DllImport("__Internal")]
        private static extern IntPtr embrace_start_view(string viewName);

        [DllImport("__Internal")]
        private static extern bool embrace_end_view(string viewId);

        [DllImport("__Internal")]
        private static extern void embrace_log_network_request(string url, string httpMethod, double startMs, double endMs,
            double bytesSent, double bytesReceived, double statusCode, string error);

        [DllImport("__Internal")]
        private static extern void embrace_log_network_client_error(string url, string httpmMethod, double startMs,
            double endMs, string errorType, string errorMessage);

        [DllImport("__Internal")]
        private static extern IntPtr embrace_start_span(string name, string parentSpanId, double startMs);

        [DllImport("__Internal")]
        private static extern void embrace_stop_span(string spanId, string errorCode, double endMs);

        [DllImport("__Internal")]
        private static extern bool embrace_add_span_event_to_span(string spanId, string name, double time,
            string attributesJson);

        [DllImport("__Internal")]
        private static extern bool embrace_add_span_attribute_to_span(string spanId, string key, string value);

        [DllImport("__Internal")]
        private static extern bool embrace_record_completed_span(string spanName, double startTime, double endTime,
            string errorCodeString, string parentSpanId, string attributesJson, string eventsJson);

        [DllImport("__Internal")]
        private static extern void embrace_log_handled_exception(string name, string message, string stacktrace);

        [DllImport("__Internal")]
        private static extern void embrace_log_unhandled_exception(string name, string message, string stacktrace);
        
        [DllImport("__Internal")]
        private static extern void embrace_log_push_notification(string title, string subtitle, string body, int badge, string category);

        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log("initializing Objc objects");
        }

        void IEmbraceProvider.StartSDK(EmbraceStartupArgs args)
        {
            if (args != null)
            {
                embrace_sdk_start_native(args.AppId,
                    (int) args.Config,
                    args.AppGroupId,
                    args.BaseUrl,
                    args.DevBaseUrl,
                    args.ConfigBaseUrl);
            }
            else
            {
                Debug.LogError("Embrace iOS support requires the use of EmbraceStartupArgs");    
            }
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            return (LastRunEndState) embrace_get_last_run_end_state();
        }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            embrace_set_user_identifier(identifier);
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            embrace_clear_user_identifier();
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            embrace_set_username(username);
        }

        void IEmbraceProvider.ClearUsername()
        {
            embrace_clear_username();
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            embrace_set_user_email(email);
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            embrace_clear_user_email();
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            embrace_set_user_as_payer();
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            embrace_clear_user_as_payer();
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            embrace_add_user_persona(persona);
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            embrace_clear_user_persona(persona);
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            embrace_clear_all_user_personas();
        }

        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            return embrace_add_session_property(key, value, permanent);
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            embrace_remove_session_property(key);
        }
        
        [Obsolete("GetSessionProperties is deprecated on iOS", false)]
        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogWarning("GetSessionProperties is deprecated on iOS");
            #endif
            return null;
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
            
            embrace_log_message_with_severity_and_properties(message, severityString, JsonConvert.SerializeObject(properties));
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            embrace_add_breadcrumb(message);
        }

        // iOS doesn't use clearUserInfo
        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            embrace_end_session();
        }

        string IEmbraceProvider.GetDeviceId()
        {
            return embrace_get_device_id().ConvertToString();
        }

        bool IEmbraceProvider.StartView(string name)
        {
            var spanId = embrace_start_view(name).ConvertToString();
            if (spanId != null)
            {
                _viewDictionary[name] = spanId;
            }

            return spanId != null;
        }

        bool IEmbraceProvider.EndView(string name)
        {
            if (_viewDictionary.TryGetValue(name, out var spanId))
            {
                return embrace_end_view(spanId);
            }

            return false;
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            embrace_set_unity_metadata(unityVersion, guid, sdkVersion);
        }

        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms,
            long bytesin, long bytesout, int code)
        {
            embrace_log_network_request(url, method.ToString(), startms, endms, bytesout, bytesin, code, null);
        }

        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms,
            string error)
        {
            embrace_log_network_request(url, method.ToString(), startms, endms, 0, 0, 0, error);
        }

        void IEmbraceProvider.InstallUnityThreadSampler()
        {
            // not supported on iOS yet
            // No-op
        }

        void IEmbraceProvider.RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            embrace_log_push_notification(iosArgs.title, iosArgs.subtitle, iosArgs.body, iosArgs.badge, iosArgs.category);
        }

        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage,
            string stacktrace)
        {
            embrace_log_unhandled_exception(exceptionName, exceptionMessage, stacktrace);
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stacktrace)
        {
            embrace_log_handled_exception(exceptionName, exceptionMessage, stacktrace);
        }

        string IEmbraceProvider.GetCurrentSessionId()
        {
            return embrace_get_session_id().ConvertToString();
        }

        string IEmbraceProvider.StartSpan(string spanName, string parentSpanId, long startTimeMs)
        {
            var spanId = embrace_start_span(spanName, parentSpanId, startTimeMs).ConvertToString();
            return spanId;
        }

        bool IEmbraceProvider.StopSpan(string spanId, int errorCode, long endTimeMs)
        {
            embrace_stop_span(spanId, errorCode.ToString(), endTimeMs);
            return false;
        }

        bool IEmbraceProvider.AddSpanEvent(string spanName, string spanId, long timestampMs,
            Dictionary<string, string> attributes)
        {
            return embrace_add_span_event_to_span(spanId, spanName, timestampMs, JsonConvert.SerializeObject(attributes));
        }

        bool IEmbraceProvider.AddSpanAttribute(string spanId, string key, string value)
        {
            return embrace_add_span_attribute_to_span(spanId, key, value);
        }

        bool IEmbraceProvider.RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode,
            string parentSpanId,
            Dictionary<string, string> attributes, EmbraceSpanEvent[] events)
        {
            return embrace_record_completed_span(spanName,
                startTimeMs, 
                endTimeMs, 
                (errorCode ?? 0).ToString(), 
                parentSpanId, 
                JsonConvert.SerializeObject(attributes), 
                JsonConvert.SerializeObject(events));
        }
        
        /// <summary>
        /// Provided for internal reference purposes only.
        /// </summary>
        /// <returns></returns>
        public static String GetSDKVersion()
        {
            return embrace_ios_sdk_version().ConvertToString();
        }
    }

    public static class IntPtrExtensions
    {
        public static string ConvertToString(this IntPtr ptr) {
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            
            var str = Marshal.PtrToStringAuto(ptr);

            Marshal.FreeHGlobal(ptr);
            
            return str;
        }
    }
#endif
}
