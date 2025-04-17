using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Allows users to manage multiple sets of EmbraceConfiguration.
    /// Represents two EmbraceConfiguration one for Android and another for iOS.
    /// </summary>
#if DeveloperMode
    [CreateAssetMenu(fileName = "Data", menuName = "EmbraceSDK/Environments")]
#endif
    public class Environments : ScriptableObject
    {
        public static readonly string[] DeviceStrings = { "Android", "iOS" };

        public Action EnvironmentsReset;

        public int activeEnvironmentIndex = -1;
        public int activeDeviceIndex = 0;
        public List<EnvironmentConfiguration> environmentConfigurations = new List<EnvironmentConfiguration>();
        public bool isDirty;

        public void Clear()
        {
            activeEnvironmentIndex = -1;

            foreach (var envConfig in environmentConfigurations)
            {
                envConfig.Clear();
            }

            environmentConfigurations.Clear();

            if (EnvironmentsReset != null)
            {
                EnvironmentsReset();
            }
        }
    }

    [Serializable]
    public class EnvironmentConfiguration
    {
        public string name;
        public string guid;
        public List<EmbraceConfiguration> sdkConfigurations;

        public EmbraceConfiguration this[EmbraceDeviceType platform]
        {
            get
            {
                for (int i = 0; i < sdkConfigurations.Count; ++i)
                {
                    if (sdkConfigurations[i].DeviceType == platform)
                    {
                        return sdkConfigurations[i];
                    }
                }

                return null;
            }
        }

        public EnvironmentConfiguration(string guid, string name = "")
        {
            this.name = name;
            this.guid = guid;
            sdkConfigurations = new List<EmbraceConfiguration>();
        }

        public void Clear()
        {
            name = null;
            guid = null;
            sdkConfigurations.Clear();
        }
    }
}