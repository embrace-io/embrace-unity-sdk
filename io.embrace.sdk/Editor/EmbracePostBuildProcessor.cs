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
                string embraceUploadSrc = sdkDirectory + "/iOS/upload";
                string embraceUploadDest = pathToBuiltProject + "/" + "upload";

                File.Copy(embraceRunSHSrc, embraceRunSHDest, true);
                File.Copy(embraceUploadSrc, embraceUploadDest, true);

                // Add phase for dSYM upload
                string runScriptName = "Embrace Symbol Upload";
                string makeRunScriptExecutable = $"chmod +x \"${{PROJECT_DIR}}/{EmbraceRunFileName}\"\n";
                string makeUploadBinaryExecutable = $"chmod +x \"${{PROJECT_DIR}}/upload\"\n";
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
            
            // Embed Embrace.framework

            string xcFrameworkCopySource = Path.Combine(PackagePath, "iOS", EmbraceXCFramework);
            string xcFrameworkCopyDestination = Path.Combine(pathToBuiltProject, EmbraceXCFramework);
            
            if (Directory.Exists(xcFrameworkCopyDestination))
            {
                var fileGuid = project.FindFileGuidByRealPath(xcFrameworkCopyDestination);
                if (!string.IsNullOrWhiteSpace(fileGuid))
                {
                    project.RemoveFileFromBuild(project.GetUnityFrameworkTargetGuid(), fileGuid);
                    project.RemoveFile(fileGuid);
                }
                Directory.Delete(xcFrameworkCopyDestination, true);
            }

            AssetDatabaseUtil.CopyDirectory(
                xcFrameworkCopySource,
                xcFrameworkCopyDestination,
                true,
                false);

            #if UNITY_2021_3_OR_NEWER
            // Later versions of Unity appear to automatically add the files to the linker phase.
            // However there seems to be an issue with the copy process. To deal with this, we
            // choose to overwrite the framework in position.
            var xcFrameworkGuid = project.AddFile(
                EmbraceXCFramework,
                "Frameworks/io.embrace.sdk/iOS/Embrace.xcframework",
                PBXSourceTree.Source);
            
            // We still must embed the framework, but we have previously added the build phase step.
            project.AddFileToEmbedFrameworks(appTargetGuid, xcFrameworkGuid);
            
            // We need to add the xcframework to the linker phase
            var frameworkTargetGuid = targetGuids[1];
            var linkPhaseGuid = project.GetFrameworksBuildPhaseByTarget(frameworkTargetGuid);
            project.AddFileToBuildSection(frameworkTargetGuid, linkPhaseGuid, xcFrameworkGuid);
            #else // !!UNITY_2021_3_OR_NEWER
            var xcFrameworkGuid = project.AddFile(
                EmbraceXCFramework,
                EmbraceXCFramework,
                PBXSourceTree.Source);
            
            string unityFrameworkGuid = project.GetUnityFrameworkTargetGuid();
            string linkPhaseGuid = project.GetFrameworksBuildPhaseByTarget(unityFrameworkGuid);
            project.AddFileToBuildSection(unityFrameworkGuid, linkPhaseGuid, xcFrameworkGuid);
            project.AddFileToEmbedFrameworks(appTargetGuid, xcFrameworkGuid);
            #endif

            project.WriteToFile(projectPath);
        }
    }
#endif
}