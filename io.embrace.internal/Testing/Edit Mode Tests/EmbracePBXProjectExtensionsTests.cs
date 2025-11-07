#if UNITY_IOS || UNITY_TVOS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EmbraceSDK.EditorView.iOS.Extensions;
using EmbraceSDK.Internal;
using NUnit.Framework;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace EmbraceSDK.Tests
{
    public class EmbracePBXProjectExtensionsTests
    {
        /// <summary>
        /// Set to true to update the golden files on disk, instead of asserting.
        /// </summary>
        private const bool WriteGoldenFiles = false;

        public struct AddLocalPackageData
        {
            public string ProjectFile;
            public string ExpectedProjectFile;
        }

        static public List<AddLocalPackageData> addLocalPackageData = new List<AddLocalPackageData>()
        {
            new AddLocalPackageData{
                ProjectFile = "project.pbxproj",
                ExpectedProjectFile = "AddLocalPackage.project.pbxproj"
            },
            new AddLocalPackageData{
                ProjectFile = "project_with_remote_package.pbxproj",
                ExpectedProjectFile = "AddLocalPackage_updated.project.pbxproj"
            }
        };

        [Test]
        public void TestAddLocalPackage([ValueSource("addLocalPackageData")] AddLocalPackageData data)
        {
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(GetPBXProjectPath(data.ProjectFile));
            pbxProject.AddLocalPackage(
                pathToBuiltProject: "Temp/TestAddLocalPackage",
                sourcePath: Path.GetFullPath("Packages/io.embrace.sdk/iOS/EmbraceUnityiOS"),
                projectPath: "EmbraceUnityiOS",
                productName: "EmbraceUnityiOS"
            );
            
            string workspace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
            string exportDir = !string.IsNullOrEmpty(workspace)
                ? Path.Combine(workspace, "test-exports")
                : Path.Combine(Application.dataPath, "io.embrace.sdk"); // fallback for local runs

            Directory.CreateDirectory(exportDir);
            File.WriteAllText(Path.Combine(exportDir, "pbx_file.txt"), pbxProject.WriteToString());
            AssertProjectIsEqual(pbxProject, data.ExpectedProjectFile);
        }

        [Test]
        public void TestRemoveFilesByProjectPath()
        {
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(GetPBXProjectPath("project_with_xcframeworks.pbxproj"));
            pbxProject.RemoveFilesByProjectPath("Frameworks/io.embrace.sdk/iOS/xcframeworks");
            AssertProjectIsEqual(pbxProject, "RemoveFilesByProjectPath.project.pbxproj");
        }

        public struct SafelyAddRemotePackageData
        {
            public string ProjectFile;
            public SwiftRefType RefType;
            public string RefValue;
            public string ExpectedProjectFile;
        }

        static public List<SafelyAddRemotePackageData> safelyAddRemotePackageData = new List<SafelyAddRemotePackageData>()
        {
            new SafelyAddRemotePackageData{
                ProjectFile = "project.pbxproj",
                RefType = SwiftRefType.Version,
                RefValue = "4.5.6",
                ExpectedProjectFile = "SafelyAddRemotePackage_version.project.pbxproj"
            },
            new SafelyAddRemotePackageData{
                ProjectFile = "project.pbxproj",
                RefType = SwiftRefType.Branch,
                RefValue = "main",
                ExpectedProjectFile = "SafelyAddRemotePackage_branch.project.pbxproj"
            },
            new SafelyAddRemotePackageData{
                ProjectFile = "project_with_remote_package.pbxproj",
                RefType = SwiftRefType.Version,
                RefValue = "4.5.6",
                ExpectedProjectFile = "SafelyAddRemotePackage_removed.project.pbxproj"
            }
        };

        [Test]
        public void TestSafelyAddRemotePackage([ValueSource("safelyAddRemotePackageData")] SafelyAddRemotePackageData data)
        {
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(GetPBXProjectPath(data.ProjectFile));
            pbxProject.SafelyAddRemotePackage(
                targetGuid: pbxProject.GetUnityFrameworkTargetGuid(),
                repositoryURL: "https://github.com/example-organization/example-package.git",
                productDependencyName: "ExampleDependencyName",
                data.RefType,
                data.RefValue,
                defaultVersion: "1.2.3"
            );
            AssertProjectIsEqual(pbxProject, data.ExpectedProjectFile);
        }

        /// <summary>
        /// Assert that the PBXProject is equal to the golden file.
        /// </summary>
        /// <param name="pbxProject">PBXProject to check.</param>
        /// <param name="filename">Filename to assert against.</param>
        private void AssertProjectIsEqual(PBXProject pbxProject, string filename)
        {
            // NOTE: reading the project appears to have some side effects,
            // so we need to do a round trip before comparing.
            var actualPbxProject = new PBXProject();
            actualPbxProject.ReadFromString(pbxProject.WriteToString());
            if (WriteGoldenFiles)
            {
                actualPbxProject.WriteToFile(GetPBXProjectPath(filename));
                return;
            }
            var expectedPbxProject = new PBXProject();
            expectedPbxProject.ReadFromFile(GetPBXProjectPath(filename));
            Assert.AreEqual(expectedPbxProject.WriteToString(), actualPbxProject.WriteToString());
        }

        /// <summary>
        /// Get the path to a PBXProject file in the Testing/Resources folder.
        /// </summary>
        private string GetPBXProjectPath(string filename)
        {
            var basePath = Application.dataPath.Replace("/Assets", "");
            var projectPath = "Packages/io.embrace.internal/Testing/Resources/PBXProjects";
            return Path.Combine(basePath, projectPath, filename);
        }

        private Int32 deterministicGuidIndex = 1;

        // Method for Unity 2021 (no parameters)
        private string DeterministicGuidGenerator()
        {
            var b = BitConverter.GetBytes(deterministicGuidIndex);
            var guid = new Guid(0, 0, 0, new byte[8] { 0, 0, 0, 0, b[3], b[2], b[1], b[0] });
            deterministicGuidIndex++;
            return guid.ToString("N").Substring(8).ToUpper();
        }

        // Method for Unity 2022+ (takes an identifier)
        private string DeterministicGuidGeneratorWithIdentifier(string identifier)
        {
            return DeterministicGuidGenerator();
        }

        [SetUp]
        public void SetUpGuidGenerator()
        {
            deterministicGuidIndex = 1;
            var pbxGuidType = typeof(PBXProject).Assembly.GetType("UnityEditor.iOS.Xcode.PBX.PBXGUID");
            var guidGeneratorType = pbxGuidType.GetNestedType("GuidGenerator", BindingFlags.NonPublic);
            var fnName = guidGeneratorType.GetMethod("Invoke").GetParameters().Length == 0
                    ? nameof(DeterministicGuidGenerator)
                    : nameof(DeterministicGuidGeneratorWithIdentifier);
            var fn = Delegate.CreateDelegate(guidGeneratorType, this, fnName);
            var setGuidGeneratorMethod = pbxGuidType
                .GetMethod("SetGuidGenerator", BindingFlags.NonPublic | BindingFlags.Static);
            setGuidGeneratorMethod.Invoke(null, new object[] { fn });
        }

        [TearDown]
        public void TearDownGuidGenerator()
        {
            var pbxGuidType = typeof(PBXProject).Assembly.GetType("UnityEditor.iOS.Xcode.PBX.PBXGUID");
            var guidGeneratorType = pbxGuidType.GetNestedType("GuidGenerator", BindingFlags.NonPublic);
            var defaultGuidGeneratorMethod = pbxGuidType.GetMethod("DefaultGuidGenerator", BindingFlags.NonPublic | BindingFlags.Static);
            var fn = defaultGuidGeneratorMethod.CreateDelegate(guidGeneratorType, null);
            var setGuidGeneratorMethod = pbxGuidType
                .GetMethod("SetGuidGenerator", BindingFlags.NonPublic | BindingFlags.Static);
            setGuidGeneratorMethod.Invoke(null, new object[] { fn });
        }
    }
}
#endif
