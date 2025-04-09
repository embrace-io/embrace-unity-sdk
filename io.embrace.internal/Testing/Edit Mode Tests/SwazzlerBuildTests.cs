#if UNITY_ANDROID
using System;
using System.IO;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class SwazzlerBuildTests
    {
        private const string LAUNCHER_TEMPLATE_PATH = "Assets/Plugins/Android/launcherTemplate.gradle";
        private const string LAUNCHER_TEMPLATE_BACKUP_PATH = "Temp/Plugins/Android/launcherTemplate.gradle.backup";

        [SetUp]
        public void SetUp()
        {
            string backupDirectory = Directory.GetParent(LAUNCHER_TEMPLATE_BACKUP_PATH).FullName;
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            File.Copy(LAUNCHER_TEMPLATE_PATH, LAUNCHER_TEMPLATE_BACKUP_PATH, true);
        }

        [TearDown]
        public void TearDown()
        {
            File.Copy(LAUNCHER_TEMPLATE_BACKUP_PATH, LAUNCHER_TEMPLATE_PATH, true);
            File.Delete(LAUNCHER_TEMPLATE_BACKUP_PATH);
        }

        /// <summary>
        /// Tests for a swazzler build issue from 5.10.0-5.11.0 that would throw the following error when building the
        /// gradle project:
        ///
        /// A problem occurred configuring project ':launcher'.
        /// > java.lang.IllegalStateException: The value for extension 'embrace-internal' property 'ndkEnabled' is final
        ///   and cannot be changed any further.
        /// </summary>
        [Test]
#if UNITY_EDITOR_OSX
        // CPU lightmapping is not supported on macOS arm64, and recompiling
        // scripts seems to trigger this to happen, causing an error which causes
        // test failures on CI (which has no GPU).
        [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
        public void BuildSucceeds_WhenAllTasksRealizedImmediately()
        {
            // Adding this to the gradle file causes all tasks to be realized immediately, which was found to trigger
            // the build error
            const string GRADLE_INSTRUCTION = "tasks.matching { true }.all {}";

            using (StreamWriter writer = File.AppendText(LAUNCHER_TEMPLATE_PATH))
            {
                writer.WriteLine(writer.NewLine);
                writer.WriteLine(GRADLE_INSTRUCTION);
            }

            var defaultConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            Assert.NotNull(defaultConfig);

            // NOTE: The test config contains override values for all fields which are used to output a json file with all available config settings.
            var testConfig = Resources.Load<AndroidConfiguration>("TestConfigurations/TestAndroidConfiguration");

            TestHelper.ConfigBackup(defaultConfig);
            TestHelper.CopyConfig(testConfig, defaultConfig);
            
            // NOTE: With the swazzler update (7.3.0) the app_id and api_token must actually be valid now
            // If this test fails, make sure you are running unity with the start_unity.sh script in order to setup the env variables
            testConfig.AppId = Environment.GetEnvironmentVariable("embrace_test_app_id");
            testConfig.SymbolUploadApiToken = Environment.GetEnvironmentVariable("embrace_test_api_token");
            Assert.IsNotNull(testConfig.AppId);
            Assert.IsNotNull(testConfig.SymbolUploadApiToken);
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
            buildPlayerOptions.locationPathName = AssetDatabaseUtil.ProjectDirectory + "/Builds/Test Builds/AndroidBuild";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            Assert.AreEqual(BuildResult.Succeeded, summary.result);

            // The reference to the config instance will not always survive the build, so we'll reload it here before
            // restoring its values.
            defaultConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            TestHelper.ConfigRestore(defaultConfig);
        }
    }
}
#endif
