using System;
using System.Collections.Generic;
using EmbraceSDK.Bugshake;
using EmbraceSDK.Internal;
using UnityEngine;

#if EMBRACE_ENABLE_BUGSHAKE_FORM
using EmbraceSDK.Bugshake;
#endif

namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Defines the methods that are available from our iOS and Android SDK.
    /// </summary>
    public interface IEmbraceProvider
    {
        /// <summary>
        /// Called automatically on awake. Does not start any monitoring or network calls.
        /// </summary>
        void InitializeSDK();
        // Public API
        void StartSDK(bool enableIntegrationTesting);
        void EndAppStartup(Dictionary<string, string> properties);
        LastRunEndState GetLastRunEndState();
        void SetUserIdentifier(string identifier);
        void ClearUserIdentifier();
        void SetUsername(string username);
        void ClearUsername();
        void SetUserEmail(string email);
        void ClearUserEmail();
        void SetUserAsPayer();
        void ClearUserAsPayer();
        void SetUserPersona(string persona);
        void AddUserPersona(string persona);
        void ClearUserPersona(string persona);
        void ClearAllUserPersonas();
        bool AddSessionProperty(string key, string value, bool permanent);
        void RemoveSessionProperty(string key);
        Dictionary<string, string> GetSessionProperties();
        void StartMoment(string name, string identifier, bool allowScreenshot, Dictionary<string, string> properties);
        void EndMoment(string name, string identifier, Dictionary<string, string> properties);
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties);
        void LogBreadcrumb(string message);
        void AddBreadcrumb(string message);
        void EndSession(bool clearUserInfo);
        string GetDeviceId();
        bool StartView(string name);
        bool EndView(string name);
        void Crash();
        void SetMetaData(string unityVersion, string guid, string sdkVersion);
        void RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code);
        void RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error);
        void LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack);
        void LogHandledUnityException(string exceptionName, string exceptionMessage, string stack);
        void InstallUnityThreadSampler();
        string GetCurrentSessionId();
        string StartSpan(string spanName, string parentSpanId, long startTimeMs);
        bool StopSpan(string spanId, int errorCode, long endTimeMs);
        bool AddSpanEvent(string spanId, string spanName, long timestampMs, Dictionary<string, string> attributes);
        bool AddSpanAttribute(string spanId, string key, string value);
        bool RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int errorCode, 
            string parentSpanId, Dictionary<string, string> attributes, EmbraceSpanEvent events);
        #if UNITY_IOS
        void RecordPushNotification(iOSPushNotificationArgs iosArgs);
        #elif UNITY_ANDROID
        void RecordPushNotification(AndroidPushNotificationArgs androidArgs);
        #if EMBRACE_ENABLE_BUGSHAKE_FORM
        void ShowBugReportForm();
        void saveShakeScreenshot(byte[] screenshot);
        void setShakeListener(UnityShakeListener listener);
        #endif
        #endif

        [Obsolete("InitNativeSdkConnection is deprecated and will be removed from a future release.", false)]
        void InitNativeSdkConnection();

        [Obsolete("logUnhandledUnityException is deprecated and will be removed from a future release, please use LogUnhandledUnityException instead.", false)]
        void logUnhandledUnityException(string exceptionMessage, string stack);
        
        [Obsolete("LogMessage with screenshot argument is deprecated and will be removed from a future release.", false)]
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties, bool allowScreenshot);
    }
}
