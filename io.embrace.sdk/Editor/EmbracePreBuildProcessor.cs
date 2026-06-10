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
            // If EMBRACE_JDK_PATH points to a JDK 17+ installation, the post-generate hook
            // will write org.gradle.java.home to gradle.properties so Gradle uses it.
            string overridePath = System.Environment.GetEnvironmentVariable("EMBRACE_JDK_PATH");
            if (!string.IsNullOrEmpty(overridePath) && GetJdkMajorVersion(overridePath) >= 17)
                return;

            // If the project's gradle.properties template already contains org.gradle.java.home,
            // the user has handled JDK configuration manually — trust them.
            if (EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.GradlePropertiesPath, out string propsContent, logWarningIfFileNotPresent: false)
                && propsContent.Contains("org.gradle.java.home"))
                return;

            string jdkPath = AndroidExternalToolsSettings.jdkRootPath;
            if (string.IsNullOrEmpty(jdkPath))
            {
                // Unity uses its bundled JDK — find it relative to the editor installation.
                jdkPath = Path.Combine(
                    EditorApplication.applicationContentsPath,
                    "PlaybackEngines", "AndroidPlayer", "OpenJDK");
            }

            int major = GetJdkMajorVersion(jdkPath);
            if (major < 0)
                return; // Can't determine version; let the build fail naturally if JDK is wrong.

            if (major < 17)
            {
                throw new BuildFailedException(
                    $"Embrace Android SDK 8.x requires JDK 17 or higher. " +
                    $"Your current JDK at '{jdkPath}' is Java {major}.\n\n" +
                    "To fix this, choose one of:\n" +
                    "  Option A — Install JDK 17 and set it in Unity Preferences > External Tools > JDK\n" +
                    "             (works on Unity 2022 and newer)\n" +
                    "  Option B — Add the following to Assets/Plugins/Android/gradleTemplate.properties:\n" +
                    "             org.gradle.java.home=/path/to/jdk17\n" +
                    "             (works on all Unity versions including 2021)");
            }
        }

        private static int GetJdkMajorVersion(string jdkPath)
        {
            string releaseFile = Path.Combine(jdkPath, "release");
            if (!File.Exists(releaseFile))
                return -1;
            string content = File.ReadAllText(releaseFile);
            // release file contains: JAVA_VERSION="17.0.9"
            // For JDK 8: "1.8.0_xxx" — captured as 1, which is correctly < 17.
            Match match = Regex.Match(content, @"JAVA_VERSION=""(\d+)");
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out int major))
                return -1;
            return major;
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