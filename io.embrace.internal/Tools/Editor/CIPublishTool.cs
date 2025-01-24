using System;
using System.Collections.Generic;
using System.IO;
using EmbraceSDK.EditorView;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Windows;
using System.Linq;

namespace EmbraceSDK
{
    public static class CIPublishTool
    {
        private const int EXPORT_ERROR_CODE = 109;
        private const string EXPORT_ERROR_MESSAGE = "Exception thrown while exporting package.";

        private const int VERSION_PARSE_ERROR_CODE = 101;
        private const string VERSION_PARSE_ERROR_MESSAGE = "Failed to parse the package version from Packages/io.embrace.sdk/package.json";

#if DeveloperMode
        /*
         * This segment is only for local developer testing.
         */
        [MenuItem("Embrace/Debug Package Asset Path Names")]
        public static void DebugPackageAssetPathNames()
        {
            // Edit is now meant to only be called in the Publish project
            var guid = AssetDatabase.GUIDFromAssetPath("Packages/io.embrace.sdk/CHANGELOG.md");
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"assetPath: {assetPath}");
        }

        [MenuItem("Embrace/Export Unity Package")]
#endif
        public static void ExportUnityPackage()
        {
            // Parse package version
            if (!TryGetPackageVersion(out string packageVersion))
            {
                Debug.LogError(VERSION_PARSE_ERROR_MESSAGE);
                EditorApplication.Exit(VERSION_PARSE_ERROR_CODE);
                return;
            }

            // Export .unitypackage
            Debug.Log("Attempting to export package");
            try
            {
                List<string> exportedPackageAssetList = new List<string>();
                exportedPackageAssetList.Add("Packages/io.embrace.sdk");

                string packageFileName = $"EmbraceSDK_{packageVersion}.unitypackage";

                AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), packageFileName, ExportPackageOptions.Recurse);

                Debug.Log("Successfully exported package");
            }
            catch (Exception e)
            {
                Debug.LogError(EXPORT_ERROR_MESSAGE);
                Debug.LogError(e.Message);
                EditorApplication.Exit(EXPORT_ERROR_CODE);
            }
        }

        private static bool TryGetPackageVersion(out string packageVersion)
        {
            TextAsset packageJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/io.embrace.sdk/package.json");

            if (packageJson == null)
            {
                packageVersion = string.Empty;
                return false;
            }

            Package package = JsonUtility.FromJson<Package>(packageJson.text);
            packageVersion = package.version;

            return true;
        }
    }
}
