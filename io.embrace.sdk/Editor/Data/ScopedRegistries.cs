using System;
using System.Collections.Generic;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Used to represent Scoped Registries attribute in the manifest file.
    /// </summary>
    [Serializable]
    public class ScopedRegistries
    {
        public ScopedRegistry[] scopedRegistries;

        public ScopedRegistries(ScopedRegistry scopedRegistry)
        {
            scopedRegistries = new ScopedRegistry[] { scopedRegistry };
        }
    }

    /// <summary>
    /// A class representation of Scoped Registries.
    /// </summary>
    [Serializable]
    public class ScopedRegistry
    {
        public string name;
        public string url;
        public string[] scopes;

        public ScopedRegistry(string name, string url, string[] scopes)
        {
            this.name = name;
            this.url = url;
            this.scopes = scopes;
        }
    }
}
