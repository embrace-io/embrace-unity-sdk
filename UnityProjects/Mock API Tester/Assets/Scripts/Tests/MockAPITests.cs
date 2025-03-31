using System.Threading.Tasks;
using Embrace.MockAPI.Models;
using EmbraceSDK;
using NUnit.Framework;

namespace Embrace.MockAPI.Tests
{
    /// <summary>
    /// This is a series of tests that will interact with the Mock API and ensure that it is working as expected.
    /// </summary>
    public class MockAPITests
    {
        private MockAPIClient _mockAPIClient;
    
        [OneTimeSetUp]
        public void Setup()
        {
            _mockAPIClient = new MockAPIClient();
        }

        /// <summary>
        /// A simple ping test to make sure the API is up and running.
        /// </summary>
        [Test]
        public async Task PingTest()
        {
            EmbraceResponse response = await _mockAPIClient.Ping();
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual("success", response.Data);
        }

        /// <summary>
        /// A config test that grabs a mock config from the API.
        /// </summary>
        [Test]
        public async Task GetConfigTest()
        {
            var response = await _mockAPIClient.GetConfig();
            Assert.IsNotNull(response);
            Assert.AreEqual(100, response.Ls);
            Assert.AreEqual(0, response.Offset);
            Assert.AreEqual(0, response.Personas.Count);
            Assert.AreEqual(100, response.Ui.Views);
            Assert.AreEqual(0, response.EventLimits.Count);
            Assert.AreEqual(100, response.Threshold);
            Assert.AreEqual(false, response.ScreenshotsEnabled);
            Assert.AreEqual(true, response.UrlConnectionRequestEnabled);
            Assert.AreEqual(false, response.DisableSessionControl);
            Assert.AreEqual(true, response.SessionControl.Enable);
            Assert.AreEqual(false, response.SessionControl.AsyncEnd);
        }
    
        /// <summary>
        /// A series of logging tests to ensure that the logging endpoint is working as expected.
        /// </summary>
        [Test]
        public async Task LogMessageTests()
        {
            // log info message
            EmbraceResponse response = await _mockAPIClient.LogMessage("Test Message", EMBSeverity.Info);
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual("success", response.Data);
        
            // log warning message
            response = await _mockAPIClient.LogMessage("Test Warning Message", EMBSeverity.Warning);
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual("success", response.Data);
        
            // log error message
            response = await _mockAPIClient.LogMessage("Test Error Message", EMBSeverity.Error);
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual("success", response.Data);
        }
    }
}
