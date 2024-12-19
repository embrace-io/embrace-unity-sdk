using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.Internal.Editor
{
    /// <summary>
    /// Internal editor utility for launching Android emulators from within the Unity editor
    /// </summary>
    public class AndroidEmulatorUtility : EditorWindow
    {
        private List<string> _emulatorNames = new List<string>();
        private Vector2 scrollPosition = Vector2.zero;

        private const string PREFS_KEY_PREFIX = nameof(AndroidEmulatorUtility) + "_";

        private const string EMULATOR_PATH_KEY = PREFS_KEY_PREFIX + nameof(EmulatorPath);
        private static string EmulatorPath
        {
            get => EditorPrefs.GetString(EMULATOR_PATH_KEY);
            set => EditorPrefs.SetString(EMULATOR_PATH_KEY, value);
        }

        private const string DEFAULT_EMULATOR_NAME_KEY = PREFS_KEY_PREFIX + nameof(DefaultEmulatorName);
        private static string DefaultEmulatorName
        {
            get => EditorPrefs.GetString(DEFAULT_EMULATOR_NAME_KEY);
            set => EditorPrefs.SetString(DEFAULT_EMULATOR_NAME_KEY, value);
        }

        private const string ADDITIONAL_ARGS_KEY = PREFS_KEY_PREFIX + nameof(AdditionalEmulatorArgs);

        private static string AdditionalEmulatorArgs
        {
            get => EditorPrefs.GetString(ADDITIONAL_ARGS_KEY);
            set => EditorPrefs.SetString(ADDITIONAL_ARGS_KEY, value);
        }

        [MenuItem("Tools/Android Emulator Utility/Settings", false, 100)]
        private static void OpenWindow()
        {
            AndroidEmulatorUtility window = GetWindow<AndroidEmulatorUtility>();
            window.minSize = new Vector2(400, 150);
            window.name = "Android Emulator Utility Settings";
            window.titleContent = new GUIContent("Android Emulator Utility");
            window.Init();
            window.Show();
        }

        private void OnFocus()
        {
            Init();
        }

        private void Init()
        {
            string path = EmulatorPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = GetDefaultEmulatorPath();
                EmulatorPath = path;
            }

            if (File.Exists(path))
            {
                GetAvailableEmulators(path, _emulatorNames);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUI.BeginChangeCheck();
            string path = EditorGUILayout.DelayedTextField("Emulator Path", EmulatorPath);
            if (EditorGUI.EndChangeCheck())
            {
                EmulatorPath = path;
                Init();
            }

            if (GUILayout.Button("Find", GUILayout.Width(100)))
            {
                path = EditorUtility.OpenFilePanel("Android Emulator", path, "");
                EmulatorPath = path;
                Init();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            string newArgs = EditorGUILayout.DelayedTextField("Additional Args", AdditionalEmulatorArgs);
            if (EditorGUI.EndChangeCheck())
            {
                AdditionalEmulatorArgs = newArgs;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Available Emulators");

            using (var scope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scope.scrollPosition;

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    EditorGUILayout.HelpBox("The specified emulator path is not valid.", MessageType.Error);
                }

                string defaultName = DefaultEmulatorName;
                for (int i = 0; i < _emulatorNames.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (EditorGUILayout.ToggleLeft(_emulatorNames[i], _emulatorNames[i].Equals(defaultName)))
                    {
                        defaultName = _emulatorNames[i];
                        DefaultEmulatorName = defaultName;
                    }

                    if (GUILayout.Button("Launch"))
                    {
                        LaunchEmulator(path, _emulatorNames[i], newArgs);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Reset"))
            {
                EditorPrefs.DeleteKey(EMULATOR_PATH_KEY);
                EditorPrefs.DeleteKey(DEFAULT_EMULATOR_NAME_KEY);
                EditorPrefs.DeleteKey(ADDITIONAL_ARGS_KEY);
                _emulatorNames.Clear();
                Init();
            }
        }

        private static void GetAvailableEmulators(string emulatorPath, List<string> namesBuffer)
        {
            namesBuffer.Clear();

            Process emulator = new Process();
            emulator.StartInfo.FileName = emulatorPath;
            emulator.StartInfo.Arguments = "-list-avds";
            emulator.StartInfo.UseShellExecute = false;
            emulator.StartInfo.RedirectStandardOutput = true;
            emulator.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data) && !namesBuffer.Contains(args.Data))
                {
                    namesBuffer.Add(args.Data);
                }
            };
            emulator.Start();
            emulator.BeginOutputReadLine();
        }

        private static void LaunchEmulator(string emulatorPath, string emulatorName, string extraArgs)
        {
            Process emulator = new Process();
            emulator.StartInfo.FileName = emulatorPath;
            emulator.StartInfo.Arguments = $"-avd {emulatorName} {extraArgs}";
            emulator.Start();
        }

        [MenuItem("Tools/Android Emulator Utility/Launch Default Emulator", false, 1)]
        private static void LaunchDefaultEmulator()
        {
            LaunchEmulator(EmulatorPath, DefaultEmulatorName, AdditionalEmulatorArgs);
        }

        [MenuItem("Tools/Android Emulator Utility/Launch Default Emulator", true, 1)]
        private static bool LaunchDefaultEmulator_Validate()
        {
            string path = EmulatorPath;
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path) &&
                   !string.IsNullOrWhiteSpace(DefaultEmulatorName);
        }

        private static string GetDefaultEmulatorPath()
        {
            #if UNITY_EDITOR_OSX
            string homeDirectory = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrWhiteSpace(homeDirectory))
            {
                return null;
            }
            return Path.Combine(homeDirectory, "Library/Android/sdk/emulator/emulator");
            #elif UNITY_EDITOR_WIN
            string homeDirectory = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            if (string.IsNullOrWhiteSpace(homeDirectory))
            {
                return null;
            }

            return Path.Combine(homeDirectory, "Library/Android/sdk/emulator/emulator.exe");
            #else
            return null;
            #endif
        }
    }
}