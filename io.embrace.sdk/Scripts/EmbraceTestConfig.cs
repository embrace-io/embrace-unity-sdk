using Newtonsoft.Json;

namespace EmbraceSDK.Tests
{
    public class EmbraceTestConfig
    {
        [JsonProperty("app_id")]
        public string AppId { get; set; }
        
        [JsonProperty("api_token")]
        public string ApiToken { get; set; }
    }
}