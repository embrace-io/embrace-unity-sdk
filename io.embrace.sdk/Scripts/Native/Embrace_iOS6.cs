using System.Runtime.InteropServices;

namespace EmbraceSDK.Internal
{
    #if UNITY_IOS || UNITY_TVOS
    public class Embrace_iOS6
    {
        [DllImport("__Internal")]
        private static extern bool embrace_sdk_is_started();
        
        public bool EmbraceSDKIsStarted() => embrace_sdk_is_started();
    }
    #endif
}