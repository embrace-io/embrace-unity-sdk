using System;
using System.Net.Http;
using System.Threading.Tasks;
using Embrace.MockAPI.Models;
using EmbraceSDK;
using Newtonsoft.Json;

namespace Embrace.MockAPI
{
    /// <summary>
    /// The Mock API is used to test out the Embrace backend without actually sending data to the Embrace servers.
    /// This class provides a way to interact with the Mock API as well as tests to ensure that the Mock API is working as expected.
    /// </summary>
    public class MockAPIClient
    {
        private readonly HttpClient _httpClient = new();
        private const string BaseUrl = "https://mock-api.emb-eng.com/namespace/";

        /// <summary>
        /// Pings the API to ensure that it is up and running.
        /// </summary>
        /// <returns>Embrace response: {Data, StatusCode}</returns>
        public async Task<EmbraceResponse> Ping()
        {
            try
            {
                string requestUrl = $"{BaseUrl}logs/api/";
                string response = await GetAsync(requestUrl);
                return JsonConvert.DeserializeObject<EmbraceResponse>(response);
            }
            catch (Exception e)
            {
                EmbraceLogger.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// Sends a message to the logging endpoint with the specified severity. It will automatically generate a unique log id.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="severity">Type of severity</param>
        /// <returns>Embrace Response: {Data, StatusCode}</returns>
        public async Task<EmbraceResponse> LogMessage(string message, EMBSeverity severity)
        {
            try
            {
                string requestUrl = $"{BaseUrl}logs/api/v1/log/logging";
                var request = new LogMessageRequest(message, severity);
                string response = await PostAsync(requestUrl, request.ToHttpContent());
                return JsonConvert.DeserializeObject<EmbraceResponse>(response);
            }
            catch (Exception e)
            {
                EmbraceLogger.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// Calls the specific endpoint using a GET request and returns the response as a string.
        /// </summary>
        /// <param name="endpoint">URL to GET request</param>
        /// <returns>Response string in JSON format</returns>
        private async Task<string> GetAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{endpoint}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Calls the specific endpoint using a POST request and returns the response as a string.
        /// </summary>
        /// <param name="endpoint">URL to POST request</param>
        /// <param name="content">Gzip compressed body to send</param>
        /// <returns>Response string in JSON format</returns>
        private async Task<string> PostAsync(string endpoint, HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            request.Headers.Add("X-Em-Aid", "abc12");
            request.Headers.Add("X-Em-Did", "test_device_id");
            content.Headers.Add("Content-Encoding", "gzip");
            content.Headers.Add("Content-Type", "application/json");
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return responseBody;
        }
    }
}