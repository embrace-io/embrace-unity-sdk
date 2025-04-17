using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    [Obsolete("Not compatible with version 1.6.0")]
    public class EmbraceSDKSettings : ScriptableObject
    {
        public List<string> environments;
        public int activeEnvironmentIndex = -1;
        public List<ConfigurationItem> environmentConfigurations = new List<ConfigurationItem>();
        public bool isDirty;

        public void Clear()
        {
            environments = new List<string>();
            activeEnvironmentIndex = -1;
            environmentConfigurations = new List<ConfigurationItem>();
        }
    }

    [Obsolete("Not compatible with version 1.6.0")]
    [Serializable]
    public class ConfigurationItem
    {
        public string environmentName;
        public string guid;
        public List<EmbraceSDKConfiguration> configurations = new List<EmbraceSDKConfiguration>();

        public ConfigurationItem(Guid guid)
        {
            this.guid = guid.ToString();
            configurations = new List<EmbraceSDKConfiguration>();
        }
    }
}