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
            var parts = swiftRef.Split(':');
            if (parts.Length != 2 || string.IsNullOrEmpty(parts[1]))
            {
                return (SwiftRefType.Version, version);
            }
            return parts[0] switch
            {
                "branch" => (SwiftRefType.Branch, parts[1]),
                "revision" => (SwiftRefType.Revision, parts[1]),
                "version" => (SwiftRefType.Version, parts[1]),
                _ => (SwiftRefType.Version, version)
            };
        }
    }

    public enum SwiftRefType
    {
        Version,
        Branch,
        Revision
    }
}
