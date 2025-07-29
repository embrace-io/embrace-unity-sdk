using System;
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

        public static void EndAppStartup()
        {
            string parentSpanId = Embrace.Instance.StartSpan("emb-app-startup", _appStartTime.ToUnixTimeMilliseconds());
            
#if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
            Embrace.Instance.RecordCompletedSpan("emb-app-loaded", _appStartTime.ToUnixTimeMilliseconds(), _firstSceneLoadedTime.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
#endif
#if EMBRACE_STARTUP_SPANS_LOADING_COMPLETE
            Embrace.Instance.RecordCompletedSpan("emb-app-init", _firstSceneLoadedTime.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
#endif
            Embrace.Instance.RecordCompletedSpan("emb-embrace-init", _embraceSDKStartTime.ToUnixTimeMilliseconds(), _embraceSDKEndTime.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
            Embrace.Instance.StopSpan(parentSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
    #endif
}