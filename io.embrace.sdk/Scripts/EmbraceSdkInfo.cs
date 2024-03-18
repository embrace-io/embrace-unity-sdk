namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Provides information about the SDK which is used by SDK during updates and by various Embrace editor windows.
    /// </summary>
    public class EmbraceSdkInfo
    {
        public string version;
        public string npmAPIEndpoint;

        // Welcome Window
        public string wAnnouncementTitle;
        public string wAnnouncementMessage;
    }
}
