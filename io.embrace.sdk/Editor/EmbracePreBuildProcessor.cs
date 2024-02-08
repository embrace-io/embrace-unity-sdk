#if UNITY_ANDROID || UNITY_IOS
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EmbraceSDK.EditorView
{
    internal class EmbracePreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

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
                    EmbraceIl2CppSymbolUtility.OnPreprocessBuild(report);
                    break;
            }
        }
    }
}
#endif