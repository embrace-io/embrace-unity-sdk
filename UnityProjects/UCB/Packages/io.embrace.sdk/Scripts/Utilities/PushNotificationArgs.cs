namespace EmbraceSDK
{
    /// <summary>
    /// iOSPushNotificationArgs is a struct that represents the arguments passed to the RecordPushNotification method on iOS.
    /// </summary>
    public struct iOSPushNotificationArgs
    {
        public readonly string title;
        public readonly string body;
        public readonly string subtitle;
        public readonly string category;
        public readonly int badge;

        public iOSPushNotificationArgs(string title, string body, string subtitle, string category, int badge)
        {
            this.title = title;
            this.body = body;
            this.subtitle = subtitle;
            this.category = category;
            this.badge = badge;
        }
    }

    /// <summary>
    /// AndroidPushNotificationArgs is a struct that represents the arguments passed to the RecordPushNotification method on Android.
    /// </summary>
    public struct AndroidPushNotificationArgs
    {
        public readonly string title;
        public readonly string body;
        public readonly string topic;
        public readonly string id;
        public readonly int? notificationPriority;
        public readonly int messageDeliveredPriority;
        public readonly bool isNotification;
        public readonly bool hasData;
        
        public AndroidPushNotificationArgs(string title, string body, string topic, string id, int? notificationPriority, int messageDeliveredPriority, bool isNotification, bool hasData)
        {
            this.title = title;
            this.body = body;
            this.topic = topic;
            this.id = id;
            this.notificationPriority = notificationPriority;
            this.messageDeliveredPriority = messageDeliveredPriority;
            this.isNotification = isNotification;
            this.hasData = hasData;
        }
    }
}