using Newtonsoft.Json;
using System;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// A class representation of the package.json file.
    /// </summary>
    [Serializable]
    public class Package
    {
        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public string unityRelease;
        public string documentationUrl;
        public string changelogUrl;
        public string licensesUrl;
        public string[] keywords;
        public Author author;
        public bool hideInEditor;
        public string type;
        public Sample[] samples;
        public Dependencies dependencies;
        public string[] testables;
    }

    [Serializable]
    public class Author
    {
        public string name;
        public string email;
        public string url;
    }

    [Serializable]
    public class Sample
    {
        public string displayName;
        public string description;
        public string path;
    }

    [Serializable]
    public class Dependencies
    {
        [JsonProperty("com.unity.nuget.newtonsoft-json")]
        public string newtonsoft;
        [JsonProperty("com.unity.nuget.mono-cecil")]
        public string monoCecil;
    }
}