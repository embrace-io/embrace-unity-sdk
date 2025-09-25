using System.Collections.Generic;
using EmbraceSDK.Internal;
using UnityEngine;

namespace EmbraceSDK.Editor
{
    /// <summary>
    /// Embrace_Stub is used in edit mode to allow our Embrace class to work while running our SDK in the editor. It does not allow users to call our API from the editor.
    /// Instead it uses the IEmbraceProvider interface to define the methods that are available from our iOS / Android SDK and provides Debug logs to inform users of the call. 
    /// </summary>
    public class Embrace_Stub: IEmbraceProvider
    {
        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_INITIALIZE_SDK);
        }

        #nullable enable
        void IEmbraceProvider.StartSDK(EmbraceStartupArgs? args)
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_START_SDK);
        }
        #nullable disable

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_GET_LAST_RUN_END_STATE);
            return LastRunEndState.Invalid;
        }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            EmbraceLogger.Log($"SetUserIdentifier {identifier}");
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_CLEAR_USER_IDENTIFIER);
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            EmbraceLogger.Log($"SetUsername {username}");
        }

        void IEmbraceProvider.ClearUsername()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_CLEAR_USERNAME);
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            EmbraceLogger.Log($"SetUserEmail {email}");
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_CLEAR_USER_EMAIL);
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_SET_USER_AS_PAYER);
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_CLEAR_USER_AS_PAYER);
        }

        void IEmbraceProvider.AddUserPersona(string persona)
        {
            EmbraceLogger.Log($"AddUserPersona {persona}");
        }

        void IEmbraceProvider.ClearUserPersona(string persona)
        {
            EmbraceLogger.Log($"ClearUserPersona {persona}");
        }

        void IEmbraceProvider.ClearAllUserPersonas()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_CLEAR_ALL_USER_PERSONAS);
        }

        bool IEmbraceProvider.AddSessionProperty(string key, string value, bool permanent)
        {
            EmbraceLogger.Log($"AddSessionProperty key: {key} value: {value}");
            return true;
        }

        void IEmbraceProvider.RemoveSessionProperty(string key)
        {
            EmbraceLogger.Log($"RemoveSessionProperty key: {key}");
        }

        Dictionary<string, string> IEmbraceProvider.GetSessionProperties()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_GET_SESSION_PROPERTIES);
            return new Dictionary<string, string>();
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

            EmbraceLogger.Log($"LogMessage severity: {severityString} message: {message}");
        }

        #if UNITY_ANDROID
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            sbyte[] attachment)
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

            EmbraceLogger.Log($"LogMessage severity: {severityString} message: {message} bytes: {attachment.Length}");
        }
        #elif UNITY_IOS
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            byte[] attachment)
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

            EmbraceLogger.Log($"LogMessage severity: {severityString} message: {message} bytes: {attachment.Length}");
        }
        #endif
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            string attachmentId, string attachmentUrl)
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
            
            EmbraceLogger.Log($"LogMessage severity: {severityString} message: {message} attachmentId: {attachmentId} attachmentUrl: {attachmentUrl}");
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            EmbraceLogger.Log($"AddBreadcrumb {message}");
        }

        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_END_SESSION);
        }

        string IEmbraceProvider.GetDeviceId()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_GET_DEVICE_ID);
            return "";
        }
        
        string IEmbraceProvider.GetCurrentSessionId()
        {
            EmbraceLogger.Log(EmbraceMessages.STUB_GET_CURRENT_SESSION_ID);
            return "";
        }

        bool IEmbraceProvider.StartView(string name)
        {
            EmbraceLogger.Log($"StartView {name}");
            return true;
        }

        bool IEmbraceProvider.EndView(string name)
        {
            EmbraceLogger.Log($"EndView {name}");
            return true;
        }

        void IEmbraceProvider.SetMetaData(string unityVersion, string guid, string sdkVersion)
        {
            EmbraceLogger.Log($"Unity Version = {unityVersion} GUID = {guid} Unity-SDK Version= {sdkVersion}");
        }
        
        void IEmbraceProvider.RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code)
        {
            EmbraceLogger.Log( $"Network Request: {url} method: {method} start: {startms} end: {endms} bytesin: {bytesin} bytesout: {bytesout}");
        }

        void IEmbraceProvider.RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error)
        {
            EmbraceLogger.Log( $"Network Request: {url} method: {method} start: {startms} end: {endms} error: {error}");
        }

        #if UNITY_IOS
        void IEmbraceProvider.RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            EmbraceLogger.Log($"Push Notification: title: {iosArgs.title} subtitle: {iosArgs.subtitle} body: {iosArgs.body} category: {iosArgs.category} badge: {iosArgs.badge}");
        }
        
        #elif UNITY_ANDROID
        void IEmbraceProvider.RecordPushNotification(AndroidPushNotificationArgs androidArgs)
        {
            EmbraceLogger.Log($"Push Notification: title: {androidArgs.title} body: {androidArgs.body} topic: {androidArgs.topic} id: {androidArgs.id} notificationPriority: {androidArgs.notificationPriority} messageDeliveredPriority: {androidArgs.messageDeliveredPriority} isNotification: {androidArgs.isNotification} hasData: {androidArgs.hasData}");
        }
        #endif
        
        void IEmbraceProvider.LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            EmbraceLogger.Log($"Unhandled Exception: {exceptionName} : {exceptionMessage} : stack : {stack}");
        }

        void IEmbraceProvider.LogHandledUnityException(string exceptionName, string exceptionMessage, string stack)
        {
            EmbraceLogger.Log($"Handled Exception: {exceptionName} : {exceptionMessage} : stack : {stack}");
        }
        
        public bool SpanExists(string spanId)
        {
            EmbraceLogger.Log($"Span Exists: span ID {spanId}");
            return true;
        }
        
        public string StartSpan(string spanName, string parentSpanId, long startTimeMs)
        {
            EmbraceLogger.Log($"Start Span: span name {spanName} parent span ID: {parentSpanId}" +
                              $" start time: {startTimeMs}");
            return "spanId";
        }

        public bool StopSpan(string spanId, int errorCode, long endTimeMs)
        {
            EmbraceLogger.Log($"Stop Span: span ID {spanId} error code: {errorCode} end time: {endTimeMs}");
            return true;
        }

        public bool AddSpanEvent(string spanId, string spanName, long timestampMs, Dictionary<string, string> spanEvent)
        {
            EmbraceLogger.Log($"Add Span Event: parent span ID: {spanId} span name: {spanName} " +
                              $"timestamp: {timestampMs} spanEvent: {spanEvent}");
            return true;
        }

        public bool AddSpanAttribute(string spanId, string key, string value)
        {
            EmbraceLogger.Log($"Add Span Attribute: span ID: {spanId} key: {key} value: {value}");
            return true;
        }
        
        public bool RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode, string parentSpanId,
            Dictionary<string, string> attributes, EmbraceSpanEvent[] embraceSpanEvent)
        {
            var attributesString = "";

            if (attributes != null)
            {
                foreach(var kvp in attributes)
                {
                    attributesString += $"{kvp.Key}: {kvp.Value}, ";
                }
            }
            
            EmbraceLogger.Log($"Record Completed Span: span name: {spanName} parent span ID: {parentSpanId}" +
                              $"Attributes: {attributesString}");
            return true;
        }
        
        public void Disable()
        {
            EmbraceLogger.Log("Disable");
        }
    }
}
