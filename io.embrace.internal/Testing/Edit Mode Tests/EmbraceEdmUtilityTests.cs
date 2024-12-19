using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class EmbraceEdmUtilityTests
    {
        #if UNITY_ANDROID
        private static KeyValuePair<string, string> _EdmDetected = new KeyValuePair<string, string>(EmbraceEdmUtility.EDM_PRESENT_PROPERTY_KEY, "true");
        private static KeyValuePair<string, string> _EdmDeactivated = new KeyValuePair<string, string>(EmbraceEdmUtility.EDM_PRESENT_PROPERTY_KEY, "false");

        private const string TestFolder = "Assets/TestFolder";
        private const string TestFileName = "TestFile.xml";

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(TestFolder);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            var testFilePath = Path.Combine(TestFolder, TestFileName);
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Test]
        public void IsFileAlreadyCreated_WhenFileExistsAndContentIsDifferent_ShouldReturnFalse()
        {
            var testFilePath = Path.Combine(TestFolder, TestFileName);
            File.WriteAllText(testFilePath, "<root><element>OldValue</element></root>");
            
            var result = EmbraceEdmUtility.IsFileAlreadyCreated("<root><element>NewValue</element></root>", testFilePath);
            
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileAlreadyCreated_WhenFileExistsAndContentIsSame_ShouldReturnTrue()
        {
            var testFilePath = Path.Combine(TestFolder, TestFileName);
            var xmlData = "<root><dependency>TestDependency</dependency></root>";
            File.WriteAllText(testFilePath, xmlData);
            
            var result = EmbraceEdmUtility.IsFileAlreadyCreated(xmlData, testFilePath);
            
            Assert.IsTrue(result);
        }
        
        [Test]
        public void IsFileAlreadyCreated_WhenFileReadFails_ShouldLogWarningAndReturnFalse()
        {
            var invalidFilePath = "C:/NonexistentDirectory/NonexistentFile.xml";
            
            var result = EmbraceEdmUtility.IsFileAlreadyCreated("<root></root>", invalidFilePath);
            
            Assert.IsFalse(result);
        }
        
        [Test]
        public void SaveDependenciesFile_WhenCalledWithValidData_ShouldCreateFileSuccessfully()
        {
            var testFilePath = Path.Combine(TestFolder, TestFileName);
            var xmlData = "<dependency>TestDependency</dependency>";
            
            var result = EmbraceEdmUtility.SaveDependenciesFile(xmlData, testFilePath);
            
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(testFilePath));
            var fileContent = File.ReadAllText(testFilePath);
            Assert.AreEqual(xmlData, fileContent);
            
            LogAssert.Expect(LogType.Log,$"{EmbraceLogger.LOG_TAG}: Embrace SDK Dependencies XML file has been generated and saved.");
        }
        
        [Test]
        public void SaveDependenciesFile_WhenCalledWithInvalidXmlData_ShouldNotCreateFileAndReturnFalse()
        {
            var testFilePath = Path.Combine(TestFolder, TestFileName);
            var invalidXmlData = "Invalid XML data";
            
            var result = EmbraceEdmUtility.SaveDependenciesFile(invalidXmlData, testFilePath);
            
            Assert.IsFalse(result);
            Assert.IsFalse(File.Exists(testFilePath));
        }
        
        [Test]
        public void SaveDependenciesFile_WhenDirectoryCreationFails_ShouldLogErrorAndReturnFalse()
        {
            var invalidPath = "C:/NonexistentDirectory/TestDependenciesFile.xml";
            var xmlData = "<root><dependency>TestDependency</dependency></root>";
            
            var result = EmbraceEdmUtility.SaveDependenciesFile(xmlData, invalidPath);
            
            Assert.IsFalse(result);
            Assert.IsFalse(File.Exists(invalidPath));
            
            LogAssert.Expect(LogType.Warning,$"{EmbraceLogger.LOG_TAG}: Failed to create the directory for the Embrace dependencies XML file.");
        }
        
        [Test]
        public void GetEDMSettings_ReturnTrue_WhenMainTemplateIsPatched()
        {
            var properties = EmbraceEdmUtility.GetEdmProperties(
                $"{EmbraceGradleUtility.ANDROID_SDK_DEPENDENCY}:1.2.3");

            Assert.IsTrue(properties.Contains(_EdmDetected));
        }

        [Test]
        public void GetEDMSettings_ReturnFalse_WhenMainTemplateIsNotPresent()
        {
            var properties = EmbraceEdmUtility.GetEdmProperties(null);

            Assert.IsTrue(properties.Contains(_EdmDeactivated));
        }

        [Test]
        public void GetSearchQueryForEdmLibrary_FindAndroidSDK()
        {
            string taggedLibrary = EmbraceEdmUtility.GetSearchQueryForEdmLibrary(EmbraceEdmUtility.ANDROID_SDK_LIB_NAME);

            Assert.IsTrue(taggedLibrary.Contains(EmbraceEdmUtility.EDM_LIBRARY_TAG));
        }

        [Test]
        public void GetSearchQueryForDefaultLibrary_FindAndroidSDK()
        {
            string taggedLibrary = EmbraceEdmUtility.GetSearchQueryForDefaultLibrary(EmbraceEdmUtility.ANDROID_SDK_LIB_NAME);

            Assert.IsTrue(taggedLibrary.Contains(EmbraceEdmUtility.DEFAULT_LIBRARY_TAG));
        }

        // These tests are separated into a nested class so the TearDown coroutine, which forces scripts to recompile,
        // only runs when absolutely necessary
        public class UserOverrideTests
        {
            [UnityTearDown]
            public IEnumerator TearDown()
            {
                ScriptingDefineUtil defineUtil = new ScriptingDefineUtil();
                defineUtil.ReadFromProjectSettings();
                defineUtil.ToggleSymbol(EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_TRUE, false);
                defineUtil.ToggleSymbol(EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_FALSE, false);
                defineUtil.ApplyModifiedProperties();
                AssetDatabaseUtil.ForceRecompileScripts();
                yield return new RecompileScripts();
            }

            [UnityTest]
            public IEnumerator GetEDMSettings_ReturnTrue_WhenManualOverrideTrue()
            {
                ScriptingDefineUtil defineUtil = new ScriptingDefineUtil();
                defineUtil.ReadFromProjectSettings();
                defineUtil.ToggleSymbol(EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_TRUE, true);
                defineUtil.ToggleSymbol(EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_FALSE, false);
                defineUtil.ApplyModifiedProperties();
                AssetDatabaseUtil.ForceRecompileScripts();

                yield return new RecompileScripts();

                bool isDefined = false;

                #if EMBRACE_USE_EDM_TRUE
                isDefined = true;
                #endif

                var properties = EmbraceEdmUtility.GetEdmProperties(null);

                Assert.IsTrue(properties.Contains(_EdmDetected));
                Assert.IsTrue(isDefined);
            }

            [UnityTest]
            public IEnumerator GetEDMSettings_ReturnFalse_WhenManualOverrideFalse()
            {
                ScriptingDefineUtil defineUtil = new ScriptingDefineUtil();
                defineUtil.ReadFromProjectSettings();
                defineUtil.ToggleSymbol(EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_TRUE, false);
                defineUtil.ToggleSymbol(EmbraceEdmUtility.EDM_MANUAL_OVERRIDE_FALSE, true);
                defineUtil.ApplyModifiedProperties();
                AssetDatabaseUtil.ForceRecompileScripts();

                yield return new RecompileScripts();

                bool isDefined = false;

                #if EMBRACE_USE_EDM_FALSE
                isDefined = true;
                #endif

                var properties = EmbraceEdmUtility.GetEdmProperties(null);

                Assert.IsTrue(properties.Contains(_EdmDeactivated));
                Assert.IsTrue(isDefined);
            }
        }
#endif
        }
    }