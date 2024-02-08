#if UNITY_2020
using System.IO;
using UnityEngine;
using UnityEditor;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// If you enable Export Project in the Android Build Settings, Unity (2020) doesn�t generate a symbols package zip. 
    /// The SymbolsUtil class copies the libmain and libunity symbol files so that our Android SDK can complete the symbols package.
    /// For more information see Unity docs: https://docs.unity3d.com/2020.3/Documentation/Manual/android-symbols.html
    /// </summary>
    public class SymbolsUtil
    {
        private static string arm64_v8a = "arm64-v8a";
        private static string armeabi_v7a = "armeabi-v7a";

        /// <summary>
        /// Completes the symbols package by moving the libmain and libunity files into the unityLibrary/symbols/<architecture>/ directory.
        /// </summary>
        /// <param name="projectPath">The path to the Unity library in the folder specified for export.</param>
        public static void CopySymbolFiles(string projectPath)
        {
            AndroidBuildType buildType = EditorUserBuildSettings.androidBuildType;
            ScriptingImplementation scriptingImplementation = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            string scriptingBackend = scriptingImplementation.ToString().ToLower();
            if(scriptingImplementation == ScriptingImplementation.Mono2x)
            {
                scriptingBackend = scriptingImplementation.ToString().ToLower().Replace("2x", "");
            }

            string appPath = EditorApplication.applicationContentsPath;
    #if UNITY_EDITOR_OSX
            // EditorApplication.applicationContentsPath returns the wrong path on mac, 
            // to ensure that we get the correct path we must remove "Unity.app".
            appPath = EditorApplication.applicationPath.Replace("Unity.app","");
    #endif

            string editorPath = $"{appPath}/PlaybackEngines/AndroidPlayer/Variations/{scriptingBackend}/{buildType}/Symbols";
            string dataPath = Application.dataPath.Replace("/Assets", "");
            bool hasArm64 = PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARM64);
            bool hasARMv7 = PlayerSettings.Android.targetArchitectures.HasFlag(AndroidArchitecture.ARMv7);

            string libunityArm64 = "";
            string libunityARMv7 = "";

            string libmainArm64 = "";
            string libmainARMv7 = "";


            if (PlayerSettings.stripEngineCode) //Strip Engine Code enabled:
            {
                if (hasArm64)
                {
                    libunityArm64 = $"{dataPath}/Temp/StagingArea/symbols/{arm64_v8a}";
                    libmainArm64 = $"{editorPath}/{arm64_v8a}";
                }

                if (hasARMv7)
                {
                    string symbolPath = "symbols";
                    if (scriptingImplementation == ScriptingImplementation.Mono2x)
                    {
                        symbolPath = "libs";
                    }
                    libunityARMv7 = $"{dataPath}/Temp/StagingArea/{symbolPath}/{armeabi_v7a}";
                    libmainARMv7 = $"{editorPath}/{armeabi_v7a}";
                }
            }
            else // Strip Engine Code disabled:
            {
                if (hasArm64)
                {
                    libunityArm64 = libmainArm64 = $"{editorPath}/{arm64_v8a}";
                }

                if (hasARMv7)
                {
                    libunityARMv7 = libmainARMv7 = $"{editorPath}/{armeabi_v7a}";
                }
            }

            if (!Directory.Exists($"{projectPath}/unityLibrary/symbols"))
            {
                Directory.CreateDirectory($"{projectPath}/unityLibrary/symbols");
            }

            if (hasArm64 && (libunityArm64 != "" || libunityARMv7 != ""))
            {
                string symbolsArm64Folder = $"{projectPath}/unityLibrary/symbols/{arm64_v8a}";
                if (!Directory.Exists(symbolsArm64Folder))
                {
                    Directory.CreateDirectory(symbolsArm64Folder);
                }

                if (libunityArm64 != "")
                {
                    MoveSymbolFiles(symbolsArm64Folder, libunityArm64, "libunity");
                }
                if (libmainArm64 != "")
                {
                    MoveSymbolFiles(symbolsArm64Folder, libmainArm64, "libmain");
                }
            }

            if (hasARMv7 && (libmainArm64 != "" || libmainARMv7 != ""))
            {
                string symbolsARMv7Folder = $"{projectPath}/unityLibrary/symbols/{armeabi_v7a}";
                if (!Directory.Exists(symbolsARMv7Folder))
                {
                    Directory.CreateDirectory(symbolsARMv7Folder);
                }

                if (libunityARMv7 != "")
                {
                    MoveSymbolFiles(symbolsARMv7Folder, libunityARMv7, "libunity");
                }
                if (libmainARMv7 != "")
                {
                    MoveSymbolFiles(symbolsARMv7Folder, libmainARMv7, "libmain");
                }
            }
        }

        private static void MoveSymbolFiles(string newSymbolsFolder, string oldSymbolsPath, string symbolFile)
        {
            if (File.Exists($"{oldSymbolsPath}/{symbolFile}.sym.so"))
            {
                FileUtil.CopyFileOrDirectory($"{oldSymbolsPath}/{symbolFile}.sym.so", $"{newSymbolsFolder}/{symbolFile}.so");
            }
            else if (File.Exists($"{oldSymbolsPath}/{symbolFile}.dbg.so"))
            {
                FileUtil.CopyFileOrDirectory($"{oldSymbolsPath}/{symbolFile}.dbg.so", $"{newSymbolsFolder}/{symbolFile}.so");
            }
            else
            {
                FileUtil.CopyFileOrDirectory($"{oldSymbolsPath}/{symbolFile}.so", $"{newSymbolsFolder}/{symbolFile}.so");
            }
        }
    }
}
#endif
