using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Editor window that welcomes users, prompts them to configure, and informs them of important changes.
    /// </summary>
    public class WelcomeEditorWindow : EmbraceEditorWindow
    {
        private static bool setup = false;
        private static Vector2 minWindowSize = new Vector2(500f, 165f);

        public static void Init()
        {
            // Don't open editor windows in batch mode to avoid CI related errors
            if (!ShouldShowEditorWindows())
            {
                return;
            }

            Setup();

            // Get existing open window or if none, make a new one:
            WelcomeEditorWindow window = (WelcomeEditorWindow)GetWindow(typeof(WelcomeEditorWindow));
            window.minSize = minWindowSize;


            if (NeedsSetup(androidConfiguration) || NeedsSetup(iOSConfiguration))
            {
                setup = true;
                window.minSize = new Vector2(window.minSize.x, window.minSize.y + 95);
            }

            if (!string.IsNullOrEmpty(sdkInfo.wAnnouncementMessage))
            {
                var titleHeight = styleConfigs.labelTitleStyle.guiStyle.CalcSize(new GUIContent(sdkInfo.wAnnouncementTitle)).y;
                var messageHeight = styleConfigs.defaultTextStyle.guiStyle.CalcSize(new GUIContent(sdkInfo.wAnnouncementMessage)).y;
                window.minSize = new Vector2(window.minSize.x, window.minSize.y + 110f + titleHeight + messageHeight);
            }

            window.maxSize = window.minSize;
            window.Show();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.BeginVertical(styleConfigs.darkBoxStyle.guiStyle);
            GUILayout.Label("Welcome", styleConfigs.welcomeStyle.guiStyle);
            GUILayout.Label("Embrace SDK " + sdkInfo.version, new GUIStyle(styleConfigs.defaultTextStyle.guiStyle) { alignment = TextAnchor.MiddleCenter });
            GUILayout.EndVertical();
            GUILayout.Label("Embrace’s Unity SDK lets you bring the deep, " +
                "introspective and native debugging power of Embrace into your Unity game or application.", new GUIStyle(styleConfigs.defaultTextStyle.guiStyle) { padding = new RectOffset(15, 15, 15, 0), wordWrap = true });

            if (!string.IsNullOrEmpty(sdkInfo.wAnnouncementMessage))
            {
                GUILayout.BeginVertical(styleConfigs.announcementBox.guiStyle);
                GUILayout.Label(sdkInfo.wAnnouncementTitle, styleConfigs.labelTitleStyle.guiStyle);
                GUILayout.Label(sdkInfo.wAnnouncementMessage, new GUIStyle(styleConfigs.defaultTextStyle.guiStyle) { wordWrap = true });
                GUILayout.EndVertical();
            }

            if (setup)
            {
                GUILayout.BeginVertical(styleConfigs.lightBoxStyle.guiStyle);
                GUILayout.Label("Setup Required", styleConfigs.labelTitleStyle.guiStyle);
                GUILayout.Label("Use the Embrace Window to configure both IOS and Android platforms.", new GUIStyle(styleConfigs.defaultTextStyle.guiStyle) { wordWrap = true });
                GUILayout.EndVertical();
            }


            GUILayout.BeginVertical(styleConfigs.darkBoxStyle.guiStyle);
            if (GUILayout.Button("Open Embrace Window"))
            {
                GettingsStartedEditorWindow.Init();
            }
            GUILayout.EndVertical();
        }

        private static bool NeedsSetup(EmbraceConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.SymbolUploadApiToken) || string.IsNullOrEmpty(configuration.AppId))
            {
                return true;
            }
            return false;
        }
    }
}