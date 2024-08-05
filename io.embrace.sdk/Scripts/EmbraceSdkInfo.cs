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

    /// <summary>
    /// This is meant to replace the EmbraceSDKInfo.json file method of storing the SDK info.
    /// By using compile time constants instead of a file, we can avoid the need to read from disk.
    /// </summary>
    public class CurrentEmbraceSDKInfo : EmbraceSdkInfo
    {
        public CurrentEmbraceSDKInfo()
        {
            version = "1.26.0";
            npmAPIEndpoint = "https://repo.embrace.io/repository/unity";
            wAnnouncementTitle = "iOS Version Update";
            wAnnouncementMessage = "The CRASH_REPORT_ENABLED setting has been replaced by CRASH_REPORT_PROVIDER. " +
                                   "Any existing configuration will be migrated automatically, " +
                                   "but please take a moment to double check the value in the Embrace settings window.";
        }
    }
}
