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