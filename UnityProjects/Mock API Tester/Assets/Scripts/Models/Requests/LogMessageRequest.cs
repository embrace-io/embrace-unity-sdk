using EmbraceSDK;
using Newtonsoft.Json;

namespace Embrace.MockAPI.Models
{
    /// <summary>
    /// Log Message Request model for the Mock API.
    /// </summary>
    public class LogMessageRequest : EmbraceRequest
    {
        [JsonProperty("et")]
        public EventData Event { get; set; }

        public LogMessageRequest(string message, EMBSeverity type)
        {
            // create a unique log id
            string logId = System.Guid.NewGuid().ToString("N");
            Event = new EventData(logId, message, type);
        }
    }

    /// <summary>
    /// Event Data model for the Mock API.
    /// </summary>
    public class EventData
    {
        [JsonProperty("li")]
        public string LogId { get; set; }
        
        [JsonProperty("t")]
        public string Type { get; set; }
        
        [JsonProperty("m")]
        public string Message { get; set; }

        public EventData(string logId, string message, EMBSeverity type)
        {
            LogId = logId;
            Message = message;
            Type = type.ToString().ToLower();
        }
    }
}