using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Provides functions that helps with accessing assets and performing operations on assets.
    /// </summary>
    internal class AssetDatabaseUtil
    {
        #region Directories

        /// <summary>
        /// The default directory where Embrace configuration data is stored.
        /// </summary>
        public const string DefaultDataDirectory = "Assets/Embrace";

        /// <summary>
        /// Provides the full path to the project root folder.
        /// </summary>
        public static string ProjectDirectory { get; } = Application.dataPath.Replace("/Assets", "");

        /// <summary>
        /// <para>
        ///     Legacy SDKs have been stored in different directories.  This method checks for the existence of previously known locations.
        /// </para>
        /// <para>
        ///     Returns null if none are found.
        /// </para>
        /// </summary>
        public static string SDKDirectory
        {
            get
            {
                if (new DirectoryInfo("Packages/io.embrace.sdk").Exists)
                {
                    return "Packages/io.embrace.sdk";
                }

                if (new DirectoryInfo("Assets/Plugins/Embrace").Exists)
                {
                    return "Assets/Plugins/Embrace";
                }

                if (new DirectoryInfo("Assets/Plugins/EmbraceSDK").Exists)
                {
                    return "Assets/Plugins/EmbraceSDK";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets and sets the directory where Embrace data is saved.  Saves the directory path any time the value is changed.
        /// </summary>
        public static string EmbraceDataDirectory
        {
            get => AssetDatabaseUtil.ProjectDirectory;
            set
            {}                    
            /*
            get => EmbraceProjectSettings.Project.GetValue<string>("dataDirectory", string.Empty);
            set => EmbraceProjectSettings.Project.SetValue<string>("dataDirectory", value);
            //*/
        }

        /// <summary>
        /// Directory where configuration scriptable objects are stored.
        /// </summary>
        public static string ConfigurationsDirectory => $"{EmbraceDataDirectory}/Configurations";

        /// <summary>
        /// Checks for existence of a folder within the project Assets folder.
        /// Creates it, and any parent folders containing it if not found.
        /// </summary>
        /// <param name="dir"></param>
        public static void EnsureFolderExists(string dir)
        {
            if (!Validator.ValidateConfigsFolderPath(dir)) dir = $"Assets/{dir}";

            // early exit
            if (new DirectoryInfo($"{ProjectDirectory}/{dir}").Exists) return;

            var folderNames = dir.Split('/');

            // Always include "Assets" as first directory
            var pathBuilder = new StringBuilder();
            pathBuilder.Append(folderNames[0]);

            // Start iteration from 2nd folder name.
            for (int i = 1; i < folderNames.Length; i++)
            {
                var fullPath = $"{ProjectDirectory}/{pathBuilder}/{folderNames[i]}";

                if (!new DirectoryInfo(fullPath).Exists)
                {
                    AssetDatabase.CreateFolder(pathBuilder.ToString(), folderNames[i]);
                    AssetDatabase.Refresh();
                }

                pathBuilder.Append($"/{folderNames[i]}");
            }
        }

        #endregion

        #region Environments

        /// <summary>
        /// Creates an instance of the Environments scriptable object.
        /// </summary>
        /// <returns></returns>
        public static Environments CreateEnvironments()
        {
            return null; // Temporary patch as we remove environments
            EnsureFolderExists(EmbraceDataDirectory);
            var environments = ScriptableObject.CreateInstance<Environments>();
            AssetDatabase.CreateAsset(environments, $"{EmbraceDataDirectory}/Environments.asset");
            EditorUtility.SetDirty(environments);
            return environments;
        }

        /// <summary>
        /// Loads the Environments asset.
        /// </summary>
        /// <param name="path">Optional path to load asset from. If not provided, default data directory is used.</param>
        /// <param name="ensureNotNull">If true, creates an Environments scriptable object not found.</param>
        /// <returns></returns>
        public static Environments LoadEnvironments(string path = null, bool ensureNotNull = true)
        {
            var environments = AssetDatabase.LoadAssetAtPath<Environments>(path ?? $"{EmbraceDataDirectory}/Environments.asset");

            if (ensureNotNull && environments == null)
            {
                environments = CreateEnvironments();
                Debug.LogWarning("Environments scriptable object not found.  A new instance was created.");

                TryRecoverEnvironments(environments);
            }

            return environments;
        }

        /// <summary>
        /// Checks the integrity of saved environment configurations, and restores any improperly deleted configs.
        /// </summary>
        /// <param name="environments"></param>
        public static bool RefreshEnvironments(Environments environments)
        {
            if (environments == null)
            {
                return false;
            }

            var configRestored = false;

            foreach (var envConfig in environments.environmentConfigurations)
            {
                for (int i = 0; i < envConfig.sdkConfigurations.Count; i++)
                {
                    var sdkConfig = envConfig.sdkConfigurations[i];

                    // The config was deleted out of the window's control.
                    if (sdkConfig == null)
                    {
                        switch (i)
                        {
                            case 0:
                                sdkConfig = CreateConfiguration<AndroidConfiguration>(envConfig.guid, envConfig.name);
                                break;
                            case 1:
                                sdkConfig = CreateConfiguration<IOSConfiguration>(envConfig.guid, envConfig.name);
                                break;
                        }

                        envConfig.sdkConfigurations[i] = sdkConfig;

                        var sb = new StringBuilder();
                        sb.Append($"{sdkConfig.DeviceType} configuration for Environment \"{envConfig.name}\" was not deleted properly. ");
                        sb.Append("Please use the settings window (Tools > Embrace > Settings) to manage environments and configurations.");

                        Debug.LogWarning(sb.ToString());

                        configRestored = true;
                    }
                }
            }

            return configRestored;
        }

        /// <summary>
        /// Attempts to rebuild an Environments object based on orphaned configuration objects found in the project.
        /// </summary>
        /// <returns></returns>
        public static void TryRecoverEnvironments(Environments environments)
        {
            var configPaths = GetAssetPaths<EmbraceConfiguration>(EmbraceDataDirectory);
            var recoveredConfigs = 0;

            // iterate through each configuration asset path found and determine if it's
            // an environment config that needs to be re-added.
            foreach (var path in configPaths)
            {
                var configAsset = LoadConfiguration<EmbraceConfiguration>(path);

                // if any sdk configs are found with environment guids defined, this implies environments were previously
                // defined and we should proceed with recovery.
                if (!string.IsNullOrEmpty(configAsset.EnvironmentGuid))
                {
                    var envConfig = environments.environmentConfigurations.FirstOrDefault(x => x.name == configAsset.EnvironmentName);

                    if (envConfig == null)
                    {
                        recoveredConfigs++;

                        // the environment configuration for this asset needs to be recreated
                        envConfig = new EnvironmentConfiguration(configAsset.EnvironmentGuid, configAsset.EnvironmentName);
                        envConfig.sdkConfigurations.Add(null); // Android Config slot
                        envConfig.sdkConfigurations.Add(null); // iOS Config slot

                        // add to the environments asset
                        environments.environmentConfigurations.Add(envConfig);
                    }

                    // set the sdk config asset in it's corresponding device slot
                    envConfig.sdkConfigurations[(int)configAsset.DeviceType] = configAsset;
                }
            }

            if (recoveredConfigs > 0)
            {
                // Since environment sdk config assets were recovered, set the active environment index before exiting.
                environments.activeEnvironmentIndex = 0;
                Debug.LogWarning($"Recovered Environments with {recoveredConfigs} configurations.");
            }

            if (environments != null)
                EditorUtility.SetDirty(environments);
        }

        #endregion

        #region Configurations

        /// <summary>
        /// Creates a configuration scriptable object with empty GUID and name properties which can be set later.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateConfiguration<T>() where T : EmbraceConfiguration
        {
            return CreateConfiguration<T>(string.Empty, string.Empty);
        }

        /// <summary>
        /// <para>
        ///     Creates a configuration scriptable object.
        /// </para>
        /// <para>
        ///     NOTE: The provided guid will also serve as the configuration name.
        /// </para>
        /// </summary>
        /// <param name="guid">The environment guid for the SDK configuration.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateConfiguration<T>(string guid) where T : EmbraceConfiguration
        {
            return CreateConfiguration<T>(guid, guid);
        }

        /// <summary>
        /// Creates a configuration scriptable object.
        /// </summary>
        /// <param name="guid">The environment guid for the SDK configuration.</param>
        /// <param name="name">The environment name for the SDK configuration.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateConfiguration<T>(string guid, string name) where T : EmbraceConfiguration
        {
            return null; // Temporary
            EnsureFolderExists(ConfigurationsDirectory);
            var config = ScriptableObject.CreateInstance<T>();
            config.SetDefaults(); // Sets default values
            config.EnvironmentName = name;
            config.EnvironmentGuid = guid;
            var prefix = string.IsNullOrEmpty(name) ? "Default" : name;
            string configPath = $"{ConfigurationsDirectory}/{prefix}{config.DeviceType}Configuration.asset";
            AssetDatabase.CreateAsset(config, configPath);
            EditorUtility.SetDirty(config);
            return config;
        }


        /// <summary>
        /// Loads environment-based configuration scriptable objects.
        /// </summary>
        /// <param name="environments">Environments configuration</param>
        /// <param name="ensureNotNull">If true, creates replacement configs if none are found.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Environment-based configuration, otherwise returns default.</returns>
        public static T LoadConfiguration<T>(Environments environments, bool ensureNotNull = true) where T : EmbraceConfiguration
        {
            if (environments != null && environments.environmentConfigurations.Count > 0 &&
                environments.activeEnvironmentIndex > -1)
            {
                if (environments.activeEnvironmentIndex >= environments.environmentConfigurations.Count)
                {
                    environments.activeEnvironmentIndex = environments.environmentConfigurations.Count - 1;
                }

                var configurations = environments.environmentConfigurations[environments.activeEnvironmentIndex];

                T configAsset = null;
                foreach (var config in configurations.sdkConfigurations)
                {
                    if (config is T typedConfig)
                    {
                        configAsset = typedConfig;
                        break;
                    }
                }

                if (ensureNotNull && configAsset == null)
                {
                    RefreshEnvironments(environments);
                }

                return configAsset;
            }
            else
            {
                return LoadConfiguration<T>(ensureNotNull: ensureNotNull);
            }
        }

        /// <summary>
        /// Loads default configuration scriptable objects.
        /// </summary>
        /// <param name="path">Optional path to load asset from. If not provided, default data directory is used.</param>
        /// <param name="ensureNotNull">If true, creates config if not found.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Default configuration.</returns>
        public static T LoadConfiguration<T>(string path = null, bool ensureNotNull = true) where T : EmbraceConfiguration
        {
            var configuration = AssetDatabase.LoadAssetAtPath<T>(path ?? $"{ConfigurationsDirectory}/Default{typeof(T).Name}.asset");

            if (ensureNotNull && configuration == null)
            {
                configuration = CreateConfiguration<T>();
            }

            return configuration;
        }

        #endregion

        #region Assets

        /// <summary>
        /// Searches for any assets of type T and returns their paths. If no objects are found, return an empty array.
        /// </summary>
        /// <param name="searchDirectories">Optional list of directories to search</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string[] GetAssetPaths<T>(params string[] searchDirectories)
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", searchDirectories);
            return guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
        }

        /// <summary>
        /// Finds and creates instances of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] GetInstances<T>()
        {
            List<Type> types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assemblies[i].GetTypes();
                }
                catch (Exception)
                {
                    continue;
                }

                for (int j = 0; j < assemblyTypes.Length; ++j)
                {
                    // Must implement classType.
                    if (!typeof(T).IsAssignableFrom(assemblyTypes[j]))
                    {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract)
                    {
                        continue;
                    }

                    types.Add(assemblyTypes[j]);
                }
            }

            List<T> result = new List<T>();

            for (int i = 0; i < types.Count; ++i)
            {
                result.Add((T)Activator.CreateInstance(types[i]));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Copies the contents of a source directory into a destination directory.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="recursive">If true, copies contents of subfolders recursively.</param>
        /// <param name="deleteSourceMetaFiles">If true, deletes any duplicated .meta files found in the destination directory.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool deleteSourceMetaFiles)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            if (deleteSourceMetaFiles)
            {
                foreach (string metaFile in System.IO.Directory.GetFiles(dir.FullName, "*.meta"))
                {
                    System.IO.File.Delete(metaFile);
                }
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true, deleteSourceMetaFiles);

                    if (deleteSourceMetaFiles)
                    {
                        foreach (string metaFile in System.IO.Directory.GetFiles(newDestinationDir, "*.meta"))
                        {
                            System.IO.File.Delete(metaFile);
                        }
                    }
                }
            }
        }

        #endregion

        #region Scripts
        /// <summary>
        /// Forces Unity to clear the build cache and recompile all script assemblies in the project.
        /// </summary>
        public static void ForceRecompileScripts()
        {
            #if UNITY_2021_1_OR_NEWER
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
            #elif UNITY_2019_3_OR_NEWER
            CompilationPipeline.RequestScriptCompilation();
            #else
            System.Type editorCompilationInterface = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
            if (editorCompilationInterface != null)
            {
                System.Reflection.BindingFlags staticBindingFlags = System.Reflection.BindingFlags.Static |
                                                                    System.Reflection.BindingFlags.Public |
                                                                    System.Reflection.BindingFlags.NonPublic;
                System.Reflection.MethodInfo dirtyAllScriptsMethod = editorCompilationInterface.GetMethod("DirtyAllScripts", staticBindingFlags);
                dirtyAllScriptsMethod?.Invoke(null, null);
            }
            #endif
        }
        #endregion
    }
}
