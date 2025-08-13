using EmbraceSDK.EditorView;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Embrace.Tools
{
    /// <summary>
    /// Base class for Embrace tools, used to setup editor windows and get Credentials.
    /// </summary>
    internal class ToolEditorWindow : EmbraceEditorWindow
    {
        public static Credentials credentials;
        public static bool hasCredentials;

        public static void GetCredentials()
        {
            string credentialsJson = "";

            try
            {
                credentialsJson = File.ReadAllText(Application.persistentDataPath + "/Credentials.json");
            }
            catch (FileNotFoundException)
            {
                string newCredentials = JsonUtility.ToJson(new Credentials());
                System.IO.File.WriteAllText(Application.persistentDataPath + "/Credentials.json", newCredentials);
            }

            if (!string.IsNullOrEmpty(credentialsJson))
            {
                credentials = JsonUtility.FromJson<Credentials>(credentialsJson);
                hasCredentials = true;
            }
        }
    }
}
