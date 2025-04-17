using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Holds data used for styling GUI box in editor windows.
    /// ScriptableObject can be created if in DeveloperMode.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/StyleConfig/TextFieldStyleConfig", order = 4)]
#endif
    public class TextFieldStyleConfig : ScriptableObject, IGUIStyleConfig
    {
        [Header("Text Field styles")]
        public Color color;
        public RectOffset border;
        public Color background;
        public Color active;
        public Color hover;
        public Color focused;
        public TextClipping clipping;


        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle _style = new GUIStyle();
                _style.border = border;
                _style.alignment = TextAnchor.MiddleLeft;
                _style.normal.textColor = color;
                _style.active.textColor = color;
                _style.hover.textColor = color;
                _style.focused.textColor = color;
                _style.normal.background = GuiUtil.MakeTexture(2, 2, background);
                _style.active.background = GuiUtil.MakeTexture(2, 2, active);
                _style.hover.background = GuiUtil.MakeTexture(2, 2, hover);
                _style.focused.background = GuiUtil.MakeTexture(2, 2, focused);
                _style.clipping = clipping;

                return _style;
            }
        }
    }
}
