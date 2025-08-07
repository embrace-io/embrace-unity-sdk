using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// General Manager provides users with general settings and controls for the Embrace SDK and its editor windows.
    /// </summary>
    [Serializable]
    [OrderedEditorItem("General", 1)]
    internal class GeneralManager : BaseSettingsManager
    {
        private bool _advancedFoldoutExpanded;
        private string _savedDataDir;
        private ScriptingDefineUtil _defineUtil;

        private const float INDENT_WIDTH = 15f;

        private ScriptingDefineSettingsItem _developerModeSetting = new ScriptingDefineSettingsItem()
        {
            symbol = "DeveloperMode",
            guiContent = new GUIContent("Developer Mode",
                "Toggling Developer Mode on provides options that give you more control over the SDK and extra tools that can assist with debugging the SDK."),
            defaultValue = false,
        };

        private ScriptingDefineSettingsItem _multiThreadedLogging = new ScriptingDefineSettingsItem()
        {
            symbol = EmbraceLogger.EMBRACE_USE_THREADING,
            guiContent = new GUIContent(
                "Capture Multi-Threaded Log Exceptions",
                "Toggling this option allows capturing of exceptions that can be thrown from threads other than the main thread."),
            defaultValue = false
        };

        private ScriptingDefineSettingsItem _autoCaptureActiveSceneAsViewSetting = new ScriptingDefineSettingsItem()
        {
            symbol = "EMBRACE_AUTO_CAPTURE_ACTIVE_SCENE_AS_VIEW",
            guiContent = new GUIContent(
                "Automatically Capture Active Scene as View (BETA)",
                "Toggling this option allows for the automatic capturing of active scene changes as views in the dashboard."),
            defaultValue = false, // We might want this to be opt-out in the future -Aly
        };
        
        private ScriptingDefineSettingsItem _autoInstrumentationFPSCapture = new ScriptingDefineSettingsItem()
        {
            symbol = "EMBRACE_AUTO_INSTRUMENTATION_FPS_CAPTURE",
            guiContent = new GUIContent(
                "Auto Instrumentation FPS Capture",
                "Toggling this option allows for automatic instrumentation of FPS capture in the dashboard."),
            defaultValue = true,
        };

        private string[] _editorSilenceLogSettingsNames;
        
        private GUIContent _editorSilenceLogSettingsContent = new GUIContent("Editor Silence Settings", "Use this to silence log settings for the editor.");
        
        // These behave slightly differently. They're more a Scripting Define Group
        private ScriptingDefineSettingsItem[] _editorSilenceLogSettings = new ScriptingDefineSettingsItem[]
        {
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_EDITOR_TYPE_LOG,
                guiContent = new GUIContent("Logs"),
                defaultValue = false,
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_EDITOR_TYPE_WARNING,
                guiContent = new GUIContent("Warnings"),
                defaultValue = false,
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_EDITOR_TYPE_ERROR,
                guiContent = new GUIContent("Errors"),
                defaultValue = false,
            },
        };
        
        private string[] _devSilenceLogSettingsNames;
        
        private GUIContent _devSilenceLogSettingsContent = new GUIContent("Development Silence Settings", "Use this to silence log settings for development builds.");
        
        // These behave slightly differently. They're more a Scripting Define Group
        private ScriptingDefineSettingsItem[] _devSilenceLogSettings = new ScriptingDefineSettingsItem[]
        {
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_DEV_TYPE_LOG,
                guiContent = new GUIContent("Logs"),
                defaultValue = false,
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_DEV_TYPE_WARNING,
                guiContent = new GUIContent("Warnings"),
                defaultValue = false,
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_DEV_TYPE_ERROR,
                guiContent = new GUIContent("Errors"),
                defaultValue = false,
            },
        };
        
        private string[] _releaseSilenceLogSettingsNames;
        
        private GUIContent _releaseSilenceLogSettingsContent = new GUIContent("Release Silence Settings", "Use this to silence log settings for release builds.");
        
        // These behave slightly differently. They're more a Scripting Define Group
        private ScriptingDefineSettingsItem[] _releaseSilenceLogSettings = new ScriptingDefineSettingsItem[]
        {
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_RELEASE_TYPE_LOG,
                guiContent = new GUIContent("Logs"),
                defaultValue = false,
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_RELEASE_TYPE_WARNING,
                guiContent = new GUIContent("Warnings"),
                defaultValue = false,
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceLogger.EMBRACE_SILENCE_RELEASE_TYPE_ERROR,
                guiContent = new GUIContent("Errors"),
                defaultValue = false,
            },
        };

        private ScriptingDefineSettingsItem[] _advancedSettings = new ScriptingDefineSettingsItem[]
        {
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceGradleUtility.EMBRACE_DISABLE_SWAZZLER_VERSION_UPDATE,
                guiContent = new GUIContent("Disable Automatic Android Swazzler Version Update",
                    "Prevents the Embrace Unity SDK from automatically updating the version of embrace-swazzler defined in the project's baseProjectTemplate.gradle."),
                defaultValue = false
            },
        };

        private GUIContent[] _edmUsageSettingsGuiContent;

        private GUIContent _edmUsageSettingsLabelGuiContent = new GUIContent("Use External Dependency Manager",
            "The External Dependency Manager for Unity (usually called Android Resolver) can conflict with dependency injection performed by " +
            "the Embrace Swazzler. Use this setting to tell the Embrace Swazzler to disable dependency injection if " +
            "the EDM is included in your project.");

        private ScriptingDefineSettingsItem[] _edmUsageSettings = new ScriptingDefineSettingsItem[]
        {
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_TRUE,
                guiContent = new GUIContent("True", "Embrace Swazzler dependency injection will be disabled."),
                defaultValue = false
            },
            new ScriptingDefineSettingsItem()
            {
                symbol = EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_FALSE,
                guiContent = new GUIContent("False", "Embrace Swazzler dependency injection will be enabled."),
                defaultValue = false
            }
        };

        public override void Initialize(MainSettingsEditor mainPreferenceEditor)
        {
            base.Initialize(mainPreferenceEditor);

            _defineUtil = new ScriptingDefineUtil();
            _edmUsageSettingsGuiContent = _edmUsageSettings.Select(x => x.guiContent)
                .Append(new GUIContent("Auto", "Embrace will automatically disable the Swazzler dependency injection if the EDM is detected."))
                .ToArray();
            _editorSilenceLogSettingsNames = _defineUtil.GetFlagNamesForSettingsItems(_editorSilenceLogSettings);
            _devSilenceLogSettingsNames = _defineUtil.GetFlagNamesForSettingsItems(_devSilenceLogSettings);
            _releaseSilenceLogSettingsNames = _defineUtil.GetFlagNamesForSettingsItems(_releaseSilenceLogSettings);
        }

        public override void OnFocus()
        {
            _savedDataDir = AssetDatabaseUtil.EmbraceDataDirectory;
        }

        public override void OnGUI()
        {
            GUILayout.Space(styleConfigs.space);

            DrawScriptingSymbolsOptions();

            GUILayout.Space(styleConfigs.space);

            DrawEmbraceDirectoryField();

            GUILayout.Space(styleConfigs.space);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset To Default"))
            {
                ResetSDK();
            }
        }

        private void DrawScriptingSymbolsOptions()
        {
            // Accommodates longest label in settings
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GetLabelWidth() + INDENT_WIDTH; // Add INDENT_WIDTH to account for advanced settings indent

            EditorGUILayout.LabelField(new GUIContent("Scripting Define Symbols", EmbraceTooltips.ScriptingDefineSymbols), styleConfigs.boldTextStyle.guiStyle);
            GUILayout.BeginVertical(styleConfigs.dividerBoxStyle.guiStyle);

            // Main Settings
            _defineUtil.GUILayoutSetting(_developerModeSetting);
            _defineUtil.GUILayoutSetting(_multiThreadedLogging);
            _defineUtil.GUILayoutSetting(_autoCaptureActiveSceneAsViewSetting);

            _defineUtil.GUILayoutSettingsAsFlags(_editorSilenceLogSettingsContent, 
                _editorSilenceLogSettings, _editorSilenceLogSettingsNames);
            _defineUtil.GUILayoutSettingsAsFlags(_devSilenceLogSettingsContent, 
                _devSilenceLogSettings, _devSilenceLogSettingsNames);
            _defineUtil.GUILayoutSettingsAsFlags(_releaseSilenceLogSettingsContent, 
                _releaseSilenceLogSettings, _releaseSilenceLogSettingsNames);
            _defineUtil.GUILayoutSetting(_autoInstrumentationFPSCapture);

            // Advanced Settings
            _advancedFoldoutExpanded = EditorGUILayout.Foldout(_advancedFoldoutExpanded, "Advanced");
            if (_advancedFoldoutExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < _advancedSettings.Length; ++i)
                {
                    _defineUtil.GUILayoutSetting(_advancedSettings[i]);
                }

                _defineUtil.GUILayoutSettingsAsSelectionList(_edmUsageSettingsLabelGuiContent, _edmUsageSettings, _edmUsageSettingsGuiContent);
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();

            _defineUtil.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        private void DrawEmbraceDirectoryField()
        {
            EditorGUILayout.LabelField(new GUIContent("Embrace Data Directory", EmbraceTooltips.DataDir), styleConfigs.boldTextStyle.guiStyle);

            _savedDataDir = EditorGUILayout.TextField(_savedDataDir, styleConfigs.defaultTextFieldStyle.guiStyle);

            if (_savedDataDir != AssetDatabaseUtil.EmbraceDataDirectory)
            {
                if (GUILayout.Button(new GUIContent("Set New Data Directory", EmbraceTooltips.DataDirButton)))
                {
                    if (!Validator.ValidateConfigsFolderPath(_savedDataDir))
                    {
                        _savedDataDir = $"Assets/{_savedDataDir}";
                    }
                    DataDirectoryWarningWindow.Init(OnUpdateDataDirectory);
                }
            }
        }

        private void OnUpdateDataDirectory()
        {
            var previousDir = AssetDatabaseUtil.EmbraceDataDirectory;
            AssetDatabaseUtil.EmbraceDataDirectory = _savedDataDir;
            ConfigsRelocationUtil.RelocateAssets(previousDir, _savedDataDir, LoadConfigurations);
        }

        private void ResetSDK()
        {
            var assetPaths = AssetDatabaseUtil.GetAssetPaths<EmbraceConfiguration>(AssetDatabaseUtil.EmbraceDataDirectory);
            foreach (var path in assetPaths)
            {
                if (!path.Contains("DefaultAndroidConfiguration") && !path.Contains("DefaultIOSConfiguration"))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }

            environments.Clear();

            AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>().SetDefaults();
            AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>().SetDefaults();

            _defineUtil.ApplyDefault(_developerModeSetting);
            _defineUtil.ApplyDefault(_multiThreadedLogging);
            _defineUtil.ApplyDefault(_autoCaptureActiveSceneAsViewSetting);
            _editorSilenceLogSettings.ToList().ForEach(_defineUtil.ApplyDefault);
            _devSilenceLogSettings.ToList().ForEach(_defineUtil.ApplyDefault);
            _releaseSilenceLogSettings.ToList().ForEach(_defineUtil.ApplyDefault);

            for (int i = 0; i < _advancedSettings.Length; ++i)
            {
                _defineUtil.ApplyDefault(_advancedSettings[i]);
            }

            for (int i = 0; i < _edmUsageSettings.Length; ++i)
            {
                _defineUtil.ApplyDefault(_edmUsageSettings[i]);
            }

            _defineUtil.ApplyModifiedProperties();
        }

        private float GetLabelWidth()
        {
            var settingsItems = new List<ScriptingDefineSettingsItem>()
            {
                _developerModeSetting,
                _multiThreadedLogging
            };

            settingsItems.AddRange(_advancedSettings);

            var guiStyle = styleConfigs.defaultToggleStyle.guiStyle;
            var longestLabelWidth = float.MinValue;
            foreach (var item in settingsItems)
            {
                var labelWidth = guiStyle.CalcSize(new GUIContent(item.guiContent.text)).x;
                if (labelWidth > longestLabelWidth)
                {
                    longestLabelWidth = labelWidth;
                }
            }

            settingsItems.Clear();

            return longestLabelWidth;
        }
    }
}