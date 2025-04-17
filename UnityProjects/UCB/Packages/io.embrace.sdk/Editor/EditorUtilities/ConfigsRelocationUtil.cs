using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Utility for relocating Embrace configuration scriptable objects after the data directory has been updated through the use the Settings editor window.
    /// </summary>
    public static class ConfigsRelocationUtil
    {
        /// <summary>
        /// A flag that can be checked repeatedly after calling RelocateAssets() method. This is meant as an alternative to using the onRefreshCompleteAction parameter.
        /// </summary>
        public static bool RefreshComplete { get; private set; }

        private static Action _onCompleteCallback;

        private static string[] _deletableExtensions =
        {
            ".meta",
            ".DS_Store"
        };

        /// <summary>
        /// Relocates any existing Embrace assets to the specified directory.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="onRefreshCompleteAction">Optional callback to invoke when process is complete</param>
        public static void RelocateAssets(string sourceDir, string destinationDir, Action onRefreshCompleteAction = null)
        {
            RefreshComplete = false;

            _onCompleteCallback = onRefreshCompleteAction;

            var sourceEnvPaths = AssetDatabaseUtil.GetAssetPaths<Environments>(sourceDir);
            var sourceConfigPaths = AssetDatabaseUtil.GetAssetPaths<EmbraceConfiguration>(sourceDir);
            var sourceTextPaths = AssetDatabaseUtil.GetAssetPaths<TextAsset>(sourceDir);

            if (sourceEnvPaths.Length == 0)
            {
                _onCompleteCallback?.Invoke();
                Debug.LogWarning($"Unable to locate Environments.asset scriptable object within {sourceDir} or any of its subdirectories ");
                return;
            }

            if (sourceConfigPaths.Length == 0)
            {
                _onCompleteCallback?.Invoke();
                Debug.LogWarning($"Unable to locate any configuration scriptable objects within {sourceDir} or any of its subdirectories ");
                return;
            }

            if (sourceEnvPaths.Length > 1)
            {
                Debug.LogWarning($"Multiple Environments.asset files found within {sourceDir}. Selecting first one found. Please delete any unused copies.");
            }

            // Move the Environments.asset scriptable object and its meta file
            AssetDatabaseUtil.EnsureFolderExists(destinationDir);
            MoveAsset(sourceEnvPaths[0], destinationDir);

            for (int i = 0; i < sourceTextPaths.Length; ++i)
            {
                MoveAsset(sourceTextPaths[i], destinationDir);
            }

            // Move all sdk configuration scriptable objects found and their meta files.
            // Load their scriptable objects and update their path properties.
            var destinationConfigDir = $"{destinationDir}/Configurations";
            AssetDatabaseUtil.EnsureFolderExists(destinationConfigDir);

            for (int i = 0; i < sourceConfigPaths.Length; i++)
            {
                var configPath = sourceConfigPaths[i];
                MoveAsset(configPath, destinationConfigDir);
            }

            // Try deleting any empty folders as a result of the relocation
            StripFileNames(sourceEnvPaths);
            StripFileNames(sourceConfigPaths);
            TryDeleteDirs(sourceEnvPaths);
            TryDeleteDirs(sourceConfigPaths);
            TryDeleteDir(sourceDir);

            EditorApplication.projectChanged += OnProjectChanged;
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Tries to move configuration scriptable objects using either Unity, or System.IO methods.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationDir"></param>
        /// <returns></returns>
        private static string MoveAsset(string sourceFilePath, string destinationDir)
        {
            var startIndex = sourceFilePath.LastIndexOf('/');
            var length = sourceFilePath.Length - startIndex;
            var fileName = sourceFilePath.Substring(startIndex, length);
            var destinationFilePath = $"{destinationDir}{fileName}";

            if (!UnityMoveAsset(sourceFilePath, destinationFilePath))
            {
                // If the file happens to be outside of the project Assets folder (e.g. during an sdk
                // upgrade, where configs are stored in the sdk package), we resort to System.IO methods.
                SystemMoveAsset(sourceFilePath, destinationFilePath);
            }

            AssetDatabase.Refresh();

            return destinationFilePath;
        }

        /// <summary>
        /// Tries to move configuration scriptable objects and their .meta files using Unity's AssetDatabase methods.
        /// This operation might fail if the files are located outside of the project Assets folder.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFilePath"></param>
        /// <returns></returns>
        private static bool UnityMoveAsset(string sourceFilePath, string destinationFilePath)
        {
            var errorMsg = AssetDatabase.MoveAsset(sourceFilePath, destinationFilePath);
            return string.IsNullOrEmpty(errorMsg);
        }

        /// <summary>
        /// Tries to moves configuration scriptable objects and their .meta files using System.IO methods.  This operation
        /// might fail if the file does not exist.
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFilePath"></param>
        private static void SystemMoveAsset(string sourceFilePath, string destinationFilePath)
        {
            var sourceFileInfo = new FileInfo(sourceFilePath);

            if (sourceFileInfo.Exists)
            {
                var sourcePath = sourceFileInfo.FullName;
                var destinationPath = new FileInfo(destinationFilePath).FullName;

                File.Move(sourcePath, destinationPath);

                // Explicitly moving any .meta files that might exist.
                var sourceMetaPath = $"{sourcePath}.meta";

                if (File.Exists(sourceMetaPath))
                {
                    var destinationMetaPath = $"{destinationPath}.meta";
                    File.Move(sourceMetaPath, destinationMetaPath);
                }
            }
            else
            {
                Debug.LogWarning($"Unable to relocate {sourceFilePath}. File did not exist.");
            }
        }

        private static void StripFileNames(string[] filePaths)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                var filePath = filePaths[i];
                filePaths[i] = filePath.Substring(0, filePath.LastIndexOf('/'));
            }
        }

        private static void TryDeleteDirs(string[] dirs)
        {
            foreach (var dir in dirs)
            {
                TryDeleteDir(dir);
            }
        }

        private static void TryDeleteDir(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);
            if (!dirInfo.Exists) return;

            if (CanDeleteDirectory(dirInfo.FullName))
            {
                Directory.Delete(dirInfo.FullName, true);

                var dirMeta = $"{dirInfo.FullName}.meta";
                if (File.Exists(dirMeta))
                {
                    File.Delete(dirMeta);
                }
            }
        }

        private static bool CanDeleteDirectory(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            var files = dirInfo.GetFiles();

            if (files.Length > 0)
            {
                var canDelete = true;

                // Check if directory contains only files that are safe to delete
                for (int i = 0; i < files.Length; i++)
                {
                    canDelete &= _deletableExtensions.Contains(files[i].Extension);

                    if (!canDelete) break;
                }

                return canDelete;
            }

            return true;
        }

        private static void OnProjectChanged()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            _onCompleteCallback?.Invoke();
            RefreshComplete = true;
        }

        /// <summary>
        /// Converts deprecated environments and configuration data created in SDK version 1.5.10 and earlier.
        /// </summary>
        public static void ConvertDeprecatedData()
        {
            var configPaths = ConvertOldConfigs();
            var envPath = ConvertOldEnvironments();
            var paths = new List<string>();
            paths.AddRange(configPaths);
            paths.Add(envPath);
            AssetDatabase.ForceReserializeAssets(paths);
        }

