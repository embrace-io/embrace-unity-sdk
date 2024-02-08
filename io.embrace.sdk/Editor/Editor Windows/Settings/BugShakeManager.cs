using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.EditorView
{
    [Serializable]
    [OrderedEditorItem("Bug Shake", 4)]
    [ExcludeFromCoverage]
    public class BugShakeManager : BaseSettingsManager
    {
        // TODO: Replace this with a size calculation based on the longest label.
        private const float INDENT_WIDTH = 80f;
        private ScriptingDefineUtil _defineUtil = new ScriptingDefineUtil();
        
        #if UNITY_ANDROID && UNITY_2020_2_OR_NEWER
        // Unfortunately EMBRACE_UNABLE_BUGSHAKE_FORM is insufficient because the user can swap build-targets
        // and the define will persist. This can create non-compiling code in strange situations that blocks the user.
        private ScriptingDefineSettingsItem _enableBugShakeForm = new ScriptingDefineSettingsItem()
        {
            symbol = "EMBRACE_ENABLE_BUGSHAKE_FORM",
            guiContent = new GUIContent(
                "Enable BugShake Form",
                "Toggling this option enables the Embrace Bug Shake form and the bug shake listener."),
            defaultValue = false,
        };
        
        private ScriptingDefineSettingsItem _useBugShakeSceneManagerOverride = new ScriptingDefineSettingsItem()
        {
            symbol = "EMBRACE_USE_BUGSHAKE_SCENE_MANAGER_OVERRIDE",
            guiContent = new GUIContent(
                "Use BugShake Scene Manager Override",
                "Toggling this option enables the Embrace SceneManagerAPI override. THIS REQUIRES THE BUGSHAKE FORM TO BE ENABLED."),
            defaultValue = false,
        };
        #endif
        public override void OnGUI()
        {
            DrawBugShakeWarning();
            DrawBugShakeSettings();
        }

        private void DrawBugShakeWarning()
        {
            EditorGUILayout.HelpBox("Bug shake is only available for users with bug shake designated accounts. The feature will NOT display in the Embrace dashboard for anyone else. " +
                                    "Additionally note that this feature is currently in beta. Please contact Embrace Customer Success if you have any questions.", MessageType.Warning);
        }
        
        private void DrawBugShakeSettings()
        {
            #if UNITY_ANDROID && UNITY_2020_2_OR_NEWER
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            
            EditorGUIUtility.labelWidth = EditorGUIUtility.labelWidth +  INDENT_WIDTH;
            EditorGUILayout.LabelField(new GUIContent("Scripting Define Symbols", EmbraceTooltips.ScriptingDefineSymbols), styleConfigs.boldTextStyle.guiStyle);
            EditorGUILayout.HelpBox("The scripting define symbol below will enable Unity Bug Shake. An Embrace Bug Shake account is required.", MessageType.Info);
            _defineUtil.GUILayoutSetting(_enableBugShakeForm);
            EditorGUILayout.HelpBox("This setting will enable the Embrace SceneManagerAPI override. This setting REQUIRES the previous setting to function.", MessageType.Info);
            _defineUtil.GUILayoutSetting(_useBugShakeSceneManagerOverride);
            
            EditorGUIUtility.labelWidth = originalLabelWidth;
            
            _defineUtil.ApplyModifiedProperties();
            #else
            EditorGUILayout.HelpBox("Unity Bug Shake is only supported on Android and requires Unity 2022.2 or newer.", MessageType.Warning);
            #endif
        }
    }
}