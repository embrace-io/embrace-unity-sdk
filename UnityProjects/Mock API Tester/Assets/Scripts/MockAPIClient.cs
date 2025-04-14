using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Embrace.MockAPI.Models;
using EmbraceSDK;
using Newtonsoft.Json;
using UnityEngine;

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
        /// Gets a mock config from the API.
        /// </summary>
        /// <returns>Config model</returns>
        public async Task<ConfigResponse> GetConfig()
        {
            try
            {
                string requestUrl = $"{BaseUrl}logs/api/v2/config?appId=abc12";
                string response = await GetAsync(requestUrl);
                ConfigResponse config = JsonConvert.DeserializeObject<ConfigResponse>(response);
                return config;
            }
            catch (Exception e)
            {
                EmbraceLogger.LogException(e);
                return null;
            }
        }

        public async Task<EmbraceResponse> PostConfig()
        {
            try
            {
                var requestUrl = $"{BaseUrl}logs/api/v2/config/abc12";
                var request = new ConfigRequest("key", "value");
                var response = await PostAsync(requestUrl, request.ToHttpContent());
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
                string response = await PostAsync(requestUrl, request.ToHttpContent(), new Dictionary<string, string>
                {
                    {"Content-Encoding", "gzip"},
                    {"Content-Type", "application/json"}
                });
                return JsonConvert.DeserializeObject<EmbraceResponse>(response);
            }
            catch (Exception e)
            {
                EmbraceLogger.LogException(e);
                return null;
            }
        }
        
        /// <summary>
        /// Makes a call to the logging endpoint with a message a status which gets converted to a byte blob
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="status">Status to send</param>
        /// <returns>Response from mock API server</returns>
        public async Task<EmbraceResponse> LogBlob(string message, string status)
        {
            try
            {
                string requestUrl = $"{BaseUrl}logs/api/v1/log/blobs";
                var request = new LogBlobRequest(message, status);
                string response = await PostAsync(requestUrl, request.ToHttpContent(), new Dictionary<string, string>
                {
                    {"Content-Encoding", "gzip"},
                    {"Content-Type", "application/json"}
                });
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
        /// <param name="headers">Optional headers for request</param>
        /// <returns>Response string in JSON format</returns>
        private async Task<string> PostAsync(string endpoint, HttpContent content, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach(var header in headers)
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }

            request.Headers.Add("X-Em-Aid", "abc12");
            request.Headers.Add("X-Em-Did", "test_device_id");
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log(responseBody);
            response.EnsureSuccessStatusCode();
            return responseBody;
        }
    }
}