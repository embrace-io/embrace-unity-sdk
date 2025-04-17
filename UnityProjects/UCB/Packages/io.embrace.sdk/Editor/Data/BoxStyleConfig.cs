using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Holds data used for styling GUI box in editor windows.
    /// ScriptableObject can be created if in DeveloperMode.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/StyleConfig/BoxStyleConfig", order = 3)]
#endif
    public class BoxStyleConfig : ScriptableObject, IGUIStyleConfig
    {
        [Header("box styles")]
        public RectOffset margin;
        public RectOffset padding;
        public Color background;

        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle _style = new GUIStyle();
                _style.margin = margin;
                _style.padding = padding;
                _style.normal.background = GuiUtil.MakeTexture(2, 2, background);

                return _style;
            }
        }
    }
}
