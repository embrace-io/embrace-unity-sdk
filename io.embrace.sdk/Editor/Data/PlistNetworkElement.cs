using System;
using Newtonsoft.Json;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Represents the nested NETWORK dictionary element in Embrace-Info.plist
    /// </summary>
    [Serializable]
    public class PlistNetworkElement : ITooltipPropertiesProvider, IJsonSerializable
    {
        [Tooltip(EmbraceTooltips.CapturePublicKey)]
        public string CAPTURE_PUBLIC_KEY;

        [Tooltip(EmbraceTooltips.DefaultCaptureLimit)]
        public int DEFAULT_CAPTURE_LIMIT;

        [Tooltip(EmbraceTooltips.Domains)]
        [JsonConverter(typeof(PlistDictionaryConverter<string, int>))]
        public PlistIntDictionary DOMAINS = new PlistIntDictionary();

        public bool ShouldSerialize()
        {
            return
                CAPTURE_PUBLIC_KEY != string.Empty ||
                DEFAULT_CAPTURE_LIMIT > 0;
        }

        public void Clear()
        {
            CAPTURE_PUBLIC_KEY = string.Empty;
            DEFAULT_CAPTURE_LIMIT = 0;
            DOMAINS.Clear();
        }
    }
}