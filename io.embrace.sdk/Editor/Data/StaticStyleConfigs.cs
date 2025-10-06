using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Static style configurations to replace ScriptableObject-based configurations.
    /// </summary>
    public static class StaticStyleConfigs
    {
        // Text styles
        public static readonly StaticTextStyleConfig HeaderTextStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.UpperCenter,
            color = new Color(0.8f, 0.8f, 0.8f, 1f),
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };

        public static readonly StaticTextStyleConfig LabelHeaderStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.MiddleLeft,
            color = new Color(0.9f, 0.9f, 0.9f, 1f),
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };

        public static readonly StaticTextStyleConfig LabelTitleStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.MiddleLeft,
            color = new Color(0.8f, 0.8f, 0.8f, 1f),
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };

        public static readonly StaticTextStyleConfig WelcomeStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.MiddleCenter,
            color = new Color(0.7f, 0.7f, 0.7f, 1f),
            fontSize = 16,
            fontStyle = FontStyle.Normal,
            wordWrap = true
        };

        public static readonly StaticTextStyleConfig DefaultTextStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.MiddleLeft,
            color = new Color(0.8f, 0.8f, 0.8f, 1f),
            fontSize = 12,
            fontStyle = FontStyle.Normal,
            wordWrap = false
        };

        public static readonly StaticTextStyleConfig BoldTextStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.MiddleLeft,
            color = new Color(0.9f, 0.9f, 0.9f, 1f),
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };

        public static readonly StaticTextStyleConfig AlertTextStyle = new StaticTextStyleConfig
        {
            alignment = TextAnchor.MiddleLeft,
            color = Color.white,
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        // Box styles
        public static readonly StaticBoxStyleConfig LightBoxStyle = new StaticBoxStyleConfig
        {
            margin = new RectOffset(4, 4, 4, 4),
            padding = new RectOffset(8, 8, 8, 8),
            background = new Color(0.4f, 0.4f, 0.4f, 1f)
        };

        public static readonly StaticBoxStyleConfig DarkBoxStyle = new StaticBoxStyleConfig
        {
            margin = new RectOffset(4, 4, 4, 4),
            padding = new RectOffset(8, 8, 8, 8),
            background = new Color(0.2f, 0.2f, 0.2f, 1f)
        };

        public static readonly StaticBoxStyleConfig ClearBoxStyle = new StaticBoxStyleConfig
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            background = Color.clear
        };

        public static readonly StaticBoxStyleConfig AnnouncementBox = new StaticBoxStyleConfig
        {
            margin = new RectOffset(4, 4, 4, 4),
            padding = new RectOffset(12, 12, 12, 12),
            background = new Color(0.2784314f, 0.6588235f, 0.9607844f, 1f)
        };

        public static readonly StaticBoxStyleConfig AlertBox = new StaticBoxStyleConfig
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(8, 8, 8, 8),
            background = new Color(0.9647059f, 0.3921569f, 0.3490196f, 1f)
        };

        public static readonly StaticBoxStyleConfig DividerBoxStyle = new StaticBoxStyleConfig
        {
            margin = new RectOffset(0, 0, 2, 2),
            padding = new RectOffset(0, 0, 1, 1),
            background = new Color(0.5f, 0.5f, 0.5f, 1f)
        };

        public static readonly StaticBoxStyleConfig HighlightBoxStyle = new StaticBoxStyleConfig
        {
            margin = new RectOffset(4, 4, 4, 4),
            padding = new RectOffset(8, 8, 8, 8),
            background = new Color(0.3f, 0.5f, 0.7f, 1f)
        };

        public static readonly StaticBoxStyleConfig WarningBoxStyle = new StaticBoxStyleConfig
        {
            margin = new RectOffset(4, 4, 4, 4),
            padding = new RectOffset(8, 8, 8, 8),
            background = new Color(1f, 0.6666667f, 0.172549f, 1f)
        };

        // Button styles
        public static readonly StaticButtonStyleConfig DefaultButtonStyle = new StaticButtonStyleConfig
        {
            fixedWidth = 100f
        };

        // Other styles
        public static readonly StaticTextFieldStyleConfig DefaultTextFieldStyle = new StaticTextFieldStyleConfig
        {
            color = new Color(0.8f, 0.8f, 0.8f, 1f),
            border = new RectOffset(3, 3, 3, 3),
            background = new Color(0.2f, 0.2f, 0.2f, 1f),
            active = new Color(0.3f, 0.3f, 0.3f, 1f),
            hover = new Color(0.25f, 0.25f, 0.25f, 1f),
            focused = new Color(0.3f, 0.3f, 0.3f, 1f),
            clipping = TextClipping.Clip
        };

        public static readonly StaticToggleStyleConfig DefaultToggleStyle = new StaticToggleStyleConfig
        {
            color = new Color(0.8f, 0.8f, 0.8f, 1f),
            background = new Color(0.2f, 0.2f, 0.2f, 1f),
            active = new Color(0.3f, 0.5f, 0.7f, 1f),
            hover = new Color(0.25f, 0.25f, 0.25f, 1f),
            focused = new Color(0.3f, 0.3f, 0.3f, 1f),
            padding = new RectOffset(4, 4, 4, 4)
        };

        // General spacing
        public static readonly int Space = 10;
    }

    // Static style configuration classes
    public class StaticTextStyleConfig : IGUIStyleConfig
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
                GUIStyle style = new GUIStyle();
                style.alignment = alignment;
                style.normal.textColor = color;
                style.fontSize = fontSize;
                style.fontStyle = fontStyle;
                style.wordWrap = wordWrap;
                return style;
            }
        }
    }

    public class StaticBoxStyleConfig : IGUIStyleConfig
    {
        public RectOffset margin;
        public RectOffset padding;
        public Color background;

        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle style = new GUIStyle();
                style.margin = margin;
                style.padding = padding;
                style.normal.background = GuiUtil.MakeTexture(2, 2, background);
                return style;
            }
        }
    }

    public class StaticButtonStyleConfig : IGUIStyleConfig
    {
        public float fixedWidth;

        public GUIStyle guiStyle
        {
            get
            {
                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.fixedWidth = fixedWidth;
                return style;
            }
        }
    }

    public class StaticTextFieldStyleConfig : IGUIStyleConfig
    {
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
                GUIStyle style = new GUIStyle();
                style.border = border;
                style.alignment = TextAnchor.MiddleLeft;
                style.normal.textColor = color;
                style.active.textColor = color;
                style.hover.textColor = color;
                style.focused.textColor = color;
                style.normal.background = GuiUtil.MakeTexture(2, 2, background);
                style.active.background = GuiUtil.MakeTexture(2, 2, active);
                style.hover.background = GuiUtil.MakeTexture(2, 2, hover);
                style.focused.background = GuiUtil.MakeTexture(2, 2, focused);
                style.clipping = clipping;
                return style;
            }
        }
    }

    public class StaticToggleStyleConfig : IGUIStyleConfig
    {
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
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleLeft;
                style.normal.textColor = color;
                style.active.textColor = color;
                style.hover.textColor = color;
                style.focused.textColor = color;
                style.normal.background = GuiUtil.MakeTexture(2, 2, background);
                style.active.background = GuiUtil.MakeTexture(2, 2, active);
                style.hover.background = GuiUtil.MakeTexture(2, 2, hover);
                style.focused.background = GuiUtil.MakeTexture(2, 2, focused);
                style.padding = padding;
                return style;
            }
        }
    }
}