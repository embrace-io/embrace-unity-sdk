using System.Collections.Generic;
// import the SDK
using EmbraceSDK;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// Demo to help with the integration process. For more info please see our documentation.
    /// https://embrace.io/docs/unity/integration/
    /// </summary>
    public class IntegrateDemo : DemoBase
    {
        public void Start()
        {
            // Call the start method to enable the SDK. Calling it as early as possible in the launch process ensures capture of the most data possible.
            // Start the SDK
            Embrace.Instance.StartSDK();
        }

        public void Sessions()
        {
            // Instead of tracking screens, Embrace uses views. Please use the Custom View API to start and end views manually.
            Embrace.Instance.StartView(DemoConstants.TEST_VIEW);
            Embrace.Instance.EndView(DemoConstants.TEST_VIEW);
        }

        public void Logs()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            // Log messages are a great tool to gain proactive visibility into a user's session and to debug issues.
            // Add Error, Warning and Info log messages to provide additional context to your user timelines and to aggregate on counts of expected issues.
            Embrace.Instance.LogMessage(
              DemoConstants.TEST_MESSAGE, // log name
              EMBSeverity.Error, // log severity
              properties // optional properties dictionary
            );
        }

        public void Crashes()
        {
            // Unity will tend to hide crashes as best it can, this debug line forces a crash in spite of that hiding.
            UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
        }

        public void Moments()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            Embrace.Instance.StartMoment(
              DemoConstants.TEST_NAME, // moment name
              DemoConstants.TEST_ID, // optional id
              false, // allow screenshot boolean
              properties // optional properties dictionary
            );

            Embrace.Instance.EndMoment(
              DemoConstants.TEST_NAME, // moment name
              DemoConstants.TEST_ID, // optional id
              properties // optional properties dictionary
            );
        }

        public void PushNotifications()
        {
#if UNITY_ANDROID
            var androidArgs = new AndroidPushNotificationArgs(DemoConstants.TEST_PN_TITLE, DemoConstants.TEST_PN_BODY,
                DemoConstants.TEST_PN_TOPIC, DemoConstants.TEST_ID,
                DemoConstants.TEST_PN_NOTIFICATION_PRIORITY, DemoConstants.TEST_PN_MESSAGE_DELIVERED_PRIORITY,
                DemoConstants.TEST_PN_IS_NOTIFICATION, DemoConstants.TEST_PN_HAS_DATA);
            Embrace.Instance.RecordPushNotification(androidArgs: androidArgs);
#elif UNITY_IOS
            var iosArgs = new iOSPushNotificationArgs(DemoConstants.TEST_PN_TITLE, DemoConstants.TEST_PN_BODY, 
                DemoConstants.TEST_PN_SUBTITLE, DemoConstants.TEST_PN_CATEGORY, DemoConstants.TEST_PN_BADGE);
            Embrace.Instance.RecordPushNotification(iosArgs);
#else
#endif
        }
    }
}
