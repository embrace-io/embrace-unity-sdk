using System.Collections;
using System.Collections.Generic;
using EmbraceSDK.Demo;
using NSubstitute;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlayIntegrationTests : PlayTestBase
    {
        /// <summary>
        /// Tests the StartView() and EndView() invocations in the integration demo.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator SessionsTest()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_INTEGRATE, waitSeconds: .25f);

            yield return PressButton(DemoConstants.BUTTON_NAME_SESSIONS);

            Embrace embrace = Embrace.Instance;
            embrace.provider.Received().StartView(DemoConstants.TEST_VIEW);
            embrace.provider.Received().EndView(DemoConstants.TEST_VIEW);

            Cleanup();
        }

        /// <summary>
        /// Tests the LogMessage invocation in the integration demo.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator LogsTest()
        {
            ProviderSetup();

            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_INTEGRATE, waitSeconds: .25f);

            yield return PressButton(DemoConstants.BUTTON_NAME_LOGS);

            Embrace.Instance.provider.Received().LogMessage(
                DemoConstants.TEST_MESSAGE,
                EMBSeverity.Error,
                Arg.Any<Dictionary<string, string>>()
            );

            Cleanup();
        }

        /// <summary>
        /// Tests the ManualPushNotification invocation in the integration demo.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ManualPushNotificationsTest()
        {
            ProviderSetup();
            
            yield return LoadScene(DemoConstants.SCENE_NAME_DEMO_HOME, waitSeconds: .25f);

            yield return PressButton(DemoConstants.SCENE_NAME_INTEGRATE, waitSeconds: .25f);
            
            yield return PressButton(DemoConstants.BUTTON_NAME_PUSH_NOTIFICATIONS, waitSeconds: .25f);

            var embrace = Embrace.Instance;
            
#if UNITY_ANDROID
            var androidArgs = new AndroidPushNotificationArgs(DemoConstants.TEST_PN_TITLE, DemoConstants.TEST_PN_BODY, 
                DemoConstants.TEST_PN_TOPIC, DemoConstants.TEST_ID, 
                DemoConstants.TEST_PN_NOTIFICATION_PRIORITY, DemoConstants.TEST_PN_MESSAGE_DELIVERED_PRIORITY,
                DemoConstants.TEST_PN_IS_NOTIFICATION, DemoConstants.TEST_PN_HAS_DATA);
            embrace.provider.Received().RecordPushNotification(androidArgs);
#elif UNITY_IOS
            var iosArgs = new iOSPushNotificationArgs(DemoConstants.TEST_PN_TITLE, DemoConstants.TEST_PN_BODY, 
                DemoConstants.TEST_PN_SUBTITLE, DemoConstants.TEST_PN_CATEGORY, DemoConstants.TEST_PN_BADGE);
            embrace.provider.Received().RecordPushNotification(iosArgs);
#else
#endif
            Cleanup();
        }
    }
}