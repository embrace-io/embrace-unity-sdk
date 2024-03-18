using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EmbraceSDK.Validators;
using UnityEditor;
using UnityEditor.VSAttribution.Embrace;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// The Gettings Started editor window, allows customers to configure android and IOS, and provides helpful links.
    /// </summary>
    public class GettingsStartedEditorWindow : EmbraceEditorWindow
    {
        private static bool? ANDROID_SELECTED = null;
        
        private int environmentIndex = -1;

        private Texture2D logo;

        public enum ConfigTypes
        {
            androidID,
            iosID,
            androidToken,
            iosToken
        }

        private static GettingsStartedEditorWindow window;
        private int activeToolbar;
        private Dictionary<ConfigTypes, string> warningMessages = new Dictionary<ConfigTypes, string>();
        private ConfigValidator<ConfigTypes> configValidator = new ConfigValidator<ConfigTypes>();

        private bool androidSettings;
        private Vector2 scrollPos;

        private static GUIContentLibrary guiContentLibrary = new GUIContentLibrary();

        private static (GUIContent content, GUIStyle style) GetContentTuple(GUIContentLibrary.GUIContentIdentifier identifier)
        {
            return guiContentLibrary.GetContentTuple(identifier, styleConfigs);
        }

        [MenuItem("Tools/Embrace/Getting Started")]
        public static void Init()
        {
            if (!ShouldShowEditorWindows())
            {
                return;
            }

            Setup();
            // Get existing open window or if none, make a new one:
            window = GetWindow<GettingsStartedEditorWindow>(EmbraceEditorConstants.WindowTitleGettingStarted);
            window.minSize = new Vector2(500f, 570f);
            window.maxSize = window.minSize;

            if (environments.environmentConfigurations.Count != 0)
            {
                window.minSize = new Vector2(window.minSize.x, window.minSize.y + 15);
                window.maxSize = window.minSize;
            }

            if (!ANDROID_SELECTED.HasValue)
            {
                environments.activeDeviceIndex = 0; // set to Android by Default
            }
            else
            {
                environments.activeEnvironmentIndex = ANDROID_SELECTED.Value ? 0 : 1; // Set to Android if true, set to iOS if false
            }

            window.Show();
        }

        public override void Awake()
        {
            base.Awake();

            logo = Resources.Load<Texture2D>("EditorImages/embrace_color_logo");
            warningMessages[ConfigTypes.androidID] = "Invalid Android App ID";
            warningMessages[ConfigTypes.androidToken] = "Invalid Android Symbol Upload API Token";
            warningMessages[ConfigTypes.iosID] = "Invalid iOS App ID";
            warningMessages[ConfigTypes.iosToken] = "Invalid iOS Symbol Upload API Token";
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (environments.environmentConfigurations.Count != 0)
            {
                ConfigureEnvironmentConfigs();
            }
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public override void OnGUI()
        {
            base.OnGUI();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            Header();

            Configuration();

            Divider();
            
            IntegrationGuide();

            Divider();

            Demo();

            Footer();

            EditorGUILayout.EndScrollView();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void Header()
        {
            GUILayout.BeginHorizontal(styleConfigs.darkBoxStyle.guiStyle, GUILayout.Height(90));
            GUI.DrawTexture(new Rect(10, 10, 251, 70), logo, ScaleMode.ScaleToFit, true,
                logo.width / (float)logo.height);
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Label("Embrace " + sdkInfo.version, styleConfigs.headerTextStyle.guiStyle);
            if (GUILayout.Button("View Changelog"))
            {
                Application.OpenURL("https://embrace.io/docs/unity/changelog/");
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Getting Started", styleConfigs.labelHeaderStyle.guiStyle);
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void Configuration()
        {
            GUILayout.BeginVertical(styleConfigs.lightBoxStyle.guiStyle, GUILayout.Height(160));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Configuration", styleConfigs.labelTitleStyle.guiStyle);

            if (environments == null)
            {
                environments = AssetDatabaseUtil.LoadEnvironments();
            }

            if (environments.environmentConfigurations.Count != 0)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Selected Configuration:", styleConfigs.defaultTextStyle.guiStyle);

                environments.activeEnvironmentIndex = EditorGUILayout.Popup(environments.activeEnvironmentIndex,
                    environments.environmentConfigurations.Select(e => e.name).ToArray());

                if (environmentIndex != environments.activeEnvironmentIndex || environments.isDirty)
                {
                    ConfigureEnvironmentConfigs();
                    environmentIndex = environments.activeEnvironmentIndex;
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(styleConfigs.space);
            var selectedEnvironment = GUILayout.Toolbar(environments.activeDeviceIndex, Environments.DeviceStrings);
            if (selectedEnvironment != environments.activeDeviceIndex)
            {
                environments.activeDeviceIndex = selectedEnvironment;
                ANDROID_SELECTED = environments.activeEnvironmentIndex == 0; // Android=0 => true, iOS=1 => false 
            }

            switch (environments.activeDeviceIndex)
            {
                case 0: // Android
                    GUILayout.Space(styleConfigs.space);
                    
                    if (!Validator.ValidateID(androidConfiguration.AppId))
                    {
                        InsertAppIDFetchBlock();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId).content,
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId).style);
                    androidConfiguration.AppId = EditorGUILayout.TextField(androidConfiguration.AppId);
                    GUILayout.EndHorizontal();
                    
                    if (!Validator.ValidateToken(androidConfiguration.SymbolUploadApiToken))
                    {
                        InsertAPITokenFetchBlock();
                    }

                    GUILayout.Space(styleConfigs.space);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAPIToken).content),
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAPIToken).style);
                    androidConfiguration.SymbolUploadApiToken =
                        EditorGUILayout.TextField(androidConfiguration.SymbolUploadApiToken);
                    GUILayout.EndHorizontal();

                    if (androidConfiguration.AppId.Length == 0 && androidConfiguration.SymbolUploadApiToken.Length == 0)
                    {
                        if (configValidator.activeWarningMessages.Contains(ConfigTypes.androidID))
                            configValidator.activeWarningMessages.Remove(ConfigTypes.androidID);
                        if (configValidator.activeWarningMessages.Contains(ConfigTypes.androidToken))
                            configValidator.activeWarningMessages.Remove(ConfigTypes.androidToken);
                        ClearToolbar(0);
                        break;
                    }

                    ShowWarning(Validator.ValidateID(androidConfiguration.AppId), ConfigTypes.androidID);
                    ShowWarning(Validator.ValidateToken(androidConfiguration.SymbolUploadApiToken),
                        ConfigTypes.androidToken);

                    if (configValidator.activeWarningMessages.Count >= 1)
                    {
                        GUILayoutUtil.Alert(
                            warningMessages[
                                configValidator.activeWarningMessages[configValidator.activeWarningMessages.Count - 1]],
                            window, GUILayoutUtil.AlertType.Warning);
                    }

                    ClearToolbar(0);
                    break;
                case 1: // iOS
                    GUILayout.Space(styleConfigs.space);
                    
                    if (!Validator.ValidateID(iOSConfiguration.AppId))
                    {
                        InsertAppIDFetchBlock();
                    }
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId).content,
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId).style);
                    iOSConfiguration.AppId = EditorGUILayout.TextField(iOSConfiguration.AppId);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(styleConfigs.space);
                    
                    if (!Validator.ValidateToken(iOSConfiguration.SymbolUploadApiToken))
                    {
                        InsertAPITokenFetchBlock();
                    }
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAPIToken).content,
                        GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAPIToken).style);
                    iOSConfiguration.SymbolUploadApiToken =
                        EditorGUILayout.TextField(iOSConfiguration.SymbolUploadApiToken);
                    GUILayout.EndHorizontal();

                    if (iOSConfiguration.AppId.Length == 0 && iOSConfiguration.SymbolUploadApiToken.Length == 0)
                    {
                        if (configValidator.activeWarningMessages.Contains(ConfigTypes.iosID))
                            configValidator.activeWarningMessages.Remove(ConfigTypes.iosID);
                        if (configValidator.activeWarningMessages.Contains(ConfigTypes.iosToken))
                            configValidator.activeWarningMessages.Remove(ConfigTypes.iosToken);
                        ClearToolbar(1);
                        break;
                    }

                    ShowWarning(Validator.ValidateID(iOSConfiguration.AppId), ConfigTypes.iosID);
                    ShowWarning(Validator.ValidateToken(iOSConfiguration.SymbolUploadApiToken), ConfigTypes.iosToken);

                    if (configValidator.activeWarningMessages.Count >= 1)
                    {
                        GUILayoutUtil.Alert(
                            warningMessages[
                                configValidator.activeWarningMessages[configValidator.activeWarningMessages.Count - 1]],
                            window, GUILayoutUtil.AlertType.Warning);
                    }

                    ClearToolbar(1);
                    break;
            }
            
            GUILayout.Space(styleConfigs.space);
            var completeVspRegistration = GUILayout.Button("Complete SDK Initialization");
            if (completeVspRegistration)
            {
                string customerId = null;
                switch (environments.activeDeviceIndex)
                {
                    case 0: // Android
                        customerId = androidConfiguration.SymbolUploadApiToken;
                        break;
                    case 1: // iOS
                        customerId = iOSConfiguration.SymbolUploadApiToken;
                        break;
                }

                if (customerId != null)
                {
                    VSAttribution.SendAttributionEvent("sdkInitialization", "embrace", customerId);
                }
            }

            GUILayout.Space(styleConfigs.space);
            var clicked = GUILayout.Button("Customize SDK Configuration");
            if (clicked)
            {
                var mainSettings = GetWindow<MainSettingsEditor>(EmbraceEditorConstants.WindowTitleSettings);
                mainSettings.MenuSelection = 1;
            }

            GUILayout.EndVertical();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void Demo()
        {
            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Samples", styleConfigs.labelHeaderStyle.guiStyle);

            GUILayout.BeginVertical(styleConfigs.lightBoxStyle.guiStyle);
            GUILayout.Label("SDK Demo", styleConfigs.labelTitleStyle.guiStyle);
            GUILayout.Label("This demo showcases examples of how to interface with the Embrace SDK.",
                new GUIStyle(styleConfigs.defaultTextStyle.guiStyle));

            GUILayout.Space(styleConfigs.space);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Import into Project"))
            {
                string destinationPath = Application.dataPath + "/Sample/Embrace SDK/" + sdkInfo.version;
                Directory.CreateDirectory(destinationPath);
                string sourcePath = Application.dataPath.Replace("/Assets", "") +
                                    "/Packages/io.embrace.sdk/Samples/Demo";
                AssetDatabaseUtil.CopyDirectory(sourcePath, destinationPath, true, true);
                AssetDatabase.Refresh();
            }

            GUILayout.Space(styleConfigs.space);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void IntegrationGuide()
        {
            GUILayout.Space(styleConfigs.space);
            GUILayout.Label("Integration Checklist", styleConfigs.labelHeaderStyle.guiStyle);

            GUILayout.BeginVertical(styleConfigs.lightBoxStyle.guiStyle);
            GUILayout.Label("Integration Components", 
                styleConfigs.labelTitleStyle.guiStyle);
            GUILayout.Label("This checklist will help you integrate the Embrace SDK into your project.", 
                styleConfigs.defaultTextStyle.guiStyle);
            GUILayout.Label("Checks will autofill as you complete each step.", 
                styleConfigs.defaultTextStyle.guiStyle);

            switch (environments.activeDeviceIndex)
            {
                case 0: // Android
                    GUILayout.Toggle(
                        Validator.ValidateID(androidConfiguration.AppId) && Validator.ValidateToken(androidConfiguration.SymbolUploadApiToken),
                        "App ID and API Token of Valid Format");
                    break;
                case 1: // iOS
                    GUILayout.Toggle(
                        Validator.ValidateID(iOSConfiguration.AppId) && Validator.ValidateToken(iOSConfiguration.SymbolUploadApiToken),
                        "App ID and API Token of Valid Format");
                    break;
            }

            if (environments.activeDeviceIndex == 0) // Handle Android Configuration Specifics
            {
                GUILayout.Space(styleConfigs.space);

                //Cache the paths to the templates.
                var doesBaseProjectTemplateExists = File.Exists(EmbraceGradleUtility.BaseProjectTemplatePath);
                var doesLauncherTemplateExists = File.Exists(EmbraceGradleUtility.LauncherTemplatePath);
                var doesGradlePropertiesTemplateExists = File.Exists(EmbraceGradleUtility.GradlePropertiesPath);

                var templatesPresent = doesBaseProjectTemplateExists
                                       && doesLauncherTemplateExists
                                       && doesGradlePropertiesTemplateExists;
                
                #if UNITY_2022_2_OR_NEWER
                var doesSettingsTemplateExists = File.Exists(EmbraceGradleUtility.SettingsTemplatePath);
                templatesPresent &= doesSettingsTemplateExists;
                #endif

                if (!templatesPresent)
                {
                    // Display warning for each missing template piece.

                    if (!doesBaseProjectTemplateExists)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissing).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissing).style);
                    }

                    if (!doesLauncherTemplateExists)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissing).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissing).style);
                    }

                    if (!doesGradlePropertiesTemplateExists)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissing).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissing).style);
                    }
                    #if UNITY_2022_2_OR_NEWER
                    if (!doesSettingsTemplateExists)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissing).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissing).style);
                    }
                    #endif
                }

                GUILayout.Toggle(templatesPresent, "Required Android Templates Present");
                
                // Validate each file as necessary
                var baseProjectTemplateValid = false;
                var launcherTemplateValid = false;
                var gradlePropertiesValid = false;
                
                #if UNITY_2022_2_OR_NEWER
                var settingsTemplateValid = false;
                #endif

                if (doesBaseProjectTemplateExists)
                {
                    #if !UNITY_2022_2_OR_NEWER
                    // Validate Base Project Template
                    var (foundImport, allRepositoriesValid) = 
                        AndroidBaseProjectTemplateValidator.Validate(EmbraceGradleUtility.BaseProjectTemplatePath);
                    #else
                    // Validate Base Project Template
                    var foundImport = AndroidBaseProjectTemplateValidator.Validate(EmbraceGradleUtility.BaseProjectTemplatePath);
                    #endif

                    if (!foundImport)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingImport).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingImport).style);
                    }

                    #if !UNITY_2022_2_OR_NEWER
                    if (!allRepositoriesValid)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingMavenCentral).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingMavenCentral).style);
                    }
                    
                    baseProjectTemplateValid = foundImport && allRepositoriesValid;
                    #else
                    baseProjectTemplateValid = foundImport;
                    #endif
                }

                #if UNITY_2022_2_OR_NEWER
                if (doesSettingsTemplateExists)
                {
                    // Validate Settings Template
                    settingsTemplateValid = AndroidSettingsTemplateValidator.Validate(EmbraceGradleUtility.SettingsTemplatePath);

                    if (!settingsTemplateValid)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissingMavenCentral).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissingMavenCentral).style);
                    }
                }
                #endif

                if (doesLauncherTemplateExists)
                {
                    // Validate Launcher Template 
                    launcherTemplateValid = AndroidLauncherTemplateValidator.Validate(EmbraceGradleUtility.LauncherTemplatePath);

                    if (!launcherTemplateValid)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissingPluginImport).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissingPluginImport).style);
                    }
                }

                if (doesGradlePropertiesTemplateExists)
                {
                    // Validate Gradle Properties Template
                    var (foundAndroidX, foundJetifier) = AndroidGradlePropertiesTemplateValidator.Validate(EmbraceGradleUtility.GradlePropertiesPath);

                    if (!foundAndroidX)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingAndroidX).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingAndroidX).style);
                    }

                    if (!foundJetifier)
                    {
                        GUILayout.Label(
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingJetifier).content,
                            GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingJetifier).style);
                    }

                    gradlePropertiesValid = foundAndroidX && foundJetifier;
                }
            
                var allGroovyRequirementsMet = baseProjectTemplateValid && launcherTemplateValid && gradlePropertiesValid;
                #if UNITY_2022_2_OR_NEWER
                allGroovyRequirementsMet &= settingsTemplateValid;
                #endif

                GUILayout.Toggle(allGroovyRequirementsMet, "Gradle/Groovy Requirements Met");
            }

            GUILayout.EndVertical();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void Footer()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(styleConfigs.darkBoxStyle.guiStyle);
            GUILayout.Label("Online Resources", styleConfigs.labelHeaderStyle.guiStyle);
            GUILayout.Space(styleConfigs.space);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Docs"))
            {
                Application.OpenURL("https://embrace.io/docs/unity/");
            }

            if (GUILayout.Button("Changelog"))
            {
                Application.OpenURL("https://embrace.io/docs/unity/changelog/");
            }

            if (GUILayout.Button("Dash"))
            {
                Application.OpenURL("https://dash.embrace.io/");
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private bool NeedsValidation(ConfigTypes type)
        {
            switch (type)
            {
                case ConfigTypes.androidID:
                    return configValidator.DetermineIfValidationIsNeeded(type, androidConfiguration.AppId.Length, 5);
                case ConfigTypes.iosID:
                    return configValidator.DetermineIfValidationIsNeeded(type, iOSConfiguration.AppId.Length, 5);
                case ConfigTypes.androidToken:
                    return configValidator.DetermineIfValidationIsNeeded(type,
                        androidConfiguration.SymbolUploadApiToken.Length, 32);
                case ConfigTypes.iosToken:
                    return configValidator.DetermineIfValidationIsNeeded(type,
                        iOSConfiguration.SymbolUploadApiToken.Length, 32);
                default:
                    break;
            }

            return false;
        }

        private void ShowWarning(bool isValid, ConfigTypes configType)
        {
            if (!isValid && NeedsValidation(configType))
            {
                if (!configValidator.activeWarningMessages.Contains(configType))
                {
                    configValidator.activeWarningMessages.Add(configType);
                }
            }
            else if (isValid)
            {
                if (configValidator.activeWarningMessages.Contains(configType))
                {
                    configValidator.activeWarningMessages.Remove(configType);
                }
            }
        }

        private void ClearToolbar(int index)
        {
            if (activeToolbar != index)
            {
                activeToolbar = index;
                GUI.FocusControl(null);
            }
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        private void Divider()
        {
            GUILayout.BeginHorizontal(styleConfigs.dividerBoxStyle.guiStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(styleConfigs.highlightBoxStyle.guiStyle);
            GUILayout.EndHorizontal();
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        protected override void OnLostFocus()
        {
            EditorUtility.SetDirty(androidConfiguration);
            EditorUtility.SetDirty(iOSConfiguration);
        }

        public class ConfigValidator<T>
        {
            public List<T> needsValidationList;
            public List<T> activeWarningMessages;

            // 60 sec in a min, 60 min in a hour, 24hr in a day and 7 days in a week = sec in a week.
            public const int MIN_SESSION_SECONDS = 60;
            public const int MAX_SESSION_SECONDS = 60 * 60 * 24 * 7;


            public ConfigValidator(List<T> needsValidationList = null, List<T> activeWarningMessages = null)
            {
                this.needsValidationList = needsValidationList ?? new List<T>();
                this.activeWarningMessages = activeWarningMessages ?? new List<T>();
            }

            public bool DetermineIfValidationIsNeeded(T config, int count, int desiredValue)
            {
                if (count >= desiredValue && !needsValidationList.Contains(config))
                {
                    needsValidationList.Add(config);
                    return true;
                }

                if (count == 0 && needsValidationList.Contains(config))
                {
                    needsValidationList.Remove(config);
                    activeWarningMessages.Remove(config);
                    return true;
                }

                return needsValidationList.Contains(config);
            }

            /// <summary>
            /// Set min value for max_session_seconds to 60 sec and max value of 604800 (week in seconds)
            /// </summary>
            public static int ClampMaxSessionsSeconds(int value)
            {
                return Mathf.Clamp(MIN_SESSION_SECONDS, MAX_SESSION_SECONDS, value);
            }
        }

        private static void InsertAppIDFetchBlock(
            string warningMessage = "App ID Required to Integrate Embrace Unity SDK")
        {
            InsertFetchBlock("App ID Required to Integrate Embrace Unity SDK",
                "Get Your App ID",
                "Navigate to the Embrace Dashboard to get your App ID");
        }

        private static void InsertAPITokenFetchBlock()
        {
            InsertFetchBlock("API Token Required to Integrate Embrace Unity SDK",
                "Get Your API Token",
                "Navigate to the Embrace Dashboard to get your API Token");
        }

        private static void InsertFetchBlock(string warningMessage, string buttonText, string buttonTooltip)
        {
            GUILayout.Label(new GUIContent(warningMessage),
                new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                {
                    alignment = TextAnchor.MiddleCenter, normal =
                        new GUIStyleState()
                        {
                            textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                            background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                        }
                });
            if (GUILayout.Button(new GUIContent(buttonText, buttonTooltip)))
            {
                Application.OpenURL("https://dash.embrace.io/app");
            }
        }
    }
}