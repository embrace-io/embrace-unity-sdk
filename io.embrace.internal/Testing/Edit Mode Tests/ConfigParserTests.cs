using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EmbraceSDK;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EmbraceSDK.Internal;

#pragma warning disable 618
namespace EmbraceSDK.Tests
{
    /// <summary>
    /// Tests ConfigParser
    /// </summary>
    public class ConfigParserTests
    {
        private const string DefaultRemoteConfig =
            "{\"ls\":100,\"event_limits\":{},\"offset\":0,\"personas\":[],\"screenshots_enabled\":false,\"threshold\":100,\"ui\":{\"views\":100},\"urlconnection_request_enabled\":true,\"metrickit_enabled\":true}";

        private const string UnityRemoteConfig =
            "{\"ls\": 100, \"event_limits\": {}, \"offset\": 0, \"personas\": [], \"screenshots_enabled\": false, \"threshold\": 100, \"ui\": {\"views\": 100}, \"urlconnection_request_enabled\": true, \"metrickit_enabled\": true, \"unity\":{\"capture_fps_data\":false,\"capture_network_requests\":true}}";


        /// <summary>
        /// Test if the Remote Config does not have Unity field ConfigParser returns null.
        /// </summary>
        [Test]
        public void RemoteConfigNoUnityField()
        {
            RemoteConfig config = ConfigParser.ParseRemoteConfig(DefaultRemoteConfig);
            Assert.IsNull(config);
        }

        /// <summary>
        /// Tests if ConfigParser.ParseRemoteConfig is able to parse out Unity fields from a remote config.
        /// </summary>
        [Test]
        public void ParseRemoteConfig()
        {
            RemoteConfig expected = new RemoteConfig();
            expected.capture_fps_data = false;
            expected.capture_network_requests = true;
            RemoteConfig result = ConfigParser.ParseRemoteConfig(UnityRemoteConfig);

            Assert.AreEqual(result.capture_fps_data, expected.capture_fps_data);
            Assert.AreEqual(result.capture_network_requests, expected.capture_network_requests);
        }

        /// <summary>
        /// Check if ConfigParser.ParseRemoteConfig is able to handle malformed JSON.
        /// </summary>
        [Test]
        public void ParseMalformedJson()
        {
            RemoteConfig result = ConfigParser.ParseRemoteConfig("{Malformed JSON: {error} }");
            LogAssert.Expect(LogType.Error, new Regex("Exception parsing json at line 1:"));
        }

        /// <summary>
        /// Check if ConfigParser.ParseRemoteConfig is able to handle null.
        /// </summary>
        [Test]
        public void ParseNull()
        {
            RemoteConfig result = ConfigParser.ParseRemoteConfig(null);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Check if ConfigParser.ParseRemoteConfig is able to handle Empty String.
        /// </summary>
        [Test]
        public void ParseEmtpyString()
        {
            RemoteConfig result = ConfigParser.ParseRemoteConfig(string.Empty);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Check if ConfigParser.ParseRemoteConfig is able to handle Whitespace.
        /// </summary>
        [Test]
        public void ParseWhitespace()
        {
            RemoteConfig result = ConfigParser.ParseRemoteConfig(" ");

            Assert.IsNull(result);
        }
    }
}
#pragma warning restore 618