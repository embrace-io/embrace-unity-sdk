using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Container of additional classes that represent nested SDK configuration fields in the embrace-config.json file.
    /// </summary>
    [Serializable]
    public class SdkConfig : ITooltipPropertiesProvider, IJsonSerializable
    {
        public App app;
        public CrashHandler crash_handler;
        public Networking networking;
        public Session session;
        public StartupMoment startup_moment;
        public Taps taps;
        public Webview webview;
#if DeveloperMode
        [Tooltip(EmbraceTooltips.BaseUrls)]
        public BaseUrls base_urls;
#endif
        public SdkConfig()
        {
            app = new App();
            crash_handler = new CrashHandler();
            networking = new Networking();
            session = new Session();
            startup_moment = new StartupMoment();
            taps = new Taps();
            webview = new Webview();
#if DeveloperMode
            base_urls = new BaseUrls();
#endif
        }

        public bool ShouldSerialize()
        {
            // TODO: This is a workaround for a bug in the Embrace Android SDK
            // version 6.14.0. We would normally omit sdk_config if none of
            // the values have changed, but in 6.14.0 omitting sdk_config causes
            // the ndk_enabled property to also be ignored. For now, force the
            // sdk_config property to always serialize.
            //
            // This should be removed when the underlying bug is fixed in 7.0.
            return true;

            /*
            var shouldSerialize =
                app.ShouldSerialize() ||
                anr.ShouldSerialize() ||
                crash_handler.ShouldSerialize() ||
                networking.ShouldSerialize() ||
                session.ShouldSerialize() ||
                startup_moment.ShouldSerialize() ||
                webview.ShouldSerialize();

#if DeveloperMode
            shouldSerialize |= base_urls.ShouldSerialize();
#endif

            return shouldSerialize;
            */
        }
    }

    [Serializable]
    public class App : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.ReportDiskUsage)]
        [OverrideBoolean(true)]
        public bool report_disk_usage;

        public bool ShouldSerialize()
        {
            return ReflectionUtil.HasBooleanOverrides(GetType(), this);
        }
    }

    [Serializable]
    public class CrashHandler : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.CrashHandler)]
        [OverrideBoolean(true)]
        public bool enabled;

        public bool ShouldSerialize()
        {
            return ReflectionUtil.HasBooleanOverrides(GetType(), this);
        }
    }

    [Serializable]
    public class Networking : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.CaptureRequestContentLength)]
        [OverrideBoolean(false)]
        public bool capture_request_content_length;

        [Tooltip(EmbraceTooltips.DisabledUrlPatterns)]
        public List<string> disabled_url_patterns = new List<string>();

        [Tooltip(EmbraceTooltips.EnableNativeMonitoring)]
        [OverrideBoolean(true)]
        public bool enable_native_monitoring;

        [Tooltip(EmbraceTooltips.TrackIdHeader)]
        public string track_id_header;

        public bool ShouldSerialize()
        {
            return
                ReflectionUtil.HasBooleanOverrides(GetType(), this) ||
                disabled_url_patterns.Count > 0 ||
                !string.IsNullOrEmpty(track_id_header);
        }
    }

    [Serializable]
    public class Session : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.AsyncEnd)]
        [OverrideBoolean(false)]
        public bool async_end;

        [Tooltip(EmbraceTooltips.MaxSessionSeconds)]
        public int max_session_seconds;

        public bool ShouldSerialize()
        {
            return
                ReflectionUtil.HasBooleanOverrides(GetType(), this) ||
                max_session_seconds > 0;
        }
    }

    [Serializable]
    public class StartupMoment : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.AutomaticallyEnd)]
        [OverrideBoolean(true)]
        public bool automatically_end;

        [Tooltip(EmbraceTooltips.StartupMomentScreenshotEnabled)]
        [OverrideBoolean(false)]
        public bool take_screenshot;

        public bool ShouldSerialize()
        {
            return ReflectionUtil.HasBooleanOverrides(GetType(), this);
        }
    }

    [Serializable]
    public class Taps : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.CaptureCoordinates)]
        [OverrideBoolean(true)]
        public bool capture_coordinates;

        public bool ShouldSerialize()
        {
            return ReflectionUtil.HasBooleanOverrides(GetType(), this);
        }
    }

    [Serializable]
    public class Webview : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.CaptureQueryParams)]
        [OverrideBoolean(true)]
        public bool capture_query_params;

        [Tooltip(EmbraceTooltips.WebViewEnableAndroid)]
        [OverrideBoolean(true)]
        public bool enable;

        public bool ShouldSerialize()
        {
            return ReflectionUtil.HasBooleanOverrides(GetType(), this);
        }
    }

    [Serializable]
    public class BaseUrls : ITooltipPropertiesProvider, IJsonSerializable
    {
        public string config;
        public string data;
        [FormerlySerializedAs("dataDev")] public string data_dev;
        public string images;

        public BaseUrls(BaseUrls baseUrls)
        {
            config = baseUrls.config;
            data = baseUrls.data;
            data_dev = baseUrls.data_dev;
            images = baseUrls.images;
        }

        public BaseUrls()
        {
            config = "";
            data = "";
            data_dev = "";
            images = "";
        }

        public bool ShouldSerialize()
        {
            return
                !string.IsNullOrEmpty(config) ||
                !string.IsNullOrEmpty(data) ||
                !string.IsNullOrEmpty(data_dev) ||
                !string.IsNullOrEmpty(images);
        }
    }
}
