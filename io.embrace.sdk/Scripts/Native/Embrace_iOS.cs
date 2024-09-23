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
        private static extern bool embrace_sdk_start_native(string appId, string appGroupId, string baseUrl,
            string devBaseUrl, string configBaseUrl);

        [DllImport("__Internal")]
        private static extern bool embrace_sdk_is_started();

        [DllImport("__Internal")]
        private static extern void crash();
        
        [DllImport("__Internal")]
        private static extern void set_unity_metadata(string unityVersion, string guid, string sdkVersion);
        
        [DllImport("__Internal")]
        private static extern void end_session();

        [DllImport("__Internal")]
        private static extern IntPtr get_device_id();

        [DllImport("__Internal")]
        private static extern IntPtr get_session_id();

        [DllImport("__Internal")]
        private static extern int get_last_run_end_state();

        [DllImport("__Internal")]
        private static extern void set_user_identifier(string userIdentifier);

        [DllImport("__Internal")]
        private static extern void clear_user_identifier();

        [DllImport("__Internal")]
        private static extern void add_breadcrumb(string @event);

        [DllImport("__Internal")]
        private static extern void set_username(string username);

        [DllImport("__Internal")]
        private static extern void clear_username();

        [DllImport("__Internal")]
        private static extern void set_user_email(string email);

        [DllImport("__Internal")]
        private static extern void clear_user_email();

        [DllImport("__Internal")]
        private static extern void add_user_persona(string persona);

        [DllImport("__Internal")]
        private static extern void clear_user_persona(string persona);

        [DllImport("__Internal")]
        private static extern void clear_all_user_personas();

        [DllImport("__Internal")]
        private static extern bool add_session_property(string key, string value, bool permanent);

        [DllImport("__Internal")]
        private static extern void remove_session_property(string key);

        [DllImport("__Internal")]
        private static extern void log_message_with_severity_and_properties(string message, string severity,
            string propsJson);

        [DllImport("__Internal")]
        private static extern void set_user_as_payer();

        [DllImport("__Internal")]
        private static extern void clear_user_as_payer();

        [DllImport("__Internal")]
        private static extern IntPtr start_view(string viewName);

        [DllImport("__Internal")]
        private static extern bool end_view(string viewId);

        [DllImport("__Internal")]
        private static extern void log_network_request(string url, string httpMethod, double startMs, double endMs,
            double bytesSent, double bytesReceived, double statusCode, string error);

        [DllImport("__Internal")]
        private static extern void log_network_client_error(string url, string httpmMethod, double startMs,
            double endMs, string errorType, string errorMessage);

        [DllImport("__Internal")]
        private static extern IntPtr start_span(string name, string parentSpanId, double startMs);

        [DllImport("__Internal")]
        private static extern void stop_span(string spanId, string errorCode, double endMs);

        [DllImport("__Internal")]
        private static extern bool add_span_event_to_span(string spanId, string name, double time,
            string attributesJson);

        [DllImport("__Internal")]
        private static extern bool add_span_attribute_to_span(string spanId, string key, string value);

        [DllImport("__Internal")]
        private static extern bool record_completed_span(string spanName, double startTime, double endTime,
            string errorCodeString, string parentSpanId, string attributesJson, string eventsJson);

        [DllImport("__Internal")]
        private static extern void log_handled_exception(string name, string message, string stacktrace);

        [DllImport("__Internal")]
        private static extern void log_unhandled_exception(string name, string message, string stacktrace);
        
        [DllImport("__Internal")]
        private static extern void log_push_notification(string title, string subtitle, string body, int badge, string category);

        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log("initializing Objc objects");
        }

        void IEmbraceProvider.StartSDK(EmbraceStartupArgs? args, bool enableIntegrationTesting)
        {
            if (args != null)
            {
                var startupArgs = args.Value;
                embrace_sdk_start_native(startupArgs.AppId,
                    startupArgs.AppGroupId,
                    startupArgs.BaseUrl,
                    startupArgs.DevBaseUrl,
                    startupArgs.ConfigBaseUrl);
            }
            else
            {
                Debug.LogError("Embrace iOS support requires the use of EmbraceStartupArgs");    
            }
        }

        void IEmbraceProvider.EndAppStartup(Dictionary<string, string> properties)
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogWarning("This function is deprecated on iOS. New Feature incoming soon. No-op for now.");
            #endif
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            return (LastRunEndState) get_last_run_end_state();
        }
        
        // Deprecated no-op
        void IEmbraceProvider.InitNativeSdkConnection() { }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            set_user_identifier(identifier);
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            clear_user_identifier();
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            set_username(username);
        }

        void IEmbraceProvider.ClearUsername()
        {
            clear_username();
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            set_user_email(email);
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            clear_user_email();
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            set_user_as_payer();
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            clear_user_as_payer();
        }
        
        void IEmbraceProvider.SetUserPersona(string persona)
        {
            add_user_persona(persona);
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            add_user_persona(persona);
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            clear_user_persona(persona);
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            clear_all_user_personas();
        }

        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            return add_session_property(key, value, permanent);
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            remove_session_property(key);
        }
        
        [Obsolete("GetSessionProperties is deprecated on iOS", false)]
        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogWarning("GetSessionProperties is deprecated on iOS");
            #endif
            return null;
        }

        // Moments are deprecated. We will turn this function into a no-op with a warning
        void IEmbraceProvider.StartMoment(string name, string identifier, bool allowScreenshot,
            Dictionary<string, string> properties)
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogWarning("Moments are deprecated on iOS");
            #endif
            
        }

        // Moments are deprecated. We will turn this function into a no-op with a warning
        void IEmbraceProvider.EndMoment(string name, string identifier, Dictionary<string, string> properties)
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogWarning("Moments are deprecated on iOS");
            #endif
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            bool allowScreenshot)
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
            
            log_message_with_severity_and_properties(message, severityString, JsonConvert.SerializeObject(properties));
        }

        // Android redirects LogBreadcrumb to AddBreadcrumb. We will do the same here.
        void IEmbraceProvider.LogBreadcrumb(string message)
        {
            add_breadcrumb(message);
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            add_breadcrumb(message);
        }

        // iOS doesn't use clearUserInfo
        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            end_session();
        }

        string IEmbraceProvider.GetDeviceId()
        {
            return get_device_id().ConvertToString();
        }

        bool IEmbraceProvider.StartView(string name)
        {
            var spanId = start_view(name).ConvertToString();
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
                return end_view(spanId);
            }

            return false;
        }

        void IEmbraceProvider.Crash()
        {
            crash();
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            set_unity_metadata(unityVersion, guid, sdkVersion);
        }

        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms,
            long bytesin, long bytesout, int code)
        {
            log_network_request(url, method.ToString(), startms, endms, bytesout, bytesin, code, null);
        }

        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms,
            string error)
        {
            log_network_request(url, method.ToString(), startms, endms, 0, 0, 0, error);
        }

        void IEmbraceProvider.InstallUnityThreadSampler()
        {
            // not supported on iOS yet
            // No-op
        }

        void IEmbraceProvider.logUnhandledUnityException(string exceptionMessage, string stack)
        {
            log_unhandled_exception("", exceptionMessage, stack);
        }

        void IEmbraceProvider.RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            log_push_notification(iosArgs.title, iosArgs.subtitle, iosArgs.body, iosArgs.badge, iosArgs.category);
        }

        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage,
            string stacktrace)
        {
            log_unhandled_exception(exceptionName, exceptionMessage, stacktrace);
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stacktrace)
        {
            log_handled_exception(exceptionName, exceptionMessage, stacktrace);
        }

        string IEmbraceProvider.GetCurrentSessionId()
        {
            return get_session_id().ConvertToString();
        }

        string IEmbraceProvider.StartSpan(string spanName, string parentSpanId, long startTimeMs)
        {
            return start_span(spanName, parentSpanId, startTimeMs).ConvertToString();
        }

        bool IEmbraceProvider.StopSpan(string spanId, int errorCode, long endTimeMs)
        {
            stop_span(spanId, errorCode.ToString(), endTimeMs);
            return false;
        }

        bool IEmbraceProvider.AddSpanEvent(string spanName, string spanId, long timestampMs,
            Dictionary<string, string> attributes)
        {
            return add_span_event_to_span(spanId, spanName, timestampMs, JsonConvert.SerializeObject(attributes));
        }

        bool IEmbraceProvider.AddSpanAttribute(string spanId, string key, string value)
        {
            return add_span_attribute_to_span(spanId, key, value);
        }

        bool IEmbraceProvider.RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode,
            string parentSpanId,
            Dictionary<string, string> attributes, EmbraceSpanEvent[] events)
        {
            return record_completed_span(spanName,
                startTimeMs, 
                endTimeMs, 
                errorCode.ToString(), 
                parentSpanId, 
                JsonConvert.SerializeObject(attributes), 
                JsonConvert.SerializeObject(events));
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
