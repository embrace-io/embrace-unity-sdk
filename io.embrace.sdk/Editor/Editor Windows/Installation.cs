using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using EmbraceSDK.Internal;
using Newtonsoft.Json;
using UnityEditor.Build;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Handles installation, setup of the Embrace SDK and adding Scoped Registries to Manifest.
    /// </summary>
    public static class Installation
    {
        public static (string, string)[] DEPRECATED_SYMBOLS
        {
            get => new (string, string)[]
            {
                ("EMBRACE_SILENCE_EDITOR_LOGS", 
                    $"{EmbraceLogger.EMBRACE_SILENCE_EDITOR_TYPE_LOG};" +
                    $"{EmbraceLogger.EMBRACE_SILENCE_EDITOR_TYPE_WARNING};" +
                    $"{EmbraceLogger.EMBRACE_SILENCE_EDITOR_TYPE_ERROR}"),
                ("EMBRACE_SILENCE_DEV_BUILD_LOGS", 
                    $"{EmbraceLogger.EMBRACE_SILENCE_DEV_TYPE_LOG};" +
                    $"{EmbraceLogger.EMBRACE_SILENCE_DEV_TYPE_WARNING};" +
                    $"{EmbraceLogger.EMBRACE_SILENCE_DEV_TYPE_ERROR}"),
                ("EMBRACE_SILENCE_RELEASE_BUILD_LOGS", 
                    $"{EmbraceLogger.EMBRACE_SILENCE_RELEASE_TYPE_LOG};" +
                    $"{EmbraceLogger.EMBRACE_SILENCE_RELEASE_TYPE_WARNING};" +
                    $"{EmbraceLogger.EMBRACE_SILENCE_RELEASE_TYPE_ERROR}")
            };
        }

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo sdkInfo = new EmbraceSdkInfo();

            if (targetFile != null)
            {
                sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);
            }
            else
            {
                // If we fail to load the EmbraceSdkInfo JSON file, most likely the AssetDatabase has not finished
                // refreshing. So we'll abort the install process and queue it up for the next editor update.
                EditorApplication.delayCall += InitializeOnLoad;
                return;
            }

            string lastUsedVersion = EmbraceProjectSettings.User.GetValue<string>(nameof(DeviceSDKInfo.version));

            if(string.IsNullOrEmpty(lastUsedVersion))
            {
                // If we fail to load a DeviceSDKInfo.json file, that means that either
                //  1. This is the first time Embrace has been imported to the project.
                //  2. This is the first time this project has been opened on this machine.
                //  3. The user manually cleared the DeviceSDKInfo.json file from their persistent data directory.
                lastUsedVersion = sdkInfo.version;
                EmbraceProjectSettings.User.SetValue<string>(nameof(DeviceSDKInfo.version), lastUsedVersion);

                // Since this branch could either be fresh import into a new project or a fresh pull to a new machine,
                // we can't assume that the EmbraceDataDirectory has not been set to a custom path. So, rather than
                // immediately setting it to its default value, we'll first try to load it and only set to default if
                // it is null or empty.
                if (string.IsNullOrEmpty(AssetDatabaseUtil.EmbraceDataDirectory))
                {
                    AssetDatabaseUtil.EmbraceDataDirectory = AssetDatabaseUtil.DefaultDataDirectory;
                }

                WelcomeEditorWindow.Init();
            }

            if (sdkInfo.version != lastUsedVersion)
            {
                // If the SDK version defined in the DeviceSDKInfo.json file does not match that in the package's
                // EmbraceSdkInfo.json file, that means that either
                //  1. This is the first time Embrace has initialized after being update from a different version.
                //  2. This user manually changed the version in their DeviceSDKInfo.json file.

                lastUsedVersion = sdkInfo.version;
                EmbraceProjectSettings.User.SetValue<string>(nameof(DeviceSDKInfo.version), lastUsedVersion);

                EmbraceGradleUtility.EnforceSwazzlerDependencyVersion();

                // If this is an upgrade, and no data directory is set, it implies the
                // previous version of the sdk was storing configs in the sdk package.
                // Versions 1.6.0 and forward store configs in the project Assets folder.
                // Any previously saved configs should be converted and relocated. This will not be
                // the case on a new install.
                if (string.IsNullOrEmpty(AssetDatabaseUtil.EmbraceDataDirectory))
                {
                    AssetDatabaseUtil.EmbraceDataDirectory = AssetDatabaseUtil.DefaultDataDirectory;
                    ConfigsRelocationUtil.ConvertDeprecatedData();
                    ConfigsRelocationUtil.RelocateAssets(AssetDatabaseUtil.SDKDirectory, AssetDatabaseUtil.DefaultDataDirectory, OnRelocationComplete);
                }
                else
                {
                    WelcomeEditorWindow.Init();
                }
            }
            else
            {
                // If the .json file stored in the Application.persistentDataPath exists and contains matching version numbers it indicates
                // a re-installation of the same SDK version. In this case, the existence of the .embrace root file should be checked.
                // If present, nothing else needs to be done. If missing, it should be created with the default data directory specified.
                if (string.IsNullOrEmpty(AssetDatabaseUtil.EmbraceDataDirectory))
                {
                    AssetDatabaseUtil.EmbraceDataDirectory = AssetDatabaseUtil.DefaultDataDirectory;
                }
            }

            if (!EmbraceProjectSettings.User.GetValue<bool>(nameof(DeviceSDKInfo.isManifestSetup)))
            {
                SetupManifest(sdkInfo);
            }

            CleanUpDeprecatedItems();
        }

        private static void OnRelocationComplete()
        {
            AssetDatabase.DeleteAsset($"{AssetDatabaseUtil.SDKDirectory}/Resources/Configurations");
            AssetDatabase.DeleteAsset($"{AssetDatabaseUtil.SDKDirectory}/Resources/Settings");
            AssetDatabase.Refresh();
            WelcomeEditorWindow.Init();
        }

        public static void SetupManifest(EmbraceSdkInfo embraceSdkInfo)
        {
            string manifestJson = "";
            Package package = null;
            try
            {
                string packageJson = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/package.json");
                package = JsonUtility.FromJson<Package>(packageJson);

                manifestJson = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json");
            }
            catch (FileNotFoundException)
            {
                EmbraceLogger.LogWarning($"Unable to load manifest json file from : {Application.dataPath.Replace("/Assets", "")}/Packages");
            }

            JObject parsedJson = JObject.Parse(manifestJson);

            // add Embrace dependency
            if (parsedJson["dependencies"] == null)
            {
                EmbraceLogger.LogWarning(
                    $"Your Manifest.json file is missing a dependencies property, you will need to add io.embrace.sdk as a dependency to your manifest manually. {Application.dataPath.Replace("/Assets", "")}/Packages/manifest.json");
                return;
            }

            if (parsedJson["dependencies"][package.name] == null)
            {
                JProperty newProperty = new JProperty(package.name, package.version);
                parsedJson["dependencies"].First.AddBeforeSelf(newProperty);
            }
            else
            {
                string parsedVersion = (string)parsedJson["dependencies"][package.name];
                // Write package version if existing version does not match and is not a local package
                if (parsedVersion != package.version && !parsedVersion.StartsWith("file"))
                {
                    parsedJson["dependencies"][package.name] = package.version;
                }
            }

            // Add Scoped Registry
            bool hasEmbraceScopedRegistry = false;
            if (parsedJson["scopedRegistries"] is JArray scopedRegistries)
            {
                foreach (JToken content in scopedRegistries)
                {
                    if ((string)content["name"] == package.name)
                    {
                        hasEmbraceScopedRegistry = true;
                        break;
                    }
                }

                if (!hasEmbraceScopedRegistry)
                {
                    List<string> scopes = new List<string>() { "io.embrace" };
                    ScopedRegistry embraceRegistry = new ScopedRegistry(package.name, embraceSdkInfo.npmAPIEndpoint, scopes.ToArray());

                    scopedRegistries.AddFirst(JToken.Parse(JsonUtility.ToJson(embraceRegistry)));
                }
            }
            else
            {
                // If the manifest file does not have a scopedRegistries property we need to add this property.
                // We do this by manually creating a scopedRegistries string and adding it before the dependencies property.
                List<string> scopes = new List<string>() { "io.embrace" };
                ScopedRegistry embraceRegistry = new ScopedRegistry(package.name, embraceSdkInfo.npmAPIEndpoint, scopes.ToArray());
                ScopedRegistries registries = new ScopedRegistries(embraceRegistry);

                string registriesJson = JsonUtility.ToJson(registries, true);
                registriesJson = registriesJson.Remove(0, 1);
                registriesJson = registriesJson.Remove(registriesJson.Length - 2, 2);
                registriesJson += "," + System.Environment.NewLine;

                string json = parsedJson.ToString((Newtonsoft.Json.Formatting)Formatting.Indented);
                int index = json.IndexOf("\"dependencies\": {");
                parsedJson = JObject.Parse(json.Insert(index, registriesJson));
            }

            EmbraceProjectSettings.User.SetValue<bool>(nameof(DeviceSDKInfo.isManifestSetup), true);
            EmbraceProjectSettings.User.Save();

            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json", parsedJson.ToString((Newtonsoft.Json.Formatting)Formatting.Indented));
        }

        private static void CleanUpDeprecatedItems()
        {
            ReplaceDeprecatedSilenceSymbols();
        }

        private static void ReplaceDeprecatedSilenceSymbols()
        {
            var replacer = new DeprecatedSymbolReplacer();

            #if UNITY_2021_3_OR_NEWER
            replacer.ReplaceDeprecatedSymbolsForBuildTarget(NamedBuildTarget.Android);
            replacer.ReplaceDeprecatedSymbolsForBuildTarget(NamedBuildTarget.iOS);
            #else
            replacer.ReplaceDeprecatedSymbolsForBuildTarget(BuildTargetGroup.Android);
            replacer.ReplaceDeprecatedSymbolsForBuildTarget(BuildTargetGroup.iOS);
            #endif
        }

        private class DeprecatedSymbolReplacer
        {
            #if UNITY_2021_3_OR_NEWER
            public void ReplaceDeprecatedSymbolsForBuildTarget(NamedBuildTarget target)
            {
                
                var symbols = PlayerSettings.GetScriptingDefineSymbols(target);
                DEPRECATED_SYMBOLS.ToList().ForEach((symbolMapping) =>
                {
                    var (deprecatedSymbol, newSymbol) = symbolMapping;
                    symbols = symbols.Replace(deprecatedSymbol, newSymbol);
                });
                PlayerSettings.SetScriptingDefineSymbols(target, symbols);
            }
            #else
            public void ReplaceDeprecatedSymbolsForBuildTarget(BuildTargetGroup group)
            {
                var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                DEPRECATED_SYMBOLS.ToList().ForEach((symbolMapping) =>
                {
                    var (deprecatedSymbol, newSymbol) = symbolMapping;
                    symbols = symbols.Replace(deprecatedSymbol, newSymbol);
                });
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
            }
            #endif
        }
    }
}