using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Embrace uses this type internally to access utilities for saving SDK related settings.
    /// </summary>
    internal static class EmbraceProjectSettings
    {
        private static readonly JsonSettingsStore _projectSettings;
        private static readonly JsonSettingsStore _userSettings;

        /// <summary>
        /// A settings store for project settings. These settings are stored in a file called .embrace in the root of the project,
        /// and are expected to be checked into version control.
        ///
        /// NOTE: If MockProjectSettings is not null, this property will return that instance instead.
        /// </summary>
        internal static ISettingsStore Project => MockProjectSettings ?? _projectSettings;

        /// <summary>
        /// A settings store for user settings. These settings are stored in a file called DeviceSDKInfo.json in the persistent data
        /// folder, and are not expected to be checked into version control.
        ///
        /// NOTE: If MockUserSettings is not null, this property will return that instance instead.
        /// </summary>
        internal static ISettingsStore User => MockUserSettings ?? _userSettings;

        /// <summary>
        /// Use this property to mock the project settings store. This is useful for testing.
        /// </summary>
        internal static ISettingsStore MockProjectSettings { get; set; }

        /// <summary>
        /// Use this property to mock the user settings store. This is useful for testing.
        /// </summary>
        internal static ISettingsStore MockUserSettings { get; set; }

        static EmbraceProjectSettings()
        {
            // The additionalfile is used here to ensure that the file is included in the compilation context for our source generators.
            string projectSettingsPath = Path.Combine(AssetDatabaseUtil.DefaultDataDirectory, "EmbraceConfig.EmbraceUnitySourceGenerator.additionalfile");
            string userSettingsPath = Path.Combine(Application.persistentDataPath, "DeviceSDKInfo.json");

            _projectSettings = new JsonSettingsStore(projectSettingsPath);
            _userSettings = new JsonSettingsStore(userSettingsPath);

            Application.focusChanged += OnFocusChanged;
            EditorApplication.quitting += OnQuitting;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        private static void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                _projectSettings.Load();
                _userSettings.Load();
            }
            else
            {
                _projectSettings.Save();
                _userSettings.Save();
            }
        }

        private static void OnQuitting()
        {
            _projectSettings.Save();
            _userSettings.Save();
        }

        private static void OnCompilationStarted(object _)
        {
            _projectSettings.Save();
            _userSettings.Save();
        }
    }
}