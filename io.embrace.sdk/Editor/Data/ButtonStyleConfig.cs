using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Holds data used for styling GUI buttons in editor windows.
    /// ScriptableObject can be created if in DeveloperMode.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/StyleConfig/ButtonStyleConfig", order = 6)]
#endif
    public class ButtonStyleConfig : ScriptableObject, IGUIStyleConfig
    {
        [Header("button styles")]
        public float fixedWidth;

        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle _style = new GUIStyle(GUI.skin.button);
                _style.fixedWidth = fixedWidth;

                return _style;
            }
        }
    }
}