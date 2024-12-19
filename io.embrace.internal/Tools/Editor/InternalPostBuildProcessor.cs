#if (UNITY_IOS || UNITY_TVOS) && UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.iOS.Xcode;

namespace Embrace.Internal.Editor
{
    internal class InternalPostBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.iOS)
            switch(report.summary.platform)
            {
                case BuildTarget.iOS:
                case BuildTarget.tvOS:
                    DisableBitcode(report);
                    break;
            }
        }

        private static void DisableBitcode(BuildReport report)
        {
            string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";


            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            //Disabling Bitcode on all targets
            pbxProject.SetBuildProperty(new string[]
            {
                pbxProject.GetUnityMainTargetGuid(),
                pbxProject.GetUnityFrameworkTargetGuid(),
                pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName()),
                pbxProject.ProjectGuid(),
            }, "ENABLE_BITCODE", "NO");

            pbxProject.WriteToFile(projectPath);
        }
    }
}
#endif