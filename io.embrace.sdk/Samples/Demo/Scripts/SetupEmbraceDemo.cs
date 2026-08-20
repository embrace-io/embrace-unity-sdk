using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmbraceSDK.Internal;
using UnityEngine;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demonstrates how to initialize the EmbraceSDK. For more information please see our documentation.
    /// https://embrace.io/docs/unity/integration/session-reporting/
    /// </summary>
    public class SetupEmbraceDemo : MonoBehaviour
    {
        public string AppId = "abcde";
        #if DeveloperMode
        public string AppGroupId = "";
        public string BaseUrl = "http://your-url.com";
        public string DevBaseUrl = "http://your-url.com";
        public string ConfigBaseUrl = "http://your-url.com";
        #endif
        
        // Start is async so we can await SDK startup below. Unity invokes MonoBehaviour lifecycle
        // methods without awaiting them, so `async void` (rather than `async Task`) is the correct
        // signature here.
        async void Start()
        {
            #if DeveloperMode && UNITY_IOS
            // This setup is for Embrace Developer Mode on iOS only.
            await Embrace.Instance.StartSDK(new EmbraceStartupArgs(AppId,
                EmbraceConfig.Default,
                AppGroupId.Length > 0 ? AppGroupId : null,
                BaseUrl.Length > 0 ? BaseUrl : null,
                DevBaseUrl.Length > 0 ? DevBaseUrl : null,
                ConfigBaseUrl.Length > 0 ? ConfigBaseUrl : null));
            #elif UNITY_IOS
            // This setup is for Embrace on iOS only.
            await Embrace.Instance.StartSDK(new EmbraceStartupArgs(AppId, EmbraceConfig.Default, null, null, null, null, new List<string> {"example.com"}));
            #else
            // This setup is for Embrace on Android.
            await Embrace.Instance.StartSDK();
            #endif

            // Awaiting StartSDK above guarantees the native SDK is ready for calls, so it's now
            // safe to make SDK calls immediately, e.g. Embrace.Instance.SetUserIdentifier(...).

            #if EMBRACE_STARTUP_SPANS && EMBRACE_STARTUP_SPANS_LOADING_COMPLETE
            await SimulateLoadingComplete();
            #elif EMBRACE_STARTUP_SPANS
            Embrace.Instance.EndAppStartup();
            #endif
        }
        
        #if EMBRACE_STARTUP_SPANS_LOADING_COMPLETE
        private async Task SimulateLoadingComplete()
        {
            await Task.Delay(2500); // Simulate some loading time
            Embrace.Instance.EndAppStartup();
        }
        #endif
    }
}
