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
        public const string EMBRACE_SCENE_LOAD_SPANS_DEFINE = "EMBRACE_SCENE_LOAD_SPANS";
        
        [Serializable]
        public struct SpanFlags
        {
            public bool RecordFirstSceneLoaded;
            public bool RecordLoadingComplete;
        }

        private ScriptingDefineUtil _scriptingDefineUtil;
        private SpanFlags _spanFlags;
        
        [Tooltip(EmbraceTooltips.StartupSpanCapture)]
        private bool _startupSpansCaptureEnabled;
        private bool _sceneLoadSpansEnabled;
        
        public override void OnGUI()
        {
            DrawStartupSpans();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawSceneLoadSpans();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if(GUILayout.Button("Apply Settings"))
            {
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_DEFINE, _startupSpansCaptureEnabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE, _spanFlags.RecordFirstSceneLoaded && _startupSpansCaptureEnabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_LOADING_COMPLETE_DEFINE, _spanFlags.RecordLoadingComplete && _startupSpansCaptureEnabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_SCENE_LOAD_SPANS_DEFINE, _sceneLoadSpansEnabled);
                _scriptingDefineUtil.ApplyModifiedProperties();
            }
        }

        private void DrawStartupSpans()
        {
            GUILayout.Label("Startup Spans", EditorStyles.boldLabel);
            _startupSpansCaptureEnabled = EditorGUILayout.Toggle(new GUIContent("emb-app-time-to-interact", EmbraceTooltips.StartupSpanCapture), _startupSpansCaptureEnabled);
            EditorGUI.BeginDisabledGroup(!_startupSpansCaptureEnabled);
            SpanFlags flags = new SpanFlags
            {
                RecordFirstSceneLoaded = EditorGUILayout.Toggle(new GUIContent("emb-app-loaded", EmbraceTooltips.StartupSpanFirstSceneLoaded), _spanFlags.RecordFirstSceneLoaded),
                RecordLoadingComplete = EditorGUILayout.Toggle(new GUIContent("emb-app-init", EmbraceTooltips.StartupSpanTimeToInteract), _spanFlags.RecordLoadingComplete)
            };
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            _spanFlags = flags;
            EditorGUILayout.HelpBox("When your app is finished loading and your user is ready to start interacting with your app you will need to call Embrace.Instance.EndAppStartup() in your code.", MessageType.Info);
        }
        
        private void DrawSceneLoadSpans()
        {
            GUILayout.Label("Scene Load Spans", EditorStyles.boldLabel);
            _sceneLoadSpansEnabled = EditorGUILayout.Toggle(new GUIContent("Enable Scene Load Spans", EmbraceTooltips.SceneLoadSpans), _sceneLoadSpansEnabled);
            EditorGUILayout.HelpBox("The Embrace SDK can automatically measure scene load times for you. This is done by overriding Unity's SceneManagerAPI. If you are already using a custom SceneManagerAPI override, this will not work or may conflict.", MessageType.Info);
        }

        public override void Initialize(MainSettingsEditor _)
        {
            base.Initialize(mainSettingsEditor);
            _scriptingDefineUtil = new ScriptingDefineUtil();
            _startupSpansCaptureEnabled = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_DEFINE);
            _sceneLoadSpansEnabled = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_SCENE_LOAD_SPANS_DEFINE);
            _spanFlags = new SpanFlags
            {
                RecordFirstSceneLoaded = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE),
                RecordLoadingComplete = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_LOADING_COMPLETE_DEFINE)
            };
        }
    }
}