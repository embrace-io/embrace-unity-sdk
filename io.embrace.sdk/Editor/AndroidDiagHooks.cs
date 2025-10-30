using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class AndroidDiagHooks : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => int.MinValue;

    public void OnPreprocessBuild(BuildReport report)
    {
        Log("PRE");
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Log("POST");
    }

    static void Log(string stage)
    {
        var options = BuildOptions.None;
        var playerPkg = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, options);

        Debug.Log($"[{stage}] Editor: {Application.unityVersion}");
        Debug.Log($"[{stage}] Editor exe: {EditorApplication.applicationPath}");
        Debug.Log($"[{stage}] Contents: {EditorApplication.applicationContentsPath}");
        Debug.Log($"[{stage}] IsBuildTargetSupported(Android): {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)}");
        Debug.Log($"[{stage}] PlaybackEngineDirectory(Android): {playerPkg}");
        Debug.Log($"[{stage}] AndroidPlayer exists? {Directory.Exists(playerPkg)}");

        // External tools Unity will try to use (empty/null = use bundled)
        Debug.Log($"[{stage}] JDK: {AndroidExternalToolsSettings.jdkRootPath}");
        Debug.Log($"[{stage}] NDK: {AndroidExternalToolsSettings.ndkRootPath}");
        Debug.Log($"[{stage}] SDK: {AndroidExternalToolsSettings.sdkRootPath}");

        // Environment overrides (CI usual suspects)
        string[] envs = { "JAVA_HOME", "JDK_HOME", "ANDROID_HOME", "ANDROID_SDK_ROOT", "ANDROID_NDK_ROOT", "ANDROID_NDK_HOME" };
        foreach (var e in envs)
            Debug.Log($"[{stage}] ENV {e}={Environment.GetEnvironmentVariable(e)}");

        // Sanity: a few expected files
        var gradleLauncher = Directory.GetFiles(Path.Combine(playerPkg ?? "", "Tools", "gradle"), "gradle-launcher-*.jar", SearchOption.AllDirectories).FirstOrDefault();
        Debug.Log($"[{stage}] Has gradle launcher? {(!string.IsNullOrEmpty(gradleLauncher))}");
    }
}