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
        private static DateTimeOffset _loadingTime;
        
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
            Embrace.Instance.StartSpan("AppStartup", _appStartTime.ToUnixTimeMilliseconds());
            
#if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
            Embrace.Instance.StartSpan("FirstSceneLoaded", _appStartTime.ToUnixTimeMilliseconds(), "AppStartup");
            Embrace.Instance.StopSpan("FirstSceneLoaded", _firstSceneLoadedTime.ToUnixTimeMilliseconds());
#endif
#if EMBRACE_STARTUP_SPANS_LOADING_TIME
            Embrace.Instance.StartSpan("LoadingTime", _loadingTime.ToUnixTimeMilliseconds(), "AppStartup");
            Embrace.Instance.StopSpan("LoadingTime", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
#endif
            Embrace.Instance.StopSpan("AppStartup", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
    }
    #endif
}