using EmbraceSDK.EditorView;
using EmbraceSDK;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EmbraceSDK.Internal;
using UnityEditor;
using UnityEngine;

namespace Embrace.Tools
{
    /// <summary>
    /// Simple Tool used for debugging SDK during development.
    /// </summary>
    internal class DebuggerTool : ToolEditorWindow
    {
        public bool developerMode;

        private int tabs;
        public enum JsonFileTypes
        {
            None,
            EmbraceSdkInfo,
            PublisherData
        }
        private int indexJsonFileType;

        [MenuItem("Embrace/Debugger Tool")]
        public static void Init()
        {
            EmbraceEditorWindow.Setup();
            // Get existing open window or if none, make a new one:
            DebuggerTool window = (DebuggerTool)GetWindow(typeof(DebuggerTool));
            window.Show();
        }

        public override void Awake()
        {
#if DeveloperMode
            developerMode = true;
#endif
        }

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.BeginVertical(styleConfigs.darkBoxStyle.guiStyle);
            GUILayout.Label("Debugger Tool", styleConfigs.labelHeaderStyle.guiStyle);
            GUILayout.Label("version 1.2.0", styleConfigs.headerTextStyle.guiStyle);
            GUILayout.EndVertical();

            tabs = GUILayout.Toolbar(tabs, new string[] { "Sandbox", "Commonly Used" });

            GUILayout.BeginVertical(styleConfigs.lightBoxStyle.guiStyle);
            switch (tabs)
            {
                case 0:
                    SandBox();
                    break;
                case 1:
                    Common();
                    break;
                default:
                    break;
            }
            GUILayout.EndVertical();
        }

        private void SandBox()
        {
            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Create json file", styleConfigs.defaultTextStyle.guiStyle);
            indexJsonFileType = EditorGUILayout.Popup(indexJsonFileType, new string[] { "None", "EmbraceSdkInfo", "PublisherData" });
            if (GUILayout.Button("Create"))
            {
                CreateJsonFile(indexJsonFileType);
            }

            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Show filepath:", styleConfigs.defaultTextStyle.guiStyle);
            if (GUILayout.Button("path"))
            {
                Debug.Log(EditorApplication.applicationContentsPath);
            }
        }

        private void Common()
        {
            GUILayout.Label("Toggle Editor Mode", styleConfigs.defaultTextStyle.guiStyle);
            if (developerMode)
            {
                if (GUILayout.Button("Disable Editor Mode"))
                {
                    string androidDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                    string iosDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, RemoveCompilerDefines(androidDefineSymbols, "DeveloperMode"));
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, RemoveCompilerDefines(iosDefineSymbols, "DeveloperMode"));

                    developerMode = false;
                }
            }
            else
            {
                if (GUILayout.Button("Editor Mode"))
                {
                    string androidDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                    string iosDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, AddCompilerDefines(androidDefineSymbols, "DeveloperMode"));
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, AddCompilerDefines(iosDefineSymbols, "DeveloperMode"));

                    developerMode = true;
                }
            }

            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Open Welcome Editor", styleConfigs.defaultTextStyle.guiStyle);
            if (GUILayout.Button("Welcome Editor Window"))
            {
                WelcomeEditorWindow.Init();
            }

            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Start Installation process", styleConfigs.defaultTextStyle.guiStyle);
            if (GUILayout.Button("Install"))
            {
                Installation.InitializeOnLoad();
            }
        }

        private string RemoveCompilerDefines(string defines, params string[] toRemove)
        {
            List<string> splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var remove in toRemove)
                splitDefines.Remove(remove);

            return string.Join(";", splitDefines.ToArray());
        }

        private string AddCompilerDefines(string defines, params string[] toAdd)
        {
            List<string> splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var add in toAdd)
                if (!splitDefines.Contains(add))
                    splitDefines.Add(add);

            return string.Join(";", splitDefines.ToArray());
        }

        private void CreateJsonFile(int indexJsonFileType)
        {
            object obj = null;
            JsonFileTypes type = JsonFileTypes.None;
            switch (indexJsonFileType)
            {
                case (int)JsonFileTypes.EmbraceSdkInfo:
                    type = JsonFileTypes.EmbraceSdkInfo;
                    obj = new EmbraceSdkInfo();
                    break;
                case (int)JsonFileTypes.PublisherData:
                    type = JsonFileTypes.PublisherData;
                    obj = new PublisherData();
                    break;
                default:
                    break;
            }

            if (type == JsonFileTypes.None) return;
            System.IO.File.WriteAllText(string.Format(Application.dataPath + "/Tools/Resources/{0}.json", type.ToString()), JsonUtility.ToJson(obj));
        }
    }
}