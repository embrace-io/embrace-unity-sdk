using System.Collections.Generic;
using System.IO;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PostBuildEnvironmentTests
    {
        private static int _testEnvsCount;
        private static string _originalDataDir;
        private static string _testDataDir;
        private static bool _refreshComplete;
        private static int _testNum = 1;

        [SetUp]
        public void TestSetup()
        {
            _testEnvsCount = 3;
            _originalDataDir = AssetDatabaseUtil.EmbraceDataDirectory;
            _testDataDir = "Assets/TestFolder";

            AssetDatabaseUtil.EmbraceDataDirectory = _testDataDir;

            // Each test setup should specify a unique environment name to avoid intermittent asset serialization errors
            // that can occur from deleting and creating scriptable objects with the same names between test runs.
            TestHelper.CreateTestEnvironments(_testEnvsCount, $"testEnv{++_testNum}");
        }

        [TearDown]
        public void TestTearDown()
        {
            // restore original value
            AssetDatabaseUtil.EmbraceDataDirectory = _originalDataDir;

            // Delete system env variables
            if (System.Environment.GetEnvironmentVariable(EnvironmentsUtil.EMBRACE_SYSTEM_ENV_INDEX) != null)
            {
                System.Environment.SetEnvironmentVariable(EnvironmentsUtil.EMBRACE_SYSTEM_ENV_INDEX, null);
            }

            if (System.Environment.GetEnvironmentVariable(EnvironmentsUtil.EMBRACE_SYSTEM_ENV_NAME) != null)
            {
                System.Environment.SetEnvironmentVariable(EnvironmentsUtil.EMBRACE_SYSTEM_ENV_NAME, null);
            }

            // Delete test files
            var assetPaths = new List<string>();
            assetPaths.AddRange(AssetDatabaseUtil.GetAssetPaths<Environments>(_testDataDir));
            TestHelper.DeleteTestAssets(assetPaths.ToArray());

            // Delete test folders
            var testDirMeta = $"{_testDataDir}.meta";
            if (Directory.Exists(_testDataDir)) Directory.Delete(_testDataDir, true);
            if (File.Exists(testDirMeta)) File.Delete(testDirMeta);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Test if system environment variable defined by index sets correct Embrace.activeEnvironmentIndex
        /// </summary>
        [Test]
        public void TestSystemVariableIndex()
        {
            var testEnvIndex = 1; // 2nd environment config index

            // Set system env variable to indicate index 1 of Embrace Environments configs
            System.Environment.SetEnvironmentVariable(EnvironmentsUtil.EMBRACE_SYSTEM_ENV_INDEX, testEnvIndex.ToString());

            // Load environments object configured with system env var
            var environments = EnvironmentsUtil.ConfigureForBuild();

            // Did Embrace Environments asset get set with system env var value?
            Assert.AreEqual(environments.activeEnvironmentIndex, testEnvIndex);
        }

        /// <summary>
        /// Test if system environment variable defined by name sets correct Embrace.activeEnvironmentIndex
        /// </summary>
        [Test]
        public void TestSystemVariableName()
        {
            var envIndex = 2;// 3rd environment config index
            var testEnvName = $"testEnv{_testNum}{envIndex}";

            // Set system env variable to name of 3rd Embrace Environments config
            System.Environment.SetEnvironmentVariable(EnvironmentsUtil.EMBRACE_SYSTEM_ENV_NAME, testEnvName);

            // Load environments object configured with system env var
            var environments = EnvironmentsUtil.ConfigureForBuild();
            var envConfig = environments.environmentConfigurations[environments.activeEnvironmentIndex];

            // Did Embrace Environments asset get set to the expected config?
            Assert.AreEqual(envConfig.name, testEnvName);
        }
    }
}