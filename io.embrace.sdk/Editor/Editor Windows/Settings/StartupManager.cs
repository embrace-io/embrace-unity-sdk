using System;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    [Serializable]
    [OrderedEditorItem("Startup", 4)]
    public class StartupManager : BaseSettingsManager
    {
        public const string EMBRACE_STARTUP_SPANS_DEFINE = "EMBRACE_STARTUP_SPANS";
        public const string EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START_DEFINE = "EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START";
        public const string EMBRACE_STARTUP_SPANS_APP_READY_DEFINE = "EMBRACE_STARTUP_SPANS_APP_READY";
        public const string EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE = "EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED";
        public const string EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT_DEFINE = "EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT";
        
        [Serializable]
        public struct SpanFlags
        {
            public bool RecordEmbraceSDKStart;
            public bool RecordAppReady;
            public bool RecordFirstSceneLoaded;
            public bool RecordTimeToInteract;
        }

        private ScriptingDefineUtil _scriptingDefineUtil;
        private SpanFlags _spanFlags;
        
        [Tooltip(EmbraceTooltips.StartupSpanCapture)]
        private bool _enabled;
        
        public override void OnGUI()
        {
            GUILayout.Label("Startup Spans", EditorStyles.boldLabel);
            _enabled = EditorGUILayout.Toggle(new GUIContent("Enable Startup Spans", EmbraceTooltips.StartupSpanCapture), _enabled);
            EditorGUI.BeginDisabledGroup(!_enabled);
            SpanFlags flags = new SpanFlags
            {
                RecordEmbraceSDKStart = EditorGUILayout.Toggle(new GUIContent("Embrace SDK Start", EmbraceTooltips.StartupSpanEmbraceSDKStart), _spanFlags.RecordEmbraceSDKStart),
                RecordAppReady = EditorGUILayout.Toggle(new GUIContent("App Ready", EmbraceTooltips.StartupSpanAppReady), _spanFlags.RecordAppReady),
                RecordFirstSceneLoaded = EditorGUILayout.Toggle(new GUIContent("First Scene Loaded", EmbraceTooltips.StartupSpanFirstSceneLoaded), _spanFlags.RecordFirstSceneLoaded),
                RecordTimeToInteract = EditorGUILayout.Toggle(new GUIContent("Time To Interact", EmbraceTooltips.StartupSpanTimeToInteract), _spanFlags.RecordTimeToInteract)
            };
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            _spanFlags = flags;
            EditorGUILayout.HelpBox("The spans 'AppReady' and 'TimeToInteract' will not be automatically recorded. " +
                                    "You need to call EmbraceStartupSpans.CallAppReady() in your code when the app is ready, " +
                                    "and EmbraceStartupSpans.CallTimeToInteract() when the user can interact with the app.", MessageType.Info);
            
            if(GUILayout.Button("Apply Settings"))
            {
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_DEFINE, _enabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START_DEFINE, _spanFlags.RecordEmbraceSDKStart && _enabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_APP_READY_DEFINE, _spanFlags.RecordAppReady && _enabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE, _spanFlags.RecordFirstSceneLoaded && _enabled);
                _scriptingDefineUtil.ToggleSymbol(EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT_DEFINE, _spanFlags.RecordTimeToInteract && _enabled);
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
                RecordEmbraceSDKStart = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START_DEFINE),
                RecordAppReady = _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_APP_READY_DEFINE),
                RecordFirstSceneLoaded =
                    _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED_DEFINE),
                RecordTimeToInteract =
                    _scriptingDefineUtil.CheckIfSettingIsEnabled(EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT_DEFINE)
            };
        }
    }
}