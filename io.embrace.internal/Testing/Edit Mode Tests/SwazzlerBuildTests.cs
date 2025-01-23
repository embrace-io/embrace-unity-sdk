#if UNITY_ANDROID
using System.IO;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

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

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/Scenes/SampleScene.unity" };
            buildPlayerOptions.locationPathName = AssetDatabaseUtil.ProjectDirectory + "/Builds/Test Builds/AndroidBuild";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            Assert.AreEqual(summary.result, BuildResult.Succeeded);

            // The reference to the config instance will not always survive the build, so we'll reload it here before
            // restoring its values.
            defaultConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            TestHelper.ConfigRestore(defaultConfig);
        }
    }
}
#endif