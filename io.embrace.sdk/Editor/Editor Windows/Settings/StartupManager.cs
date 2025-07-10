using System;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    [Serializable]
    [OrderedEditorItem("Startup", 4)]
    public class StartupManager : BaseSettingsManager
    {
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
        private bool _enabled;
        
        public override void OnGUI()
        {
            GUILayout.Label("Startup Spans", EditorStyles.boldLabel);
            _enabled = EditorGUILayout.Toggle("Enable Startup Spans", _enabled);
            EditorGUI.BeginDisabledGroup(!_enabled);
            // create a checkbox for each flag
            SpanFlags flags = new SpanFlags
            {
                RecordEmbraceSDKStart = EditorGUILayout.Toggle("Embrace SDK Start", _spanFlags.RecordEmbraceSDKStart),
                RecordAppReady = EditorGUILayout.Toggle("App Ready", _spanFlags.RecordAppReady),
                RecordFirstSceneLoaded = EditorGUILayout.Toggle("First Scene Loaded", _spanFlags.RecordFirstSceneLoaded),
                RecordTimeToInteract = EditorGUILayout.Toggle("Time to Interact", _spanFlags.RecordTimeToInteract)
            };
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            _spanFlags = flags;
            
            if(GUILayout.Button("Apply Settings"))
            {
                _scriptingDefineUtil.ToggleSymbol("EMBRACE_STARTUP_SPANS", _enabled);
                _scriptingDefineUtil.ToggleSymbol("EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START", _spanFlags.RecordEmbraceSDKStart && _enabled);
                _scriptingDefineUtil.ToggleSymbol("EMBRACE_STARTUP_SPANS_APP_READY", _spanFlags.RecordAppReady && _enabled);
                _scriptingDefineUtil.ToggleSymbol("EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED", _spanFlags.RecordFirstSceneLoaded && _enabled);
                _scriptingDefineUtil.ToggleSymbol("EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT", _spanFlags.RecordTimeToInteract && _enabled);
                _scriptingDefineUtil.ApplyModifiedProperties();
            }
        }

        public override void Initialize(MainSettingsEditor _)
        {
            base.Initialize(mainSettingsEditor);
            _scriptingDefineUtil = new ScriptingDefineUtil();
            _enabled = _scriptingDefineUtil.CheckIfSettingIsEnabled("EMBRACE_STARTUP_SPANS");
            _spanFlags = new SpanFlags
            {
                RecordEmbraceSDKStart = _scriptingDefineUtil.CheckIfSettingIsEnabled("EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START"),
                RecordAppReady = _scriptingDefineUtil.CheckIfSettingIsEnabled("EMBRACE_STARTUP_SPANS_APP_READY"),
                RecordFirstSceneLoaded =
                    _scriptingDefineUtil.CheckIfSettingIsEnabled("EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED"),
                RecordTimeToInteract =
                    _scriptingDefineUtil.CheckIfSettingIsEnabled("EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT")
            };
        }
    }
}