using System;
using System.Collections.Generic;
using UnityEngine;

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
        void StartSDK(EmbraceStartupArgs args = null);
        LastRunEndState GetLastRunEndState();
        void SetUserIdentifier(string identifier);
        void ClearUserIdentifier();
        void SetUsername(string username);
        void ClearUsername();
        void SetUserEmail(string email);
        void ClearUserEmail();
        void SetUserAsPayer();
        void ClearUserAsPayer();
        void AddUserPersona(string persona);
        void ClearUserPersona(string persona);
        void ClearAllUserPersonas();
        bool AddSessionProperty(string key, string value, bool permanent);
        void RemoveSessionProperty(string key);
        Dictionary<string, string> GetSessionProperties();
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties);
        #if UNITY_ANDROID
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            sbyte[] attachment);
        #elif UNITY_IOS
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties = null, byte[] attachment = null);
        #endif
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties,
            string attachmentId, string attachmentUrl);
        void AddBreadcrumb(string message);
        void EndSession(bool clearUserInfo);
        string GetDeviceId();
        bool StartView(string name);
        bool EndView(string name);
        void SetMetaData(string unityVersion, string guid, string sdkVersion);
        void RecordCompletedNetworkRequest(string url, HTTPMethod method, long startms, long endms, long bytesin, long bytesout, int code);
        void RecordIncompleteNetworkRequest(string url, HTTPMethod method, long startms, long endms, string error);
        void LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack);
        void LogHandledUnityException(string exceptionName, string exceptionMessage, string stack);
        string GetCurrentSessionId();
        string StartSpan(string spanName, string parentSpanId, long startTimeMs);
        bool StopSpan(string spanId, int errorCode, long endTimeMs);
        bool AddSpanEvent(string spanId, string spanName, long timestampMs, Dictionary<string, string> attributes);
        bool AddSpanAttribute(string spanId, string key, string value);
        bool RecordCompletedSpan(string spanName, long startTimeMs, long endTimeMs, int? errorCode, 
            string parentSpanId, Dictionary<string, string> attributes, EmbraceSpanEvent[] events);
        #if UNITY_IOS
        void RecordPushNotification(iOSPushNotificationArgs iosArgs);
        #elif UNITY_ANDROID
        void RecordPushNotification(AndroidPushNotificationArgs androidArgs);
        #endif
    }
}
