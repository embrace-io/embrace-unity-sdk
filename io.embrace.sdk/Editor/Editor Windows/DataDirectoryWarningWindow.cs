using System;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// A warning dialog shown when a user tries to update the configuration data directory.
    /// </summary>
    public class DataDirectoryWarningWindow : EditorWindow
    {
        private StyleConfigs _styleConfigs;
        private GUIStyle _messageStyle;
        private GUIStyle _boldStyle;

        private string _message1;
        private string _message2;

        private static Action _onContinue;

        public static void Init(Action onContinueAction)
        {
            // If the editor is running in batch mode we'll never get a response, so we'll skip opening the window
            // and continue immediately.
            if (!EmbraceEditorWindow.ShouldShowEditorWindows())
            {
                onContinueAction?.Invoke();
                return;
            }

            _onContinue = onContinueAction;
            DataDirectoryWarningWindow window = GetWindow<DataDirectoryWarningWindow>(true, EmbraceEditorConstants.WindowTitleWarning);
            window.maxSize = new Vector2(320, 135);
            window.minSize = window.maxSize;
            window.Show();
        }

        private void OnFocus()
        {
            _styleConfigs = Resources.Load<StyleConfigs>("StyleConfigs/MainStyleConfigs");

            _messageStyle = new GUIStyle(_styleConfigs.defaultTextStyle.guiStyle);
            _messageStyle.wordWrap = true;
            _messageStyle.alignment = TextAnchor.MiddleCenter;

            _boldStyle = new GUIStyle(_styleConfigs.boldTextStyle.guiStyle);
            _boldStyle.wordWrap = true;
            _boldStyle.alignment = TextAnchor.MiddleCenter;

            _message1 = "Setting a different Embrace data directory will relocate existing configurations.";
            _message2 = "We recommend avoiding existing directories which contain non-Embrace assets.";
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(_styleConfigs.darkBoxStyle.guiStyle);

            EditorGUILayout.TextArea(_message1, _messageStyle);
            GUILayout.Space(_styleConfigs.space);
            EditorGUILayout.TextArea(_message2, _boldStyle);
            GUILayout.Space(_styleConfigs.space);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(_styleConfigs.space * 2f);

            if (GUILayout.Button("Cancel", _styleConfigs.defaultButtonStyle.guiStyle))
            {
                Close();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Continue", _styleConfigs.defaultButtonStyle.guiStyle))
            {
                _onContinue?.Invoke();
                Close();
            }

            GUILayout.Space(_styleConfigs.space * 2f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}