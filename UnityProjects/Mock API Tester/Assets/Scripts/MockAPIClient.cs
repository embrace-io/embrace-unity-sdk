using System;
using System.Net.Http;
using System.Threading.Tasks;
using EmbraceSDK;
using NUnit.Framework;
using UnityEngine;

namespace Embrace.MockAPI
{
    /// <summary>
    /// The Mock API is used to test out the Embrace backend without actually sending data to the Embrace servers.
    /// This class provides a way to interact with the Mock API as well as tests to ensure that the Mock API is working as expected.
    /// </summary>
    public class MockAPIClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://mock-api.emb-eng.com/namespace/";
        
        public MockAPIClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> LogMessage(string message, EMBSeverity severity)
        {
            try
            {
                string requestUrl = $"{BaseUrl}logs/api/";
                Debug.Log($"Requesting URL: {requestUrl}");
                var response = await GetAsync(requestUrl);
                return response;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private async Task<string> GetAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{endpoint}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> PostAsync(string endpoint, HttpContent content)
        {
            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}