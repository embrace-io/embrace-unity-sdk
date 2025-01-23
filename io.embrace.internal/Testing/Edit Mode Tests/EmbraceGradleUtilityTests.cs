using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class EmbraceGradleUtilityTests
    {
        public struct DependencyMatchTestCase
        {
            public string sourceText;
            public string expectedVersion;
            public string description;
        }

        public struct DependencyReplaceTestCase
        {
            public string sourceText;
            public string newVersion;
            public string expectedResult;
            public string description;
        }

        private static string[] _dependencyTestCases =  { "io.embrace:embrace-swazzler", "io.embrace:embrace-swazzler:"};

        private static string[] _nonMatchingSourceTextCases =
            { "", " ", "__no_dependency__", "dependencies { classpath \"io.embrace:embrace-android-sdk:0.1.2\" }" };

        private static DependencyMatchTestCase[] _extractVersionDependencyTestCases =
        {
            // Non-matching cases
            new DependencyMatchTestCase() { sourceText = null, expectedVersion = null, description = "Source text is null" },
            new DependencyMatchTestCase() { sourceText = "", expectedVersion = null, description = "Source text is empty"},
            new DependencyMatchTestCase() { sourceText = "  ", expectedVersion = null, description = "Source text is whitespace"},
            new DependencyMatchTestCase() { sourceText = "_NO_DEPENDENCY_", expectedVersion = null, description = "Source text does not contain any android dependencies"},
            new DependencyMatchTestCase() { sourceText = "dependencies { classpath \"io.embrace:embrace-other:0.1.2", expectedVersion = null, description = "Source text does not contain matching dependency"},

            // Matching cases
            new DependencyMatchTestCase() { sourceText = "io.embrace:embrace-swazzler:0.1.2", expectedVersion = "0.1.2", description = "Source text equal to dependency without quotes" },
            new DependencyMatchTestCase() { sourceText = "\"io.embrace:embrace-swazzler:0.1.2\"", expectedVersion = "0.1.2", description = "Source text equal to dependency inside double quotes"},
            new DependencyMatchTestCase() { sourceText = "\'io.embrace:embrace-swazzler:0.1.2\'", expectedVersion = "0.1.2", description = "Source text equal to dependency inside single quotes"},
            new DependencyMatchTestCase() { sourceText = "dependencies { classpath \"io.embrace:embrace-swazzler:0.1.2\" }", expectedVersion = "0.1.2", description = "Source text contains matching numeric version"},
            new DependencyMatchTestCase() { sourceText = "dependencies { classpath \'io.embrace:embrace-swazzler:0.1.2\' }", expectedVersion = "0.1.2", description = "Source text contains matching numeric version in single quotes"},
            new DependencyMatchTestCase() { sourceText = "dependencies { classpath \"io.embrace:embrace-swazzler:0.1.2-alpha2\" }", expectedVersion = "0.1.2-alpha2", description = "Source text contains matching dependency with alphanumeric version in double quotes"},
            new DependencyMatchTestCase() { sourceText = "dependencies { classpath \'io.embrace:embrace-swazzler:0.1.2-alpha2\' }", expectedVersion = "0.1.2-alpha2", description = "Source text contains matching dependency with alphanumeric version in single quotes"},
        };

        private static DependencyReplaceTestCase[] _replaceVersionDependencyTestCases =
        {
            new DependencyReplaceTestCase() { sourceText = null, newVersion = "1.2.3", expectedResult = null, description = "Source text is null" },
            new DependencyReplaceTestCase() { sourceText = "__NO_MATCH__", newVersion = "1.2.3", expectedResult = "__NO_MATCH__", description = "Source text does not contain match"},
            new DependencyReplaceTestCase() { sourceText = "io.embrace:embrace-swazzler:0.1.2", newVersion = null, expectedResult = "io.embrace:embrace-swazzler:0.1.2", description = "New version is null" },
            new DependencyReplaceTestCase() { sourceText = "io.embrace:embrace-swazzler:0.1.2", newVersion = "1.2.3", expectedResult = "io.embrace:embrace-swazzler:1.2.3", description = "Source text is dependency without quotes."},
            new DependencyReplaceTestCase() { sourceText = "\"io.embrace:embrace-swazzler:0.1.2\"", newVersion = "1.2.3", expectedResult = "\"io.embrace:embrace-swazzler:1.2.3\"", description = "Source text is dependency inside double quotes." },
            new DependencyReplaceTestCase() { sourceText = "\'io.embrace:embrace-swazzler:0.1.2\'", newVersion = "1.2.3", expectedResult = "\'io.embrace:embrace-swazzler:1.2.3\'", description = "Source text is dependency inside single quotes." },
            new DependencyReplaceTestCase() { sourceText = "dependencies { classpath \"io.embrace:embrace-swazzler:0.1.2\" }", newVersion = "1.2.3", expectedResult =  "dependencies { classpath \"io.embrace:embrace-swazzler:1.2.3\" }", description = "Source text matches in dependency block"},
            new DependencyReplaceTestCase() { sourceText = "dependencies { classpath \"io.embrace:embrace-swazzler:0.1.2-alpha2\" }", newVersion = "1.2.3", expectedResult =  "dependencies { classpath \"io.embrace:embrace-swazzler:1.2.3\" }", description = "Source text contains non-numeric characters in version"},
            new DependencyReplaceTestCase() { sourceText = "dependencies { classpath \"io.embrace:embrace-swazzler:0.1.2\" }", newVersion = "1.2.3-alpha4", expectedResult =  "dependencies { classpath \"io.embrace:embrace-swazzler:1.2.3-alpha4\" }", description = "New version text contains non-numeric characters"},
        };

        [Test]
        public void TryParseDependencyVersion_ReturnsExpectedValues_ForAllTestCases(
            [ValueSource(nameof(_dependencyTestCases))] string dependency,
            [ValueSource(nameof(_extractVersionDependencyTestCases))] DependencyMatchTestCase testCase)
        {
            bool success = EmbraceGradleUtility.TryParseDependencyVersion(testCase.sourceText, dependency, out string version);
            string failureMessage = $"Failed test case description: {testCase.description}";
            Assert.AreEqual(testCase.expectedVersion != null, success, failureMessage);
            Assert.AreEqual(testCase.expectedVersion, version, failureMessage);
        }

        [Test]
        public void ReplaceDependencyVersion_ReturnsExpectedValues_ForAllTestCases(
            [ValueSource(nameof(_dependencyTestCases))] string dependency,
            [ValueSource(nameof(_replaceVersionDependencyTestCases))] DependencyReplaceTestCase testCase)
        {
            string newText =
                EmbraceGradleUtility.ReplaceDependencyVersion(testCase.sourceText, dependency, testCase.newVersion);
            Assert.AreEqual(testCase.expectedResult, newText, $"Failed test case description: {testCase.description}");
        }

        [Test, TestMustExpectAllLogs]
        public void SwazzlerAndAndroidSdkVersionsMatch()
        {
            Assert.DoesNotThrow(EmbraceGradleUtility.EnforceSwazzlerDependencyVersion);
        }

        [Test, TestMustExpectAllLogs]
        public void TryParseEdmAndroidSdkDependencyVersion_DoesNotThrow()
        {
            bool result = false;
            string version = null;
            Assert.DoesNotThrow(() =>
            {
                result = EmbraceGradleUtility.TryParseEdmAndroidSdkDependencyVersion(out version);
            });
            Assert.IsTrue(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }

        [Test, TestMustExpectAllLogs]
        public void TryReadBaseProjectGradleTemplate_DoesNotThrow()
        {
            bool result = false;
            string gradleSource = null;
            Assert.DoesNotThrow(() =>
            {
                result = EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.BaseProjectTemplatePath, out gradleSource);
            });
            Assert.IsTrue(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(gradleSource));
        }

        [Test, TestMustExpectAllLogs]
        public void TryReadLauncherGradleTemplate_DoesNotThrow()
        {
            bool result = false;
            string gradleSource = null;
            Assert.DoesNotThrow(() =>
            {
                result = EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.LauncherTemplatePath, out gradleSource);
            });
            Assert.IsTrue(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(gradleSource));
        }

        [Test, TestMustExpectAllLogs]
        public void TryReadGradlePropertiesTemplate_DoesNotThrow()
        {
            bool result = false;
            string gradleSource = null;
            Assert.DoesNotThrow(() =>
            {
                result = EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.GradlePropertiesPath, out gradleSource);
            });
            Assert.IsTrue(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(gradleSource));
        }
        
        [Test]
        public void VerifyIfSwazzlerAndBugshakeArePresentSimultaneously_DoesNotThrowsException_WhenOnlyOnePluginIsDefined()
        {
            Assert.DoesNotThrow(EmbraceGradleUtility.VerifyIfSwazzlerAndBugshakeArePresentSimultaneously);
        }
        
        #if UNITY_2022_2_OR_NEWER
        [Test, TestMustExpectAllLogs]
        public void TryReadSettingsGradle_DoesNotThrow()
        {
            bool result = false;
            string gradleSource = null;
            Assert.DoesNotThrow(() =>
            {
                result = EmbraceGradleUtility.TryReadGradleTemplate(EmbraceGradleUtility.SettingsTemplatePath, out gradleSource);
            });
            Assert.IsTrue(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(gradleSource));
        }
        #endif

        public class SetGradleProperties
        {
            private string _testFile;

            private string TEST_FILE_CONTENT = $"valueA=true{Environment.NewLine}valueB=false{Environment.NewLine}";

            [SetUp]
            public void SetUp()
            {
                _testFile = Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Temp/testProperties.gradle");
            }

            [TearDown]
            public void TearDown()
            {
                if (File.Exists(_testFile))
                {
                    File.Delete(_testFile);
                }
            }

            private void CreateTestFile(string content = "")
            {
                File.WriteAllText(_testFile, content);
            }

            private string ReadFileContents()
            {
                return File.ReadAllText(_testFile);
            }

            [Test, TestMustExpectAllLogs]
            public void DoesNotThrow_WhenPropertiesIsNull()
            {
                CreateTestFile(TEST_FILE_CONTENT);
                Assert.DoesNotThrow(() => EmbraceGradleUtility.WriteGradlePropertiesToFile(_testFile, null));
                Assert.AreEqual(TEST_FILE_CONTENT, ReadFileContents());
            }

            [Test, TestMustExpectAllLogs]
            public void DoesNotThrow_WhenPropertiesIsEmpty()
            {
                CreateTestFile(TEST_FILE_CONTENT);
                Assert.DoesNotThrow(() => EmbraceGradleUtility.WriteGradlePropertiesToFile(
                    _testFile, new KeyValuePair<string, string>[0]));
                Assert.AreEqual(TEST_FILE_CONTENT, ReadFileContents());
            }

            [Test, TestMustExpectAllLogs]
            public void WritesProperties_WhenExistingFileIsEmpty()
            {
                CreateTestFile();

                EmbraceGradleUtility.WriteGradlePropertiesToFile(_testFile, new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valueA", "true"),
                    new KeyValuePair<string, string>("valueB", "false"),
                });

                string result = ReadFileContents();

                Assert.AreEqual(TEST_FILE_CONTENT, result);
            }

            [Test, TestMustExpectAllLogs]
            public void AppendsProperties_WhenTheyDontExist()
            {
                CreateTestFile(TEST_FILE_CONTENT);

                EmbraceGradleUtility.WriteGradlePropertiesToFile(_testFile, new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valueC", "true"),
                    new KeyValuePair<string, string>("valueD", "false"),
                });

                string result = ReadFileContents();
                string expected =
                    $"{TEST_FILE_CONTENT}valueC=true{Environment.NewLine}valueD=false{Environment.NewLine}";

                Assert.AreEqual(expected, result);
            }

            [Test, TestMustExpectAllLogs]
            public void ChangesValues_WhenPropertiesAlreadyExist()
            {
                CreateTestFile(TEST_FILE_CONTENT);

                EmbraceGradleUtility.WriteGradlePropertiesToFile(_testFile, new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valueA", "false"),
                    new KeyValuePair<string, string>("valueB", "true"),
                });

                string result = ReadFileContents();
                string expected = $"valueA=false{Environment.NewLine}valueB=true{Environment.NewLine}";

                Assert.AreEqual(expected, result);
            }

            [Test, TestMustExpectAllLogs]
            public void ChangesValues_WhenPropertiesAlreadyExist_WithSpaces()
            {
                CreateTestFile($"valueA = true {Environment.NewLine}valueB = false {Environment.NewLine}");

                EmbraceGradleUtility.WriteGradlePropertiesToFile(_testFile, new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valueA", "false"),
                    new KeyValuePair<string, string>("valueB", "true"),
                });

                string result = ReadFileContents();
                string expected = $"valueA=false{Environment.NewLine}valueB=true{Environment.NewLine}";

                Assert.AreEqual(expected, result);
            }

            [Test, TestMustExpectAllLogs]
            public void SkipsProperties_WithNullOrEmptyKeysOrValues()
            {
                CreateTestFile(TEST_FILE_CONTENT);

                EmbraceGradleUtility.WriteGradlePropertiesToFile(_testFile, new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valueA", "false"),
                    new KeyValuePair<string, string>("valueC", "true"),
                    new KeyValuePair<string, string>(null, "true"),
                    new KeyValuePair<string, string>(string.Empty, "true"),
                    new KeyValuePair<string, string>("nullValue", null),
                    new KeyValuePair<string, string>("emptyValue", string.Empty),
                });

                string result = ReadFileContents();
                string expected = $"valueA=false{Environment.NewLine}valueB=false{Environment.NewLine}valueC=true{Environment.NewLine}";

                Assert.AreEqual(expected, result);
            }
        }
    }
}