using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.Build;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Gathers line number and method name mapping data from the il2cpp build process and copies it into the exported
    /// XCode and Android projects. This data is used by the Embrace SDK to map native stack traces to C# source files.
    ///
    /// This behavior can be disabled by adding the EMBRACE_DISABLE_IL2CPP_SYMBOL_MAPPING define to the project.
    /// </summary>
    internal static class EmbraceIl2CppSymbolUtility
    {
        // The Android Swazzler will attempt to upload il2cpp symbol mapping data when this property is set to true.
        internal const string SWAZZLER_FEATURE_GRADLE_PROPERTY = "embrace.uploadIl2CppMappingFiles";

        // Subdirectory within the unityLibrary gradle project where we will write the symbol mapping data.
        // This path is rooted in <gradle_output_directory>/unityLibrary because that is the path that unity
        // provides to the OnPostGenerateGradleAndroidProject callback.
        private const string GRADLE_MAPPING_INFO_DIRECTORY =
            "src/main/il2cppOutputProject/Source/il2cppOutput/Symbols";

        // Subdirectory within a XCode project where we will write the symbol mapping data
        private const string XCODE_MAPPING_INFO_DIRECTORY = "Classes/Native/Symbols";

        // This option instructs il2cpp to emit line number mappings and source info comments.
        private const string LINE_MAPPING_FLAG = "--emit-source-mapping";

        // This option instructions il2cpp to emit a TSV file mapping C++ method names to C# method and assembly names.
        private const string METHOD_MAPPING_FLAG = "--emit-method-map";

        // File name and extension for the JSON file that maps C++ source file name and line numbers to C# equivalents.
        internal const string LINE_MAPPING_FILE_NAME = "LineNumberMappings.json";

        // File name and extension for the TSV file that maps C++ symbol names to their C# equivalents.
        internal const string METHOD_MAPPING_FILE_NAME = "MethodMap.tsv";

        // The name of the directory where il2cpp will write its symbol mapping data.
        private const string SYMBOL_FOLDER_NAME = "Symbols/";

        // Regex to match source info comments in il2cpp output. Expected format:
        //
        //    //<source_info:/path/to/c-sharp/source.cs:line_number>
        //
        // Some earlier versions of Unity include a space between the // and <, like so:
        //
        //    // <source_info:/path/to-c-sharp/source.cs:line_number>
        //
        private static Regex sourceInfoRegex = new Regex("//\\s?<source_info:(.*):(.*)>", RegexOptions.Compiled);

        // The index of the soure file group in the sourceInfoRegex
        private const int SOURCE_FILE_GROUP_INDEX = 1;

        // The index of the line number in the sourceInfoRegex
        private const int LINE_NUMBER_GROUP_INDEX = 2;

        internal static void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
#if !EMBRACE_DISABLE_IL2CPP_SYMBOL_MAPPING
            if (report.summary.platform == BuildTarget.Android || report.summary.platform == BuildTarget.iOS)
            {
                EnableBuildMetadata();

                // If the current build directory already contains an existing build Unity may not delete
                // the existing il2cpp metadata. We need to delete it ourselves to ensure that the correct data
                // is copied by the post build process.
                DeleteStaleData(report.summary.outputPath);
            }
#endif
        }

        /// <summary>
        /// Add additional args in PlayerSettings to instruct il2cpp to emit source and method mapping metadata.
        /// </summary>
        internal static void EnableBuildMetadata()
        {
            string existingSettings = PlayerSettings.GetAdditionalIl2CppArgs();

            bool modified = false;
            StringBuilder sb = new StringBuilder(existingSettings);
            if (!existingSettings.Contains(LINE_MAPPING_FLAG))
            {
                AppendArgumentWithSpace(sb, LINE_MAPPING_FLAG);
                modified = true;
            }

            if (!existingSettings.Contains(METHOD_MAPPING_FLAG))
            {
                AppendArgumentWithSpace(sb, METHOD_MAPPING_FLAG);
                modified = true;
            }

            if (modified)
            {
                PlayerSettings.SetAdditionalIl2CppArgs(sb.ToString());
            }
        }

        /// <summary>
        /// Appends the argument to the string builder with a space prefix.
        /// </summary>
        private static void AppendArgumentWithSpace(StringBuilder sb, string argument)
        {
            sb.Append(" ");
            sb.Append(argument);
        }

        /// <summary>
        /// Deletes existing il2cpp symbol metadata from a previous build.
        /// </summary>
        private static void DeleteStaleData(string buildDirectory)
        {
            string finalSymbolOutpuDirectory = GetFinalIl2CppSymbolOutputPath(buildDirectory);
            if (Directory.Exists(finalSymbolOutpuDirectory))
            {
                Directory.Delete(finalSymbolOutpuDirectory, true);
            }
        }

        /// <summary>
        /// Gathers il2cpp symbol metadata and copies it into the exported build to be uploaded by the native SDKs.
        /// </summary>
        public static bool AssembleSourceMappingInfo(string buildDirectory)
        {
#if EMBRACE_DISABLE_IL2CPP_SYMBOL_MAPPING
            return false;
#else
#if UNITY_ANDROID
#if UNITY_2021_3_OR_NEWER
            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
#else
            if(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
#endif
            {
                return false;
            }
#endif

            string finalSymbolOutputDirectoryPath = GetFinalIl2CppSymbolOutputPath(buildDirectory);
            string il2cppSourceOutputPath = GetIl2CppSourceOutputPath(buildDirectory);
            if (!Directory.Exists(finalSymbolOutputDirectoryPath))
            {
                // Unity didn't copy the il2cpp symbols to the gradle/xcode project, so we need to grab them from the Library
                string librarySymbolsDirectory = Path.Combine(il2cppSourceOutputPath, SYMBOL_FOLDER_NAME);
                if (!Directory.Exists(librarySymbolsDirectory))
                {
                    // Couldn't find the symbols in the Library either, so abort the process.
#if EMBRACE_POST_BUILD_ASSERTS
                    Debug.LogAssertion(
                        $"Unable to locate il2cpp mapping symbols at {finalSymbolOutputDirectoryPath} or {librarySymbolsDirectory}. Native stack traces will not be translated to C# symbol names.");
#endif
                    EmbraceLogger.LogWarning(
                        $"Unable to locate il2cpp mapping symbols at {finalSymbolOutputDirectoryPath} or {librarySymbolsDirectory}. Native stack traces will not be translated to C# symbol names.");
                    return false;
                }

                AssetDatabaseUtil.CopyDirectory(librarySymbolsDirectory, finalSymbolOutputDirectoryPath, true, false);
            }

            bool hasLineMappings = true;
            bool hasMethodMappings = true;

            string lineMappingFilePath = Path.Combine(finalSymbolOutputDirectoryPath, LINE_MAPPING_FILE_NAME);
            FileInfo lineMappingFile = new FileInfo(lineMappingFilePath);
            // Older versions of Unity create an empty line mapping file, so if it appears that is the case here we will
            // regenerate it by parsing the source_info comments in the il2cpp generated source code.
            if (!lineMappingFile.Exists || lineMappingFile.Length < 10)
            {
                EmbraceLogger.Log($"Regenerating il2cpp line number mappings. Output path: {lineMappingFilePath}");
                ParseLineMappingsFromSourceInfo(il2cppSourceOutputPath, lineMappingFilePath);

                lineMappingFile.Refresh();
                if (!lineMappingFile.Exists || lineMappingFile.Length < 10)
                {
                    hasLineMappings = false;
#if EMBRACE_POST_BUILD_ASSERTS
                    Debug.Assert(lineMappingFile.Exists && lineMappingFile.Length > 10,
                        "Failed to regenerate il2cpp line number mappings.");
#endif
                }
            }

            // Verify that the method mapping file exists. We expect all versions of Unity to be able to generate this
            // file, so we do not attempt to regenerate it.
            string methodMappingFilePath = Path.Combine(finalSymbolOutputDirectoryPath, METHOD_MAPPING_FILE_NAME);
            if (!File.Exists(methodMappingFilePath))
            {
                hasMethodMappings = false;

#if EMBRACE_POST_BUILD_ASSERTS
                Debug.LogAssertion(
                    $"Failed to locate il2cpp method mapping file at {methodMappingFilePath}. Native stack traces will not be converted to C# symbols.");
#endif
                EmbraceLogger.LogWarning(
                    $"Failed to locate il2cpp method mapping file at {methodMappingFilePath}. Native stack traces will not be converted to C# symbols.");
            }

            return hasLineMappings || hasMethodMappings;
#endif
        }

        /// <summary>
        /// Parses source info comments in generated il2cpp source and creates a JSON file of line mappings from
        /// C++ to C# source.
        /// </summary>
        /// <param name="sourceDirectory">The directory containing the il2cpp source code.</param>
        /// <param name="outputPath">The path to write the JSON mappings file to.</param>
        internal static void ParseLineMappingsFromSourceInfo(string sourceDirectory, string outputPath)
        {
            // The structure of the mappings json file is as follows:
            //
            //  {
            //      "C++ Source File Path": {
            //          "C# Source File Path": {
            //              "C++ Line Number": "C# Line Number"
            //          }
            //      }
            //  }
            //
            // We use 3 deep nested dictionaries to represent this structure, then serialize it to JSON.
            Dictionary<string, Dictionary<string, Dictionary<int, int>>> allMappings =
                new Dictionary<string, Dictionary<string, Dictionary<int, int>>>();
            foreach (string filePath in Directory.EnumerateFiles(sourceDirectory, "*.cpp"))
            {
                string[] fileContents = File.ReadAllLines(filePath);

                Dictionary<string, Dictionary<int, int>> fileMappings = null;

                for (int i = 0; i < fileContents.Length; ++i)
                {
                    if (!TryParsePathFromSourceInfoComment(fileContents[i], out string originalSourcePath,
                            out int lineNumber))
                    {
                        continue;
                    }

                    if (fileMappings == null)
                    {
                        fileMappings = new Dictionary<string, Dictionary<int, int>>();
                        allMappings[filePath] = fileMappings;
                    }

                    if (!fileMappings.TryGetValue(originalSourcePath, out Dictionary<int, int> lineMappings))
                    {
                        lineMappings = new Dictionary<int, int>();
                        fileMappings[originalSourcePath] = lineMappings;
                    }

                    lineMappings[i + 1] = lineNumber;
                }
            }

            string serializedMappings = JsonConvert.SerializeObject(allMappings, Formatting.None);

            File.WriteAllText(outputPath, serializedMappings);
        }

        /// <summary>
        /// Attempts to parse the string as an il2cpp source info comment.
        /// </summary>
        /// <param name="comment">The string containing the potential source info comment</param>
        /// <param name="path">If successfully parsed, the path to the equivalent C# source file. Otherwise null.</param>
        /// <param name="lineNumber">If successfully parsed, the line number in the C# source file that corresponds to
        /// the following line in the C++ source. Otherwise -1.</param>
        /// <returns>True if successfully parsed as a source info comment, false otherwise.</returns>
        private static bool TryParsePathFromSourceInfoComment(string comment, out string path, out int lineNumber)
        {
            Match match = sourceInfoRegex.Match(comment);

            if (match.Success)
            {
                path = match.Groups[SOURCE_FILE_GROUP_INDEX].Value;
                lineNumber = int.Parse(match.Groups[LINE_NUMBER_GROUP_INDEX].Value);
                return true;
            }

            path = null;
            lineNumber = -1;
            return false;
        }

        internal static string GetFinalIl2CppSymbolOutputPath(string buildOutputPath)
        {
#if UNITY_IOS
            return Path.Combine(buildOutputPath, XCODE_MAPPING_INFO_DIRECTORY);
#elif UNITY_ANDROID
            return Path.Combine(buildOutputPath, GRADLE_MAPPING_INFO_DIRECTORY);
#else
            return buildOutputPath;
#endif
        }

        /// <summary>
        /// Resolves the path to the Temp or Library subdirectory in which Unity exported the il2cpp source code
        /// </summary>
        internal static string GetIl2CppSourceOutputPath(string buildOutputPath)
        {
#if UNITY_IOS
#if UNITY_2022_1_OR_NEWER
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Library/Bee/artifacts/iOS/il2cppOutput/cpp/");
#else
            return Path.Combine(buildOutputPath, "Classes/Native");
#endif
#elif UNITY_ANDROID
        #if UNITY_2021_1_OR_NEWER
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Library/Bee/artifacts/Android/il2cppOutput/cpp/");
        #elif UNITY_2020_1_OR_NEWER
            string cachePath = Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Library/Il2cppBuildCache/Android");
            DirectoryInfo cacheDirectory = new DirectoryInfo(cachePath);
            DirectoryInfo sourceDirectory = cacheDirectory.EnumerateDirectories()
                .FirstOrDefault((d) => Directory.Exists(Path.Combine(d.FullName, "il2cppOutput/Symbols")));
            if (sourceDirectory == null || !sourceDirectory.Exists)
            {
                DirectoryInfo[] subDirs = cacheDirectory.GetDirectories();
                if (subDirs.Length > 0)
                {
                    return Path.Combine(subDirs[0].FullName, "il2cppOutput/");
                }

                return Path.Combine(cachePath, "il2cppOutput/");
            }
            return Path.Combine(sourceDirectory.FullName, "il2cppOutput/");
        #else
            if (Application.platform == RuntimePlatform.LinuxEditor)
            {
                return Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Temp/StagingArea/Il2Cpp/il2cppOutput");
            }
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Temp/StagingArea/Il2cpp/il2cppOutput");
        #endif
#else
            return buildOutputPath;
#endif
        }
    }
}