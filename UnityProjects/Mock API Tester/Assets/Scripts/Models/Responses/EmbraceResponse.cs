using Newtonsoft.Json;

namespace Embrace.MockAPI.Models
{
    /// <summary>
    /// Basic response model for the Embrace Mock API.
    /// </summary>
    public class EmbraceResponse
    {
        [JsonProperty("status")]
        public int StatusCode { get; set; }
        
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}