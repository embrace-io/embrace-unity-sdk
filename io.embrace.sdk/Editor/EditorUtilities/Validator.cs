using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Checks if app data is valid by verifying that the App token and ID are using the correct characters and are the correct length.
    /// </summary>
    public class Validator
    {
        public const int AppIDLength = 5;
        public const int ApiTokenLength = 32;

        public static bool ValidateID(string input)
        {
            if (input.Length != AppIDLength || string.IsNullOrEmpty(input)) return false;

            Regex regex = new Regex(@"[A-Za-z0-9]{5}");

            return regex.IsMatch(input);
        }

        public static bool ValidateToken(string input)
        {
            if (input.Length != ApiTokenLength || string.IsNullOrEmpty(input)) return false;

            Regex regex = new Regex(@"[A-Za-z0-9]{32}");

            return regex.IsMatch(input);
        }

        public static void ValidateConfiguration(EmbraceConfiguration config)
        {
            if (config == null)
            {
                throw new UnityEditor.Build.BuildFailedException("EmbraceSDK: No config scriptable object found, add the EmbraceSDK component via Component/Scripts and configure it with your API Token via the Unity Inspector");
            }
            if (!ValidateID(config.AppId))
            {
                throw new UnityEditor.Build.BuildFailedException(string.Format("EmbraceSDK: APP_ID not correctly formatted, it should be a {0} character string.", AppIDLength));
            }
            if (!ValidateToken(config.SymbolUploadApiToken))
            {
                throw new UnityEditor.Build.BuildFailedException(string.Format("EmbraceSDK: API_TOKEN not correctly formatted, it should be a {0} digit hexadecimal number.", ApiTokenLength));
            }
        }

        /// <summary>
        /// Checks if a relative folder path starts with the Assets folder.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static bool ValidateConfigsFolderPath(string dir)
        {
            return dir.IndexOf("Assets/", StringComparison.Ordinal) == 0;
        }

    }
}
