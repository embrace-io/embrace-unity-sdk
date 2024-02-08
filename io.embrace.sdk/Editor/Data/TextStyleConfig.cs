using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Holds data used for styling GUI Text in editor windows.
    /// ScriptableObject can be created if in DeveloperMode.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/StyleConfig/TextStyleConfig", order = 2)]
#endif
    public class TextStyleConfig : ScriptableObject, IGUIStyleConfig
    {
        public TextAnchor alignment;
        public Color color;
        public int fontSize;
        public FontStyle fontStyle;
        public bool wordWrap;

        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle _style = new GUIStyle();
                _style.alignment = alignment;
                _style.normal.textColor = color;
                _style.fontSize = fontSize;
                _style.fontStyle = fontStyle;
                _style.wordWrap = wordWrap;

                return _style;
            }
        }
    }
}
