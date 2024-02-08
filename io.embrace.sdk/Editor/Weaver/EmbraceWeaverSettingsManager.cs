using System;
using System.Collections.Generic;
using EmbraceSDK.Networking;
using EmbraceSDK.EditorView;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace EmbraceSDK.Editor.Weaver
{
    /// <summary>
    /// Used in the settings editor window to configure options for network capture weaving.
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Network Capture", 3)]
    [UnityEngine.TestTools.ExcludeFromCoverage]
    public class EmbraceWeaverSettingsManager : BaseSettingsManager
    {
        private ScriptingDefineUtil _defineUtil;

        private bool _didWriteSettings;
        private EmbracePostCompilationProcessor.Settings _weaverSettings;

        private ReorderableList _excludedAssembliesList;
        private Vector2 _scrollPosition;

        /// <summary>
        /// The excluded assemblies list is serialized as a list of strings but displayed as a list of
        /// AssemblyDefinitionAsset references. To avoid having to resolve the assembly definition path and load the
        /// asset each time the list is re-drawn, we cache the references in this dictionary.
        /// </summary>
        private Dictionary<string, AssemblyDefinitionAsset> _asmdefCache = new Dictionary<string, AssemblyDefinitionAsset>();

        private ScriptingDefineSettingsItem _weavingEnabledSetting = new ScriptingDefineSettingsItem()
        {
            symbol = EmbracePostCompilationProcessor.EMBRACE_WEAVER_ENABLED,
            guiContent = new GUIContent("Automatic Network Capture Weaving (BETA)", EmbraceTooltips.AutomatedNetworkCaptureWeaving),
            defaultValue = false,
        };

        private ScriptingDefineSettingsItem _disableWeavingInEditorSetting = new ScriptingDefineSettingsItem()
        {
            symbol = EmbracePostCompilationProcessor.EMBRACE_WEAVER_BUILDS_ONLY,
            guiContent = new GUIContent("Weave Builds Only", EmbraceTooltips.WeaverEditorOnly),
            defaultValue = false,
        };

#if UNITY_2020_1_OR_NEWER
        private ScriptingDefineSettingsItem _captureDataProcessingErrors = new ScriptingDefineSettingsItem()
        {
            symbol = NetworkCapture.EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS,
            guiContent = new GUIContent("Capture Data Processing Errors", EmbraceTooltips.CaptureDataProcessingErrors),
            defaultValue = false,
        };
#endif

        private ScriptingDefineSettingsItem _weaverVerboseLoggingSetting = new ScriptingDefineSettingsItem()
        {
            symbol = EmbracePostCompilationProcessor.EMBRACE_WEAVER_VERBOSE_LOGGING,
            guiContent = new GUIContent("Verbose Logging"),
            defaultValue = false,
        };

        public override void Initialize(MainSettingsEditor mainSettingsEditor)
        {
            base.Initialize(mainSettingsEditor);

            _defineUtil = new ScriptingDefineUtil();

            _weaverSettings = EmbracePostCompilationProcessor.Settings.LoadSettings();

            _excludedAssembliesList = new ReorderableList(_weaverSettings.excludedAssemblyNames, typeof(string), true, true, true, true);
            _excludedAssembliesList.drawHeaderCallback += rect =>
                EditorGUI.LabelField(rect, "Excluded Assemblies", styleConfigs.headerTextStyle.guiStyle);
            _excludedAssembliesList.drawElementCallback += OnDrawExcludedAssemblyListElement;
            _excludedAssembliesList.onAddCallback += l => l.list.Add(string.Empty);
        }

        public override void OnGUI()
        {
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GetLabelWidth();

            EditorGUILayout.BeginVertical(styleConfigs.darkBoxStyle.guiStyle);

            _defineUtil.GUILayoutSetting(_weavingEnabledSetting);

            if (_defineUtil.CheckIfSettingIsEnabled(_weavingEnabledSetting))
            {
                _defineUtil.GUILayoutSetting(_disableWeavingInEditorSetting);

                #if UNITY_2020_1_OR_NEWER
                _defineUtil.GUILayoutSetting(_captureDataProcessingErrors);
                #endif

                _defineUtil.GUILayoutSetting(_weaverVerboseLoggingSetting);

                using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.ExpandHeight(false)))
                {
                    _scrollPosition = scope.scrollPosition;

                    EditorGUI.BeginChangeCheck();

                    _excludedAssembliesList.DoLayoutList();

                    if (EditorGUI.EndChangeCheck())
                    {
                        EmbracePostCompilationProcessor.Settings.SaveSettings(_weaverSettings);
                        _didWriteSettings = true;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                        {
                            _weaverSettings.Reset();
                            EmbracePostCompilationProcessor.Settings.SaveSettings(_weaverSettings);
                            _didWriteSettings = true;
                        }
                        if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false)))
                        {
                            AssetDatabaseUtil.ForceRecompileScripts();
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = originalLabelWidth;

            _defineUtil.ApplyModifiedProperties();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();

            if (_didWriteSettings)
            {
                _didWriteSettings = false;
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Though the excluded assemblies are stored as strings (the assembly name), we'll draw the list as an Object
        /// reference field for an AssemblyDefinitionAsset for ease of use.
        /// </summary>
        private void OnDrawExcludedAssemblyListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            string assemblyName = _excludedAssembliesList.list[index] as string;

            if (!_asmdefCache.TryGetValue(assemblyName, out AssemblyDefinitionAsset asmdef))
            {
                if (!string.IsNullOrWhiteSpace(assemblyName) && CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName) is string asmdefPath)
                {
                    asmdef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);
                    _asmdefCache[assemblyName] = asmdef;
                }
            }

            EditorGUI.BeginChangeCheck();
            asmdef = EditorGUI.ObjectField(rect, GUIContent.none, asmdef, typeof(AssemblyDefinitionAsset), false) as AssemblyDefinitionAsset;
            if (EditorGUI.EndChangeCheck())
            {
                if (asmdef != null)
                {
                    // AssemblyDefinitionAsset derives from TextAsset. Therefore the name property is the name of the
                    // file, not the assembly. We need to parse the JSON content of the file to get the assembly name.
                    _excludedAssembliesList.list[index] = JObject.Parse(asmdef.text)?["name"]?.ToString() ?? string.Empty;
                }
                else
                {
                    _excludedAssembliesList.list[index] = string.Empty;
                }
            }
        }


        private float GetLabelWidth()
        {
            var guiStyle = styleConfigs.defaultToggleStyle.guiStyle;
            return guiStyle.CalcSize(new GUIContent(_weavingEnabledSetting.guiContent)).x;
        }
    }
}