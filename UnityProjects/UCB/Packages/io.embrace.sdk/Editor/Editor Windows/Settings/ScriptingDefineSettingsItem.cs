using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Defines a setting which applies a scripting define symbol when enabled.
    /// </summary>
    public class ScriptingDefineSettingsItem
    {
        /// <summary>
        /// The scripting define symbol to apply when this setting is enabled.
        /// </summary>
        public string symbol;

        /// <summary>
        /// The GUIContent displayed in the editor for this setting.
        /// </summary>
        public GUIContent guiContent;

        /// <summary>
        /// Whether the symbol should be enabled by default.
        /// </summary>
        public bool defaultValue;
    }
}