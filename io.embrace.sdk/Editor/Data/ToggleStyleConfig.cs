using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds data used for styling GUI toggle in editor windows.
/// ScriptableObject can be created if in DeveloperMode.
/// </summary>
namespace EmbraceSDK.EditorView
{
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/StyleConfig/ToggleStyleConfig", order = 5)]
#endif
    public class ToggleStyleConfig : ScriptableObject, IGUIStyleConfig
    {
        [Header("Toggle styles")]
        public Color color;
        public Color background;
        public Color active;
        public Color hover;
        public Color focused;
        public RectOffset padding;

        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle _style = new GUIStyle();
                _style.alignment = TextAnchor.MiddleLeft;
                _style.normal.textColor = color;
                _style.active.textColor = color;
                _style.hover.textColor = color;
                _style.focused.textColor = color;
                _style.normal.background = GuiUtil.MakeTexture(2, 2, background);
                _style.active.background = GuiUtil.MakeTexture(2, 2, active);
                _style.hover.background = GuiUtil.MakeTexture(2, 2, hover);
                _style.focused.background = GuiUtil.MakeTexture(2, 2, focused);
                _style.padding = padding;

                return _style;
            }
        }
    }
}