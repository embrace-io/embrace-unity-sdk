using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK
{
    #if EMBRACE_STARTUP_SPANS
    /// <summary>
    /// Helper functions to record startup spans in the Embrace SDK.
    /// Everything is automatically recorded when the developer calls EndAppStartup.
    /// If you are using the Embrace SDK, you can call CallAppReady and CallTimeToInteract in your code to record those spans.
    /// </summary>
    public static class EmbraceStartupSpans
    {
        private class ChildSpan
        {
            public Dictionary<string, string> Attributes;
            public long StartTime;
            public long EndTime;
        }

        private static readonly Dictionary<string, ChildSpan> _childSpans = new();
        private static DateTimeOffset _appStartTime;
        private static DateTimeOffset _firstSceneLoadedTime;
        private static DateTimeOffset _embraceSDKStartTime;
        private static DateTimeOffset _embraceSDKEndTime;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void StartApplication()
        {
            _appStartTime = DateTimeOffset.UtcNow;
        }
        
        #if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void FirstSceneLoaded()
        {
            _firstSceneLoadedTime = DateTimeOffset.UtcNow;
        }
        #endif

        public static void RecordStartSDKTime()
        {
            _embraceSDKStartTime = DateTimeOffset.UtcNow;
        }

        public static void RecordStopSDKTime()
        {
            _embraceSDKEndTime = DateTimeOffset.UtcNow;
        }
        
        public static void StartChildSpan(string spanName)
        {
            if (string.IsNullOrEmpty(spanName))
            {
                Debug.LogWarning("EmbraceStartupSpans: Span name must not be null or empty.");
                return;
            }

            if (_childSpans.ContainsKey(spanName))
            {
                Debug.LogWarning($"EmbraceStartupSpans: Span '{spanName}' already exists. It will be overwritten.");
                _childSpans.Remove(spanName);
            }
            
            long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _childSpans[spanName] = new ChildSpan
            {
                Attributes = new Dictionary<string, string>(),
                StartTime = startTime,
                EndTime = 0
            };
        }
        
        public static void StopChildSpan(string spanId, Dictionary<string, string> attributes = null)
        {
            if (string.IsNullOrEmpty(spanId))
            {
                Debug.LogWarning("EmbraceStartupSpans: Span ID must not be null or empty.");
                return;
            }

            if (_childSpans.TryGetValue(spanId, out var span) == false)
            {
                Debug.LogWarning($"EmbraceStartupSpans: Span '{spanId}' does not exist. Cannot stop span.");
                return;
            }
            
            long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            span.EndTime = endTime;
            span.Attributes = attributes;
        }

        public static void EndAppStartup(Dictionary<string, string> attributes = null)
        {
            string parentSpanId = Embrace.Instance.StartSpan("emb-app-startup", _appStartTime.ToUnixTimeMilliseconds());
            
#if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
            Embrace.Instance.RecordCompletedSpan("emb-app-loaded", _appStartTime.ToUnixTimeMilliseconds(), _firstSceneLoadedTime.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
#endif
#if EMBRACE_STARTUP_SPANS_LOADING_COMPLETE
            Embrace.Instance.RecordCompletedSpan("emb-app-time-to-interactive", _firstSceneLoadedTime.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
#endif
            Embrace.Instance.RecordCompletedSpan("emb-embrace-init", _embraceSDKStartTime.ToUnixTimeMilliseconds(), _embraceSDKEndTime.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);

            // add any additional attributes to the parent span
            if (attributes != null)
            {
                foreach (var kvp in attributes)
                {
                    Embrace.Instance.AddSpanAttribute(parentSpanId, kvp.Key, kvp.Value);
                }
            }

            // record all child spans
            foreach ((string spanName, var childSpan) in _childSpans)
            {
                Embrace.Instance.RecordCompletedSpan(spanName, childSpan.StartTime, childSpan.EndTime, attributes: childSpan.Attributes, parentSpanId: parentSpanId);
            }
            
            _childSpans.Clear();
            
            // finally, stop the parent span
            Embrace.Instance.StopSpan(parentSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
    #endif
}