using System;
using EmbraceSDK.Internal;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Base class for Embrace Editor Windows. Handles setting up windows and provides base methods.
    /// </summary>
    internal class EmbraceEditorWindow : EditorWindow
    {
        protected static EmbraceSdkInfo sdkInfo;

        protected static Environments environments;
        protected static EmbraceConfiguration androidConfiguration;
        protected static EmbraceConfiguration iOSConfiguration;

        protected static bool isSetup;
        protected Color backgroundColor = new Color(0.2196079f, 0.2196079f, 0.2196079f);

        public static void Setup()
        {
            ConstructStyles();

            ResetEnvironment();

            isSetup = true;
        }

        private static void ConstructStyles()
        {
            TextAsset infoFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(infoFile.text);
        }

        private static void ResetEnvironment()
        {
            environments = AssetDatabaseUtil.LoadEnvironments();
            androidConfiguration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(environments);
            iOSConfiguration = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(environments);
        }

        public virtual void Awake()
        {
            if (!isSetup)
            {
                Setup();
            }
        }

        public virtual void OnEnable()
        {
            if (!isSetup)
            {
                Setup();
            }
        }

        protected virtual void OnFocus()
        {
            if (environments != null &&
                environments.environmentConfigurations != null &&
                environments.environmentConfigurations.Count > 0)
            {
                // environments scriptable object, or configurations may have been
                // altered outside of this window's control. Use environments data
                // to determine if any data restoration needs to occur
                if (AssetDatabaseUtil.RefreshEnvironments(environments))
                {
                    ConfigureEnvironmentConfigs();
                }
            }
            else
            {
                // Otherwise, ensure default configs are not null.
                if (androidConfiguration == null || iOSConfiguration == null)
                {
                    ResetEnvironment();
                }
            }
        }

        protected virtual void OnLostFocus()
        {
            // Implementation in subclasses
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        public virtual void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), GuiUtil.MakeTexture(2, 2, backgroundColor), ScaleMode.StretchToFill);
        }

        protected void ConfigureEnvironmentConfigs()
        {
            foreach (var config in environments.environmentConfigurations[environments.activeEnvironmentIndex].sdkConfigurations)
            {
                if (config.DeviceType == EmbraceDeviceType.Android)
                {
                    androidConfiguration = config;
                }
                else if (config.DeviceType == EmbraceDeviceType.IOS)
                {
                    iOSConfiguration = config;
                }
            }

            environments.isDirty = false;
        }

        /// <summary>
        /// Returns true if Embrace editor windows should be allowed to open in this editor session.
        /// </summary>
        public static bool ShouldShowEditorWindows()
        {
            #if EMBRACE_SHOW_EDITOR_WINDOWS_IN_BATCHMODE
            return true;
            #else
            return !Application.isBatchMode;
            #endif
        }
    }
}