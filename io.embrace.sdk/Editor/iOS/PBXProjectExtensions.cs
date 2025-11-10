#if UNITY_IOS || UNITY_TVOS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EmbraceSDK.Internal;
using UnityEditor.iOS.Xcode;

namespace EmbraceSDK.EditorView.iOS.Extensions
{
    /// <summary>
    /// Provides extension methods for the PBXProject class to manage Swift package dependencies in an Xcode project.
    /// </summary>
    public static class PBXProjectExtensions
    {
        /// <summary>
        /// Adds a local Swift package to the Xcode project.
        /// </summary>
        /// <param name="project">The PBXProject instance representing the Xcode project.</param>
        /// <param name="pathToBuiltProject">The path to the Xcode project.</param>
        /// <param name="sourcePath">The source path of the local Swift package within the Unity project.</param>
        /// <param name="projectPath">The path where the Swift package will be added in the Xcode project.</param>
        /// <param name="productName">The name of the product dependency to be added.</param>
        /// <remarks>
        /// This method copies the Swift package from the source path to the
        /// destination path within the built Unity project, adds a folder
        /// reference to the Xcode project, and sets up the necessary build
        /// configurations and dependencies.
        /// </remarks>
        public static void AddLocalPackage(this PBXProject project, string pathToBuiltProject, string sourcePath, string projectPath, string productName)
        {
            try
            {
                var destPath = Path.Combine(pathToBuiltProject, projectPath);
                CopySwiftPackage(sourcePath, destPath);
                if (project.FindFileGuidByProjectPath(projectPath) == null)
                {
                    // The Source specification here is called out rather than leaving as implicit.
                    // Refer to the documentation here: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/iOS.Xcode.PBXSourceTree.html
                    // The reason for projectPath's usage twice is that the project is copied into the root of the resulting built project.
                    // As a result we only need to call out the folder itself as a reference.
                    project.AddFolderReference(projectPath, projectPath, PBXSourceTree.Source); 
                }
                var unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
                var (productDependency, productDependencyGuid) = CreateSwiftPackageProductDependency(productName);
                AddSectionBaseEntry(project, "swiftPackageDependency", productDependency);
                AddPackageProductDependencyToTarget(project, unityFrameworkTargetGuid, productDependencyGuid);
                var buildFileGuid = AddPackageProductDependencyAsBuildFile(project, unityFrameworkTargetGuid, productDependencyGuid);
                AddBuildFileToFrameworksBuildPhase(project, unityFrameworkTargetGuid, buildFileGuid);
            }
            catch (Exception e)
            {
                EmbraceLogger.Log($"Error adding local Swift package dependency to PBXProject: {e}");
            }
        }

