using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Collection of style configs used by the editor windows.
    /// ScriptableObject can be created if in DeveloperMode.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/StyleConfig/StyleConfigs", order = 1)]
#endif
    public class StyleConfigs : ScriptableObject
    {
        [Header("Font styles")]
        public TextStyleConfig headerTextStyle;
        public TextStyleConfig labelHeaderStyle;
        public TextStyleConfig labelTitleStyle;
        public TextStyleConfig welcomeStyle;
        public TextStyleConfig defaultTextStyle;
        public TextStyleConfig boldTextStyle;
        public TextStyleConfig alertTextStyle;

        [Header("Box styles")]
        public BoxStyleConfig lightBoxStyle;
        public BoxStyleConfig darkBoxStyle;
        public BoxStyleConfig clearBoxStyle;
        public BoxStyleConfig announcementBox;
        public BoxStyleConfig alertBox;
        public BoxStyleConfig dividerBoxStyle;
        public BoxStyleConfig highlightBoxStyle;
        public BoxStyleConfig warningBoxStyle;

        [Header("Button styles")]
        public ButtonStyleConfig defaultButtonStyle;

        [Header("Overall style")]
        public int space;
        public TextFieldStyleConfig defaultTextFieldStyle;
        public ToggleStyleConfig defaultToggleStyle;
    }
}
