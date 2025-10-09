using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// iOS specific configuration data set by the editor windows and exported to an Embrace-Info.plist configuration file.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "IOSConfiguration", menuName = "EmbraceSDK/IOSConfiguration")]
#endif
    [Serializable]
    public class IOSConfiguration : EmbraceConfiguration, ITooltipPropertiesProvider
    {
        internal const string EMBRACE_CRASH_REPORTER_VALUE = "Embrace";
        internal const string CRASHLYTICS_CRASH_REPORTER_VALUE = "Crashlytics";
        internal const string NONE_CRASH_REPORTER_VALUE = "None";

        [Tooltip(EmbraceTooltips.AppId)]
        [SerializeField]
        [FormerlySerializedAs("API_KEY")]
        [JsonProperty("API_KEY")]
        private string appId = string.Empty;

        public override string AppId
        {
            get => appId;
            set => appId = value;
        }

        [Tooltip(EmbraceTooltips.ApiToken)]
        [SerializeField]
        [FormerlySerializedAs("API_TOKEN")]
        [JsonProperty("API_TOKEN")]
        private string symbolUploadApiToken = string.Empty;

        public override string SymbolUploadApiToken
        {
            get => symbolUploadApiToken;
            set => symbolUploadApiToken = value;
        }

        public override EmbraceDeviceType DeviceType => EmbraceDeviceType.IOS;

        [Tooltip(EmbraceTooltips.CrashReportEnabled)]
        [OverrideBoolean(false)]
        [HideInInspector, JsonIgnore]
        [Obsolete("CRASH_REPORT_ENABLED is deprecated and will be removed in a future release. Please use CRASH_REPORT_PROVIDER instead.")]
        public bool CRASH_REPORT_ENABLED;

        [Tooltip(EmbraceTooltips.CrashReportProvider)]
        [StringOptionField(EMBRACE_CRASH_REPORTER_VALUE, CRASHLYTICS_CRASH_REPORTER_VALUE, NONE_CRASH_REPORTER_VALUE)]
        public string CRASH_REPORT_PROVIDER;

        [Tooltip(EmbraceTooltips.StartupMomentScreenshotEnabled)]
        [OverrideBoolean(false)]
        public bool STARTUP_MOMENT_SCREENSHOT_ENABLED;

        [Tooltip(EmbraceTooltips.CaptureCoordinates)]
        [OverrideBoolean(true)]
        public bool CAPTURE_COORDINATES;

        [Tooltip(EmbraceTooltips.CaptureTappedElements)]
        [OverrideBoolean(true)]
        public bool CAPTURE_TAPPED_ELEMENTS;

        [Tooltip(EmbraceTooltips.BackgroundFetchCaptureEnable)]
        [OverrideBoolean(false)]
        public bool BACKGROUND_FETCH_CAPTURE_ENABLE;

        [Tooltip(EmbraceTooltips.CollectNetworkRequestMetrics)]
        [OverrideBoolean(true)]
        public bool COLLECT_NETWORK_REQUEST_METRICS;

        [Tooltip(EmbraceTooltips.EnableAutomaticViewCapture)]
        [OverrideBoolean(true)]
        public bool ENABLE_AUTOMATIC_VIEW_CAPTURE;

        [Tooltip(EmbraceTooltips.EnableWkAutoReload)]
        [OverrideBoolean(false)]
        public bool ENABLE_WK_AUTO_RELOAD;

        [Tooltip(EmbraceTooltips.UrlSessionCaptureFilters)]
        public List<string> URLSESSION_CAPTURE_FILTERS = new List<string>();

        [Tooltip(EmbraceTooltips.StartupAutoEndSeconds)]
        public int STARTUP_AUTOEND_SECONDS;

        [Tooltip(EmbraceTooltips.WebViewStripQueryParams)]
        [OverrideBoolean(false)]
        public bool WEBVIEW_STRIP_QUERYPARAMS;

        [Tooltip(EmbraceTooltips.WebViewEnableIOS)]
        [OverrideBoolean(true)]
        public bool WEBVIEW_ENABLE;

        [Tooltip(EmbraceTooltips.Network)]
        public PlistNetworkElement NETWORK = new PlistNetworkElement();

        [Tooltip(EmbraceTooltips.NsurlConnectionProxyEnable)]
        [OverrideBoolean(true)]
        public bool NSURLCONNECTION_PROXY_ENABLE;

        [Tooltip(EmbraceTooltips.MaxSessionSeconds)]
        public int MAX_SESSION_SECONDS;

        [Tooltip(EmbraceTooltips.TraceIdHeaderName)]
        public string TRACE_ID_HEADER_NAME;

        [Tooltip(EmbraceTooltips.CustomPathHeaderInfo)]
        [JsonConverter(typeof(PlistDictionaryConverter<string, string>))]
        public PlistStringDictionary CUSTOM_PATH_HEADER_INFO = new PlistStringDictionary();

        public override void SetDefaults()
        {
            appId = string.Empty;
            symbolUploadApiToken = string.Empty;
#pragma warning disable 0618
            CRASH_REPORT_ENABLED = true;
#pragma warning restore 0618
            CRASH_REPORT_PROVIDER = EMBRACE_CRASH_REPORTER_VALUE;
            STARTUP_MOMENT_SCREENSHOT_ENABLED = false;
            CAPTURE_COORDINATES = true;
            CAPTURE_TAPPED_ELEMENTS = true;
            BACKGROUND_FETCH_CAPTURE_ENABLE = false;
            COLLECT_NETWORK_REQUEST_METRICS = true;
            ENABLE_AUTOMATIC_VIEW_CAPTURE = true;
            ENABLE_WK_AUTO_RELOAD = false;
            URLSESSION_CAPTURE_FILTERS.Clear();
            STARTUP_AUTOEND_SECONDS = 0;
            WEBVIEW_STRIP_QUERYPARAMS = false;
            WEBVIEW_ENABLE = true;
            NETWORK.Clear();
            NSURLCONNECTION_PROXY_ENABLE = true;
            MAX_SESSION_SECONDS = 0;
            TRACE_ID_HEADER_NAME = string.Empty;
            CUSTOM_PATH_HEADER_INFO.Clear();
        }

        internal void OnValidate()
        {
            if (string.IsNullOrEmpty(CRASH_REPORT_PROVIDER))
            {
#pragma warning disable 0618
                if (CRASH_REPORT_ENABLED)
#pragma warning restore 0618
                {
                    CRASH_REPORT_PROVIDER = EMBRACE_CRASH_REPORTER_VALUE;
                }
                else if(UnityEditor.Compilation.CompilationPipeline.GetPrecompiledAssemblyNames().Any(name => name.Contains("Firebase.Crashlytics")))
                {
                    CRASH_REPORT_PROVIDER = CRASHLYTICS_CRASH_REPORTER_VALUE;
                }
                else
                {
                    CRASH_REPORT_PROVIDER = NONE_CRASH_REPORTER_VALUE;
                }
            }
        }
    }
}