using UnityEngine;
using System;

namespace EmbraceSDK.EditorView
{
    [Obsolete("Not compatible with version 1.6.0")]
    [Serializable]
    public class EmbraceSDKConfiguration : ScriptableObject
    {
        [Header("Embrace App ID")]
        public string APP_ID;

        [Header("Embrace API Token")]
        public string API_TOKEN;

        [Header("Enable NDK")]
        public bool NDK_ENABLED = true;

        [Header("Enable Async Upload")]
        public bool ASYNC_UPLOAD = false;

        [Header("Enable Native Monitoring")]
        public bool enable_native_monitoring;

        [Header("Capture Request Content Length")]
        public bool capture_request_content_length = false;

        [Header("Max Session Seconds")]
        public int max_session_seconds = 60;

        [Header("Enable Embrace Crash Reporting")]
        public bool USE_EMBRACE_CRASH_REPORTING = true;

#if DeveloperMode
        [Header("base_urls")]
        public BaseUrls base_urls;
#endif

        public enum DeviceType
        {
            Android,
            IOS
        }
        [Header("Target Device")]
        public DeviceType deviceType;

        public EnvironmentConfig environment;

        public void Clear()
        {
            APP_ID = "";
            API_TOKEN = "";
            NDK_ENABLED = true;
            ASYNC_UPLOAD = false;
            USE_EMBRACE_CRASH_REPORTING = true;
            enable_native_monitoring = true;
            capture_request_content_length = false;
            max_session_seconds = 60;

#if DeveloperMode
            base_urls = new BaseUrls();
#endif
        }
    }
}