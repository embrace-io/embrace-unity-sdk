using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
#if UNITY_ANDROID
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor.Android;
    using Newtonsoft.Json;

    public class EmbracePostBuildProcessor : IPostGenerateGradleAndroidProject
    {
        public const string EMBRACE_SYSTEM_ENV_VAR = "EMBRACE_ENVIRONMENTS_INDEX";
        public const string EMBRACE_CUSTOM_SYMBOLS_PATTERN = @"embrace\s*{[^}]*customSymbolsDirectory\.set\(\s*""(?<path>[^""]*)""\s*\)[^}]*}";
        public const string SYMBOLS_DIR = "symbols";
        public const string ARCH_DIR = "arm64-v8a";

        public const string EMBRACE_CUSTOM_SYMBOLS_PROP = @"embrace {{ customSymbolsDirectory.set(""{0}"") }}";

        public int callbackOrder
        {
            get { return 0; }
        }

        // Android gradle fixup
        public void OnPostGenerateGradleAndroidProject(string projectPath)
        {
            // Unity has historically been inconsistent with whether the projectPath is the path to the root of the gradle
            // project or the unityLibrary directory within the project.
            string gradleProjectRootPath = projectPath;
            #if DeveloperMode
            // Solely available for internal debugging purposes in case this fails.
            Debug.Log($"gradle path: {gradleProjectRootPath}");
            #endif
            DirectoryInfo gradleProjectRootDirectory = new DirectoryInfo(gradleProjectRootPath);
            if (gradleProjectRootDirectory.Name == "unityLibrary")
            {
                gradleProjectRootPath = gradleProjectRootDirectory.Parent.FullName;
            }

            WriteEmbraceConfig(gradleProjectRootPath);

            EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.MainTemplatePath, out string mainTemplate);

            var gradlePropertiesWriteBuffer = new List<KeyValuePair<string, string>>();

            gradlePropertiesWriteBuffer.AddRange(EmbraceEdmUtility.GetEdmProperties(mainTemplate));
            gradlePropertiesWriteBuffer.Add(new KeyValuePair<string, string>(EmbraceIl2CppSymbolUtility.SWAZZLER_FEATURE_GRADLE_PROPERTY, EmbraceIl2CppSymbolUtility.AssembleSourceMappingInfo(projectPath) ? "true" : "false"));

            EmbraceGradleUtility.WriteEmbraceGradleProperties(gradleProjectRootPath, gradlePropertiesWriteBuffer);

            // Actually we should assume the path for each Unity version for now and enforce with tests rather than trying to be smart.
            
            var matchingParent = Directory.EnumerateDirectories(projectPath, SYMBOLS_DIR, SearchOption.TopDirectoryOnly)
                .Select(path => path) 
                .FirstOrDefault();

            if (matchingParent != null)
            {
                var archDir = Directory.EnumerateDirectories(matchingParent, ARCH_DIR, SearchOption.TopDirectoryOnly)
                    .Select(path => path)
                    .FirstOrDefault();

                if (archDir == null)
                {
                    EmbraceLogger.LogError($"No arch folders found: {matchingParent}; double check if you have enabled the Create symbols.zip setting");
                }
                else
                {
                    // We now check if the path matches
                    EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.LauncherTemplatePath,
                        out string launcherTemplate);

                    var match = Regex.Match(launcherTemplate, EMBRACE_CUSTOM_SYMBOLS_PATTERN);
                    if (!match.Success)
                    {
                        // No match found. We should create the section we need
                        File.AppendAllLines(EmbraceGradleUtility.LauncherTemplatePath, new[]
                        {
                            String.Format(EMBRACE_CUSTOM_SYMBOLS_PROP, matchingParent)
                        });
                    }
                    
                }
            }
            else
            {
                EmbraceLogger.LogError("No il2cpp symbols found; double check if you have enabled the Create symbols.zip setting");
            }
            
#if UNITY_2020
            if (EditorUserBuildSettings.exportAsGoogleAndroidProject == true)
            {
                SymbolsUtil.CopySymbolFiles(gradleProjectRootPath);
                EmbraceLogger.Log("Embrace has copied the libmain and libunity symbol files to unityLibrary/symbols/<architecture>/ so that our Android SDK can complete the symbols package.");
            }
