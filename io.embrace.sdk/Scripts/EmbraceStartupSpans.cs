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
        public static string ParentSpanId { get; private set; }

        private static DateTimeOffset _appStartTime;
        private static DateTimeOffset _firstSceneLoadedTime;
        private static DateTimeOffset _embraceSDKStartTime;
        private static DateTimeOffset _embraceSDKEndTime;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void StartApplication()
        {
            _appStartTime = DateTimeOffset.UtcNow;
            ParentSpanId = Embrace.Instance.StartSpan("emb-app-startup", _appStartTime.ToUnixTimeMilliseconds());
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
        
        public static string StartChildSpan(string spanName)
        {
            if (string.IsNullOrEmpty(spanName))
            {
                Debug.LogWarning("EmbraceStartupSpans: Span name must not be null or empty.");
                return null;
            }

            return Embrace.Instance.StartSpan(spanName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), parentSpanId: ParentSpanId);
        }
        
        public static void StopChildSpan(string spanId, Dictionary<string, string> attributes = null)
        {
            if (string.IsNullOrEmpty(spanId))
            {
                Debug.LogWarning("EmbraceStartupSpans: Span ID must not be null or empty.");
                return;
            }
            
            if (attributes != null)
            {
                foreach (var kvp in attributes)
                {
                    Embrace.Instance.AddSpanAttribute(spanId, kvp.Key, kvp.Value);
                }
            }

            Embrace.Instance.StopSpan(spanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        public static void EndAppStartup(Dictionary<string, string> attributes = null)
        {
            
#if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
            Embrace.Instance.RecordCompletedSpan("emb-app-loaded", _appStartTime.ToUnixTimeMilliseconds(), _firstSceneLoadedTime.ToUnixTimeMilliseconds(), parentSpanId: ParentSpanId);
#endif
#if EMBRACE_STARTUP_SPANS_LOADING_COMPLETE
            Embrace.Instance.RecordCompletedSpan("emb-app-time-to-interactive", _firstSceneLoadedTime.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), parentSpanId: ParentSpanId);
#endif
            Embrace.Instance.RecordCompletedSpan("emb-embrace-init", _embraceSDKStartTime.ToUnixTimeMilliseconds(), _embraceSDKEndTime.ToUnixTimeMilliseconds(), parentSpanId: ParentSpanId);

            if (attributes != null)
            {
                foreach (var kvp in attributes)
                {
                    Embrace.Instance.AddSpanAttribute(ParentSpanId, kvp.Key, kvp.Value);
                }
            }
            
            Embrace.Instance.StopSpan(ParentSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
    #endif
}