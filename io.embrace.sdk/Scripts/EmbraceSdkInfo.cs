namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Provides information about the SDK which is used by SDK during updates and by various Embrace editor windows.
    /// </summary>
    public class EmbraceSdkInfo
    {
        public string version;
        public string swiftRef;
        public string npmAPIEndpoint;

        // Welcome Window
        public string wAnnouncementTitle;
        public string wAnnouncementMessage;

        public (SwiftRefType type, string value) SwiftRef()
        {
            var parts = swiftRef?.Split(':');
            if (parts == null)
            {
                // Default to the version of the Unity package.
                return (SwiftRefType.Version, version);
            }
            if (parts.Length != 2 || string.IsNullOrEmpty(parts[1]))
            {
                EmbraceLogger.LogFormat(UnityEngine.LogType.Warning, "Invalid swiftRef {0}. Defaulting to package version.", swiftRef);
                return (SwiftRefType.Version, version);
            }
            switch (parts[0])
            {
                case "branch":
                    return (SwiftRefType.Branch, parts[1]);
                case "revision":
                    return (SwiftRefType.Revision, parts[1]);
                case "version":
                    return (SwiftRefType.Version, parts[1]);
                default:
                    EmbraceLogger.LogFormat(UnityEngine.LogType.Warning, "Invalid type {0} for swiftRef {1}. Defaulting to package version.", parts[0], swiftRef);
                    return (SwiftRefType.Version, version);
            }
        }
    }

    public enum SwiftRefType
    {
        Version,
        Branch,
        Revision
    }
}
