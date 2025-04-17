using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// A collection of utilities for detecting the presence and mode of operation of the
    /// External Dependency Manager for Unity.
    /// </summary>
    internal static class EmbraceEdmUtility
    {
        public const string EDM_MANUAL_OVERRIDE_TRUE = "EMBRACE_USE_EDM_TRUE";
        public const string EDM_MANUAL_OVERRIDE_FALSE = "EMBRACE_USE_EDM_FALSE";

        public const string EDM_PRESENT_PROPERTY_KEY = "embrace.externalDependencyManager";
        public const string EDM_LOCAL_FILE_DEPS_PROPERTY_KEY = "embrace.edmLocalFileDependencies";
        public const string EDM_VERSION_KEY = "embrace.edmVersion";

        internal const string ANDROID_SDK_LIB_NAME = "io.embrace.embrace-android-sdk";
        internal const string EDM_LIBRARY_TAG = " l:Gpsr";
        internal const string DEFAULT_LIBRARY_TAG = " t:DefaultAsset";

        private const string EDM_VERSION_TYPE_NAMESPACE = "Google";
        private const string EDM_VERSION_TYPE_NAME = "AndroidResolverVersionNumber";
        private const string EDM_VERSION_GETTER_NAME = "Value";
        
        private const string DEPENDENCIES_FILE_NAME = "EmbraceSDKDependencies.xml";
        
        /// <summary>
        /// Detects the presence and settings of the EDM and writes the results to a gradle.properties file.
        /// </summary>
        /// <param name="mainTemplate">The content of the main template file.</param>
        public static IList<KeyValuePair<string, string>> GetEdmProperties(string mainTemplate)
        {
            #if EMBRACE_USE_EDM_TRUE // User override EDM on
            KeyValuePair<string, string>[] edmGradleProperties = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(EDM_PRESENT_PROPERTY_KEY, "true"),
            };
            #elif EMBRACE_USE_EDM_FALSE // User override EDM off
            KeyValuePair<string, string>[] edmGradleProperties = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(EDM_PRESENT_PROPERTY_KEY, "false"),
            };
            #else // Auto detect EDM
            IList<KeyValuePair<string, string>> edmGradleProperties = DetectEdmSettings(mainTemplate);
            #endif

            return edmGradleProperties;

        }

        private static IList<KeyValuePair<string, string>> DetectEdmSettings(string mainTemplate)
        {
            bool isMainTemplatePatchedByEdm = (
                mainTemplate != null &&
                EmbraceGradleUtility.TryParseDependencyVersion(mainTemplate, EmbraceGradleUtility.ANDROID_SDK_DEPENDENCY, out string _));

            List<string> candidateAssetGuids = new List<string>();
            candidateAssetGuids.AddRange(AssetDatabase.FindAssets(GetSearchQueryForEdmLibrary(ANDROID_SDK_LIB_NAME)));
            candidateAssetGuids.AddRange(AssetDatabase.FindAssets(GetSearchQueryForDefaultLibrary(ANDROID_SDK_LIB_NAME)));

            bool edmLocalFileDependencies = ProjectContainsDownloadedAndroidDependencies(ANDROID_SDK_LIB_NAME, candidateAssetGuids, out string _);

            List<KeyValuePair<string, string>> edmGradleProperties = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(EDM_PRESENT_PROPERTY_KEY, (isMainTemplatePatchedByEdm || edmLocalFileDependencies) ? "true" : "false"),
                new KeyValuePair<string, string>(EDM_LOCAL_FILE_DEPS_PROPERTY_KEY, edmLocalFileDependencies ? "true" : "false"),
            };

            if (TryGetEdmVersion(out string edmVersion))
            {
                edmGradleProperties.Add(new KeyValuePair<string, string>(EDM_VERSION_KEY, edmVersion));
            }

            return edmGradleProperties;
        }

        /// <summary>
        /// Returns true if the AssetDatabase contains the dependency.
        /// </summary>
        /// <param name="dependency">The dependency name.</param>
        /// <param name="candidateAssetGuids">The assets GUIDs used to search the downloaded dependencies.</param>
        /// <param name="version">The version string returned by the EDM, or null if reflection fails.</param>
        public static bool ProjectContainsDownloadedAndroidDependencies(string dependency, List<string> candidateAssetGuids, out string version)
        {
            foreach (string assetGuid in candidateAssetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (assetPath.Contains(dependency) && assetPath.EndsWith("aar"))
                {
                    version = assetPath;
                    return true;
                }
            }

            version = null;
            return false;
        }

        /// <summary>
        /// Uses reflection to get the version of the EDM in use in the project, if any.
        /// </summary>
        /// <param name="version">The version string returned by the EDM, or null if reflection fails.</param>
        /// <returns>True of reflection successfully found the version number, otherwise false.</returns>
        public static bool TryGetEdmVersion(out string version)
        {
            const BindingFlags publicStaticGetterBindingFlags = BindingFlags.Static
                                                                | BindingFlags.Public
                                                                | BindingFlags.GetProperty;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                PropertyInfo versionProperty = assembly
                    .GetTypes()
                    .Where(t => t.Namespace == EDM_VERSION_TYPE_NAMESPACE && t.Name == EDM_VERSION_TYPE_NAME)
                    .Select(t => t.GetProperty(EDM_VERSION_GETTER_NAME, publicStaticGetterBindingFlags))
                    .FirstOrDefault(t => t != null);

                if (versionProperty == null) continue;
                
                version = versionProperty.GetValue(null).ToString();
                return true;
            }

            version = null;
            return false;
        }

        /// <summary>
        /// The EDM applies a "Gpsr" tag to all libraries it downloads, so we can use that tag to easily search for them.
        /// </summary>
        /// <param name="The name of the library to be tagged"></param>
        /// <returns>The name of the EDM tagged library</returns>
        public static string GetSearchQueryForEdmLibrary(string library)
        {
            return library + EDM_LIBRARY_TAG;
        }

        /// <summary>
        /// In case the EDM fails to tag a library, we'll also search by type (aars will use the `DefaultAsset` import type).
        /// </summary>
        /// <param name="The name of the library to be tagged"></param>
        /// <returns>The default name of the tagged library</returns>
        public static string GetSearchQueryForDefaultLibrary(string library)
        {
            return library + DEFAULT_LIBRARY_TAG;
        }
        
        /// <summary>
        /// Generates the Android SDK dependencies XML file used to override the Swazzler version and the External
        /// Dependency Manager to resolve dependencies.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void GenerateDependenciesFile()
        {
            var dependenciesXMLData = $"\n<dependencies>\n  <androidPackages>\n    <repositories>\n      <repository>https://repo.maven.apache.org/maven2</repository>\n      <repository>https://dl.google.com/dl/android/maven2</repository>\n    </repositories>\n    \n    <androidPackage spec=\"io.embrace:embrace-android-sdk:{VersionsRepository.ANDROID_SDK_VERSION}\">\n    </androidPackage>\n    <androidPackage spec=\"io.embrace:embrace-android-okhttp3:{VersionsRepository.ANDROID_SDK_VERSION}\">\n    </androidPackage>\n  </androidPackages>\n</dependencies>\n";
            var packagePath = Path.Combine("Packages", "io.embrace.sdk", "Editor");
            var filePath = Path.Combine(packagePath, DEPENDENCIES_FILE_NAME);
            
            // Creates the file if it doesn't exist or if the file has been modified.
            if (!File.Exists(filePath))
            {
                SaveDependenciesFile(dependenciesXMLData, filePath);
                return;
            }

            if (!IsFileAlreadyCreated(dependenciesXMLData, filePath))
            {
                SaveDependenciesFile(dependenciesXMLData, filePath);
                return;
            }

            EmbraceLogger.Log("The Embrace SDK Dependencies XML file already exists.");
        }

        internal static bool SaveDependenciesFile(string dependenciesXMLData, string filePath)
        {
            try
            {
                var dependenciesDoc = new XmlDocument();
                dependenciesDoc.LoadXml(dependenciesXMLData);
            
                var directory = Path.GetDirectoryName(filePath);
                if (directory != null && Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    EmbraceLogger.LogWarning("Failed to create the directory for the Embrace dependencies XML file.");
                    return false;
                }
                
                dependenciesDoc.Save(filePath);
                EmbraceLogger.Log("Embrace SDK Dependencies XML file has been generated and saved.");
                return true;
            }
            catch (XmlException ex)
            {
                EmbraceLogger.LogWarning("Failed to generate the Embrace dependencies XML file.", ex);
                return false;
            }
        }
        
        internal static bool IsFileAlreadyCreated(string newDependenciesXMLData, string filePath)
        {
            try
            {
                var existingDependenciesXMLData = File.ReadAllText(filePath);
                return newDependenciesXMLData.Equals(existingDependenciesXMLData);
            }
            catch (DirectoryNotFoundException ex)
            {
                EmbraceLogger.LogWarning($"Failed to find directory for the {filePath} file.", ex);
                return false;
            }
            catch (FileNotFoundException ex)
            {
                EmbraceLogger.LogWarning("Failed to read the Embrace dependencies XML file.", ex);
                return false;
            }
        }
    }
}
