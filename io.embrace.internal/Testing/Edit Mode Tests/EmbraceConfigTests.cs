using System.Collections.Generic;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace EmbraceSDK.Tests
{
    public class EmbraceConfigTests
    {
        private static (bool, bool, bool, bool, bool)[] _embraceConfigParameters = new[]
        {
            (true, false, false, false, false),
            (false, true, false, false, false),
            (false, false, true, false, false),
            (false, false, false, true, false),
            (false, false, false, false, true),
        };

        [Test]
        public void BaseUrls_Copy_Constructor_Makes_Shallow_Copy()
        {
            BaseUrls a = new BaseUrls()
            {
                config = "config",
                data = "data",
                data_dev = "dataDev",
                images = "images"
            };

            BaseUrls b = new BaseUrls(a);

            Assert.AreEqual(a.config, b.config);
            Assert.AreEqual(a.data, b.data);
            Assert.AreEqual(a.data_dev, b.data_dev);
            Assert.AreEqual(a.images, b.images);
        }

        [Test]
        public void CreateSDKConfigs_Assigns_Fields_Correctly([ValueSource(nameof(_embraceConfigParameters))] (bool, bool, bool, bool, bool) parameters)
        {
            var configuration = ScriptableObject.CreateInstance<AndroidConfiguration>();

            configuration.sdk_config.networking.capture_request_content_length = parameters.Item1;
            configuration.sdk_config.networking.enable_native_monitoring = parameters.Item2;
            configuration.sdk_config.startup_moment.automatically_end = parameters.Item3;
            configuration.sdk_config.session.async_end = parameters.Item4;
            configuration.sdk_config.taps.capture_coordinates = parameters.Item5;

            Assert.IsNotNull(configuration.sdk_config.networking);
            Assert.IsNotNull(configuration.sdk_config.session);
            Assert.IsNotNull(configuration.sdk_config.startup_moment);
            Assert.IsNotNull(configuration.sdk_config.taps);

            Assert.AreEqual(parameters.Item1, configuration.sdk_config.networking.capture_request_content_length);
            Assert.AreEqual(parameters.Item2, configuration.sdk_config.networking.enable_native_monitoring);
            Assert.AreEqual(parameters.Item3, configuration.sdk_config.startup_moment.automatically_end);
            Assert.AreEqual(parameters.Item4, configuration.sdk_config.session.async_end);
            Assert.AreEqual(parameters.Item5, configuration.sdk_config.taps.capture_coordinates);

            Object.DestroyImmediate(configuration);
        }

        [Test]
        public void Scoped_Registry_Constructor_Creates_Valid_Instance()
        {
            const string NAME = "name";
            const string URL = "url";
            const string SCOPE = "scope";

            ScopedRegistry registry = new ScopedRegistry(NAME, URL, new[] { SCOPE });
            ScopedRegistries registries = new ScopedRegistries(registry);

            Assert.IsNotNull(registries.scopedRegistries);
            Assert.AreEqual(1, registries.scopedRegistries.Length);
            Assert.AreEqual(registry, registries.scopedRegistries[0]);

            Assert.IsNotNull(registries.scopedRegistries[0].scopes);
            Assert.AreEqual(1, registries.scopedRegistries[0].scopes.Length);

            Assert.AreEqual(NAME, registries.scopedRegistries[0].name);
            Assert.AreEqual(URL, registries.scopedRegistries[0].url);
            Assert.AreEqual(SCOPE, registries.scopedRegistries[0].scopes[0]);
        }

        [Test]
        public void AndroidConfiguration_Clear()
        {
            var configuration = ScriptableObject.CreateInstance<AndroidConfiguration>();

            configuration.AppId = "12345";
            configuration.SymbolUploadApiToken = "0123456789";
            configuration.sdk_config.app.report_disk_usage = true;
            configuration.sdk_config.crash_handler.enabled = true;
            configuration.sdk_config.networking.capture_request_content_length = true;
            configuration.sdk_config.networking.enable_native_monitoring = true;
            configuration.sdk_config.networking.disabled_url_patterns = new List<string> { "url1", "url2" };
            configuration.sdk_config.networking.track_id_header = "header";
            configuration.sdk_config.session.max_session_seconds = 42;
            configuration.sdk_config.session.async_end = true;
            configuration.sdk_config.startup_moment.automatically_end = true;
            configuration.sdk_config.startup_moment.take_screenshot = true;
            configuration.sdk_config.taps.capture_coordinates = true;
            configuration.sdk_config.webview.capture_query_params = true;
            configuration.sdk_config.webview.enable = true;
            configuration.SetDefaults();

            var defaultConfiguration = ScriptableObject.CreateInstance<AndroidConfiguration>();
            defaultConfiguration.SetDefaults();

            Assert.AreEqual(defaultConfiguration.AppId, configuration.AppId);
            Assert.AreEqual(defaultConfiguration.SymbolUploadApiToken, configuration.SymbolUploadApiToken);
            Assert.AreEqual(defaultConfiguration.sdk_config.app.report_disk_usage, configuration.sdk_config.app.report_disk_usage);
            Assert.AreEqual(defaultConfiguration.sdk_config.crash_handler.enabled, configuration.sdk_config.crash_handler.enabled);
            Assert.AreEqual(defaultConfiguration.sdk_config.networking.capture_request_content_length, configuration.sdk_config.networking.capture_request_content_length);
            Assert.AreEqual(defaultConfiguration.sdk_config.networking.enable_native_monitoring, configuration.sdk_config.networking.enable_native_monitoring);
            Assert.AreEqual(defaultConfiguration.sdk_config.networking.disabled_url_patterns.Count, configuration.sdk_config.networking.disabled_url_patterns.Count);
            Assert.AreEqual(defaultConfiguration.sdk_config.networking.track_id_header, configuration.sdk_config.networking.track_id_header);
            Assert.AreEqual(defaultConfiguration.sdk_config.session.max_session_seconds, configuration.sdk_config.session.max_session_seconds);
            Assert.AreEqual(defaultConfiguration.sdk_config.session.async_end, configuration.sdk_config.session.async_end);
            Assert.AreEqual(defaultConfiguration.sdk_config.startup_moment.automatically_end, configuration.sdk_config.startup_moment.automatically_end);
            Assert.AreEqual(defaultConfiguration.sdk_config.startup_moment.take_screenshot, configuration.sdk_config.startup_moment.take_screenshot);
            Assert.AreEqual(defaultConfiguration.sdk_config.taps.capture_coordinates, configuration.sdk_config.taps.capture_coordinates);
            Assert.AreEqual(defaultConfiguration.sdk_config.webview.capture_query_params, configuration.sdk_config.webview.capture_query_params);
            Assert.AreEqual(defaultConfiguration.sdk_config.webview.enable, configuration.sdk_config.webview.enable);

            Object.DestroyImmediate(configuration);
            Object.DestroyImmediate(defaultConfiguration);
        }

        [Test]
        public void IOSConfiguration_Clear()
        {
            var configuration = ScriptableObject.CreateInstance<IOSConfiguration>();

            configuration.AppId = "12345";
            configuration.SymbolUploadApiToken = "0123456789";
            configuration.CRASH_REPORT_ENABLED = true;
            configuration.NETWORK.CAPTURE_PUBLIC_KEY = "publicKey";
            configuration.NETWORK.DEFAULT_CAPTURE_LIMIT = 3;
            configuration.NETWORK.DOMAINS.keys.Add("key1");
            configuration.NETWORK.DOMAINS.values.Add(3);
            configuration.WEBVIEW_ENABLE = true;
            configuration.MAX_SESSION_SECONDS = 30;
            configuration.TRACE_ID_HEADER_NAME = "traceId";
            configuration.CAPTURE_COORDINATES = true;
            configuration.CRASH_REPORT_ENABLED = true;
            configuration.ENABLE_WK_AUTO_RELOAD = true;
            configuration.DISABLED_URL_PATTERNS = new List<string> { "url1", "url2" };
            configuration.CUSTOM_PATH_HEADER_INFO.keys.Add("key2");
            configuration.CUSTOM_PATH_HEADER_INFO.values.Add("value2");
            configuration.CAPTURE_TAPPED_ELEMENTS = true;
            configuration.STARTUP_AUTOEND_SECONDS = 10;
            configuration.WEBVIEW_STRIP_QUERYPARAMS = true;
            configuration.URLSESSION_CAPTURE_FILTERS = new List<string> { "filter1", "filter2" };
            configuration.ENABLE_AUTOMATIC_VIEW_CAPTURE = true;
            configuration.NSURLCONNECTION_PROXY_ENABLE = true;
            configuration.BACKGROUND_FETCH_CAPTURE_ENABLE = true;
            configuration.COLLECT_NETWORK_REQUEST_METRICS = true;
            configuration.STARTUP_MOMENT_SCREENSHOT_ENABLED = true;
            configuration.SetDefaults();

            var defaultConfiguration = ScriptableObject.CreateInstance<IOSConfiguration>();
            defaultConfiguration.SetDefaults();

            Assert.AreEqual(defaultConfiguration.AppId, configuration.AppId);
            Assert.AreEqual(defaultConfiguration.SymbolUploadApiToken, configuration.SymbolUploadApiToken);
            Assert.AreEqual(defaultConfiguration.CRASH_REPORT_ENABLED, configuration.CRASH_REPORT_ENABLED);
            Assert.AreEqual(defaultConfiguration.NETWORK.CAPTURE_PUBLIC_KEY, configuration.NETWORK.CAPTURE_PUBLIC_KEY);
            Assert.AreEqual(defaultConfiguration.NETWORK.DEFAULT_CAPTURE_LIMIT, configuration.NETWORK.DEFAULT_CAPTURE_LIMIT);
            Assert.AreEqual(defaultConfiguration.NETWORK.DOMAINS.keys.Count, configuration.NETWORK.DOMAINS.keys.Count);
            Assert.AreEqual(defaultConfiguration.NETWORK.DOMAINS.values.Count, configuration.NETWORK.DOMAINS.values.Count);
            Assert.AreEqual(defaultConfiguration.WEBVIEW_ENABLE, configuration.WEBVIEW_ENABLE);
            Assert.AreEqual(defaultConfiguration.MAX_SESSION_SECONDS, configuration.MAX_SESSION_SECONDS);
            Assert.AreEqual(defaultConfiguration.TRACE_ID_HEADER_NAME, configuration.TRACE_ID_HEADER_NAME);
            Assert.AreEqual(defaultConfiguration.CAPTURE_COORDINATES, configuration.CAPTURE_COORDINATES);
            Assert.AreEqual(defaultConfiguration.CRASH_REPORT_ENABLED, configuration.CRASH_REPORT_ENABLED);
            Assert.AreEqual(defaultConfiguration.ENABLE_WK_AUTO_RELOAD, configuration.ENABLE_WK_AUTO_RELOAD);
            Assert.AreEqual(defaultConfiguration.DISABLED_URL_PATTERNS.Count, configuration.DISABLED_URL_PATTERNS.Count);
            Assert.AreEqual(defaultConfiguration.CUSTOM_PATH_HEADER_INFO.keys.Count, configuration.CUSTOM_PATH_HEADER_INFO.keys.Count);
            Assert.AreEqual(defaultConfiguration.CUSTOM_PATH_HEADER_INFO.values.Count, configuration.CUSTOM_PATH_HEADER_INFO.values.Count);
            Assert.AreEqual(defaultConfiguration.CAPTURE_TAPPED_ELEMENTS, configuration.CAPTURE_TAPPED_ELEMENTS);
            Assert.AreEqual(defaultConfiguration.STARTUP_AUTOEND_SECONDS, configuration.STARTUP_AUTOEND_SECONDS);
            Assert.AreEqual(defaultConfiguration.WEBVIEW_STRIP_QUERYPARAMS, configuration.WEBVIEW_STRIP_QUERYPARAMS);
            Assert.AreEqual(defaultConfiguration.URLSESSION_CAPTURE_FILTERS.Count, configuration.URLSESSION_CAPTURE_FILTERS.Count);
            Assert.AreEqual(defaultConfiguration.ENABLE_AUTOMATIC_VIEW_CAPTURE, configuration.ENABLE_AUTOMATIC_VIEW_CAPTURE);
            Assert.AreEqual(defaultConfiguration.NSURLCONNECTION_PROXY_ENABLE, configuration.NSURLCONNECTION_PROXY_ENABLE);
            Assert.AreEqual(defaultConfiguration.BACKGROUND_FETCH_CAPTURE_ENABLE, configuration.BACKGROUND_FETCH_CAPTURE_ENABLE);
            Assert.AreEqual(defaultConfiguration.COLLECT_NETWORK_REQUEST_METRICS, configuration.COLLECT_NETWORK_REQUEST_METRICS);
            Assert.AreEqual(defaultConfiguration.STARTUP_MOMENT_SCREENSHOT_ENABLED, configuration.STARTUP_MOMENT_SCREENSHOT_ENABLED);

            Object.DestroyImmediate(configuration);
            Object.DestroyImmediate(defaultConfiguration);
        }

        private static readonly (bool legacyValue, string currentValue, string migratedValue)[] CrashProviderMigrationTestCases = {
            (true, "", "Embrace"),
            (true, null, "Embrace"),
            (false, "", "None"),
            (false, null, "None")
        };

        [Test]
        public void IOSConfiguration_OnValidate_ProperlyMigratesCrashProviderSetting(
            [ValueSource(nameof(CrashProviderMigrationTestCases))] (bool legacyValue, string currentValue, string migratedValue) testInputs)
        {
            var configuration = ScriptableObject.CreateInstance<IOSConfiguration>();

#pragma warning disable 0618
            configuration.CRASH_REPORT_ENABLED = testInputs.legacyValue;
#pragma warning restore 0618

            configuration.CRASH_REPORT_PROVIDER = testInputs.currentValue;
            configuration.OnValidate();

            Assert.AreEqual(testInputs.migratedValue, configuration.CRASH_REPORT_PROVIDER);

            Object.DestroyImmediate(configuration);
        }
    }
}