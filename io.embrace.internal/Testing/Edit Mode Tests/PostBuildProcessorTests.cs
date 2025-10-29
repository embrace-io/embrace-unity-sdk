using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TestTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;
using System;
using System.Text.RegularExpressions;
#if UNITY_IOS || UNITY_TVOS
using UnityEditor.iOS.Xcode;
#endif

namespace EmbraceSDK.Tests
{
    /// <summary>
    /// Unit tests for the EmbracePostBuildProcessor.cs
    /// </summary>
    public class PostBuildProcessorTests
    {
#if UNITY_ANDROID
        [OneTimeSetUp]
        public void EnsureAndroidActive()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                // Make sure support exists (fast fail if the runner is misconfigured)
                Assume.That(BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android),
                    "Android Build Support not installed for this editor process");

                bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                Assume.That(ok, "Failed to switch active build target to Android");
            }
        }
        
        [Test]
        // <summary>
        // Test if the regex catches the data we need.
        // </summary>
        public void TestEmbraceCustomSymbolsRegex() {
            var testWithNewlines = @"embrace {
                customSymbolsDirectory.set(""/test/path"")
            }";

            var match = Regex.Match(testWithNewlines, EmbracePostBuildProcessor.EMBRACE_CUSTOM_SYMBOLS_PATTERN);

            Assert.IsTrue(match.Success);
            Assert.AreEqual(match.Groups["path"].Value, "/test/path");            
        }
        /// <summary>
        /// Test if config fields which override default values are included in the output json.
        /// Override values are based on our public Android SDK documentation.
        /// </summary>
        [Test]
        public void TestOverrideAndroidConfigFields()
        {
            // NOTE: The android config json file is structured such that there are three root level objects: "app_id", "api_token", and "sdk_config".
            // The majority of overrides are contained within nested objects of the sdk_config object.  In order to include these nested objects
            // their fields must contain an override value.

            // Instantiate an Android Config and set default values.
            var androidConfig = ScriptableObject.CreateInstance<AndroidConfiguration>();
            androidConfig.SetDefaults();

            // Set the 2 required root fields which are always included.
            androidConfig.AppId = TestHelper.AppId;
            androidConfig.SymbolUploadApiToken = TestHelper.ApiToken;

            // Determine how many root fields are included by default.
            // Should be 3:
            //   1. root property "app_id"
            //   2. root property "api_token"
            //   3. root property "ndk_enabled"
            var defaultConfigJson = JsonConvert.SerializeObject(androidConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            var defaultConfigJObject = JObject.Parse(defaultConfigJson);
            var defaultRootProperties = defaultConfigJObject.Properties().Select(p => p.Name).ToList();
            defaultRootProperties.Sort();

            // TODO: This should be 3 instead of 4 once we are not forcing
            // sdk_config to be included. See SdkConfig#ShouldSerialize comment.
            Assert.AreEqual(
                new List<string>() { "api_token", "app_id", "ndk_enabled", "sdk_config" },
                defaultRootProperties
            );

            // Override 3 nested properties in the "sdk_config" object of type boolean, int, string, and List<T> respectively.
            androidConfig.sdk_config.app.report_disk_usage = false;
            androidConfig.sdk_config.session.max_session_seconds = 10;
            androidConfig.sdk_config.networking.track_id_header = "test-header";
            androidConfig.sdk_config.networking.disabled_url_patterns = new List<string>() { "test.com" };

            var overrideConfigJson = JsonConvert.SerializeObject(androidConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            var overrideConfigJObject = JObject.Parse(overrideConfigJson);
            var overrideNestedProperties = overrideConfigJObject["sdk_config"].Children().Select(t => t.Path).ToList();
            overrideNestedProperties.Sort();

            Assert.AreEqual(
                new List<string>() { "sdk_config.app", "sdk_config.networking", "sdk_config.session" },
                overrideNestedProperties
            );

            // Clear all nested overrides. Root nodes should be 3 ("app_id", "api_token", and "ndk_enabled").
            androidConfig.sdk_config.app.report_disk_usage = true;
            androidConfig.sdk_config.session.max_session_seconds = 0;
            androidConfig.sdk_config.networking.track_id_header = string.Empty;
            androidConfig.sdk_config.networking.disabled_url_patterns.Clear();

            overrideConfigJson = JsonConvert.SerializeObject(androidConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            overrideConfigJObject = JObject.Parse(overrideConfigJson);
            var overrideRootProperties = overrideConfigJObject.Properties().Select(p => p.Name).ToList();
            overrideRootProperties.Sort();

            // TODO: This should be 3 instead of 4 once we are not forcing
            // sdk_config to be included. See SdkConfig#ShouldSerialize comment.
            Assert.AreEqual(
                new List<string>() { "api_token", "app_id", "ndk_enabled", "sdk_config" },
                overrideRootProperties
            );

            //cleanup
            Object.DestroyImmediate(androidConfig);
        }

        /// <summary>
        /// Test if OnPostGenerateGradleAndroidProject() builds the embrace-config.json correctly.
        /// </summary>
        [Test, Order(2)]
        public void GenerateGradleTest()
        {
            var androidConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);
            var testConfig = Resources.Load<AndroidConfiguration>("TestConfigurations/TestAndroidConfiguration");

            Assert.NotNull(androidConfig);

            TestHelper.ConfigBackup(androidConfig);
            TestHelper.CopyConfig(testConfig, androidConfig);

            string embraceConfigString =
                JsonConvert.SerializeObject(
                    androidConfig,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new SDKConfigContractResolver()
                    }
                );

            DirectoryInfo tempDirectory = Directory.CreateDirectory(AssetDatabaseUtil.ProjectDirectory + "/TempBuild");
            DirectoryInfo tempMainDirectory = Directory.CreateDirectory(tempDirectory.FullName + "/launcher/src/main");
            File.WriteAllText(tempMainDirectory.FullName + "/embrace-config.json", "");

            EmbracePostBuildProcessor.WriteEmbraceConfig(tempDirectory.FullName);

            string builtEmbraceConfigString = File.ReadAllText(tempDirectory.FullName + "/launcher/src/main/embrace-config.json");
            Assert.AreEqual(embraceConfigString, builtEmbraceConfigString);

            TestGradleSymbolsPath();

            //cleanup
            TestHelper.ConfigRestore(androidConfig);
            Directory.Delete(tempDirectory.FullName, true);
        }

        public void TestGradleSymbolsPath() {
            // We need to check if the expected folder(s) show up in the expected path.
            var expectedSubPath = "Library/Bee/Android/Prj/IL2CPP/Gradle/unityLibrary/symbols";
            var targetFolder = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, expectedSubPath);
            
            Assert.IsTrue(Directory.Exists(targetFolder));
            Assert.IsTrue(
                Directory.EnumerateDirectories(targetFolder, EmbracePostBuildProcessor.ARCH_DIR, SearchOption.TopDirectoryOnly).Any()
            );
        }

        /// <summary>
        /// Test Android build.
        /// </summary>
        [Test, Order(1), Timeout(600_000)]
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void BuildAndroidTest()
        {
            var defaultConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            Assert.NotNull(defaultConfig);

            // NOTE: The test config contains override values for all fields which are used to output a json file with all available config settings.
            var testConfig = Resources.Load<AndroidConfiguration>("TestConfigurations/TestAndroidConfiguration");

            // NOTE: With the swazzler update (7.3.0) the app_id and api_token must actually be valid now
            // If this test fails, make sure you are running unity with the start_unity.sh script in order to setup the env variables
            testConfig.AppId = Environment.GetEnvironmentVariable("EMBRACE_TEST_APP_ID");
            testConfig.SymbolUploadApiToken = Environment.GetEnvironmentVariable("EMBRACE_TEST_API_TOKEN");
          
            TestHelper.ConfigBackup(defaultConfig);
            TestHelper.CopyConfig(testConfig, defaultConfig);
            
            Assert.IsNotNull(testConfig.AppId);
            Assert.IsNotNull(testConfig.SymbolUploadApiToken);

            var userValue = EditorUserBuildSettings.androidCreateSymbols;
            EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
            LogAssert.Expect(LogType.Assert, new Regex(@"Trying to add file .*boot\.config.*does not appear to exist on disk right now"));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
            buildPlayerOptions.locationPathName = AssetDatabaseUtil.ProjectDirectory + "/Builds/Test Builds/AndroidBuild";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Android;
            buildPlayerOptions.options = BuildOptions.None;
            BuildResult summary = BuildAndroid(buildPlayerOptions);
            Assert.IsTrue(summary == BuildResult.Succeeded);

            // In older versions of Unity the reference to the default config will not survive the build, so we'll
            // reload it here before restoring its values.
            defaultConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);
            
            TestHelper.ConfigRestore(defaultConfig);
            EditorUserBuildSettings.androidCreateSymbols = userValue;
        }

        private BuildResult BuildAndroid(BuildPlayerOptions buildPlayerOptions)
        {
            Debug.Log($"[Diag] Editor path: {UnityEditor.EditorApplication.applicationPath}");
            Debug.Log($"[Diag] Android supported: {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)}");
            Debug.Log($"[Diag] Android engine dir: {BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None)}");
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }

            return summary.result;
        }
