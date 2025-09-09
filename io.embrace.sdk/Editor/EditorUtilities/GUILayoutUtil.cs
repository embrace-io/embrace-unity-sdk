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

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public static void Alert(string message, EditorWindow window, AlertType type, float size = 55)
        {
            var alertBoxStyle = GetAlertBoxStyle(type);

            warningtimer -= Time.deltaTime;
            if (warningtimer > 0) return;

            GUILayout.BeginArea(new Rect(0, 0, window.maxSize.x, size), alertBoxStyle.guiStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("x", StaticStyleConfigs.LabelHeaderStyle.guiStyle))
            {
                warningtimer = 2;
            }
            GUILayout.Space(20);
            GUILayout.Label(type.ToString() + ": " + message, StaticStyleConfigs.AlertTextStyle.guiStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }


        private static StaticBoxStyleConfig GetAlertBoxStyle(AlertType type)
        {
            var alertBoxStyle = new StaticBoxStyleConfig
            {
                margin = StaticStyleConfigs.AlertBox.margin,
                padding = StaticStyleConfigs.AlertBox.padding
            };

            switch (type)
            {
                case AlertType.Error:
                    alertBoxStyle.background = new Color(0.9647059f, 0.3921569f, 0.3490196f);
                    break;
                case AlertType.Success:
                    alertBoxStyle.background = new Color(0.1843137f, 0.7254902f, 0.5254902f);
                    break;
                case AlertType.Info:
                    alertBoxStyle.background = new Color(0.2784314f, 0.6588235f, 0.9607844f);
                    break;
                case AlertType.Warning:
                    alertBoxStyle.background = new Color(1, 0.6666667f, 0.172549f);
                    break;
                default:
                    alertBoxStyle.background = StaticStyleConfigs.AlertBox.background;
                    break;
            }

            return alertBoxStyle;
        }
    }
}