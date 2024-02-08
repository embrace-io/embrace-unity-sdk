using UnityEngine;
using Newtonsoft.Json;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Base configuration data set by the editor windows and exported to both embrace-config.json and Embrace-Info.plist SDK configuration files.
    /// </summary>
    public abstract class EmbraceConfiguration : ScriptableObject
    {
        [JsonIgnore]
        public abstract string AppId { get; set; }

        [JsonIgnore]
        public abstract string SymbolUploadApiToken { get; set; }

        [JsonIgnore]
        [System.Obsolete("ApiToken property has been deprecated. Please use SymbolUploadApiToken property instead", false)]
        public string ApiToken
        {
            get => SymbolUploadApiToken;
            set => SymbolUploadApiToken = value;
        }

        [JsonIgnore]
        public abstract EmbraceDeviceType DeviceType { get; }

        [JsonIgnore]
        public string EnvironmentName { get; set; }

        [JsonIgnore]
        public string EnvironmentGuid { get; set; }

        public abstract void SetDefaults();
    }
}