#endif
        }

        /// <summary>
        /// Loads the active Android configuration and serializes it to embrace-config.json in the gradle project.
        /// </summary>
        public static void WriteEmbraceConfig(string projectPath)
        {
            string embraceConfigString = "";

#if UNITY_2019_3_OR_NEWER
            FileInfo fileInfo = new FileInfo(string.Format("{0}/launcher/src/main/{1}", projectPath, "embrace-config.json"));
            if (fileInfo.Directory.Exists == false)
            {
                projectPath = Directory.GetParent(projectPath).FullName;
                fileInfo = new FileInfo(string.Format("{0}/launcher/src/main/{1}", projectPath, "embrace-config.json"));
            }
#else
            FileInfo fileInfo = new FileInfo(string.Format("{0}/src/main/{1}", projectPath, "embrace-config.json"));
#endif

            var config = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(EnvironmentsUtil.ConfigureForBuild(), ensureNotNull: false);

            if (config == null)
            {
                var sb = new StringBuilder();
                sb.Append("EmbraceSDK: No configuration scriptable object found. ");
                sb.Append("To create one open the Getting Started window [Tools > Embrace > Getting Started], ");
                sb.Append("and Configure it with your API key and API token.");
                throw new UnityEditor.Build.BuildFailedException(sb.ToString());
            }

            Validator.ValidateConfiguration(config);
            embraceConfigString =
                JsonConvert.SerializeObject(
                    config,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new SDKConfigContractResolver()
                    }
                );

            using (StreamWriter writer = new StreamWriter(fileInfo.FullName))
            {
                writer.Write(embraceConfigString);
            }

        }
    }
#endif

