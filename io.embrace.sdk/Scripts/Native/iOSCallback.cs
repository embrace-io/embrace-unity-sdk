using System;
using AOT;

namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Allows us to send data from iOS to Unity.
    /// This is done by passing a delegate to a iOS function which takes a C-style function pointer. The iOS SDK then
    /// uses this function pointer to send messages to Unity
    /// </summary>
    [Obsolete("iOSCallback is deprecated and will be removed in a future release.")]
    public class iOSCallback
    {
        public delegate void CallBackMessageDelegate(string config);

        /// <summary>
        /// Called by the iOS SDK.
        /// Allows us to receive remote config json data from iOS. 
        /// </summary>
        /// <param name="json">Remote config json data.</param>
        [MonoPInvokeCallback(typeof(CallBackMessageDelegate))]
        public static void UpdateRemoteConfig(string json)
        {
            RemoteConfig rc = ConfigParser.ParseRemoteConfig(json);
        }
    }
}