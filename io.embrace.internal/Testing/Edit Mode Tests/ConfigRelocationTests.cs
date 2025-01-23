using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using EmbraceSDK.EditorView;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.Tests
{
    public class ConfigRelocationTests
    {
        private static string GetRelativeAssetsSourceDir()
        {
            return "Assets/TestConfigsSourceFolder";
        }

        private static string GetRelativeAssetsDestinationDir()
        {
            return "Assets/TestConfigsDestinationFolder";
        }

        private static string GetAssetsSourceTestDir()
        {
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, GetRelativeAssetsSourceDir());
        }

        private static string GetAssetsDestinationTestDir()
        {
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, GetRelativeAssetsDestinationDir());
        }

        /// <summary>
        /// This test updates the embrace configuration data path and checks value stored in the file
        /// stored in the project root.  The value set and the value loaded are checked for equality.
        /// </summary>
        [Test]
        public void UpdatingEmbraceDataDirectorySavesValue()
        {
            var originalDir = AssetDatabaseUtil.EmbraceDataDirectory;
            var testDataDir = "Assets/TestFolder";
            var testConfigDirectory = $"{testDataDir}/Configurations";

            AssetDatabaseUtil.EmbraceDataDirectory = testDataDir;

            var settingsFilePath = Path.Combine(AssetDatabaseUtil.ProjectDirectory, ".embrace");
            var settingsText = File.ReadAllText(settingsFilePath);
            var settingsJObj = JObject.Parse(settingsText);
            var settingsToken = settingsJObj["dataDirectory"];

            Assert.IsFalse(string.IsNullOrEmpty(settingsToken.Value<string>()));
            Assert.AreEqual(settingsToken.ToString(), AssetDatabaseUtil.EmbraceDataDirectory);
            Assert.AreEqual(testConfigDirectory, AssetDatabaseUtil.ConfigurationsDirectory);

            // restore original value
            AssetDatabaseUtil.EmbraceDataDirectory = originalDir;
        }

        /// <summary>
        /// This test creates default and environment-based configuration scriptable objects and
        /// then relocates them using the same methods our Editor windows invoke.  After the relocation
        /// occurs, the test configs are loaded and checked against the initial ones for equality.
        /// </summary>
        [Test]
        public void RelocatingConfigsPersistsData()
        {
            var originalDir = AssetDatabaseUtil.EmbraceDataDirectory;
            var sourceDir = GetRelativeAssetsSourceDir();
            var destinationDir = GetRelativeAssetsDestinationDir();

            AssetDatabaseUtil.EmbraceDataDirectory = sourceDir;
            AssetDatabaseUtil.EnsureFolderExists(sourceDir);
            AssetDatabaseUtil.EnsureFolderExists(destinationDir);

            var testEnvsCount = 3;

            var environments = TestHelper.CreateTestEnvironments(testEnvsCount, "testEnv");
            var defaultAndroidConfig = AssetDatabaseUtil.CreateConfiguration<AndroidConfiguration>();
            var defaultIOSConfig = AssetDatabaseUtil.CreateConfiguration<IOSConfiguration>();

            defaultAndroidConfig.SetDefaults();
            defaultAndroidConfig.AppId = TestHelper.AppId;
            defaultAndroidConfig.SymbolUploadApiToken = TestHelper.ApiToken;

            defaultIOSConfig.SetDefaults();
            defaultIOSConfig.AppId = TestHelper.AppId;
            defaultIOSConfig.SymbolUploadApiToken = TestHelper.ApiToken;

            ConfigsRelocationUtil.RelocateAssets(sourceDir, destinationDir);

            var sw = new Stopwatch();
            sw.Start();
            while (!ConfigsRelocationUtil.RefreshComplete && sw.ElapsedMilliseconds < 5000L)
            {
                // waiting a max of 5 seconds for an asset refresh
            }

            sw.Stop();

            AssetDatabaseUtil.EmbraceDataDirectory = destinationDir;

            var envsPath = AssetDatabaseUtil.GetAssetPaths<Environments>(destinationDir);
            var configPaths = AssetDatabaseUtil.GetAssetPaths<EmbraceConfiguration>(destinationDir);
            var loadedEnvironments = AssetDatabaseUtil.LoadEnvironments();
            var loadedDefaultAndroidConfig = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>();
            var loadedDefaultIOSConfig = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>();

            // Did data move to the right directory?
            Assert.Greater(envsPath.Length, 0);
            Assert.Greater(configPaths.Length, 0);

            // Do the number of configs expected equal [total environments X 2 configs + 2 default configs]?
            Assert.AreEqual(configPaths.Length, testEnvsCount * 2 + 2);

            // Do the loaded default configs contain same data?
            TestHelper.AssertAndroidConfigsAreEqual(defaultAndroidConfig, loadedDefaultAndroidConfig);
            TestHelper.AssertIOSConfigsAreEqual(defaultIOSConfig, loadedDefaultIOSConfig);

            // Does the loaded environments scriptable object contain the same data?
            Assert.AreEqual(loadedEnvironments.environmentConfigurations.Count, testEnvsCount);

            // Do the environment sdk configurations contain the same data?
            TestHelper.AssertEnvironmentsAreEqual(environments, loadedEnvironments);

            // cleanup
            AssetDatabaseUtil.EmbraceDataDirectory = originalDir;

            var assetPaths = new List<string>();
            assetPaths.AddRange(AssetDatabaseUtil.GetAssetPaths<Environments>(destinationDir));
            assetPaths.AddRange(AssetDatabaseUtil.GetAssetPaths<Environments>(destinationDir));

            TestHelper.DeleteTestAssets(assetPaths.ToArray());

            var sourceFullDir = GetAssetsSourceTestDir();
            var sourceMetaPath = $"{sourceFullDir}.meta";
            var destinationFullDir = GetAssetsDestinationTestDir();
            var destinationMetaPath = $"{destinationFullDir}.meta";

            if (Directory.Exists(sourceFullDir)) Directory.Delete(sourceFullDir, true);
            if (Directory.Exists(destinationFullDir)) Directory.Delete(destinationFullDir, true);
            if (File.Exists(sourceMetaPath)) File.Delete(sourceMetaPath);
            if (File.Exists(destinationMetaPath)) File.Delete(destinationMetaPath);

            AssetDatabase.Refresh();
        }
    }
}