#endif

#if UNITY_IOS || UNITY_TVOS
        private bool _hasBuiltIOS;

        /// <summary>
        /// Test iOS build.
        /// </summary>
        [Test, Order(1)]
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void BuildiOSTest()
        {
            var defaultConfig = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(ensureNotNull: false);

            Assert.NotNull(defaultConfig);

            // NOTE: The test config contains override values for all fields which is used to output a plist with all available config settings.
            var testConfig = Resources.Load<IOSConfiguration>("TestConfigurations/TestIOSConfiguration");

            TestHelper.ConfigBackup(defaultConfig);
            TestHelper.CopyConfig(testConfig, defaultConfig);

            BuildResult summary = BuildiOS();
            if (summary == BuildResult.Succeeded)
            {
                _hasBuiltIOS = true;
            }

            Assert.IsTrue(summary == BuildResult.Succeeded);

            // In older versions of Unity the reference to the default config will not survive the build, so we'll
            // reload it here before restoring its values.
            defaultConfig = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(ensureNotNull: false);

            //cleanup
            TestHelper.ConfigRestore(defaultConfig);
        }

        /// <summary>
        /// Test if config fields which override default values are included in the output json.
        /// Override values are based on our public iOS SDK documentation.
        /// </summary>
        [Test, Order(2)]
        public void TestOverrideIOSConfigFields()
        {
            // Instantiate an iOS Config and set default values.
            var iosConfig = ScriptableObject.CreateInstance<IOSConfiguration>();
            iosConfig.SetDefaults();

            // Set the 2 required root fields which are always included.
            iosConfig.AppId = TestHelper.AppId;
            iosConfig.SymbolUploadApiToken = TestHelper.ApiToken;

            // Determine how many root fields are included by default
            var defaultConfigJson = JsonConvert.SerializeObject(iosConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            var defaultConfigJObject = JObject.Parse(defaultConfigJson);
            var defaultPropertiesCount = defaultConfigJObject.Properties().Count();

            // Override 5 additional root properties of type boolean, string, int, EmbracePlistDictionary, and PlistNetworkElement respectively.
            // NOTE: Although the root "NETWORK" element contains its own fields, it counts as one root property. If any of its fields contain a value it gets included.
            iosConfig.STARTUP_MOMENT_SCREENSHOT_ENABLED = true;
            iosConfig.TRACE_ID_HEADER_NAME = "test-header";
            iosConfig.STARTUP_AUTOEND_SECONDS = 10;
            iosConfig.CUSTOM_PATH_HEADER_INFO.keys.Add("test-key");
            iosConfig.CUSTOM_PATH_HEADER_INFO.values.Add("test-value");
            iosConfig.NETWORK.DEFAULT_CAPTURE_LIMIT = 10;
            iosConfig.NETWORK.DOMAINS.keys = new List<string>() { "test.com" };
            iosConfig.NETWORK.DOMAINS.values = new List<int>() { 3 };

            var overrideConfigJson = JsonConvert.SerializeObject(iosConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            var overrideConfigJObject = JObject.Parse(overrideConfigJson);
            var overridePropertiesCount = overrideConfigJObject.Properties().Count();

            // Assert 5 additional fields are present in the output JSON
            Assert.AreEqual(defaultPropertiesCount + 5, overridePropertiesCount);

            // Clear 5 overrides, but leave the field in the "DEFAULT_CAPTURE_LIMIT" in "NETWORK" dictionary set.
            // This should result in 1 additional override field in the output json
            iosConfig.STARTUP_MOMENT_SCREENSHOT_ENABLED = false;
            iosConfig.TRACE_ID_HEADER_NAME = string.Empty;
            iosConfig.STARTUP_AUTOEND_SECONDS = 0;
            iosConfig.CUSTOM_PATH_HEADER_INFO.keys.Clear();
            iosConfig.CUSTOM_PATH_HEADER_INFO.values.Clear();
            iosConfig.NETWORK.DOMAINS.keys.Clear();
            iosConfig.NETWORK.DOMAINS.values.Clear();

            overrideConfigJson = JsonConvert.SerializeObject(iosConfig, new JsonSerializerSettings { ContractResolver = new SDKConfigContractResolver() });
            overrideConfigJObject = JObject.Parse(overrideConfigJson);
            overridePropertiesCount = overrideConfigJObject.Properties().Count();

            Assert.AreEqual(defaultPropertiesCount + 1, overridePropertiesCount);

            //cleanup
            Object.DestroyImmediate(iosConfig);
        }


        /// <summary>
        /// Test if plist has deserializedConfig values from EmbraceConfiguration.
        /// </summary>
        [Test, Order(2)]
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void ConfigToPlistDocumentTest()
        {
            if (!_hasBuiltIOS)
            {
                BuildiOSTest();
            }

            // NOTE: The test config contains override values for all fields which are used to output a plist with all available config settings.
            var testConfig = Resources.Load<IOSConfiguration>("TestConfigurations/TestIOSConfiguration");

            var plist = new PlistDocument();
            plist.ReadFromFile(AssetDatabaseUtil.ProjectDirectory + "/Builds/Test Builds/iOSBuild/Embrace-Info.plist");
            var root = plist.root;
            var loadedPlistRoot = root.values;
            var fieldInfos = ReflectionUtil.GetDeclaredInstanceFields(testConfig.GetType());
            var containsAllKeys = true;

            foreach (var fieldInfo in fieldInfos)
            {
                if(GetAttribute<JsonIgnoreAttribute>(fieldInfo, true) != null)
                {
                    continue;
                }

                string fieldName = GetAttribute<JsonPropertyAttribute>(fieldInfo, true)?.PropertyName ?? fieldInfo.Name;
                containsAllKeys &= loadedPlistRoot.ContainsKey(fieldName);
            }

            if (containsAllKeys)
            {
                Assert.AreEqual(loadedPlistRoot["API_KEY"].AsString(), testConfig.AppId);
                Assert.AreEqual(loadedPlistRoot["API_TOKEN"].AsString(), testConfig.SymbolUploadApiToken);
                Assert.AreEqual(loadedPlistRoot["CRASH_REPORT_PROVIDER"].AsString(), testConfig.CRASH_REPORT_PROVIDER);
                Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["CAPTURE_PUBLIC_KEY"].AsString(), testConfig.NETWORK.CAPTURE_PUBLIC_KEY);
                Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["DEFAULT_CAPTURE_LIMIT"].AsInteger(), testConfig.NETWORK.DEFAULT_CAPTURE_LIMIT);
                Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["DOMAINS"].AsDict().values.Count, testConfig.NETWORK.DOMAINS.keys.Count);
                Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["DOMAINS"].AsDict().values.Count, testConfig.NETWORK.DOMAINS.values.Count);
                Assert.AreEqual(loadedPlistRoot["WEBVIEW_ENABLE"].AsBoolean(), testConfig.WEBVIEW_ENABLE);
                Assert.AreEqual(loadedPlistRoot["MAX_SESSION_SECONDS"].AsInteger(), testConfig.MAX_SESSION_SECONDS);
                Assert.AreEqual(loadedPlistRoot["TRACE_ID_HEADER_NAME"].AsString(), testConfig.TRACE_ID_HEADER_NAME);
                Assert.AreEqual(loadedPlistRoot["CAPTURE_COORDINATES"].AsBoolean(), testConfig.CAPTURE_COORDINATES);
                Assert.AreEqual(loadedPlistRoot["ENABLE_WK_AUTO_RELOAD"].AsBoolean(), testConfig.ENABLE_WK_AUTO_RELOAD);
                Assert.AreEqual(loadedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Count, testConfig.CUSTOM_PATH_HEADER_INFO.keys.Count);
                Assert.AreEqual(loadedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Count, testConfig.CUSTOM_PATH_HEADER_INFO.values.Count);
                Assert.AreEqual(loadedPlistRoot["CAPTURE_TAPPED_ELEMENTS"].AsBoolean(), testConfig.CAPTURE_TAPPED_ELEMENTS);
                Assert.AreEqual(loadedPlistRoot["STARTUP_AUTOEND_SECONDS"].AsInteger(), testConfig.STARTUP_AUTOEND_SECONDS);
                Assert.AreEqual(loadedPlistRoot["WEBVIEW_STRIP_QUERYPARAMS"].AsBoolean(), testConfig.WEBVIEW_STRIP_QUERYPARAMS);
                Assert.AreEqual(loadedPlistRoot["ENABLE_AUTOMATIC_VIEW_CAPTURE"].AsBoolean(), testConfig.ENABLE_AUTOMATIC_VIEW_CAPTURE);
                Assert.AreEqual(loadedPlistRoot["NSURLCONNECTION_PROXY_ENABLE"].AsBoolean(), testConfig.NSURLCONNECTION_PROXY_ENABLE);
                Assert.AreEqual(loadedPlistRoot["BACKGROUND_FETCH_CAPTURE_ENABLE"].AsBoolean(), testConfig.BACKGROUND_FETCH_CAPTURE_ENABLE);
                Assert.AreEqual(loadedPlistRoot["COLLECT_NETWORK_REQUEST_METRICS"].AsBoolean(), testConfig.COLLECT_NETWORK_REQUEST_METRICS);
                Assert.AreEqual(loadedPlistRoot["STARTUP_MOMENT_SCREENSHOT_ENABLED"].AsBoolean(), testConfig.STARTUP_MOMENT_SCREENSHOT_ENABLED);
            }
            else
            {
                Assert.Fail();
            }
        }

        /// <summary>
        /// Test if plist serialization operations produce same data
        /// </summary>
        [Test, Order(2)]
        public void PlistSerializationTest()
        {
            // Load test plist which has test values assigned for every field available.
            var loadedPlist = new PlistDocument();
            loadedPlist.ReadFromFile("Packages/io.embrace.internal/Testing/Resources/TestConfigurations/Embrace-Info.plist");

            // Serialize plist to JSON and convert to an IOSConfiguration scriptable object
            var plistJson = PlistUtil.ToJson(loadedPlist);
            var deserializedConfig = JsonConvert.DeserializeObject<IOSConfiguration>(plistJson);
            var loadedPlistRoot = loadedPlist.root.values;

            // Assert loaded plist dictionary contains same keys as deserialized config
            var fieldInfos = ReflectionUtil.GetDeclaredInstanceFields(deserializedConfig.GetType());
            foreach (var fieldInfo in fieldInfos)
            {
                if (GetAttribute<JsonIgnoreAttribute>(fieldInfo, true) != null)
                {
                    continue;
                }

                string fieldName = GetAttribute<JsonPropertyAttribute>(fieldInfo, true)?.PropertyName ?? fieldInfo.Name;
                Assert.IsTrue(loadedPlistRoot.ContainsKey(fieldName));
            }

            // Assert the test data in the loaded PlistDocument matches its deserialized scriptable object
            Assert.AreEqual(loadedPlistRoot["API_KEY"].AsString(), deserializedConfig.AppId);
            Assert.AreEqual(loadedPlistRoot["API_TOKEN"].AsString(), deserializedConfig.SymbolUploadApiToken);
            Assert.AreEqual(loadedPlistRoot["CRASH_REPORT_PROVIDER"].AsString(), deserializedConfig.CRASH_REPORT_PROVIDER);
            Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["CAPTURE_PUBLIC_KEY"].AsString(), deserializedConfig.NETWORK.CAPTURE_PUBLIC_KEY);
            Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["DEFAULT_CAPTURE_LIMIT"].AsInteger(), deserializedConfig.NETWORK.DEFAULT_CAPTURE_LIMIT);
            Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["DOMAINS"].AsDict().values.Keys.ToList()[0], deserializedConfig.NETWORK.DOMAINS.keys[0]);
            Assert.AreEqual(loadedPlistRoot["NETWORK"].AsDict()["DOMAINS"].AsDict().values.Values.ToList()[0].AsInteger(), deserializedConfig.NETWORK.DOMAINS.values[0]);
            Assert.AreEqual(loadedPlistRoot["WEBVIEW_ENABLE"].AsBoolean(), deserializedConfig.WEBVIEW_ENABLE);
            Assert.AreEqual(loadedPlistRoot["MAX_SESSION_SECONDS"].AsInteger(), deserializedConfig.MAX_SESSION_SECONDS);
            Assert.AreEqual(loadedPlistRoot["TRACE_ID_HEADER_NAME"].AsString(), deserializedConfig.TRACE_ID_HEADER_NAME);
            Assert.AreEqual(loadedPlistRoot["CAPTURE_COORDINATES"].AsBoolean(), deserializedConfig.CAPTURE_COORDINATES);
            Assert.AreEqual(loadedPlistRoot["ENABLE_WK_AUTO_RELOAD"].AsBoolean(), deserializedConfig.ENABLE_WK_AUTO_RELOAD);
            Assert.AreEqual(loadedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Keys.ToList()[0], deserializedConfig.CUSTOM_PATH_HEADER_INFO.keys[0]);
            Assert.AreEqual(loadedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Values.ToList()[0].AsString(), deserializedConfig.CUSTOM_PATH_HEADER_INFO.values[0]);
            Assert.AreEqual(loadedPlistRoot["CAPTURE_TAPPED_ELEMENTS"].AsBoolean(), deserializedConfig.CAPTURE_TAPPED_ELEMENTS);
            Assert.AreEqual(loadedPlistRoot["STARTUP_AUTOEND_SECONDS"].AsInteger(), deserializedConfig.STARTUP_AUTOEND_SECONDS);
            Assert.AreEqual(loadedPlistRoot["WEBVIEW_STRIP_QUERYPARAMS"].AsBoolean(), deserializedConfig.WEBVIEW_STRIP_QUERYPARAMS);
            Assert.AreEqual(loadedPlistRoot["ENABLE_AUTOMATIC_VIEW_CAPTURE"].AsBoolean(), deserializedConfig.ENABLE_AUTOMATIC_VIEW_CAPTURE);
            Assert.AreEqual(loadedPlistRoot["NSURLCONNECTION_PROXY_ENABLE"].AsBoolean(), deserializedConfig.NSURLCONNECTION_PROXY_ENABLE);
            Assert.AreEqual(loadedPlistRoot["BACKGROUND_FETCH_CAPTURE_ENABLE"].AsBoolean(), deserializedConfig.BACKGROUND_FETCH_CAPTURE_ENABLE);
            Assert.AreEqual(loadedPlistRoot["COLLECT_NETWORK_REQUEST_METRICS"].AsBoolean(), deserializedConfig.COLLECT_NETWORK_REQUEST_METRICS);
            Assert.AreEqual(loadedPlistRoot["STARTUP_MOMENT_SCREENSHOT_ENABLED"].AsBoolean(), deserializedConfig.STARTUP_MOMENT_SCREENSHOT_ENABLED);

            // Instantiate an IOSConfiguration scriptable object
            var iOSConfig = ScriptableObject.CreateInstance<IOSConfiguration>();
            iOSConfig.AppId = TestHelper.AppId;
            iOSConfig.SymbolUploadApiToken = TestHelper.ApiToken;
            iOSConfig.NETWORK.CAPTURE_PUBLIC_KEY = "test_public_key";
            iOSConfig.NETWORK.DEFAULT_CAPTURE_LIMIT = 99;
            iOSConfig.NETWORK.DOMAINS.keys.Add("domain1");
            iOSConfig.NETWORK.DOMAINS.values.Add(10);
            iOSConfig.CUSTOM_PATH_HEADER_INFO.keys.Add("HEADER");
            iOSConfig.CUSTOM_PATH_HEADER_INFO.values.Add("test-header");
            iOSConfig.CUSTOM_PATH_HEADER_INFO.keys.Add("RELATIVE_URL_PATH");
            iOSConfig.CUSTOM_PATH_HEADER_INFO.values.Add("test/url/path");

            // Serialize config to JSON and convert to a plist document
            var configJson = JsonConvert.SerializeObject(
                iOSConfig,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new SDKConfigContractResolver()
                }
            );

            var deserializedPlist = PlistUtil.FromJson(configJson);
            var deserializedPlistRoot = deserializedPlist.root.values;

            // Assert the test data in the loaded PlistDocument matches its deserialized scriptable object
            Assert.AreEqual(deserializedPlistRoot["NETWORK"].AsDict()["CAPTURE_PUBLIC_KEY"].AsString(), iOSConfig.NETWORK.CAPTURE_PUBLIC_KEY);
            Assert.AreEqual(deserializedPlistRoot["NETWORK"].AsDict()["DEFAULT_CAPTURE_LIMIT"].AsInteger(), iOSConfig.NETWORK.DEFAULT_CAPTURE_LIMIT);
            Assert.AreEqual(deserializedPlistRoot["NETWORK"].AsDict()["DOMAINS"].AsDict().values.Keys.ToList()[0], iOSConfig.NETWORK.DOMAINS.keys[0]);
            Assert.AreEqual(deserializedPlistRoot["NETWORK"].AsDict()["DOMAINS"].AsDict().values.Values.ToList()[0].AsInteger(), iOSConfig.NETWORK.DOMAINS.values[0]);
            Assert.AreEqual(deserializedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Keys.ToList()[0], iOSConfig.CUSTOM_PATH_HEADER_INFO.keys[0]);
            Assert.AreEqual(deserializedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Keys.ToList()[1], iOSConfig.CUSTOM_PATH_HEADER_INFO.keys[1]);
            Assert.AreEqual(deserializedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Values.ToList()[0].AsString(), iOSConfig.CUSTOM_PATH_HEADER_INFO.values[0]);
            Assert.AreEqual(deserializedPlistRoot["CUSTOM_PATH_HEADER_INFO"].AsDict().values.Values.ToList()[1].AsString(), iOSConfig.CUSTOM_PATH_HEADER_INFO.values[1]);


            //cleanup
            Object.DestroyImmediate(iOSConfig);
            Object.DestroyImmediate(deserializedConfig);
        }

        /// <summary>
        /// Test if .pbxproj file is created.
        /// </summary>
        [Test, Order(2)]
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void PBXProjectTest()
        {
            if (!_hasBuiltIOS)
            {
                BuildiOSTest();
            }

            string filePath = AssetDatabaseUtil.ProjectDirectory + "/Builds/Test Builds/iOSBuild/Unity-iPhone.xcodeproj/project.pbxproj";
            Assert.IsTrue(File.Exists(filePath));
        }

