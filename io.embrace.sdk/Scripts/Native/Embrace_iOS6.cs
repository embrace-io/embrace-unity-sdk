using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace EmbraceSDK.Internal
{
    #if UNITY_IOS || UNITY_TVOS
    public class Embrace_iOS6
    {
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_start_native(string appId, string appGroupId, string baseUrl, string devBaseUrl, string configBaseUrl);
        
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_is_started();

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
        private static extern void add_session_property(string key, string value, bool permanent);
        
        [DllImport("__Internal")]
        private static extern void remove_session_property(string key);

        [DllImport("__Internal")]
        private static extern void log_message_with_severity_and_properties(string message, string severity, string propsJson);

        [DllImport("__Internal")]
        private static extern void set_user_as_payer();
        
        [DllImport("__Internal")]
        private static extern void clear_user_as_payer();

        [DllImport("__Internal")]
        private static extern IntPtr start_view(string viewName);

        [DllImport("__Internal")]
        private static extern void end_view(string viewId);

        [DllImport("__Internal")]
        private static extern void log_network_request(string url, string httpMethod, double startMs, double endMs,
            double bytesSent, double bytesReceived, double statusCode);

        [DllImport("__Internal")]
        private static extern void log_network_client_error(string url, string httpmMethod, double startMs,
            double endMs, string errorType, string errorMessage);

        [DllImport("__Internal")]
        private static extern IntPtr start_span(string name, string parentSpanId, double startMs);

        [DllImport("__Internal")]
        private static extern void stop_span(string spanId, string errorCode, double endMs);

        [DllImport("__Internal")]
        private static extern void add_span_event_to_span(string spanId, string name, double time,
            string attributesJson);

        [DllImport("__Internal")]
        private static extern void add_span_attribute_to_span(string spanId, string key, string value);
        
        [DllImport("__Internal")]
        private static extern void record_completed_span(string spanName, double startTime, double endTime, string errorCodeString, string parentSpanId, string attributesJson, string eventsJson);
        
        [DllImport("__Internal")]
        private static extern void log_handled_exception(string name, string message, string stacktrace);

        [DllImport("__Internal")]
        private static extern void log_unhandled_exception(string name, string message, string stacktrace);

        [DllImport("__Internal")] // TODO: Remove this function before release
        private static extern IntPtr test_string();
        
        public bool EmbraceSDKIsStarted() => embrace_sdk_is_started();

        public bool EmbraceSDKStartNative(string appId, string appGroupId = null,
            (string baseUrl, string devBaseUrl, string configBaseUrl)? endpoints = null)
        {
            if (endpoints == null)
            {
                return embrace_sdk_start_native(appId, appGroupId, null, null, null);
            }
            else
            {
                var (baseUrl, devBaseUrl, configBaseUrl) = endpoints.Value;
                return embrace_sdk_start_native(appId, appGroupId, baseUrl, devBaseUrl, configBaseUrl);
            }
        } 

        public string GetDeviceId() => get_device_id().ConvertToString();
        
        public string GetSessionId() => get_session_id().ConvertToString();

        public LastRunEndState GetLastRunEndState() => 
            (LastRunEndState) get_last_run_end_state();
        
        public void SetUserIdentifier(string userIdentifier) => 
            set_user_identifier(userIdentifier);
        
        public void ClearUserIdentifier() => clear_user_identifier();
        
        public void AddBreadcrumb(string @event) => add_breadcrumb(@event);
        
        public void SetUsername(string username) => set_username(username);
        
        public void ClearUsername() => clear_username();
        
        public void SetUserEmail(string email) => set_user_email(email);
        
        public void ClearUserEmail() => clear_user_email();
        
        public void AddUserPersona(string persona) => add_user_persona(persona);
        
        public void ClearUserPersona(string persona) => clear_user_persona(persona);
        
        public void ClearAllUserPersonas() => clear_all_user_personas();
        
        public void AddSessionProperty(string key, string value, bool permanent) => 
            add_session_property(key, value, permanent);
        
        public void RemoveSessionProperty(string key) => remove_session_property(key);

        public void LogMessageWithSeverityAndProperties(string message, EMBSeverity severity,
            Dictionary<string, string> props)
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
            
            log_message_with_severity_and_properties(message, severityString, JsonConvert.SerializeObject(props));
        } 
        
        public void SetUserAsPayer() => set_user_as_payer();
        
        public void ClearUserAsPayer() => clear_user_as_payer();
        
        public string StartView(string viewName) => start_view(viewName).ConvertToString();
        
        public void EndView(string viewId) => end_view(viewId);
        
        public void LogNetworkRequest(string url, string httpMethod, double startMs, double endMs,
            double bytesSent, double bytesReceived, double statusCode) => 
            log_network_request(url, httpMethod, startMs, endMs, bytesSent, bytesReceived, statusCode);
        
        public void LogNetworkClientError(string url, string httpMethod, double startMs, double endMs, string errorType, string errorMessage) => 
            log_network_client_error(url, httpMethod, startMs, endMs, errorType, errorMessage);
        
        public string StartSpan(string name, string parentSpanId, double startMs) => 
            start_span(name, parentSpanId, startMs).ConvertToString();
        
        public void StopSpan(string spanId, string errorCode, double endMs) => 
            stop_span(spanId, errorCode, endMs);
        
        public void AddSpanEventToSpan(string spanId, string name, double time, Dictionary<string, string> attributes) =>
            add_span_event_to_span(spanId, name, time, JsonConvert.SerializeObject(attributes));
        
        public void AddSpanAttributeToSpan(string spanId, string key, string value) =>
            add_span_attribute_to_span(spanId, key, value);
        
        public void RecordCompletedSpan(string spanName, 
            long startTimeMs, 
            long endTimeMs, 
            string errorCodeString, 
            string parentSpanId, 
            Dictionary<string, string> attributes, 
            EmbraceSpanEvent[] events)
        {
            record_completed_span(spanName, 
                startTimeMs, 
                endTimeMs, 
                errorCodeString, 
                parentSpanId,
                JsonConvert.SerializeObject(attributes), 
                JsonConvert.SerializeObject(events));
        }
        
        public void LogHandledException(string name, string message, string stacktrace) => 
            log_handled_exception(name, message, stacktrace);
        
        public void LogUnhandledException(string name, string message, string stacktrace) =>
            log_unhandled_exception(name, message, stacktrace);

        public string TestString() => test_string().ConvertToString();
    }

    public static class IntPtrExtensions
    {
        public static string ConvertToString(this IntPtr ptr) {
            var str = Marshal.PtrToStringAuto(ptr);

            Marshal.FreeHGlobal(ptr);
            
            return str;
        }
    }
    #endif
}