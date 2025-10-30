#if UNITY_ANDROID || UNITY_IOS
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace EmbraceSDK.EditorView
{
    internal class EmbracePreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        private static string DIR_PATH_XCFRAMEWORKS = "Packages/io.embrace.sdk/iOS/xcframeworks";

        public void OnPreprocessBuild(BuildReport report)
        {
            // Diagnostic logs
            var pkg = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None);
            Debug.Log($"[[Diag Pre]] Editor: {Application.unityVersion}");
            Debug.Log($"[[Diag Pre]] Editor exe: {EditorApplication.applicationPath}");
            Debug.Log($"[[Diag Pre]] Contents: {EditorApplication.applicationContentsPath}");
            Debug.Log($"[[Diag Pre]] IsBuildTargetSupported(Android): {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)}");
            Debug.Log($"[[Diag Pre]] PlaybackEngineDirectory(Android): {pkg}");
            Debug.Log($"[[Diag Pre]] AndroidPlayer exists? {Directory.Exists(pkg)}");

            Debug.Log($"[[Diag Pre]] SDK: {AndroidExternalToolsSettings.sdkRootPath}");
            Debug.Log($"[[Diag Pre]] NDK: {AndroidExternalToolsSettings.ndkRootPath}");
            Debug.Log($"[[Diag Pre]] JDK: {AndroidExternalToolsSettings.jdkRootPath}");

            foreach (var e in new[] { "JAVA_HOME","JDK_HOME","ANDROID_HOME","ANDROID_SDK_ROOT","ANDROID_NDK_ROOT","ANDROID_NDK_HOME" })
                Debug.Log($"[[Diag Pre]] ENV {e}={Environment.GetEnvironmentVariable(e)}");

            var gradleLauncher = Directory.Exists(Path.Combine(pkg ?? "", "Tools")) 
                ? Directory.GetFiles(Path.Combine(pkg ?? "", "Tools", "gradle"), "gradle-launcher-*.jar", SearchOption.AllDirectories).FirstOrDefault()
                : null;
            Debug.Log($"[[Diag Pre]] Has gradle launcher? {!string.IsNullOrEmpty(gradleLauncher)}");
            
            switch (report.summary.platform)
            {
                case BuildTarget.Android:
                    // Generate the dependencies file before the External Dependency Manager resolves the dependencies at build time.
                    EmbraceEdmUtility.GenerateDependenciesFile();
                    EmbraceGradleUtility.VerifyIfSwazzlerAndBugshakeArePresentSimultaneously();
                    // The following line is currently commented out because we cannot guarantee which version of the Android SDK or swazzler
                    // that they are using with the current bugshake implementation. We will need to revisit this in the future.
                    EmbraceGradleUtility.EnforceSwazzlerDependencyVersion();
                    EmbraceIl2CppSymbolUtility.OnPreprocessBuild(report);
                    break;
                case BuildTarget.iOS:
                    RemoveXCFrameworkDirectories();
                    EmbraceIl2CppSymbolUtility.OnPreprocessBuild(report);
                    break;
            }
        }

        private void RemoveXCFrameworkDirectories()
        {
            var xcframeworksPath = Path.GetFullPath(DIR_PATH_XCFRAMEWORKS);
            if (Directory.Exists(xcframeworksPath))
            {
                try
                {
                    Directory.Delete(xcframeworksPath, true);
                }
                catch (Exception exc)
                {
                    Debug.LogError($"Error deleting xcframework directories: {exc}");
                }
                
            }
        }
    }
}
#endif