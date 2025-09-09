using EmbraceSDK.Editor.Weaver;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class SettingsManagerTests
    {
        [Test]
        public void AssetDatabaseUtilFindsAllExpectedSettingsManagers()
        {
            BaseSettingsManager[] managers = AssetDatabaseUtil.GetInstances<BaseSettingsManager>();

            Assert.IsNotNull(GetInstanceOfType<BaseSettingsManager, GeneralManager>(managers));
            Assert.IsNotNull(GetInstanceOfType<BaseSettingsManager, ConfigurationManager>(managers));
            Assert.IsNotNull(GetInstanceOfType<BaseSettingsManager, EmbraceWeaverSettingsManager>(managers));
            Assert.IsNotNull(GetInstanceOfType<BaseSettingsManager, EmbraceSpansManager>(managers));

            Assert.AreEqual(4, managers.Length);
        }

        [Test]
        public void ConfigurationManagerInitializesWithoutErrors()
        {
            BaseSettingsManager[] managers = AssetDatabaseUtil.GetInstances<BaseSettingsManager>();
            ConfigurationManager configManager = GetInstanceOfType<BaseSettingsManager, ConfigurationManager>(managers);

            MainSettingsEditor settingsEditor = MainSettingsEditor.CreateInstance<MainSettingsEditor>();

            configManager.Initialize(settingsEditor);

            LogAssert.NoUnexpectedReceived();

            Object.DestroyImmediate(settingsEditor);
        }

        [Test]
        public void GeneralManagerInitializesWithoutErrors()
        {
            BaseSettingsManager[] managers = AssetDatabaseUtil.GetInstances<BaseSettingsManager>();
            GeneralManager generalManager = GetInstanceOfType<BaseSettingsManager, GeneralManager>(managers);

            MainSettingsEditor settingsEditor = MainSettingsEditor.CreateInstance<MainSettingsEditor>();

            generalManager.Initialize(settingsEditor);

            LogAssert.NoUnexpectedReceived();

            Object.DestroyImmediate(settingsEditor);
        }

        private TDerivedType GetInstanceOfType<TBaseType, TDerivedType>(TBaseType[] array)
            where TDerivedType : class, TBaseType
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i] is TDerivedType instance)
                {
                    return instance;
                }
            }

            return null;
        }
    }
}