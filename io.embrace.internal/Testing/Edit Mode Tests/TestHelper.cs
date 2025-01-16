using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;

namespace EmbraceSDK.Tests
{
    /// <summary>
    /// This class provides configuration field values and convenience methods for testing purposes.
    /// </summary>
    public static class TestHelper
    {
        public const string AppId = "12345";
        public const string ApiToken = "test27e891ad45853949004eb7y5b9fr";

        private static string _defaultJsonBackup;

        /// <summary>
        /// <para>
        ///     Saves a single test configuration scriptable object in memory so it can later be restored using the ConfigRestore method.
        /// </para>
        /// <para>
        ///     NOTE: Assume you have two configs: ConfigA and ConfigB. After calling this method for ConfigA, ConfigRestore() must be called
        ///     in order to restore it before backing up ConfigB. Otherwise, the Json stored in memory for ConfigA will be overwritten.
        /// </para>
        /// </summary>
        /// <param name="deserializedConfig"></param>
        public static void ConfigBackup(EmbraceConfiguration deserializedConfig)
        {
            _defaultJsonBackup = JsonConvert.SerializeObject(deserializedConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
        }

        /// <summary>
        /// Copies the values of the source config into the destination config if they are the same Type.
        /// </summary>
        /// <param name="sourceConfig"></param>
        /// <param name="destinationConfig"></param>
        /// <exception cref="System.ArgumentException">Thrown if configuration types do not match.</exception>
        public static void CopyConfig(EmbraceConfiguration sourceConfig, EmbraceConfiguration destinationConfig)
        {
            if (sourceConfig.GetType() != destinationConfig.GetType())
            {
                throw new ArgumentException($"Unable to copy source Type {sourceConfig.GetType()} to destination Type {destinationConfig.GetType()}.");
            }

            var sourceJson = JsonConvert.SerializeObject(sourceConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            JsonConvert.PopulateObject(sourceJson, destinationConfig);
            EditorUtility.SetDirty(destinationConfig);
        }

        /// <summary>
        /// Restores a test configuration scriptable object that's been backed up to Json using the ConfigBackup method.
        /// </summary>
        /// <param name="deserializedConfig"></param>
        public static void ConfigRestore(EmbraceConfiguration deserializedConfig)
        {
            deserializedConfig.SetDefaults();
            JsonConvert.PopulateObject(_defaultJsonBackup, deserializedConfig);
            EditorUtility.SetDirty(deserializedConfig);
        }

        /// <summary>
        /// <para>
        ///     Creates an Environments scriptable object with the specified number of configurations which will be enumerated using the given name prefix.
        /// </para>
        /// <para>
        ///     For Example: If count = 3, and namePrefix = "testEnv", the environments created will be "testEnv0", "testEnv1" and "testEnv2", along with
        ///     their corresponding Android and iOS configurations.
        /// </para>
        /// </summary>
        /// <param name="count"></param>
        /// <param name="namePrefix"></param>
        /// <returns></returns>
        public static Environments CreateTestEnvironments(int count, string namePrefix)
        {
            var environments = AssetDatabaseUtil.CreateEnvironments();
            var envGuids = new string[count];
            var envNames = new string[count];

            for (int i = 0; i < count; i++)
            {
                envNames[i] = $"{namePrefix}{i}";
                envGuids[i] = Guid.NewGuid().ToString();

                var envConfig = new EnvironmentConfiguration(envGuids[i], envNames[i]);

                for (int j = 0; j < 2; j++)
                {
                    EmbraceConfiguration sdkConfig;

                    if (j == 0)
                    {
                        sdkConfig = AssetDatabaseUtil.CreateConfiguration<AndroidConfiguration>(envGuids[i], envNames[i]);
                    }
                    else
                    {
                        sdkConfig = AssetDatabaseUtil.CreateConfiguration<IOSConfiguration>(envGuids[i], envNames[i]);
                    }

                    sdkConfig.SetDefaults();
                    sdkConfig.AppId = AppId;
                    sdkConfig.SymbolUploadApiToken = ApiToken;

                    EditorUtility.SetDirty(sdkConfig);

                    envConfig.sdkConfigurations.Add(sdkConfig);
                }

                environments.environmentConfigurations.Add(envConfig);
            }

            environments.activeEnvironmentIndex = 0;

            EditorUtility.SetDirty(environments);

            AssetDatabase.Refresh();

            return environments;
        }

        /// <summary>
        /// Iterates through element stored in the provided environments and asserts equality on all fields of each sdk configuration found.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="compare"></param>
        public static void AssertEnvironmentsAreEqual(Environments source, Environments compare)
        {
            var sourceEnvConfigs = source.environmentConfigurations;
            var compareEnvConfigs = compare.environmentConfigurations;

            Assert.AreEqual(sourceEnvConfigs.Count, compareEnvConfigs.Count);

            for (int i = 0; i < sourceEnvConfigs.Count; i++)
            {
                var sourceEnvConfig = sourceEnvConfigs[i];
                var compareEnvConfig = compareEnvConfigs[i];

                Assert.AreEqual(sourceEnvConfig.guid, compareEnvConfig.guid);
                Assert.AreEqual(sourceEnvConfig.name, compareEnvConfig.name);

                var sourceSdkConfigs = sourceEnvConfig.sdkConfigurations;
                var compareSdkConfigs = compareEnvConfig.sdkConfigurations;

                Assert.AreEqual(sourceSdkConfigs.Count, 2);
                Assert.AreEqual(compareSdkConfigs.Count, 2);

                var sourceAndroidConfig = (AndroidConfiguration)sourceSdkConfigs[0];
                var compareAndroidConfig = (AndroidConfiguration)compareSdkConfigs[0];
                AssertAndroidConfigsAreEqual(sourceAndroidConfig, compareAndroidConfig);

                var sourceIOSConfig = (IOSConfiguration)sourceSdkConfigs[1];
                var compareIOSConfig = (IOSConfiguration)compareSdkConfigs[1];
                AssertIOSConfigsAreEqual(sourceIOSConfig, compareIOSConfig);
            }
        }

        /// <summary>
        /// Compares two AndroidConfiguration scriptable objects and evaluates all fields.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static void AssertAndroidConfigsAreEqual(AndroidConfiguration source, AndroidConfiguration compare)
        {
            Assert.AreEqual(source.AppId, compare.AppId);
            Assert.AreEqual(source.SymbolUploadApiToken, compare.SymbolUploadApiToken);
            Assert.AreEqual(source.sdk_config.app.report_disk_usage, compare.sdk_config.app.report_disk_usage);
            Assert.AreEqual(source.sdk_config.crash_handler.enabled, compare.sdk_config.crash_handler.enabled);
            Assert.AreEqual(source.sdk_config.networking.capture_request_content_length, compare.sdk_config.networking.capture_request_content_length);
            Assert.AreEqual(source.sdk_config.networking.enable_native_monitoring, compare.sdk_config.networking.enable_native_monitoring);
            Assert.AreEqual(source.sdk_config.networking.track_id_header, compare.sdk_config.networking.track_id_header);
            CompareLists(source.sdk_config.networking.disabled_url_patterns, compare.sdk_config.networking.disabled_url_patterns);
            Assert.AreEqual(source.sdk_config.session.async_end, compare.sdk_config.session.async_end);
            Assert.AreEqual(source.sdk_config.session.max_session_seconds, compare.sdk_config.session.max_session_seconds);
            Assert.AreEqual(source.sdk_config.startup_moment.automatically_end, compare.sdk_config.startup_moment.automatically_end);
            Assert.AreEqual(source.sdk_config.startup_moment.take_screenshot, compare.sdk_config.startup_moment.take_screenshot);
            Assert.AreEqual(source.sdk_config.taps.capture_coordinates, compare.sdk_config.taps.capture_coordinates);
            Assert.AreEqual(source.sdk_config.webview.capture_query_params, compare.sdk_config.webview.capture_query_params);
            Assert.AreEqual(source.sdk_config.webview.enable, compare.sdk_config.webview.enable);
        }

        private static void CompareLists<T>(List<T> source, List<T> compare)
        {
            Assert.AreEqual(source.Count, compare.Count);

            for (int i = 0; i < source.Count; i++)
            {
                Assert.AreEqual(source[i], compare[i]);
            }
        }

        /// <summary>
        /// Compares two IOSConfiguration scriptable objects and evaluates all fields.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static void AssertIOSConfigsAreEqual(IOSConfiguration source, IOSConfiguration compare)
        {
            Assert.AreEqual(source.AppId, compare.AppId);
            Assert.AreEqual(source.SymbolUploadApiToken, compare.SymbolUploadApiToken);
            Assert.AreEqual(source.CRASH_REPORT_ENABLED, compare.CRASH_REPORT_ENABLED);
            Assert.AreEqual(source.NETWORK.CAPTURE_PUBLIC_KEY, compare.NETWORK.CAPTURE_PUBLIC_KEY);
            Assert.AreEqual(source.NETWORK.DEFAULT_CAPTURE_LIMIT, compare.NETWORK.DEFAULT_CAPTURE_LIMIT);
            CompareEmbracePlistDictionary(source.NETWORK.DOMAINS, compare.NETWORK.DOMAINS);
            Assert.AreEqual(source.WEBVIEW_ENABLE, compare.WEBVIEW_ENABLE);
            Assert.AreEqual(source.MAX_SESSION_SECONDS, compare.MAX_SESSION_SECONDS);
            Assert.AreEqual(source.TRACE_ID_HEADER_NAME, compare.TRACE_ID_HEADER_NAME);
            Assert.AreEqual(source.CAPTURE_COORDINATES, compare.CAPTURE_COORDINATES);
            Assert.AreEqual(source.CRASH_REPORT_ENABLED, compare.CRASH_REPORT_ENABLED);
            Assert.AreEqual(source.ENABLE_WK_AUTO_RELOAD, compare.ENABLE_WK_AUTO_RELOAD);
            Assert.AreEqual(source.DISABLED_URL_PATTERNS.Count, compare.DISABLED_URL_PATTERNS.Count);
            CompareEmbracePlistDictionary(source.CUSTOM_PATH_HEADER_INFO, compare.CUSTOM_PATH_HEADER_INFO);
            Assert.AreEqual(source.CAPTURE_TAPPED_ELEMENTS, compare.CAPTURE_TAPPED_ELEMENTS);
            Assert.AreEqual(source.STARTUP_AUTOEND_SECONDS, compare.STARTUP_AUTOEND_SECONDS);
            Assert.AreEqual(source.WEBVIEW_STRIP_QUERYPARAMS, compare.WEBVIEW_STRIP_QUERYPARAMS);
            Assert.AreEqual(source.ENABLE_AUTOMATIC_VIEW_CAPTURE, compare.ENABLE_AUTOMATIC_VIEW_CAPTURE);
            Assert.AreEqual(source.NSURLCONNECTION_PROXY_ENABLE, compare.NSURLCONNECTION_PROXY_ENABLE);
            Assert.AreEqual(source.BACKGROUND_FETCH_CAPTURE_ENABLE, compare.BACKGROUND_FETCH_CAPTURE_ENABLE);
            Assert.AreEqual(source.COLLECT_NETWORK_REQUEST_METRICS, compare.COLLECT_NETWORK_REQUEST_METRICS);
            Assert.AreEqual(source.STARTUP_MOMENT_SCREENSHOT_ENABLED, compare.STARTUP_MOMENT_SCREENSHOT_ENABLED);
        }

        private static void CompareEmbracePlistDictionary<TKey, TValue>(EmbracePlistDictionary<TKey, TValue> source, EmbracePlistDictionary<TKey, TValue> compare)
        {
            Assert.AreEqual(source.keys.Count, compare.keys.Count);
            Assert.AreEqual(source.values.Count, compare.values.Count);

            for (int i = 0; i < source.keys.Count; i++)
            {
                Assert.AreEqual(source.keys[i], compare.keys[i]);
                Assert.AreEqual(source.values[i], compare.values[i]);
            }
        }

        public static void DeleteTestAssets(params string[] assetPaths)
        {
#if UNITY_2020_1_OR_NEWER
            AssetDatabase.DeleteAssets(assetPaths, new List<string>());
#else
            for (int i = 0; i < assetPaths.Length; i++)
            {
                AssetDatabase.DeleteAsset(assetPaths[i]);
            }
#endif
        }
    }
}