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
        private static ScriptingDefineUtil _defineUtil = new ScriptingDefineUtil();
        
        public override void OnGUI()
        {
            DrawBugShakeWarning();
            DrawBugShakeSettingsTurnedOff();
        }

        private void DrawBugShakeWarning()
        {
            EditorGUILayout.HelpBox("The Embrace Bug Shake Beta has ended and the feature has been sunset. " +
                                    "Any and all bug-shake functionality is no longer supported. " +
                                    "Please contact Embrace Customer Success if you have any questions.",
                MessageType.Warning);
        }

        private static void DrawBugShakeSettingsTurnedOff()
        {
            _defineUtil.ToggleSymbol("EMBRACE_ENABLE_BUGSHAKE_FORM", false);
            _defineUtil.ToggleSymbol("EMBRACE_USE_BUGSHAKE_SCENE_MANAGER_OVERRIDE", false);
            _defineUtil.ApplyModifiedProperties();
        }
    }
}