using System.Net.Http;
using Newtonsoft.Json;

namespace Embrace.MockAPI.Models
{
    public class ConfigRequest : EmbraceRequest
    {
        [JsonProperty("payload")]
        public string Payload { get; set; }

        public ConfigRequest(string key, string value)
        {
            var kvp = new { key, value };
            Payload = JsonConvert.SerializeObject(kvp);
        }

        public override HttpContent ToHttpContent()
        {
            return new StringContent(Payload);
        }
    }
}