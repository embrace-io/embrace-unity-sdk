using System;

namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Represents the Unity fields in the RemoteConfig.
    /// </summary>
    [Serializable]
    public class RemoteConfig
    {
        public bool capture_fps_data;
        public bool capture_network_requests;
    }
}