#if UNITY_2020_1_OR_NEWER && !EMBRACE_DISABLE_IL2CPP_SYMBOL_MAPPING
        // Unity 2019 does not generate this file properly, so we can only test if it matches ours in 2020+
        [Test, Order(2)]
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void EmbraceIl2CppUtility_ParseLineMappingsFromSourceInfo_ReturnsSameResult_AsUnityDefault()
        {
            if (!_hasBuiltIOS)
            {
                BuildiOSTest();
            }
            string buildPath = Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Builds/Test Builds/iOSBuild");
            Assert.IsTrue(Directory.Exists(buildPath));
            string sourceDirectory = EmbraceIl2CppSymbolUtility.GetIl2CppSourceOutputPath(buildPath);
            Assert.IsTrue(Directory.Exists(sourceDirectory));
            string symbolDirectory = EmbraceIl2CppSymbolUtility.GetFinalIl2CppSymbolOutputPath(buildPath);
            Assert.IsTrue(Directory.Exists(symbolDirectory));
            string unityLineMappingsPath = Path.Combine(symbolDirectory, EmbraceIl2CppSymbolUtility.LINE_MAPPING_FILE_NAME);
            Assert.IsTrue(File.Exists(unityLineMappingsPath));

            const string TEST_LINE_MAPPINGS_FILE_NAME = "test_line_mappings.json";
            string generatedLineMappingsPath = Path.Combine(symbolDirectory, TEST_LINE_MAPPINGS_FILE_NAME);
            EmbraceIl2CppSymbolUtility.ParseLineMappingsFromSourceInfo(sourceDirectory, generatedLineMappingsPath);

            Dictionary<string, Dictionary<string, Dictionary<int, int>>> ourMappings =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<int, int>>>>(File.ReadAllText(generatedLineMappingsPath));
            Dictionary<string, Dictionary<string, Dictionary<int, int>>> unityMappings =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<int, int>>>>(File.ReadAllText(unityLineMappingsPath));

            Assert.AreEqual(ourMappings.Count, unityMappings.Count);

            foreach (var unityNativeSourceFile in unityMappings)
            {
                // The native source files Unity uses to generate the file are in a different directory than ours, so we
                // match on the file name rather than the full path
                string fuzzyMatchedKey = ourMappings.Keys.FirstOrDefault(f =>
                    Path.GetFileName(f) == Path.GetFileName(unityNativeSourceFile.Key));
                Assert.IsNotNull(fuzzyMatchedKey);
                var ourNativeSourceFile = ourMappings[fuzzyMatchedKey];
                foreach (var unityManagedSourceFile in unityNativeSourceFile.Value)
                {
                    var ourManagedSourceFile = ourNativeSourceFile[unityManagedSourceFile.Key];
                    foreach (var unityLineMapping in unityManagedSourceFile.Value)
                    {
                        Assert.AreEqual(unityLineMapping.Value, ourManagedSourceFile[unityLineMapping.Key]);
                    }
                }
            }
        }
