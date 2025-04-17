using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Helps parses through config data.
    /// </summary>
    [Obsolete("ConfigParser is obsolete and will be removed in a future release.")]
    public class ConfigParser
    {
        private const string UNITY_TOKEN = "unity";

        /// <summary>
        /// Parses Remote Config looking for the 'unity' property, if found it serializes data to RemoteConfig.
        /// </summary>
        /// <param name="json">JSON to parse through.</param>
        /// <returns>Return RemoteConfig, if unity field is not found return null.</returns>
        public static RemoteConfig ParseRemoteConfig(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                JToken jToken;
                try
                {
                    jToken = JObject.Parse(json).SelectToken(UNITY_TOKEN);
                }
                catch (JsonReaderException jex)
                {
                    EmbraceLogger.LogError($"Exception parsing json at line {jex.LineNumber}: {jex.Message}");
                    return null;
                }

                if (jToken != null)
                {
                    RemoteConfig remoteConfig = jToken.ToObject<RemoteConfig>();
                    return remoteConfig;
                }
            }

            return null;
        }
    }
}