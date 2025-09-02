using System.Collections.Generic;

namespace EmbraceSDK.Utilities
{
    internal static class EmbraceSpanIdTracker
    {
        private static readonly Dictionary<string, string> _nameToSpanId = new();

        public static string GetSpanId(string name)
        {
            return _nameToSpanId.GetValueOrDefault(name);
        }
        
        public static bool HasSpanId(string name)
        {
            return _nameToSpanId.ContainsKey(name);
        }
        
        public static void AddSpanId(string name, string spanId)
        {
            _nameToSpanId[name] = spanId;
        }
        
        public static void RemoveSpanId(string name)
        {
            _nameToSpanId.Remove(name);
        }
    }
}