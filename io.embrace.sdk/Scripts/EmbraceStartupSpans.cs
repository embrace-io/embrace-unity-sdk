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
        public static DateTimeOffset AppStartTime => _appStartTime;
        
        private static DateTimeOffset _appStartTime;
        private static DateTimeOffset _firstSceneLoadedTime;
        
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

        public static void EndAppStartup()
        {
            string parentSpanId = Embrace.Instance.StartSpan("AppStartup", _appStartTime.ToUnixTimeMilliseconds());
            Embrace.Instance.RecordCompletedSpan("LoadingComplete", _firstSceneLoadedTime.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
            
#if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
            Embrace.Instance.RecordCompletedSpan("FirstSceneLoaded", _appStartTime.ToUnixTimeMilliseconds(), _firstSceneLoadedTime.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
#endif
#if EMBRACE_STARTUP_SPANS_LOADING_COMPLETE
            Embrace.Instance.RecordCompletedSpan("LoadingComplete", _firstSceneLoadedTime.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), parentSpanId: parentSpanId);
#endif
            Embrace.Instance.StopSpan(parentSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
    #endif
}