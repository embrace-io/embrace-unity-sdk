using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace EmbraceSDK.Tests
{
    public class StyleConfigTests
    {
        [Test]
        public void BoxStyleConfig_Returns_Valid_GUIStyle()
        {
            BoxStyleConfig style = BoxStyleConfig.CreateInstance<BoxStyleConfig>();

            style.margin = new RectOffset(0, 1, 0, 1);
            style.background = Color.black;
            style.padding = new RectOffset(0, 1, 0, 1);

            GUIStyle guiStyle = style.guiStyle;
            Object.DestroyImmediate(style);

            Assert.IsNotNull(guiStyle);
        }

        [Test]
        public void TextStyleConfig_Returns_Valid_GUIStyle()
        {
            TextStyleConfig style = TextStyleConfig.CreateInstance<TextStyleConfig>();

            style.alignment = TextAnchor.LowerCenter;
            style.color = Color.black;
            style.fontSize = 12;
            style.wordWrap = true;

            GUIStyle guiStyle = style.guiStyle;
            Object.DestroyImmediate(style);

            Assert.IsNotNull(guiStyle);
        }

        [Test]
        public void TextFieldStyleConfig_Returns_Valid_GUIStyle()
        {
            TextFieldStyleConfig style = TextFieldStyleConfig.CreateInstance<TextFieldStyleConfig>();

            style.color = Color.black;
            style.active = Color.black;
            style.focused = Color.black;
            style.hover = Color.black;
            style.border = new RectOffset(0, 1, 0, 1);

            GUIStyle guiStyle = style.guiStyle;
            Object.DestroyImmediate(style);

            Assert.IsNotNull(guiStyle);
        }

        [Test]
        public void ToggleStyleConfig_Returns_Valid_GUIStyle()
        {
            ToggleStyleConfig style = ToggleStyleConfig.CreateInstance<ToggleStyleConfig>();

            style.color = Color.black;
            style.active = Color.black;
            style.focused = Color.black;
            style.hover = Color.black;
            style.padding = new RectOffset(0, 0, 0, 0);

            GUIStyle guiStyle = style.guiStyle;
            Object.DestroyImmediate(style);

            Assert.IsNotNull(guiStyle);
        }

        [Test]
        public void All_Style_Configs_Are_Valid()
        {
            string[] guids = AssetDatabase.FindAssets("t: StyleConfigs");
            for (int i = 0; i < guids.Length; ++i)
            {
                StyleConfigs configs = AssetDatabase.LoadAssetAtPath<StyleConfigs>(AssetDatabase.GUIDToAssetPath(guids[i]));

                Assert.IsNotNull(configs);

                Assert.IsNotNull(configs.headerTextStyle);
                Assert.IsNotNull(configs.labelHeaderStyle);
                Assert.IsNotNull(configs.labelTitleStyle);
                Assert.IsNotNull(configs.welcomeStyle);
                Assert.IsNotNull(configs.defaultTextStyle);
                Assert.IsNotNull(configs.alertTextStyle);
                Assert.IsNotNull(configs.lightBoxStyle);
                Assert.IsNotNull(configs.darkBoxStyle);
                Assert.IsNotNull(configs.clearBoxStyle);
                Assert.IsNotNull(configs.announcementBox);
                Assert.IsNotNull(configs.alertBox);
                Assert.IsNotNull(configs.dividerBoxStyle);
                Assert.IsNotNull(configs.highlightBoxStyle);
                Assert.IsNotNull(configs.defaultTextFieldStyle);
                Assert.IsNotNull(configs.defaultToggleStyle);
            }
        }
    }
}