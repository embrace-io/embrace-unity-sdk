using System;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// The EmbraceStartupManager contains the settings for managing startup spans in the Embrace SDK.
    /// Note: Please do not use this class directly in your code. It is intended for use within the Embrace SDK editor settings.
    /// </summary>
    [Serializable]
    [OrderedEditorItem("Spans", 4)]
    internal class EmbraceSpansManager : BaseSettingsManager
    {
        public const string EMBRACE_STARTUP_SPANS_DEFINE = "EMBRACE_STARTUP_SPANS";
        public const string EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE = "EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED";
        public const string EMBRACE_STARTUP_SPANS_LOADING_COMPLETE_DEFINE = "EMBRACE_STARTUP_SPANS_LOADING_COMPLETE";
        
        [Serializable]
        public struct SpanFlags
        {
            public bool RecordFirstSceneLoaded;
            public bool RecordLoadingComplete;
        }

        private ScriptingDefineUtil _scriptingDefineUtil;
        private SpanFlags _spanFlags;
        
        [Tooltip(EmbraceTooltips.StartupSpanCapture)]
        private bool _enabled;
        
        public override void OnGUI()
        {
            DrawStartupSpans();
            EditorGUILayout.Space();
        }

        private void DrawStartupSpans()
        {
            GUILayout.Label("Startup Spans", EditorStyles.boldLabel);
            _enabled = EditorGUILayout.Toggle(new GUIContent("emb-app-time-to-interact", EmbraceTooltips.StartupSpanCapture), _enabled);
            EditorGUI.BeginDisabledGroup(!_enabled);
            SpanFlags flags = new SpanFlags
            {
                RecordFirstSceneLoaded = EditorGUILayout.Toggle(new GUIContent("emb-app-loaded", EmbraceTooltips.StartupSpanFirstSceneLoaded), _spanFlags.RecordFirstSceneLoaded),
                RecordLoadingComplete = EditorGUILayout.Toggle(new GUIContent("emb-app-init", EmbraceTooltips.StartupSpanTimeToInteract), _spanFlags.RecordLoadingComplete)
            };
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            _spanFlags = flags;
            EditorGUILayout.HelpBox("When your app is finished loading and your user is ready to start interacting with your app you will need to call Embrace.Instance.EndAppStartup() in your code.", MessageType.Info);
            
            if(GUILayout.Button("Apply Settings"))
            {
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_DEFINE, _enabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE, _spanFlags.RecordFirstSceneLoaded && _enabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_LOADING_COMPLETE_DEFINE, _spanFlags.RecordLoadingComplete && _enabled);
                _scriptingDefineUtil.ApplyModifiedProperties();
            }
        }

        public override void Initialize(MainSettingsEditor _)
        {
            base.Initialize(mainSettingsEditor);
            _scriptingDefineUtil = new ScriptingDefineUtil();
            _enabled = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_DEFINE);
            _spanFlags = new SpanFlags
            {
                RecordFirstSceneLoaded = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE),
                RecordLoadingComplete = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_LOADING_COMPLETE_DEFINE)
            };
        }
    }
}