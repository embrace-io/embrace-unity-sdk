using System;
using UnityEngine;

namespace EmbraceSDK
{
    #if EMBRACE_STARTUP_SPANS
    public static class EmbraceStartupSpans
    {
        private static DateTimeOffset _appStartTime;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void StartApplication()
        {
            _appStartTime = DateTimeOffset.UtcNow;
        }
        
        #if EMBRACE_STARTUP_SPANS_FIRST_SCENE_LOADED
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void FirstSceneLoaded()
        {
            PostCompletedSpan(DateTimeOffset.UtcNow, "FirstSceneLoaded");
        }
        #endif
        
        #if EMBRACE_STARTUP_SPANS_APP_READY
        public static void CallAppReady()
        {
            PostCompletedSpan(DateTimeOffset.UtcNow, "AppReady");
        }
        #endif
        
        #if EMBRACE_STARTUP_SPANS_EMBRACE_SDK_START
        public static void CallEmbraceSDKStart()
        {
            PostCompletedSpan(DateTimeOffset.UtcNow, "EmbraceSDKStart");
        }
        #endif
        
        #if EMBRACE_STARTUP_SPANS_TIME_TO_INTERACT
        public static void CallTimeToInteract()
        {
            PostCompletedSpan(DateTimeOffset.UtcNow, "TimeToInteract");
        }
        #endif

        private static void PostCompletedSpan(DateTimeOffset timeOffset, string spanName)
        {
            long startTimeMs = _appStartTime.ToUnixTimeMilliseconds();
            long readyTimeMs = timeOffset.ToUnixTimeMilliseconds();
            Embrace.Instance.RecordCompletedSpan(spanName, startTimeMs, readyTimeMs);
        }
    }
    #endif
}