#endif

        [Test, Order(500)] // The intention is for this test to go last
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void BuildiOSSimulatorTest()
        {
            var defaultConfig = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(ensureNotNull: false);

            Assert.NotNull(defaultConfig);

            // NOTE: The test config contains override values for all fields which is used to output a plist with all available config settings.
            var testConfig = Resources.Load<IOSConfiguration>("TestConfigurations/TestIOSConfiguration");

            TestHelper.ConfigBackup(defaultConfig);
            TestHelper.CopyConfig(testConfig, defaultConfig);

            Assert.DoesNotThrow(() => BuildiOS(true));

            // This is an issue with 2019 where the defaultConfig does not survive the build process
            // As a result, we're reinitializing it before restoring.
            if (defaultConfig == null)
            {
                defaultConfig = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(ensureNotNull: false);
            }

            //cleanup
            TestHelper.ConfigRestore(defaultConfig);
        }

        private T GetAttribute<T>(System.Reflection.MemberInfo member, bool inherit)
            where T : System.Attribute
        {
            object[] attributes = member.GetCustomAttributes(inherit);
            for(int i = 0; i < attributes.Length; ++i)
            {
                if(attributes[i] is T attr)
                {
                    return attr;
                }
            }
            return null;
        }

        // In Unity 2019.3 the iOS target was split into two targets, a launcher and the framework.
        // We have to be able to integrate with both target setups.
#if UNITY_2019_3_OR_NEWER
        private static string[] GetProjectNames(PBXProject project)
        {
            return new[]
            {
                project.GetUnityMainTargetGuid(),
                project.GetUnityFrameworkTargetGuid()
            };
        }
#else
        private static string[] GetProjectNames(PBXProject project)
        {
            return new[]
            {
                project.TargetGuidByName(PBXProject.GetUnityTargetName())
            };
        }
#endif

        private BuildResult BuildiOS(bool buildSimulator = false)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
            buildPlayerOptions.locationPathName = AssetDatabaseUtil.ProjectDirectory + "/Builds/Test Builds/iOSBuild";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = default;
            BuildSummary summary = default;

            if (buildSimulator)
            {
                var userSdkVersion = PlayerSettings.iOS.sdkVersion;
                PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;

                report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                summary = report.summary;

                PlayerSettings.iOS.sdkVersion = userSdkVersion;
            }
            else
            {
                report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                summary = report.summary;
            }

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }

            return summary.result;
        }
#endif
    }
}