#if UNITY_IOS || UNITY_TVOS
    using System.IO;
    using System.Text;
    using UnityEditor;
    using Newtonsoft.Json;
    using UnityEditor.Callbacks;
    using UnityEditor.iOS.Xcode;
    using EmbraceSDK.EditorView.iOS.Extensions;

    public class EmbracePostBuildProcessor
    {
        public const string EmbracePlistName = "Embrace-Info.plist";
        public const string EmbraceRunFileName = "embrace_run.sh";
        public const string EmbraceSwiftPackageUrl = "https://github.com/embrace-io/embrace-unity-sdk";
        public const string EmbraceSwiftPackageProductName = "EmbraceUnityiOS";
        public static readonly string[] Configs = { "Debug", "ReleaseForRunning" };
        public const string PBXBuildSettingKey_EmbraceId = "EMBRACE_ID";
        public const string PBXBuildSettingKey_EmbraceToken = "EMBRACE_TOKEN";
        public static string PackagePath => Path.GetFullPath("Packages/io.embrace.sdk");

        // In Unity 2019.3 the iOS target was split into two targets, a launcher and the framework.
        // We have to be able to integrate with both target setups.
#if UNITY_2019_3_OR_NEWER
        private static string[] GetProjectNames(PBXProject project)
        {
            return new[]
            {
                project.GetUnityMainTargetGuid(),
                project.GetUnityFrameworkTargetGuid()
            };
        }
#else
        private static string[] GetProjectNames(PBXProject project)
        {
            return new[]
            {
                project.TargetGuidByName(PBXProject.GetUnityTargetName())
            };
        }
#endif

        // iOS Xcode project fixup
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var config = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(EnvironmentsUtil.ConfigureForBuild(), ensureNotNull: false);
            if (config == null)
            {
                var sb = new StringBuilder();
                sb.Append("EmbraceSDK: No configuration scriptable object found. ");
                sb.Append("To create one open the Getting Started window [Tools > Embrace > Getting Started], ");
                sb.Append("and Configure it with your API key and API token.");
                throw new UnityEditor.Build.BuildFailedException(sb.ToString());
            }

            Validator.ValidateConfiguration(config);
            EmbraceLogger.Log($"Config Loaded with app ID: {config.AppId}");

            var sdkDirectory = AssetDatabaseUtil.SDKDirectory;
            var projectPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

            // Copy il2cpp symbol mapping data
            EmbraceIl2CppSymbolUtility.AssembleSourceMappingInfo(pathToBuiltProject);

            // Load pbxproj
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var targetGuids = GetProjectNames(project);
            var appTargetGuid = targetGuids[0];

            // Enable dSYM
            foreach (string targetGuid in targetGuids)
            {
                foreach (string targetConfig in Configs)
                {
                    string configGuid = project.BuildConfigByName(targetGuid, targetConfig);
                    project.SetBuildPropertyForConfig(configGuid, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
                }
            }

            // TODO -- Also make this conditional on Bitcode being disabled in the project. There is no point in uploading the symbol files if
            //         Bitcode is enabled since Apple will produce new symbols files for Bitcode-enabled projects.
            if (config.AppId != null && config.SymbolUploadApiToken != null)
            {
                // Copy run.sh script and upload binary
                string embraceRunSHSrc = sdkDirectory + "/iOS/run.sh";
                string embraceRunSHDest = pathToBuiltProject + "/" + EmbraceRunFileName;

                string embraceUploadSrc = sdkDirectory + "/iOS/embrace_symbol_upload.darwin";
                string embraceUploadDest = pathToBuiltProject + "/" + "embrace_symbol_upload.darwin";

                File.Copy(embraceRunSHSrc, embraceRunSHDest, true);
                File.Copy(embraceUploadSrc, embraceUploadDest, true);

                // Add phase for dSYM upload
                string runScriptName = "Embrace Symbol Upload";
                string makeRunScriptExecutable = $"chmod +x \"${{PROJECT_DIR}}/{EmbraceRunFileName}\"\n";
                string makeUploadBinaryExecutable = $"chmod +x \"${{PROJECT_DIR}}/embrace_symbol_upload.darwin\"\n";
                string runScript = $"EMBRACE_FRAMEWORK_SEARCH_DEPTH=0 \"${{PROJECT_DIR}}/{EmbraceRunFileName}\"";
                string runScriptPhase = makeRunScriptExecutable + makeUploadBinaryExecutable + runScript;

                string[] phases = project.GetAllBuildPhasesForTarget(appTargetGuid);
                bool embracePhaseExists = false;
                foreach (string item in phases)
                {
                    if (project.GetBuildPhaseName(item) == runScriptName)
                    {
                        project.SetBuildProperty(appTargetGuid, PBXBuildSettingKey_EmbraceId, config.AppId);
                        project.SetBuildProperty(appTargetGuid, PBXBuildSettingKey_EmbraceToken, config.SymbolUploadApiToken);
                        embracePhaseExists = true;
                        break;
                    }
                }

                if (embracePhaseExists == false)
                {
                    project.AddShellScriptBuildPhase(appTargetGuid, runScriptName, "/bin/sh", runScriptPhase);
                    project.AddBuildProperty(appTargetGuid, PBXBuildSettingKey_EmbraceId, config.AppId);
                    project.AddBuildProperty(appTargetGuid, PBXBuildSettingKey_EmbraceToken, config.SymbolUploadApiToken);
                }
            }

            // Generate Embrace-Info.plist
            var plistJson =
                JsonConvert.SerializeObject(
                    config,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new SDKConfigContractResolver()
                    }
                );
            var plist = PlistUtil.FromJson(plistJson);

            plist.WriteToFile(pathToBuiltProject + "/" + EmbracePlistName);

            var resourcesBuildPhase = project.GetResourcesBuildPhaseByTarget(appTargetGuid);
            var resourcesFilesGuid = project.AddFile(EmbracePlistName, "/" + EmbracePlistName, PBXSourceTree.Source);
            project.AddFileToBuildSection(appTargetGuid, resourcesBuildPhase, resourcesFilesGuid);

            // Remove any Embrace XCFrameworks build references left over from
            // versions prior to v2.2.0. These were only used by previous
            // versions of the Embrace Unity SDK, but will cause conflicts with
            // the Swift package if they are not removed.
            project.RemoveFilesByProjectPath("Frameworks/io.embrace.sdk/iOS/xcframeworks");

            // Remove the remote SPM package dependency added in v2.2.0. After
            // this point, we depend on a local Swift package instead, to make
            // versioning consistent.
            project.RemoveRemotePackage(project.GetUnityFrameworkTargetGuid(), EmbraceSwiftPackageUrl);

            // Copy the local Swift package files and add a dependency on it.
            project.AddLocalPackage(
                pathToBuiltProject,
                sourcePath: Path.Combine(PackagePath, "iOS/EmbraceUnityiOS"),
                projectPath: "EmbraceUnityiOS",
                productName: "EmbraceUnityiOS"
            );

            project.WriteToFile(projectPath);
        }
    }
#endif
}