        /// <summary>
        /// Removes any files starting with the path from the Xcode project.
        /// </summary>
        /// <param name="project">The PBXProject instance representing the Xcode project.</param>
        /// <param name="projectPath">The path to the files to be removed.</param>
        public static void RemoveFilesByProjectPath(this PBXProject project, string projectPath)
        {
            try
            {
                var guids = project
                    .GetRealPathsOfAllFiles(PBXSourceTree.Source)
                    .Where(file => file.StartsWith(projectPath))
                    .Select(file => (file, project.FindFileGuidByProjectPath(file)));
                foreach (var (file, guid) in guids)
                {
                    project.RemoveFile(guid);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("Failed to remove Embrace XCFrameworks from Xcode project: {0}", e);
            }
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
        public static void SafelyAddRemotePackage(this PBXProject project, string targetGuid, string repositoryURL, string productDependencyName, SwiftRefType refType, string refValue, string defaultVersion)
        {
            try
            {
                RemoveRemotePackage(project, project.GetUnityFrameworkTargetGuid(), repositoryURL);
                var packageGuid = refType switch
                {
                    SwiftRefType.Branch => project.AddRemotePackageReferenceAtBranch(repositoryURL, refValue),
                    SwiftRefType.Revision => project.AddRemotePackageReferenceAtRevision(repositoryURL, refValue),
                    SwiftRefType.Version => project.AddRemotePackageReferenceAtVersion(repositoryURL, refValue),
                    _ => project.AddRemotePackageReferenceAtVersion(repositoryURL, defaultVersion)
                };
                project.AddRemotePackageFrameworkToProject(targetGuid, productDependencyName, packageGuid, weak: false);
            }
            catch (Exception e)
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
        public static void RemoveRemotePackage(this PBXProject project, string targetGuid, string expectedRepositoryURL)
        {
            var packageGuids = FindSectionBaseEntries(project, "remoteSwiftPackage", package =>
            {
                return expectedRepositoryURL == GetFieldValue(package, "repositoryURL") as string;
            });
            var dependencyGuids = FindSectionBaseEntries(project, "swiftPackageDependency", dependency =>
            {
                var packageGuid = GetFieldValue(dependency, "package") as string;
                return packageGuids.Contains(packageGuid);
            });
            var buildFileGuids = FindSectionBaseEntries(project, "buildFiles", buildFile =>
            {
                var dependencyGuid = GetFieldValue(buildFile, "productRef") as string;
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

        internal static void CopySwiftPackage(string sourcePath, string destPath)
        {
            var sourceFiles = Directory
                .GetFiles(sourcePath, "*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f);
                    return string.Equals(ext, ".swift", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(ext, ".resolved", StringComparison.OrdinalIgnoreCase);
                })
                .ToArray();
            foreach (var sourceFile in sourceFiles)
            {
                var destFile = Path.Combine(destPath, Path.GetRelativePath(sourcePath, sourceFile));
                var destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                File.Copy(sourceFile, destFile, true);
            }
        }

        internal const BindingFlags instanceBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        internal const BindingFlags staticBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        internal static List<string> FindSectionBaseEntries(PBXProject project, string sectionFieldName, System.Func<object, bool> predicate)
        {
            var guids = new List<string>();
            var projectData = GetProjectData(project);
            var section = GetFieldValue(projectData, sectionFieldName);
            var objects = section.GetType().GetMethod("GetObjects", instanceBindingFlags).Invoke(section, null) as IEnumerable;
            foreach (var obj in objects)
            {
                if (predicate(obj))
                {
                    guids.Add(obj.GetType().GetField("guid", instanceBindingFlags).GetValue(obj) as string);
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
            var projectData = GetProjectData(project);
            var section = GetFieldValue(projectData, sectionFieldName);
            section.GetType().GetMethod("RemoveEntry", instanceBindingFlags).Invoke(section, new object[] { guid });
            return guid;
        }

        internal static void AddSectionBaseEntry(PBXProject project, string sectionFieldName, object entry)
        {
            var projectData = GetProjectData(project);
            var section = GetFieldValue(projectData, sectionFieldName);
            section.GetType().GetMethod("AddEntry", instanceBindingFlags).Invoke(section, new object[] { entry });
        }

        internal static (object, string) CreateSwiftPackageProductDependency(string productName)
        {
            var packageGuid = "";
            var dependency = typeof(PBXProject).Assembly
                .GetType("UnityEditor.iOS.Xcode.PBX.XCSwiftPackageProductDependencyData")
                .GetConstructor(new Type[] { typeof(string), typeof(string) })
                .Invoke(new object[] { productName, packageGuid });
            var dependencyGuid = GetFieldValue(dependency, "guid") as string;
            return (dependency, dependencyGuid);
        }

        internal static string AddPackageProductDependencyAsBuildFile(PBXProject project, string targetGuid, string productGuid)
        {
            var buildFile = typeof(PBXProject).Assembly
                .GetType("UnityEditor.iOS.Xcode.PBX.PBXBuildFileData")
                .GetMethod("CreateFromProduct", new Type[] { typeof(string), typeof(bool), typeof(string) })
                .Invoke(null, new object[] { productGuid, false, null });
            var buildFileGuid = GetFieldValue(buildFile, "guid") as string;
            var projectData = GetProjectData(project);
            projectData.GetType()
                .GetMethod("BuildFilesAdd", instanceBindingFlags)
                .Invoke(projectData, new object[] { targetGuid, buildFile });
            return buildFileGuid;
        }

        internal static void AddPackageProductDependencyToTarget(PBXProject project, string targetGuid, string productDependencyGuid)
        {
            var target = GetNativeTarget(project, targetGuid);
            var targetDependencies = GetFieldValue(target, "packageDependencies");
            targetDependencies.GetType()
                .GetMethod("AddGUID", instanceBindingFlags)
                .Invoke(targetDependencies, new object[] { productDependencyGuid });
        }

        internal static void AddBuildFileToFrameworksBuildPhase(PBXProject project, string targetGuid, string buildFileGuid)
        {
            var projectData = GetProjectData(project);
            var phaseGuid = project.AddFrameworksBuildPhase(targetGuid);
            var phase = projectData.GetType()
                .GetMethod("BuildSectionAny", new Type[] { typeof(string) })
                .Invoke(projectData, new object[] { phaseGuid });
            var phaseFiles = GetFieldValue(phase, "files");
            phaseFiles.GetType()
                .GetMethod("AddGUID", instanceBindingFlags)
                .Invoke(phaseFiles, new object[] { buildFileGuid });
        }

        internal static object GetFieldValue(object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, instanceBindingFlags).GetValue(obj);
        }

        internal static object GetNativeTarget(PBXProject project, string targetGuid)
        {
            var projectData = GetProjectData(project);
            var targets = GetFieldValue(projectData, "nativeTargets");
            var targetsEntries = GetFieldValue(targets, "m_Entries") as IDictionary;
            return targetsEntries[targetGuid];
        }

        internal static object GetProjectData(PBXProject project)
        {
            return GetFieldValue(project, "m_Data");
        }
    }
}
#endif