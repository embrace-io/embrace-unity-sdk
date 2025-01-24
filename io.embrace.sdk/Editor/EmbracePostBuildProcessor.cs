using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using EmbraceSDK.Internal;

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
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using UnityEditor.iOS.Xcode;
    using UnityEditor.iOS.Xcode.Extensions;
    using UnityEditor.Callbacks;

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

            // Add SPM dependency
            var sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(Resources.Load<TextAsset>("Info/EmbraceSdkInfo").text);
            var (swiftRefType, swiftRefValue) = sdkInfo.SwiftRef();
            SafelyAddRemotePackage(
                project,
                project.GetUnityFrameworkTargetGuid(),
                EmbraceSwiftPackageUrl,
                EmbraceSwiftPackageProductName,
                swiftRefType,
                swiftRefValue,
                sdkInfo.version
            );

            project.WriteToFile(projectPath);
        }

        /// <summary>
        /// Safely adds a remote Swift package to the Xcode project.
        /// </summary>
        /// <param name="project">The PBXProject instance representing the Xcode project.</param>
        /// <param name="targetGuid">The target GUID that the remote package will be added to.</param>
        /// <param name="repositoryURL">The URL of the remote Swift package repository.</param>
        /// <param name="productDependencyName">The name of the product dependency to be added.</param>
        /// <param name="refType">The type of reference for the Swift package (Branch, Revision, Version).</param>
        /// <param name="refValue">The value of the reference (e.g., branch name, revision hash, version number).</param>
        /// <param name="defaultVersion">The version of the Swift package to use if refType is not valid.</param>
        internal static void SafelyAddRemotePackage(PBXProject project, string targetGuid, string repositoryURL, string productDependencyName, SwiftRefType refType, string refValue, string defaultVersion)
        {
            try
            {
                RemoveRemotePackage(project, project.GetUnityFrameworkTargetGuid(), EmbraceSwiftPackageUrl);
                var packageGuid = refType switch
                {
                    SwiftRefType.Branch => project.AddRemotePackageReferenceAtBranch(EmbraceSwiftPackageUrl, refValue),
                    SwiftRefType.Revision => project.AddRemotePackageReferenceAtRevision(EmbraceSwiftPackageUrl, refValue),
                    SwiftRefType.Version => project.AddRemotePackageReferenceAtVersion(EmbraceSwiftPackageUrl, refValue),
                    _ => project.AddRemotePackageReferenceAtVersion(EmbraceSwiftPackageUrl, defaultVersion)
                };
                project.AddRemotePackageFrameworkToProject(project.GetUnityFrameworkTargetGuid(), productDependencyName, packageGuid, weak: false);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("Failed to add Embrace SDK package to Xcode project: {0}", e);
            }
        }

        /// <summary>
        /// Removes a remote Swift package from the Xcode project.
        /// </summary>
        /// <param name="project">The PBXProject instance representing the Xcode project.</param>
        /// <param name="targetGuid">The target GUID that the remove package was added to.</param>
        /// <param name="expectedRepositoryURL">The expected repository URL of the Swift package to be removed.</param>
        /// <remarks>
        /// This is required because the public functions for adding a remote
        /// dependency are not idempotent, and Unity does not expose the
        /// types necessary to remove it. We need to use reflection to ensure
        /// we don't add the same package multiple times.
        /// </remarks>
        internal static void RemoveRemotePackage(PBXProject project, string targetGuid, string expectedRepositoryURL)
        {
            var packageGuids = FindSectionBaseEntries(project, "remoteSwiftPackage", obj =>
            {
                return expectedRepositoryURL == obj.GetType().GetField("repositoryURL").GetValue(obj) as string;
            });
            var dependencyGuids = FindSectionBaseEntries(project, "swiftPackageDependency", obj =>
            {
                var packageGuid = obj.GetType().GetField("package").GetValue(obj) as string;
                return packageGuids.Contains(packageGuid);
            });
            var buildFileGuids = FindSectionBaseEntries(project, "buildFiles", obj =>
            {
                var dependencyGuid = obj.GetType().GetField("productRef").GetValue(obj) as string;
                return dependencyGuids.Contains(dependencyGuid);
            });
            foreach (var buildFileGuid in buildFileGuids)
            {
                project.RemoveFileFromBuild(targetGuid, buildFileGuid);
            }
            foreach (var dependencyGuid in dependencyGuids)
            {
                RemoveSectionBaseEntry(project, "swiftPackageDependency", dependencyGuid);
            }
            foreach (var packageGuid in packageGuids)
            {
                RemoveSectionBaseEntry(project, "remoteSwiftPackage", packageGuid);
            }
        }

        internal static List<string> FindSectionBaseEntries(PBXProject project, string sectionFieldName, System.Func<object, bool> predicate)
        {
            var guids = new List<string>();
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var projectData = project.GetType().GetField("m_Data", bindingFlags).GetValue(project);
            var section = projectData.GetType().GetField(sectionFieldName, bindingFlags).GetValue(projectData);
            var objects = section.GetType().GetMethod("GetObjects", bindingFlags).Invoke(section, null) as IEnumerable;
            foreach (var obj in objects)
            {
                if (predicate(obj))
                {
                    guids.Add(obj.GetType().GetField("guid", bindingFlags).GetValue(obj) as string);
                }
            }
            return guids;
        }

        internal static string RemoveSectionBaseEntry(PBXProject project, string sectionFieldName, string guid)
        {
            if (guid == null)
            {
                return null;
            }
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var projectData = project.GetType().GetField("m_Data", bindingFlags).GetValue(project);
            var section = projectData.GetType().GetField(sectionFieldName, bindingFlags).GetValue(projectData);
            section.GetType().GetMethod("RemoveEntry", bindingFlags).Invoke(section, new object[] { guid });
            return guid;
        }
    }
#endif
}
