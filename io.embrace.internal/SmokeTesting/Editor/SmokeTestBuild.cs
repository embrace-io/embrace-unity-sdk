using System;
using System.Collections;
using System.Collections.Generic;
using EmbraceSDK.EditorView;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;

namespace Embrace.Internal.SmokeTests
{
    public static class SmokeTestBuild
    {
        private const string APP_ID_ARG = "--smokeTestAppId=";
        private const string DEFAULT_APP_ID = "12345";

        private const string SYMBOL_UPLOAD_TOKEN_ARG = "--smokeTestApiToken=";
        private const string DEFAULT_SYMBOL_UPLOAD_API_TOKEN = "test27e891ad45853949004eb7y5b9fr";

        private const string OUTPUT_PATH_ARG = "--smokeTestOutputPath=";
        private const string DEFAULT_BUILD_OUTPUT_PATH = "Builds/SmokeTest";

        private const string SMOKE_TEST_DRIVER_SCENE_PATH =
            "Packages/io.embrace.internal/SmokeTesting/Scenes/SmokeTest.unity";

        public static void Create()
        {
            EmbraceConfiguration defaultConfig = GetConfiguration();

            Assert.IsNotNull(defaultConfig);

            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;

            defaultConfig.AppId = GetCommandLineArgument(APP_ID_ARG, DEFAULT_APP_ID);
            defaultConfig.SymbolUploadApiToken =
                GetCommandLineArgument(SYMBOL_UPLOAD_TOKEN_ARG, DEFAULT_SYMBOL_UPLOAD_API_TOKEN);

            EditorUtility.SetDirty(defaultConfig);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { SMOKE_TEST_DRIVER_SCENE_PATH };
            buildPlayerOptions.locationPathName = GetCommandLineArgument(OUTPUT_PATH_ARG, DEFAULT_BUILD_OUTPUT_PATH);
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = BuildOptions.None;
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

            Assert.IsTrue(buildReport.summary.result == BuildResult.Succeeded);
        }

        private static EmbraceConfiguration GetConfiguration()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.iOS:
                    return AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(ensureNotNull: false);

                default:
                    throw new PlatformNotSupportedException(
                        $"Smoke test builds are only supported on iOS. Current build target is {EditorUserBuildSettings.activeBuildTarget}");
            }
        }

        private static string GetCommandLineArgument(string argPrefix, string defaultValue)
        {
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith(argPrefix))
                {
                    return arg.Substring(argPrefix.Length);
                }
            }

            return defaultValue;
        }
    }
}