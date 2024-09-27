using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Android specific configuration data set by the editor windows and exported to an embrace-config.json configuration file.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "AndroidConfiguration", menuName = "EmbraceSDK/AndroidConfiguration")]
#endif
    [Serializable]
    public class AndroidConfiguration : EmbraceConfiguration, ITooltipPropertiesProvider
    {
        [Tooltip(EmbraceTooltips.AppId)]
        [SerializeField]
        [FormerlySerializedAs("app_id")]
        [JsonProperty("app_id")]
        private string appId = string.Empty;

        public override string AppId
        {
            get => appId;
            set => appId = value;
        }

        [Tooltip(EmbraceTooltips.ApiToken)]
        [SerializeField]
        [FormerlySerializedAs("api_token")]
        [JsonProperty("api_token")]
        private string symbolUploadApiToken = string.Empty;

        public override string SymbolUploadApiToken
        {
            get => symbolUploadApiToken;
            set => symbolUploadApiToken = value;
        }

        public override EmbraceDeviceType DeviceType => EmbraceDeviceType.Android;

        #pragma warning disable 414
        [JsonProperty]
        [OverrideBoolean(false)] // doc-defined default value
        private bool ndk_enabled = true;
        #pragma warning restore 414

        public SdkConfig sdk_config = new SdkConfig();

        public override void SetDefaults()
        {
            appId = string.Empty;
            symbolUploadApiToken = string.Empty;
            ndk_enabled = true; // hidden unity-specific non-default value (always true requested by Android team).
            sdk_config.app.report_disk_usage = true;
            sdk_config.anr.capture_google = false;
            sdk_config.anr.capture_unity_thread = false;
            sdk_config.crash_handler.enabled = true;
            sdk_config.networking.capture_request_content_length = false;
            sdk_config.networking.enable_native_monitoring = true;
            sdk_config.networking.disabled_url_patterns.Clear();
            sdk_config.networking.track_id_header = string.Empty;
            sdk_config.session.async_end = false;
            sdk_config.session.max_session_seconds = 0;
            sdk_config.startup_moment.automatically_end = true;
            sdk_config.startup_moment.take_screenshot = false;
            sdk_config.taps.capture_coordinates = true;
            sdk_config.webview.capture_query_params = true;
            sdk_config.webview.enable = true;
#if DeveloperMode
            sdk_config.base_urls.config = string.Empty;
            sdk_config.base_urls.data = string.Empty;
            sdk_config.base_urls.data_dev = string.Empty;
            sdk_config.base_urls.images = string.Empty;
#endif
        }
    }
}