#pragma warning disable 0618
        private static string[] ConvertOldConfigs()
        {
            var oldConfigPaths = AssetDatabaseUtil.GetAssetPaths<EmbraceSDKConfiguration>();
            for (int i = 0; i < oldConfigPaths.Length; i++)
            {
                var oldPath = oldConfigPaths[i];
                var oldConfig = AssetDatabase.LoadAssetAtPath<EmbraceSDKConfiguration>(oldPath);

                EmbraceConfiguration newConfig;

                if (oldConfig.deviceType == EmbraceSDKConfiguration.DeviceType.Android)
                {
                    newConfig = ScriptableObject.CreateInstance<AndroidConfiguration>();
                    newConfig.SetDefaults();

                    var androidConfig = (AndroidConfiguration)newConfig;
                    androidConfig.sdk_config.session.async_end = oldConfig.ASYNC_UPLOAD;
                    androidConfig.sdk_config.session.max_session_seconds = oldConfig.max_session_seconds;
                    androidConfig.sdk_config.networking.capture_request_content_length = oldConfig.capture_request_content_length;
                    androidConfig.sdk_config.networking.enable_native_monitoring = oldConfig.enable_native_monitoring;
                }
                else
                {
                    newConfig = ScriptableObject.CreateInstance<IOSConfiguration>();
                    newConfig.SetDefaults();

                    var iosConfig = (IOSConfiguration)newConfig;
                    iosConfig.CRASH_REPORT_ENABLED = oldConfig.USE_EMBRACE_CRASH_REPORTING;
                }

                newConfig.AppId = oldConfig.APP_ID;
                newConfig.SymbolUploadApiToken = oldConfig.API_TOKEN;

                string newConfigPath = $"{AssetDatabaseUtil.SDKDirectory}/Resources/Configurations/{oldConfig.name}.asset";

                AssetDatabase.DeleteAsset(oldPath);
                AssetDatabase.CreateAsset(newConfig, newConfigPath);
            }

            return oldConfigPaths;
        }

        private static string ConvertOldEnvironments()
        {
            var newConfigPaths = AssetDatabaseUtil.GetAssetPaths<EmbraceConfiguration>(AssetDatabaseUtil.SDKDirectory);
            var oldEnvPaths = AssetDatabaseUtil.GetAssetPaths<EmbraceSDKSettings>(AssetDatabaseUtil.SDKDirectory);
            var oldEnv = AssetDatabase.LoadAssetAtPath<EmbraceSDKSettings>(oldEnvPaths[0]);
            var newEnv = ScriptableObject.CreateInstance<Environments>();

            newEnv.activeEnvironmentIndex = oldEnv.activeEnvironmentIndex;

            foreach (var oldConfigItem in oldEnv.environmentConfigurations)
            {
                var newEnvConfig = new EnvironmentConfiguration(oldConfigItem.guid, oldConfigItem.environmentName);
                var androidConfigName = $"{oldConfigItem.environmentName}AndroidEnvironmentConfiguration";
                var iOSConfigName = $"{oldConfigItem.environmentName}IOSEnvironmentConfiguration";
                var androidConfigPath = newConfigPaths.First(x => x.Contains(androidConfigName));
                var iOSConfigPath = newConfigPaths.First(x => x.Contains(iOSConfigName));
                var androidConfig = AssetDatabase.LoadAssetAtPath<AndroidConfiguration>(androidConfigPath);
                var iosConfig = AssetDatabase.LoadAssetAtPath<IOSConfiguration>(iOSConfigPath);
                androidConfig.EnvironmentName = iosConfig.EnvironmentName = oldConfigItem.environmentName;
                androidConfig.EnvironmentGuid = iosConfig.EnvironmentGuid = oldConfigItem.guid;
                newEnvConfig.sdkConfigurations.Add(androidConfig);
                newEnvConfig.sdkConfigurations.Add(iosConfig);
                newEnv.environmentConfigurations.Add(newEnvConfig);
            }

            var newEnvPath = $"{AssetDatabaseUtil.SDKDirectory}/Resources/Settings/Environments.asset";
            AssetDatabase.DeleteAsset(oldEnvPaths[0]);
            AssetDatabase.CreateAsset(newEnv, newEnvPath);

            return newEnvPath;
        }
#pragma warning restore 0618
    }
}