using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmbraceSDK
{
    public class EmbraceSpanEvent
    {
        [JsonProperty(PropertyName = "name")]
        private string _name;
        [JsonProperty(PropertyName = "timestampMs")]
        private long _timestampMs;
        [JsonProperty(PropertyName = "timestampNanos")]
        private long _timestampNanos;
        [JsonProperty(PropertyName = "attributes")]
        private Dictionary<string, string> _attributes;
        
        public EmbraceSpanEvent(string name, long timestampMs, long timestampNanos, Dictionary<string, string> attributes)
        {
            this._name = name;
            this._timestampMs = timestampMs;
            this._timestampNanos = timestampNanos;
            this._attributes = attributes;
        }
        
        public string GetName()
        {
            return _name;
        }
        
        public long GetTimestampMs()
        {
            return _timestampMs;
        }
        
        public long GetTimestampNanos()
        {
            return _timestampNanos;
        }
        
        public Dictionary<string, string> GetAttributes()
        {
            return _attributes;
        }
    }
}