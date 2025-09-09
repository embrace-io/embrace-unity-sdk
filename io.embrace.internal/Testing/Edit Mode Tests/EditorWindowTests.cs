using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EmbraceSDK.EditorView;
using UnityEditor;
using System.IO;
using EmbraceSDK.Internal;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NSubstitute;

namespace EmbraceSDK.Tests
{
    [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
    public class EditorWindowTests : IEmbraceTest
    {
        private const int imgSize = 2;

        private static string[] configValidatorTestValues = { "abcdefg", "123456", "     ", "a", "abcdefghi" };
        private static int[] maxSessionSecondsTestValues = { int.MinValue, -50, 60, 25225, int.MaxValue };

        [TearDown]
        public void TearDown()
        {
            EmbraceProjectSettings.MockProjectSettings = null;
            EmbraceProjectSettings.MockUserSettings = null;
        }

        /// <summary>
        /// Test if Welcome window is opened after SDK is updated.
        /// </summary>
        [Test]
        public void InstallationUpdateSDKOpensWelcomeWindow()
        {
            ISettingsStore mockUserSettings = Substitute.For<ISettingsStore>();
            mockUserSettings.GetValue<string>(nameof(DeviceSDKInfo.version), Arg.Any<string>()).Returns("0.0.0");
            mockUserSettings.GetValue<bool>(nameof(DeviceSDKInfo.isManifestSetup), Arg.Any<bool>()).Returns(true);
            EmbraceProjectSettings.MockUserSettings = mockUserSettings;

            Installation.InitializeOnLoad();

            Assert.AreEqual(EmbraceEditorWindow.ShouldShowEditorWindows(),EditorWindow.HasOpenInstances<WelcomeEditorWindow>());

            Cleanup();
        }

        /// <summary>
        /// Test if Welcome window is opened after SDK is imported for the first time.
        /// </summary>
        [Test]
        public void InstallationFirstTimeOpensWelcomeWindow()
        {
            ISettingsStore mockUserSettings = Substitute.For<ISettingsStore>();
            mockUserSettings.GetValue<string>(nameof(DeviceSDKInfo.version), Arg.Any<string>()).Returns((string)null);
            mockUserSettings.GetValue<bool>(nameof(DeviceSDKInfo.isManifestSetup), Arg.Any<bool>()).Returns(true);
            EmbraceProjectSettings.MockUserSettings = mockUserSettings;

            Installation.InitializeOnLoad();

            Assert.AreEqual(EmbraceEditorWindow.ShouldShowEditorWindows(),EditorWindow.HasOpenInstances<WelcomeEditorWindow>());

            Cleanup();
        }

        /// <summary>
        /// Test that Welcome window does not open if SDK is imported but is the same version and not an update or is being imported for the first time.
        /// </summary>
        [Test]
        public void InstallationWelcomeWindowNotOpened()
        {
            Installation.InitializeOnLoad();

            Assert.IsFalse(EditorWindow.HasOpenInstances<WelcomeEditorWindow>());
        }

        /// <summary>
        /// Test if InitializeOnLoad() is updating the version in EmbraceSdkInfo.json after SDK is updated.
        /// </summary>
        [Test]
        public void InstallationDeviceJsonFileUpdated()
        {
            // setup
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo testSdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);

            ISettingsStore mockUserSettings = Substitute.For<ISettingsStore>();
            mockUserSettings.GetValue<string>(nameof(DeviceSDKInfo.version), Arg.Any<string>()).Returns("0.0.0");
            mockUserSettings.GetValue<bool>(nameof(DeviceSDKInfo.isManifestSetup), Arg.Any<bool>()).Returns(true);
            EmbraceProjectSettings.MockUserSettings = mockUserSettings;

            // test
            Installation.InitializeOnLoad();

            mockUserSettings.Received().SetValue(nameof(DeviceSDKInfo.version), testSdkInfo.version, true);
        }

        /// <summary>
        /// Test if InitializeOnLoad() is creating an EmbraceSdkInfo.json on device when SDK is imported for the first time.
        /// </summary>
        [Test]
        public void InstallationAddsDeviceJson()
        {
            ISettingsStore mockUserSettings = Substitute.For<ISettingsStore>();
            mockUserSettings.GetValue<string>(nameof(DeviceSDKInfo.version), Arg.Any<string>()).Returns((string)null);
            mockUserSettings.GetValue<bool>(nameof(DeviceSDKInfo.isManifestSetup), Arg.Any<bool>()).Returns(true);
            EmbraceProjectSettings.MockUserSettings = mockUserSettings;

            Installation.InitializeOnLoad();

            mockUserSettings.Received().SetValue(nameof(DeviceSDKInfo.version), Arg.Any<string>(), true);
        }

        /// <summary>
        /// Test manifest that has an existing but different embrace dependency.
        /// </summary>
        [Test]
        public void ManifestWithDifferentEmbraceDependency()
        {
            string originalManifest = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json");
            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json", UpdateEmbraceDependency("ManifestNoEmbraceDependency", "2.1.0"));

            TestManifestFile(originalManifest, "ResultingManifest");
        }

        /// <summary>
        /// Test manifest with the same embrace version for dependency.
        /// </summary>
        [Test]
        public void ManifestWithSameEmbraceDependency()
        {
            string originalManifest = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json");
            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json", UpdateEmbraceDependency("ManifestNoEmbraceDependency"));

            TestManifestFile(originalManifest, "ResultingManifest");
        }

        /// <summary>
        /// Test manifest with no embrace dependency.
        /// </summary>
        [Test]
        public void ManifestWithNoEmbraceDependency()
        {
            string originalManifest = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json");
            TextAsset newManifest = Resources.Load<TextAsset>("TestManifests/ManifestNoEmbraceDependency");
            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json", newManifest.text);

            TestManifestFile(originalManifest, "ResultingManifest");
        }

        /// <summary>
        /// Test that the GuiUtil is creating the correct texture.
        /// </summary>
        [Test]
        public void MakeTextureTest()
        {
            Texture2D texture = GuiUtil.MakeTexture(imgSize, imgSize, Color.white);
            Assert.IsNotNull(texture);

            for (int x = 0; x < imgSize; x++)
            {
                for (int y = 0; y < imgSize; y++)
                {
                    Color color = texture.GetPixel(x, y);
                    Assert.AreEqual(Color.white, color);
                }
            }
        }

        /// <summary>
        /// Test that the Validator is correctly validating the token when you call ValidateToken.
        /// </summary>
        [Test]
        public void ValidateTokenTest()
        {
            string validInput = "Test27e891ad458e89K1004eb75Lb9f1";
            Assert.IsTrue(Validator.ValidateToken(validInput));

            string toMany = "Test27e891ad458e89K1004eb75Lb9f12";
            Assert.IsFalse(Validator.ValidateToken(toMany));

            string notEnough = "Test";
            Assert.IsFalse(Validator.ValidateToken(notEnough));

            string invalidChar = "Test27e891ad458e89K1004eb75Lb9f1%";
            Assert.IsFalse(Validator.ValidateToken(invalidChar));
        }

        /// <summary>
        /// Test that the Validator is correctly validating the ID when you call ValidateID.
        /// </summary>
        [Test]
        public void ValidateIDTest()
        {
            string validInput = "Rr3Ee";
            Assert.IsTrue(Validator.ValidateID(validInput));

            string toMany = "RrFEeeeeeeeeeeee";
            Assert.IsFalse(Validator.ValidateID(toMany));

            string notEnough = "Rr";
            Assert.IsFalse(Validator.ValidateID(notEnough));

            string invalidChar = "RrFE%";
            Assert.IsFalse(Validator.ValidateID(invalidChar));
        }

        /// <summary>
        /// Test that the Init function opens an instance of the GettingStartedEditorWindow without errors, and
        /// subsequent Awake and OnEnable calls do not throw.
        /// </summary>
        [Test]
        [TestMustExpectAllLogs]
        public void GettingStartedEditorWindowInitializesSuccessfully()
        {
            GettingsStartedEditorWindow.Init();

            Assert.AreEqual(EmbraceEditorWindow.ShouldShowEditorWindows(), EditorWindow.HasOpenInstances<GettingsStartedEditorWindow>());

            GettingsStartedEditorWindow window = EditorWindow.GetWindow<GettingsStartedEditorWindow>();

            window.Awake();
            window.Close();

            LogAssert.NoUnexpectedReceived();
        }

        /// <summary>
        /// Test that the Init function opens an instance of the MainSettingsWindow without errors, and
        /// subsequent Awake and OnEnable calls do not throw.
        /// </summary>
        [Test]
        [TestMustExpectAllLogs]
        public void MainSettingsWindowInitializesSuccessfully()
        {
            MainSettingsEditor.Init();

            Assert.AreEqual(EmbraceEditorWindow.ShouldShowEditorWindows(),EditorWindow.HasOpenInstances<MainSettingsEditor>());

            MainSettingsEditor window = EditorWindow.GetWindow<MainSettingsEditor>();

            window.Awake();
            window.OnEnable();

            window.Close();

            LogAssert.NoUnexpectedReceived();
        }

        /// <summary>
        /// Test that the GettingStartedEditorWindow.ConfigValidator adds configs to the validation list when appropriate.
        /// </summary>
        [Test]
        public void GettingStartedWindowConfigValidator([ValueSource(nameof(configValidatorTestValues))] string testValue)
        {
            GettingsStartedEditorWindow.ConfigValidator<GettingsStartedEditorWindow.ConfigTypes> validator =
                new GettingsStartedEditorWindow.ConfigValidator<GettingsStartedEditorWindow.ConfigTypes>();

            bool needsValidation;

            needsValidation = validator.DetermineIfValidationIsNeeded(GettingsStartedEditorWindow.ConfigTypes.iosID, testValue.Length - 1, testValue.Length);

            Assert.IsFalse(needsValidation);
            Assert.IsFalse(validator.needsValidationList.Contains(GettingsStartedEditorWindow.ConfigTypes.iosID));

            needsValidation = validator.DetermineIfValidationIsNeeded(GettingsStartedEditorWindow.ConfigTypes.iosID, testValue.Length, testValue.Length);

            Assert.IsTrue(needsValidation);
            Assert.IsTrue(validator.needsValidationList.Contains(GettingsStartedEditorWindow.ConfigTypes.iosID));
        }

        /// <summary>
        /// Test that the GettingStartedWindowConfigValidator.ClampMaxSession returns values in the expected range
        /// </summary>
        [Test]
        public void TestGettingStartedWindowConfigValidatorClampMaxSessionSeconds([ValueSource(nameof(maxSessionSecondsTestValues))] int testValue)
        {
            int clampedValue = GettingsStartedEditorWindow.ConfigValidator<SnapAxis>.ClampMaxSessionsSeconds(testValue);

            Assert.GreaterOrEqual(clampedValue, GettingsStartedEditorWindow.ConfigValidator<GettingsStartedEditorWindow.ConfigTypes>.MIN_SESSION_SECONDS);
            Assert.LessOrEqual(clampedValue, GettingsStartedEditorWindow.ConfigValidator<GettingsStartedEditorWindow.ConfigTypes>.MAX_SESSION_SECONDS);
        }

        /// <summary>
        /// Test that the ConfigurationItem constructor initializes all fields correctly
        /// </summary>
        [Test]
        public void TestConfigurationItemConstructorCreatesValidInstance()
        {
            string guid = Guid.NewGuid().ToString();
            EnvironmentConfiguration item = new EnvironmentConfiguration(guid);

            Assert.IsNotNull(item.sdkConfigurations);
            Assert.AreEqual(guid, item.guid);
        }

        private string UpdateEmbraceDependency(string fileName, string version = null)
        {
            TextAsset newManifest = Resources.Load<TextAsset>("TestManifests/" + fileName);
            string manifestJson = newManifest.text;
            JObject parsedJson = JObject.Parse(manifestJson);
            string packageJson = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/package.json");
            Package package = JsonUtility.FromJson<Package>(packageJson);
            if(!string.IsNullOrEmpty(version))
                package.version = version;

            // add Embrace dependency
            if (parsedJson["dependencies"][package.name] == null)
            {
                JProperty newProperty = new JProperty(package.name, package.version);
                parsedJson["dependencies"].First.AddBeforeSelf(newProperty);
            }
            else
            {
                if ((string)parsedJson["dependencies"][package.name] != package.version)
                {
                    parsedJson["dependencies"][package.name] = package.version;
                }
            }

            return parsedJson.ToString(Formatting.Indented);
        }

        private void TestManifestFile(string originalManifest, string resultingManifest)
        {
            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo originalSdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);
            AssetDatabase.Refresh();

            // Set up mock setting store to return false when the Installation script checks if the manifest is already set up.
            // Also have it return the real version property to stop the welcome window from opening when this test is run.
            string version = EmbraceProjectSettings.User.GetValue<string>(nameof(DeviceSDKInfo.version));
            EmbraceProjectSettings.MockUserSettings = Substitute.For<ISettingsStore>();
            EmbraceProjectSettings.MockUserSettings.GetValue<string>(nameof(DeviceSDKInfo.version)).Returns(version);

            Installation.InitializeOnLoad();
            string manifestResult = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json");

            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/manifest.json", originalManifest);
            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/Resources/Info/EmbraceSdkInfo.json", JsonUtility.ToJson(originalSdkInfo));
            
            Debug.Log("---Expected---");
            Debug.Log(UpdateEmbraceDependency(resultingManifest));
            Debug.Log("---Actual---");
            Debug.Log(manifestResult);
            Debug.Log("---End---");
            
            Assert.AreEqual(UpdateEmbraceDependency(resultingManifest), manifestResult);
        }

        public void Cleanup()
        {
            WelcomeEditorWindow window = (WelcomeEditorWindow)EditorWindow.GetWindow(typeof(WelcomeEditorWindow));
            window.Close();
        }
    }
}