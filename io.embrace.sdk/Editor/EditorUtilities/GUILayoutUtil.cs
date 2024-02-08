using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// GUI Utility that shows alert messages in editor windows.
    /// </summary>
    public class GUILayoutUtil
    {
        public enum AlertType
        {
            Error,
            Success,
            Info,
            Warning
        }

        private static float warningtimer;
        private static StyleConfigs styleConfigs;

        public static void Setup()
        {
            styleConfigs = Resources.Load<StyleConfigs>("StyleConfigs/MainStyleConfigs");
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public static void Alert(string message, EditorWindow window, AlertType type, float size = 55)
        {
            if (styleConfigs == null) Setup();
            SetColor(type);

            warningtimer -= Time.deltaTime;
            if (warningtimer > 0) return;

            GUILayout.BeginArea(new Rect(0, 0, window.maxSize.x, size), styleConfigs.alertBox.guiStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("x", styleConfigs.labelHeaderStyle.guiStyle))
            {
                warningtimer = 2;
            }
            GUILayout.Space(20);
            GUILayout.Label(type.ToString() + ": " + message, styleConfigs.alertTextStyle.guiStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }


        private static void SetColor(AlertType type)
        {
            switch (type)
            {
                case AlertType.Error:
                    styleConfigs.alertBox.background = new Color(0.9647059f, 0.3921569f, 0.3490196f);
                    break;
                case AlertType.Success:
                    styleConfigs.alertBox.background = new Color(0.1843137f, 0.7254902f, 0.5254902f);
                    break;
                case AlertType.Info:
                    styleConfigs.alertBox.background = new Color(0.2784314f, 0.6588235f, 0.9607844f);
                    break;
                case AlertType.Warning:
                    styleConfigs.alertBox.background = new Color(1, 0.6666667f, 0.172549f);
                    break;
                default:
                    break;
            }
        }
    }
}