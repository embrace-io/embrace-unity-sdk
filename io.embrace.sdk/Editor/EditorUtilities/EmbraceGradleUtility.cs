using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal.VersionControl;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Utility class containing functions for working with Unity's gradle template files.
    /// </summary>
    internal static class EmbraceGradleUtility
    {
        public const string EMBRACE_DISABLE_SWAZZLER_VERSION_UPDATE = nameof(EMBRACE_DISABLE_SWAZZLER_VERSION_UPDATE);

        // File names/paths
        private const string EDM_DEPENDENCY_XML_FILE_NAME = "EmbraceSDKDependencies";
        private const string BASE_PROJECT_GRADLE_TEMPLATE_PATH = "Plugins/Android/baseProjectTemplate.gradle";
        private const string LAUNCHER_TEMPLATE_PATH = "Plugins/Android/launcherTemplate.gradle";
        private const string MAIN_GRADLE_TEMPLATE_PATH = "Plugins/Android/mainTemplate.gradle";
        private const string GRADLE_PROPERTIES_FILE_NAME = "gradle.properties";
        private const string GRADLE_PROPERTIES_TEMPLATE_PATH = "Plugins/Android/gradleTemplate.properties";
#if UNITY_2022_2_OR_NEWER
        private const string SETTINGS_TEMPLATE_PATH = "Plugins/Android/settingsTemplate.gradle";
#endif

        // Regex groups
        private const string DEPENDENCY_GROUP_NAME = "dependency";
        private const string VERSION_GROUP_NAME = "version";

        // Dependencies
#if EMBRACE_ENABLE_BUGSHAKE_FORM
        public const string SWAZZLER_DEPENDENCY = "io.embrace:embrace-bug-shake-gradle-plugin";
#else
        public const string SWAZZLER_DEPENDENCY = "io.embrace:embrace-swazzler";
#endif
#if EMBRACE_ENABLE_BUGSHAKE_FORM
        public const string ANDROID_SDK_DEPENDENCY = "io.embrace:embrace-bug-shake-gradle-plugin";
#else
        public const string ANDROID_SDK_DEPENDENCY = "io.embrace:embrace-android-sdk";
#endif

        public static string BaseProjectTemplatePath { get; } = Path.Combine(Application.dataPath, BASE_PROJECT_GRADLE_TEMPLATE_PATH);
        public static string LauncherTemplatePath { get; } = Path.Combine(Application.dataPath, LAUNCHER_TEMPLATE_PATH);
        public static string GradlePropertiesPath { get; } = Path.Combine(Application.dataPath, GRADLE_PROPERTIES_TEMPLATE_PATH);
#if UNITY_2022_2_OR_NEWER
        public static string SettingsTemplatePath { get; } = Path.Combine(Application.dataPath, SETTINGS_TEMPLATE_PATH);
#endif
        public static string MainTemplatePath { get; } = Path.Combine(Application.dataPath, MAIN_GRADLE_TEMPLATE_PATH);

        /// <summary>
        /// Parses the Android SDK dependency version defined in EmbraceSDKDependencies.xml and the swazzler version
        /// defined in baseProjectTemplate.gradle and updates the gradle template if they do not match.
        /// </summary>
        public static void EnforceSwazzlerDependencyVersion()
        {
            // The EMBRACE_DISABLE_SWAZZLER_VERSION_UPDATE can be defined in Unity player settings to disable this
            // functionality. We also disable it when the active build target is not Android or iOS because our symbols
            // are only applied to those platforms.
#if !EMBRACE_DISABLE_SWAZZLER_VERSION_UPDATE && (UNITY_IOS || UNITY_TVOS || UNITY_ANDROID)
            if (!TryParseEdmAndroidSdkDependencyVersion(out string xmlVersion) ||
                !TryReadGradleTemplate(BaseProjectTemplatePath, out string gradleSource, logWarningIfFileNotPresent: true))
            {
                EmbraceLogger.LogWarning($"EmbraceGradleUtility failed to verify Embrace Android SDK and Swazzler versions.");
                return;
            }

            if (!TryParseDependencyVersion(gradleSource, SWAZZLER_DEPENDENCY, out string gradleVersion))
            {
                EmbraceLogger.LogWarning($"Failed to parse Embrace Swazzler version from {BaseProjectTemplatePath}. Please confirm you have added the swazzler to your gradle template.");
                return;
            }

            if (gradleVersion != xmlVersion)
            {
                try
                {
                    string newGradleText = ReplaceDependencyVersion(gradleSource, SWAZZLER_DEPENDENCY, xmlVersion);
                    File.WriteAllText(BaseProjectTemplatePath, newGradleText);
                    EmbraceLogger.Log(
                        $"Updated embrace-swazzler version from {gradleVersion} to {xmlVersion} in Assets/{BASE_PROJECT_GRADLE_TEMPLATE_PATH}.");
                }
                catch (System.Exception e)
                {
                    EmbraceLogger.LogError($"EmbraceGradleUtility encountered {e.GetType().Name} while updating swazzler version: {e.Message}");
                }
            }
#endif
        }

        /// <summary>
        /// Verifies that the swazzler and bugshake are not present simultaneously in the gradle template.
        /// </summary>
        public static void VerifyIfSwazzlerAndBugshakeArePresentSimultaneously()
        {
            if (TryReadGradleTemplate(BaseProjectTemplatePath, out string gradleSource, logWarningIfFileNotPresent: true))
            {
#if EMBRACE_ENABLE_BUGSHAKE_FORM
                if (gradleSource.Contains("io.embrace:embrace-swazzler"))
                {
                    throw new UnityEditor.Build.BuildFailedException($"EmbraceGradleUtility found the embrace-swazzler classpath in " +
                                             $"{BaseProjectTemplatePath}. The embrace-bug-shake-gradle-plugin is not compatible " +
                                             $"with embrace-swazzler and should not run simultaneously. Please remove the embrace-swazzler classpath from {BaseProjectTemplatePath} and build again.");
                }
#else 
                if (gradleSource.Contains("io.embrace:embrace-bug-shake-gradle-plugin"))
                {
                    throw new UnityEditor.Build.BuildFailedException("EmbraceGradleUtility found the embrace-bug-shake-gradle-plugin classpath in " +
                                                                     $"{BaseProjectTemplatePath}. The embrace-swazzler is not compatible " +
                                                                     $"with embrace-bug-shake-gradle-plugin and should not run simultaneously." +
                                                                     $"Please remove the embrace-bug-shake-gradle-plugin classpath from {BaseProjectTemplatePath} and build again.");
                }
#endif
            }
        }

        /// <summary>
        /// Tries to parse the dependency version for the Android SDK defined in the package's EmbraceSDKDependencies.xml
        /// </summary>
        /// <param name="version">The version of the Android SDK dependency, or null if parsing fails.</param>
        /// <returns>True if version was successfully parsed, otherwise false.</returns>
        public static bool TryParseEdmAndroidSdkDependencyVersion(out string version)
        {
            version = null;

            string[] assets = AssetDatabase.FindAssets(EDM_DEPENDENCY_XML_FILE_NAME);
            if (assets.Length != 1)
            {
                EmbraceLogger.LogWarning($"Found {assets.Length} assets matching EmbraceSDKDependencies.xml");
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(assets[0]);
            TextAsset xml = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            bool success = TryParseDependencyVersion(xml.text, ANDROID_SDK_DEPENDENCY, out version);
            if (!success)
            {
                EmbraceLogger.LogError($"Failed to parse Android SDK dependency version from {EDM_DEPENDENCY_XML_FILE_NAME}");
            }

            return success;
        }

        /// <summary>
        /// Attempts to open and read the contents of the given gradle template file.
        /// </summary>
        /// <param name="templatePath">The path to the gradle template file.</param>
        /// <param name="templateContent">The contents of the file will be assigned to this string reference if successful.</param>
        /// <param name="logWarningIfFileNotPresent">If true, will log a warning to the console if the file does not exist at the given path.</param>
        /// <returns>True if the file is successfully read, otherwise false.</returns>
        public static bool TryReadGradleTemplate(string templatePath, out string templateContent, bool logWarningIfFileNotPresent = false)
        {
            if (!File.Exists(templatePath))
            {
                if (logWarningIfFileNotPresent)
                {
                    EmbraceLogger.LogWarning($"No gradle template found at {templatePath}. Please create a custom gradle template in Unity's Player Settings.");
                }
                templateContent = null;
                return false;
            }

            try
            {
                templateContent = File.ReadAllText(templatePath);
                return true;
            }
            catch (Exception e)
            {
                EmbraceLogger.LogError($"Failed to parse contents of {templatePath} with error: {e.Message}");
                templateContent = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to parse the version of the given dependency defined in <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text that contains the dependency.</param>
        /// <param name="dependency">The dependency version to parse (for example, "io.embrace:embrace-swazzler").</param>
        /// <param name="version">The version of the dependency, or null if parsing fails.</param>
        /// <returns>True if parsing was successful, otherwise false.</returns>
        public static bool TryParseDependencyVersion(string text, string dependency, out string version)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(dependency))
            {
                version = null;
                return false;
            }

            Regex regex = GetDependencyVersionRegex(dependency);
            Match match = regex.Match(text);

            if (match.Success)
            {
                version = match.Groups[VERSION_GROUP_NAME].Value;
                return true;
            }

            version = null;
            return false;
        }

        /// <summary>
        /// Replaces the version of an Android dependency in a given string.
        /// </summary>
        /// <param name="text">The text containing the Android dependency.</param>
        /// <param name="dependency">The dependency to update.</param>
        /// <param name="newVersion">The new version of the dependency.</param>
        /// <returns>The text with the updated dependency, or <paramref name="text"/> if the operation failed.</returns>
        public static string ReplaceDependencyVersion(string text, string dependency, string newVersion)
        {
            if (string.IsNullOrWhiteSpace(text) || dependency == null || newVersion == null)
            {
                return text;
            }

            Regex regex = GetDependencyVersionRegex(dependency);
            return regex.Replace(text, (match) => $"{match.Groups[DEPENDENCY_GROUP_NAME]}{newVersion}");
        }

        /// <summary>
        /// Writes the Embrace gradle properties to the gradle.properties file.
        /// </summary>
        /// <param name="gradleFilePath">The properties.gradle file path</param>
        /// <param name="propertiesToWrite">The properties to write to the file.</param>
        public static void WriteEmbraceGradleProperties(string gradleFilePath, IList<KeyValuePair<string, string>> propertiesToWrite)
        {
            WriteGradlePropertiesToFile(Path.Combine(gradleFilePath, GRADLE_PROPERTIES_FILE_NAME), propertiesToWrite);
        }

        /// <summary>
        /// Sets the given property values in the gradle properties file at the given path.
        ///
        /// If the properties already exist in the file, their values will be replaced. If they do not exist,
        /// they will be appended at the end of the file.
        /// </summary>
        /// <param name="gradleFilePath">The path to the gradle file.</param>
        /// <param name="gradleProperties">The properties to write on the gradle file.</param>
        public static void WriteGradlePropertiesToFile(string gradleFilePath, IList<KeyValuePair<string, string>> gradleProperties)
        {
            try
            {
                if (gradleProperties == null || gradleProperties.Count == 0)
                {
                    return;
                }

                List<string> existingProperties = new List<string>();
                existingProperties.AddRange(File.ReadAllLines(gradleFilePath));

                foreach (KeyValuePair<string, string> property in gradleProperties)
                {
                    if (string.IsNullOrEmpty(property.Key) || string.IsNullOrWhiteSpace(property.Value))
                    {
                        continue;
                    }

                    bool propertyExists = false;
                    string propertyString = $"{property.Key}={property.Value}";
                    for (int i = 0; i < existingProperties.Count; ++i)
                    {
                        string key = existingProperties[i].Split('=')[0].Trim();
                        if (key == property.Key)
                        {
                            propertyExists = true;
                            existingProperties[i] = propertyString;
                            break;
                        }
                    }

                    if (!propertyExists)
                    {
                        existingProperties.Add(propertyString);
                    }
                }

                File.WriteAllLines(gradleFilePath, existingProperties);
            }
            catch (Exception e)
            {
                EmbraceLogger.LogError($"Encountered {e.GetType().Name} while writing properties to {gradleFilePath}: {e.Message}");
                throw;
            }
        }

        private static Regex GetDependencyVersionRegex(string dependency)
        {
            // Regex expecting an android dependency like "io.embrace:embrace-swazzler:5.9.0"
            // Splits the match into two groups:
            //      - dependency: io.embrace:embrace-swazzler:
            //      - version: 5.9.0
            return new Regex($"(?<{DEPENDENCY_GROUP_NAME}>{dependency}\\:?)(?<{VERSION_GROUP_NAME}>[^\"\']+)");
        }
    }
}
