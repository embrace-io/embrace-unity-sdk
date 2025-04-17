#if UNITY_2020 && UNITY_ANDROID
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.TestTools;
using EmbraceSDK.EditorView;
using UnityEditor.Build;
using UnityEditor.SceneManagement;

namespace EmbraceSDK.Tests
{
    /// <summary>
    /// Unit tests for SymbolsUtil.cs
    /// </summary>
    public class AndroidSymbolsTest
    {
        private const string apiToken = "test27e891ad45853949004eb7y5b9fr";
        private const string appID = "12345";

        private bool exportAsGoogleAndroidProject;
        private AndroidArchitecture targetArchitectures;
        private ScriptingImplementation scriptingBackend;

        /// <summary>
        /// Testing that symbol files are moved to correct location with Unity 2020.
        /// Scripting backend: Mono
        /// Build type: Release
        /// Architecture: ARMv7
        /// </summary>
        /// <returns></returns>
        [Test]
        public void MovingSymbolsMonoReleaseARMv7()
        {
            CacheDefaultValues();
            EmbraceConfiguration configuration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            Assert.NotNull(configuration);

            configuration.AppId = appID;
            configuration.SymbolUploadApiToken = apiToken;
            configuration.SetDirty();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = GetProjectPath() + "/Builds/Test Builds/symbolsExportTest";
            buildPlayerOptions.target = BuildTarget.Android;

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "testScene.unity");
            string path = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.CloseScene(scene, true);
            buildPlayerOptions.scenes = new[] { path };

            BuildResult summary = BuildAndroid(buildPlayerOptions);

            Assert.IsTrue(summary == BuildResult.Succeeded);
            AssertARMv7(buildPlayerOptions);

            Cleanup();
        }

        /// <summary>
        /// Testing that symbol files are moved to correct location with Unity 2020.
        /// Scripting backend: Mono
        /// Build type: Development
        /// Architecture: Arm64 and ARMv7
        /// </summary>
        /// <returns></returns>
        [Test]
        public void MovingSymbolsMonoDevelopmentARMv7()
        {
            CacheDefaultValues();
            EmbraceConfiguration configuration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            Assert.NotNull(configuration);

            configuration.AppId = appID;
            configuration.SymbolUploadApiToken = apiToken;
            configuration.SetDirty();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = GetProjectPath() + "/Builds/Test Builds/symbolsExportTest";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.Development;

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "testScene.unity");
            string path = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.CloseScene(scene, true);
            buildPlayerOptions.scenes = new[] { path };

            BuildResult summary = BuildAndroid(buildPlayerOptions);

            Assert.IsTrue(summary == BuildResult.Succeeded);
            AssertARMv7(buildPlayerOptions);

            Cleanup();
        }

        /// <summary>
        /// Testing that symbol files are moved to correct location with Unity 2020.
        /// Scripting backend: IL2CPP
        /// Build type: Release
        /// Architecture: Arm64 and ARMv7
        /// </summary>
        /// <returns></returns>
        [Test]
        public void MovingSymbolsIL2CPPReleaseArm64ARMv7()
        {
            CacheDefaultValues();
            EmbraceConfiguration configuration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            Assert.NotNull(configuration);

            configuration.AppId = appID;
            configuration.SymbolUploadApiToken = apiToken;
            configuration.SetDirty();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = GetProjectPath() + "/Builds/Test Builds/symbolsExportTest";
            buildPlayerOptions.target = BuildTarget.Android;

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "testScene.unity");
            string path = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.CloseScene(scene, true);
            buildPlayerOptions.scenes = new[] { path };

            BuildResult summary = BuildAndroid(buildPlayerOptions);

            Assert.IsTrue(summary == BuildResult.Succeeded);
            AssertARM64(buildPlayerOptions);
            AssertARMv7(buildPlayerOptions);

            Cleanup();
        }

        /// <summary>
        /// Testing that symbol files are moved to correct location with Unity 2020.
        /// Scripting backend: IL2CPP
        /// Build type: Release
        /// Architecture: Arm64 and ARMv7
        /// </summary>
        /// <returns></returns>
        [Test]
        public void MovingSymbolsIL2CPPDevelopmentArm64ARMv7()
        {
            CacheDefaultValues();
            EmbraceConfiguration configuration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(ensureNotNull: false);

            Assert.NotNull(configuration);

            configuration.AppId = appID;
            configuration.SymbolUploadApiToken = apiToken;
            configuration.SetDirty();

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = GetProjectPath() + "/Builds/Test Builds/symbolsExportTest";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.Development;

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, "testScene.unity");
            string path = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.CloseScene(scene, true);
            buildPlayerOptions.scenes = new[] { path };

            BuildResult summary = BuildAndroid(buildPlayerOptions);

            Assert.IsTrue(summary == BuildResult.Succeeded);
            AssertARM64(buildPlayerOptions);
            AssertARMv7(buildPlayerOptions);

            Cleanup();
        }

        private void AssertARM64(BuildPlayerOptions buildPlayerOptions)
        {
            Assert.IsTrue(Directory.Exists(buildPlayerOptions.locationPathName + "/unityLibrary/symbols/arm64-v8a"));
            Assert.IsTrue(File.Exists(buildPlayerOptions.locationPathName + "/unityLibrary/symbols/arm64-v8a/libunity.so"));
            Assert.IsTrue(File.Exists(buildPlayerOptions.locationPathName + "/unityLibrary/symbols/arm64-v8a/libmain.so"));
        }

        private void AssertARMv7(BuildPlayerOptions buildPlayerOptions)
        {
            Assert.IsTrue(Directory.Exists(buildPlayerOptions.locationPathName + "/unityLibrary/symbols/armeabi-v7a"));
            Assert.IsTrue(File.Exists(buildPlayerOptions.locationPathName + "/unityLibrary/symbols/armeabi-v7a/libunity.so"));
            Assert.IsTrue(File.Exists(buildPlayerOptions.locationPathName + "/unityLibrary/symbols/armeabi-v7a/libmain.so"));
        }

        private void CacheDefaultValues()
        {
            targetArchitectures = PlayerSettings.Android.targetArchitectures;
            scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            exportAsGoogleAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
        }

        private void Cleanup()
        {
            FileUtil.DeleteFileOrDirectory(GetProjectPath() + "/Builds/Test Builds/symbolsExportTest");
            PlayerSettings.Android.targetArchitectures = targetArchitectures;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, scriptingBackend);
            EditorUserBuildSettings.exportAsGoogleAndroidProject = exportAsGoogleAndroidProject;
        }

        private BuildResult BuildAndroid(BuildPlayerOptions buildPlayerOptions)
        {
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

        private string GetProjectPath()
        {
            return Application.dataPath.Replace("/Assets", "");
        }
    }
}
#endif
