using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Embrace.Tools
{ 
    /// <summary>
    /// A class repersentation of the YAML Doc Configs used by Doc repo
    /// </summary>
    public class DocsConfig
    {
        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string baseURL { get; set; }

        public bool disableHugoGeneratorInject { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string languageCode { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string title { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string theme { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string publishDir { get; set; }

        public Outputs outputs { get; set; }
        
        [YamlMember(Alias = "params", ApplyNamingConventions = false)]
        public Params _params { get; set; }
        public Markup markup { get; set; }
    }

    public class Outputs
    {
        public List<string> page;
        public List<string> home;
        public List<string> section;
    }

    public class Params
    {
        public SdkVersion sdkVersion;
        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string BookSection { get; set; }
        public string BookMenuReset { get; set; }
        public string BookTheme { get; set; }
    }

    public class SdkVersion
    {
        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string ios { get; set; }
        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string android { get; set; }
        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string rn { get; set; }
        [YamlMember(ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string unity { get; set; }
        [YamlMember(Alias = "unity_android", ApplyNamingConventions = false, ScalarStyle = ScalarStyle.DoubleQuoted)]
        public string unityAndroid { get; set; }
    }

    public class Markup
    {
        public Goldmark goldmark { get; set; }
    }

    public class Goldmark
    {
        public Renderer renderer { get; set; }
    }

    public class Renderer
    {
        [YamlMember(Alias = "unsafe", ApplyNamingConventions = false)]
        public bool _unsafe { get; set; }
    }
}