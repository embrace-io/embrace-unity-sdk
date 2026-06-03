#if UNITY_ANDROID || UNITY_IOS
using System.IO;
using System.Text.RegularExpressions;
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
            switch (report.summary.platform)
            {
                case BuildTarget.Android:
                    ValidateJdkVersion();
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

#if UNITY_ANDROID
        private static void ValidateJdkVersion()
        {
            string jdkPath = AndroidExternalToolsSettings.jdkRootPath;

            if (string.IsNullOrEmpty(jdkPath))
            {
                // Unity uses its bundled JDK — find it relative to the editor installation.
                jdkPath = Path.Combine(
                    EditorApplication.applicationContentsPath,
                    "PlaybackEngines", "AndroidPlayer", "OpenJDK");
            }

            string releaseFile = Path.Combine(jdkPath, "release");
            if (!File.Exists(releaseFile))
                return; // Can't determine version; let the build fail naturally if JDK is wrong.

            string content = File.ReadAllText(releaseFile);
            // release file contains a line like: JAVA_VERSION="17.0.9"
            // For JDK 8 it is "1.8.0_xxx"; for 9+ it is the major version directly.
            Match match = Regex.Match(content, @"JAVA_VERSION=""(\d+)");
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out int major))
                return;

            if (major < 17)
            {
                throw new BuildFailedException(
                    $"Embrace Android SDK 8.x requires JDK 17 or higher. " +
                    $"Your current JDK at '{jdkPath}' is Java {major}.\n\n" +
                    "To fix this:\n" +
                    "  1. Install JDK 17 (e.g. from https://adoptium.net)\n" +
                    "  2. In Unity, open Preferences > External Tools > JDK\n" +
                    "  3. Uncheck 'JDK Installed with Unity' and point to your JDK 17 installation");
            }
        }
#endif

        private void RemoveXCFrameworkDirectories()
        {
            var xcframeworksPath = Path.GetFullPath(DIR_PATH_XCFRAMEWORKS);
            if (Directory.Exists(xcframeworksPath))
            {
                try
                {
                    Directory.Delete(xcframeworksPath, true);
                }
                catch (System.Exception exc)
                {
                    Debug.LogError($"Error deleting xcframework directories: {exc}");
                }
                
            }
        }
    }
}
#endif