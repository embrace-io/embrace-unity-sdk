using System.Collections.Generic;
using Newtonsoft.Json;

namespace Embrace.MockAPI.Models
{
    /// <summary>
    /// Config Response model for the Mock API.
    /// </summary>
    public class ConfigResponse
    {
        [JsonProperty("disable_session_control")]
        public bool DisableSessionControl { get; set; }
    
        [JsonProperty("event_limits")]
        public Dictionary<string, object> EventLimits { get; set; } = new();
    
        [JsonProperty("ls")]
        public int Ls { get; set; }
    
        [JsonProperty("offset")]
        public int Offset { get; set; }
    
        [JsonProperty("personas")]
        public List<object> Personas { get; set; } = new();
    
        [JsonProperty("screenshots_enabled")]
        public bool ScreenshotsEnabled { get; set; }
    
        [JsonProperty("session_control")]
        public SessionControl SessionControl { get; set; }
    
        [JsonProperty("threshold")]
        public int Threshold { get; set; }
    
        [JsonProperty("ui")]
        public UiModel Ui { get; set; }
    
        [JsonProperty("urlconnection_request_enabled")]
        public bool UrlConnectionRequestEnabled { get; set; }
    }

    public class SessionControl
    {
        [JsonProperty("async_end")]
        public bool AsyncEnd { get; set; }
    
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }

    public class UiModel
    {
        [JsonProperty("views")]
        public int Views { get; set; }
    }

}