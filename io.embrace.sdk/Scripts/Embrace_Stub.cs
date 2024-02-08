using System.Collections.Generic;
using UnityEngine;

#if EMBRACE_ENABLE_BUGSHAKE_FORM
using EmbraceSDK.Bugshake;
#endif

namespace EmbraceSDK
{
    /// <summary>
    /// Embrace_Stub is used in edit mode to allow our Embrace class to work while running our SDK in the editor. It does not allow users to call our API from the editor.
    /// Instead it uses the IEmbraceProvider interface to define the methods that are available from our iOS / Android SDK and provides Debug logs to inform users of the call. 
    /// </summary>
    public class Embrace_Stub: IEmbraceProvider
    {
        void IEmbraceProvider.InitializeSDK()
        {
            EmbraceLogger.Log("InitializeSDK");
        }

        void IEmbraceProvider.StartSDK(bool enableIntegrationTesting)
        {
            EmbraceLogger.Log("StartSDK");
        }

        void IEmbraceProvider.EndAppStartup(Dictionary<string, string> properties)
        {
            EmbraceLogger.Log("EndAppStartup");
        }

        LastRunEndState IEmbraceProvider.GetLastRunEndState()
        {
            EmbraceLogger.Log("GetLastRunEndState");
            return LastRunEndState.Invalid;
        }

        void IEmbraceProvider.InitNativeSdkConnection()
        {
            EmbraceLogger.Log("InitUnityCallback");
        }

        void IEmbraceProvider.SetUserIdentifier(string identifier)
        {
            EmbraceLogger.Log($"SetUserIdentifier {identifier}");
        }

        void IEmbraceProvider.ClearUserIdentifier()
        {
            EmbraceLogger.Log("ClearUserIdentifier");
        }

        void IEmbraceProvider.SetUsername(string username)
        {
            EmbraceLogger.Log($"SetUsername {username}");
        }

        void IEmbraceProvider.ClearUsername()
        {
            EmbraceLogger.Log("ClearUsername");
        }

        void IEmbraceProvider.SetUserEmail(string email)
        {
            EmbraceLogger.Log($"SetUserEmail {email}");
        }

        void IEmbraceProvider.ClearUserEmail()
        {
            EmbraceLogger.Log("ClearUserEmail");
        }

        void IEmbraceProvider.SetUserAsPayer()
        {
            EmbraceLogger.Log("SetUserAsPayer");
        }

        void IEmbraceProvider.ClearUserAsPayer()
        {
            EmbraceLogger.Log("ClearUserAsPayer");
        }

        void IEmbraceProvider.SetUserPersona(string persona)
        {
            EmbraceLogger.Log($"SetUserPersona {persona}");
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
            EmbraceLogger.Log("ClearAllUserPersonas");
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
            EmbraceLogger.Log("GetSessionProperties");
            return new Dictionary<string, string>();
        }

        void IEmbraceProvider.StartMoment(string name, string identifier, bool allowScreenshot, Dictionary<string, string> properties)
        {
            EmbraceLogger.Log($"StartMoment {name}");
        }

        void IEmbraceProvider.EndMoment(string name, string identifier, Dictionary<string, string> properties)
        {
            EmbraceLogger.Log($"EndMoment {name}");
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
        void IEmbraceProvider.LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties, bool allowScreenshot)
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

        void IEmbraceProvider.LogBreadcrumb(string message)
        {
            EmbraceLogger.Log($"LogBreadcrumb {message}");
        }

        void IEmbraceProvider.AddBreadcrumb(string message)
        {
            EmbraceLogger.Log($"AddBreadcrumb {message}");
        }

        void IEmbraceProvider.EndSession(bool clearUserInfo)
        {
            EmbraceLogger.Log("EndSession");
        }

        string IEmbraceProvider.GetDeviceId()
        {
            EmbraceLogger.Log("GetDeviceId");
            return "";
        }
        
        string IEmbraceProvider.GetCurrentSessionId()
        {
            EmbraceLogger.Log("GetCurrentSessionId");
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

        void IEmbraceProvider.Crash()
        {
            EmbraceLogger.Log("Crash");
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

        void IEmbraceProvider.InstallUnityThreadSampler()
        {
            EmbraceLogger.Log("InstallUnityThreadSampler");
        }

        void IEmbraceProvider.logUnhandledUnityException(string exceptionMessage, string stack)
        {
            EmbraceLogger.Log($"Unhandled Exception: {exceptionMessage} : stack : {stack}");
        }

#if UNITY_IOS
        void IEmbraceProvider.RecordPushNotification(iOSPushNotificationArgs iosArgs)
        {
            EmbraceLogger.Log($"Push Notification: title: {iosArgs.title} subtitle: {iosArgs.subtitle} body: {iosArgs.body} category: {iosArgs.category} badge: {iosArgs.badge}");
        }
        #elif UNITY_ANDROID

        #if EMBRACE_ENABLE_BUGSHAKE_FORM
        void IEmbraceProvider.ShowBugReportForm()
        {
            EmbraceLogger.Log("Attempted to show the native Bug Report Form");
        }

        void IEmbraceProvider.setShakeListener(UnityShakeListener listener)
        {
            EmbraceLogger.Log("Embrace UnityShakeListener registration attempted");
        }
        
        public void saveShakeScreenshot(byte[] screenshot)
        {
            EmbraceLogger.Log($"Tried to take a screenshot as byte array {screenshot}");
        }
        #endif
        
        public AndroidJavaObject GetUnityInternalInterface()
        {
            EmbraceLogger.Log("GetUnityInternalInterface");
            return null;
        }
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
    }
}