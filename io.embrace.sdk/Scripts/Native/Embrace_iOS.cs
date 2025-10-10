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
            string devBaseUrl, string configBaseUrl, string[] ignoredUrls, int ignoredUrlsLength);

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
        private static extern void embrace_log_message_with_attachment(string message, string severity,
            string propsJson, byte[] attachment, int length);

        [DllImport("__Internal")]
        private static extern void embrace_log_message_with_attachment_url(string message, string severity,
            string propsJson, string attachmentId, string attachmentUrl);

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
        private static extern bool embrace_stop_span(string spanId, string errorCode, double endMs);

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

        [DllImport("__Internal")]
        private static extern void embrace_disable();
        
        [DllImport("__Internal")]
        private static extern bool embrace_span_exists(string spanId);
        
        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log(EmbraceMessages.IOS_SDK_INITIALIZED);
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
                    args.ConfigBaseUrl,
                    args.IgnoredUrls?.ToArray(),
                    args.IgnoredUrls?.Count ?? 0);
            }
            else
            {
                Debug.LogError(EmbraceMessages.STARTUP_ARGS_ERROR);    
            }
        }

        bool IsReadyForCalls()
        {
            return embrace_sdk_is_started();
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.LAST_RUN_STATE_ERROR);
                return LastRunEndState.Invalid;
            }
            
            return (LastRunEndState) embrace_get_last_run_end_state();
        }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.SET_USER_IDENTIFIER_ERROR);
                return;
            }
            
            embrace_set_user_identifier(identifier);
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.CLEAR_USER_IDENTIFIER_ERROR);
                return;
            }
            
            embrace_clear_user_identifier();
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.SET_USERNAME_ERROR);
                return;
            }
            
            embrace_set_username(username);
        }

        void IEmbraceProvider.ClearUsername()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.CLEAR_USERNAME_ERROR);
                return;
            }
            
            embrace_clear_username();
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.SET_USER_EMAIL_ERROR);
                return;
            }
            
            embrace_set_user_email(email);
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.CLEAR_USER_EMAIL_ERROR);
                return;
            }
            
            embrace_clear_user_email();
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.SET_USER_AS_PAYER_ERROR);
                return;
            }
            
            embrace_set_user_as_payer();
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.CLEAR_USER_AS_PAYER_ERROR);
                return;
            }
            
            embrace_clear_user_as_payer();
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.ADD_USER_PERSONA_ERROR);
                return;
            }
            
            embrace_add_user_persona(persona);
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.CLEAR_USER_PERSONA_ERROR);
                return;
            }
            
            embrace_clear_user_persona(persona);
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.CLEAR_ALL_USER_PERSONAS_ERROR);
                return;
            }
            
            embrace_clear_all_user_personas();
        }

        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.ADD_SESSION_PROPERTY_ERROR);
                return false;
            }
            
            return embrace_add_session_property(key, value, permanent);
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.REMOVE_SESSION_PROPERTY_ERROR);
                return;
            }
            
            embrace_remove_session_property(key);
        }
        
        [Obsolete("GetSessionProperties is deprecated on iOS", false)]
        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogWarning(EmbraceMessages.GET_SESSION_PROPERTIES_DEPRECATED);
            #endif
            return null;
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.LOG_MESSAGE_ERROR);
                return;
            }
            
            if (severity.TryConvertToString(out var severityString))
            {
                embrace_log_message_with_severity_and_properties(message, severityString, JsonConvert.SerializeObject(properties));    
            }
            else
            {
                EmbraceLogger.LogError(EmbraceMessages.STRING_CONVERSION_ERROR);
            }
        }
        
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties, byte[] attachment)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.LOG_MESSAGE_ERROR);
                return;
            }
            
            if (severity.TryConvertToString(out var severityString))
            {
                embrace_log_message_with_attachment(message, severityString, JsonConvert.SerializeObject(properties), attachment, attachment.Length);    
            }
            else
            {
                EmbraceLogger.LogError(EmbraceMessages.STRING_CONVERSION_ERROR);
            }
        }

        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            string attachmentId, string attachmentUrl)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.LOG_MESSAGE_ERROR);
                return;
            }
            
            if (severity.TryConvertToString(out var severityString))
            {
                embrace_log_message_with_attachment_url(message, severityString, JsonConvert.SerializeObject(properties), attachmentId, attachmentUrl);
            }
            else
            {
                EmbraceLogger.LogError(EmbraceMessages.STRING_CONVERSION_ERROR);
            }
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.ADD_BREADCRUMB_ERROR);
                return;
            }
            
            embrace_add_breadcrumb(message);
        }

        // iOS doesn't use clearUserInfo
        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.END_SESSION_ERROR);
                return;
            }
            
            embrace_end_session();
        }

        string IEmbraceProvider.GetDeviceId()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.GET_DEVICE_ID_ERROR);
                return null;
            }
            
            return embrace_get_device_id().ConvertToString();
        }

        bool IEmbraceProvider.StartView(string name)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.START_VIEW_ERROR);
                return false;
            }
            
            var spanId = embrace_start_view(name).ConvertToString();
            if (spanId != null)
            {
                _viewDictionary[name] = spanId;
            }

            return spanId != null;
        }

        bool IEmbraceProvider.EndView(string name)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.END_VIEW_ERROR);
                return false;
            }
            
            if (_viewDictionary.TryGetValue(name, out var spanId))
            {
                return embrace_end_view(spanId);
            }

            return false;
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.SET_METADATA_ERROR);
                return;
            }
            
            embrace_set_unity_metadata(unityVersion, guid, sdkVersion);
        }

        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms,
            long bytesin, long bytesout, int code)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.RECORD_COMPLETED_NETWORK_REQUEST_ERROR);
                return;
            }
            
            embrace_log_network_request(url, method.ToString(), startms, endms, bytesout, bytesin, code, null);
        }

        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms,
            string error)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.RECORD_INCOMPLETE_NETWORK_REQUEST_ERROR);
                return;
            }
            
            embrace_log_network_request(url, method.ToString(), startms, endms, 0, 0, 0, error);
        }

        void IEmbraceProvider.RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.RECORD_PUSH_NOTIFICATION_ERROR);
                return;
            }
            
            embrace_log_push_notification(iosArgs.title, iosArgs.subtitle, iosArgs.body, iosArgs.badge, iosArgs.category);
        }

        void IEmbraceProvider.Disable()
        {
            embrace_disable();
        }

        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage,
            string stacktrace)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.LOG_UNHANDLED_UNITY_EXCEPTION_ERROR);
                return;
            }
            
            embrace_log_unhandled_exception(exceptionName, exceptionMessage, stacktrace);
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stacktrace)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.LOG_HANDLED_UNITY_EXCEPTION_ERROR);
                return;
            }
            
            embrace_log_handled_exception(exceptionName, exceptionMessage, stacktrace);
        }

        string IEmbraceProvider.GetCurrentSessionId()
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.GET_CURRENT_SESSION_ID_ERROR);
                return null;
            }
            
            return embrace_get_session_id().ConvertToString();
        }
        
        bool IEmbraceProvider.SpanExists(string spanId)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.SPAN_EXISTS_ERROR);
                return false;
            }
            
            return embrace_span_exists(spanId);
        }

        string IEmbraceProvider.StartSpan(string spanName, string parentSpanId, long startTimeMs)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.START_SPAN_ERROR);
                return null;
            }
            
            var spanId = embrace_start_span(spanName, parentSpanId, startTimeMs).ConvertToString();
            return spanId;
        }

        bool IEmbraceProvider.StopSpan(string spanId, int errorCode, long endTimeMs)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.STOP_SPAN_ERROR);
                return false;
            }
            
            return embrace_stop_span(spanId, IntToStringErrorCode(errorCode), endTimeMs);
        }

        bool IEmbraceProvider.AddSpanEvent(string spanName, string spanId, long timestampMs,
            Dictionary<string, string> attributes)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.ADD_SPAN_EVENT_ERROR);
                return false;
            }
            
            return embrace_add_span_event_to_span(spanId, spanName, timestampMs, JsonConvert.SerializeObject(attributes));
        }

        bool IEmbraceProvider.AddSpanAttribute(string spanId, string key, string value)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.ADD_SPAN_ATTRIBUTE_ERROR);
                return false;
            }
            
            return embrace_add_span_attribute_to_span(spanId, key, value);
        }

        bool IEmbraceProvider.RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode,
            string parentSpanId,
            Dictionary<string, string> attributes, EmbraceSpanEvent[] events)
        {
            if (IsReadyForCalls() == false)
            {
                EmbraceLogger.LogError(EmbraceMessages.RECORD_COMPLETED_SPAN_ERROR);
                return false;
            }
            
            // because we are serializing the attributes and events to JSON we need to ensure that they are not null
            attributes ??= new Dictionary<string, string>();
            events ??= Array.Empty<EmbraceSpanEvent>();
            
            return embrace_record_completed_span(spanName,
                startTimeMs, 
                endTimeMs, 
                IntToStringErrorCode(errorCode ?? 0), 
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

        private static string IntToStringErrorCode(int errorCode)
        {
            string errCodeString = "";
            switch (errorCode)
            {
                case 1: errCodeString = "Failure"; break;
                case 2: errCodeString = "UserAbandon"; break;
                case 3: errCodeString = "Unknown"; break;
            }

            return errCodeString;
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

    public static class EmbSeverityExtensions
    {
        public static bool TryConvertToString(this EMBSeverity severity, out string str)
        {
            switch (severity)
            {
                case EMBSeverity.Info:
                    str = "info";
                    return true;
                case EMBSeverity.Warning:
                    str = "warning";
                    return true;
                case EMBSeverity.Error:
                    str = "error";
                    return true;
                default:
                    str = null;
                    return false;
            }
        }
    }
#endif
}
