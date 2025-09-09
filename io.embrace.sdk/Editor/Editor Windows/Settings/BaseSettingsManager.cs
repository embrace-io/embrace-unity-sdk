using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// The BaseSettingsManager is an abstract class which allows for various categories to the drawn to the MainSettingsEditor pane.
    /// </summary>
    [Serializable]
    internal abstract class BaseSettingsManager : ISettingsWindow
    {
        protected MainSettingsEditor mainSettingsEditor;
        protected Environments environments;
        protected EmbraceConfiguration androidConfiguration;
        protected EmbraceConfiguration iOSConfiguration;

        public BaseSettingsManager()
        {
        }

        /// <summary>
        /// Initialize the manager after deserialization.
        /// </summary>
        public virtual void Initialize(MainSettingsEditor mainSettingsEditor)
        {
            this.mainSettingsEditor = mainSettingsEditor;
            LoadConfigurations();
        }

        protected void LoadConfigurations()
        {
            environments = AssetDatabaseUtil.LoadEnvironments();
            androidConfiguration = AssetDatabaseUtil.LoadConfiguration<AndroidConfiguration>(environments);
            iOSConfiguration = AssetDatabaseUtil.LoadConfiguration<IOSConfiguration>(environments);
        }

        /// <summary>
        /// Draws the Manager.
        /// </summary>
        public abstract void OnGUI();

        /// <summary>
        /// Handles when the Manager gains focus
        /// </summary>
        public virtual void OnFocus()
        {
        }

        /// <summary>
        /// Handles when the Manager loses focus
        /// </summary>
        public virtual void OnLostFocus()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }
}