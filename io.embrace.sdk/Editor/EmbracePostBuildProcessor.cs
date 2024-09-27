using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace EmbraceSDK.EditorView
{
#if UNITY_ANDROID
    using UnityEditor.Android;

    public class EmbracePostBuildProcessor : IPostGenerateGradleAndroidProject
    {
        public const string EMBRACE_SYSTEM_ENV_VAR = "EMBRACE_ENVIRONMENTS_INDEX";

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
            EmbraceLogger.Log($"Config Loaded with key: {config.AppId}");
            EmbraceLogger.Log($"Config has bug shake setting: {config.sdk_config.bug_shake.shake_detect_enabled}");
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
    using System.Diagnostics;
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
    using UnityEditor.Callbacks;

    public class EmbracePostBuildProcessor
    {
        public const string EmbracePlistName = "Embrace-Info.plist";
        public const string EmbraceRunFileName = "embrace_run.sh";
        public static readonly string[] Configs = { "Debug", "ReleaseForRunning" };
        public const string PBXBuildSettingKey_EmbraceId = "EMBRACE_ID";
        public const string PBXBuildSettingKey_EmbraceToken = "EMBRACE_TOKEN";
        public static string PackagePath => Path.GetFullPath("Packages/io.embrace.sdk");
        public const string EmbraceXCFramework = "Embrace.xcframework";

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
            
            // Embed iOS frameworks
            
            /*
             * It is worth noting that Unity *has* changed the default behavior of how xcframeworks are handled.
             * Previously we needed to add the files to the project, add them to the linker phase, and then embed them.
             * Now, Unity will automatically add the xcframeworks to the project and the linker phase, but we still need
             * to embed them. This behavior is not documented, but it is consistent with the behavior we have observed.
             * As a result, this code is fragile and may need to be updated in the future.
             */
            
            string xcFrameworkSource = Path.Combine(PackagePath, "iOS", "xcframeworks");
            string xcFrameworkProjectPath = "Frameworks/io.embrace.sdk/iOS/xcframeworks";
            
            var xcFrameworks = Directory.GetDirectories(xcFrameworkSource, "*.xcframework", SearchOption.TopDirectoryOnly);
            
            foreach (var xcFramework in xcFrameworks)
            {
                var xcFrameworkGuid = project.FindFileGuidByProjectPath(
                    $"{xcFrameworkProjectPath}/{Path.GetFileName(xcFramework)}");
                
                project.AddFileToEmbedFrameworks(appTargetGuid, xcFrameworkGuid);
            }

            project.WriteToFile(projectPath);
        }
    }
#endif
}