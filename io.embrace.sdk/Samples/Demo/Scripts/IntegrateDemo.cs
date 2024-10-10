using System;
using System.Collections;
using System.Collections.Generic;
// import the SDK
using EmbraceSDK;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// Demo to help with the integration process. For more info please see our documentation.
    /// https://embrace.io/docs/unity/integration/
    /// </summary>
    public class IntegrateDemo : DemoBase
    {
        [Header("Unity Web Request Buttons")]
        public Button startParentSpanButton;
        public Button stopParentSpanButton;
        public Button startChildFailureSpanButton;
        public Button startChildUserAbandonSpanButton;
        public Button startChildUnknownSpanButton;
        public Button recordCompletedSpanButton;
        
        private string _parentSpanId;
        
        public void Start()
        {
            // Call the start method to enable the SDK. Calling it as early as possible in the launch process ensures capture of the most data possible.
            // Start the SDK
            Embrace.Instance.StartSDK();
            startParentSpanButton.onClick.AddListener(StartParentSpan);
            stopParentSpanButton.onClick.AddListener(StopParentSpan);
            startChildFailureSpanButton.onClick.AddListener(RecordFailureChildSpan);
            startChildUserAbandonSpanButton.onClick.AddListener(RecordAbandonChildSpan);
            startChildUnknownSpanButton.onClick.AddListener(RecordUnknownChildSpan);
            recordCompletedSpanButton.onClick.AddListener(RecordCompletedSpan);
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
        
        private void StartParentSpan()
        {
            var startTime = GetCurrentMillisecondsPosix();
            _parentSpanId = Embrace.Instance.StartSpan(DemoConstants.PARENT_SPAN, startTime);
        }
        
        private void StopParentSpan()
        {
            var endTime = GetCurrentMillisecondsPosix();
            Embrace.Instance.StopSpan(_parentSpanId, endTime);
        }
        
        private void RecordFailureChildSpan()
        {
            var startTime = GetCurrentMillisecondsPosix();
            var id = Embrace.Instance.StartSpan(DemoConstants.CHILD_FAILURE_SPAN, startTime, _parentSpanId);
            Embrace.Instance.AddSpanAttribute(
                id,
                $"{DemoConstants.CHILD_FAILURE_SPAN}-att-key1", 
                $"{DemoConstants.CHILD_FAILURE_SPAN}-att-value1");
            Embrace.Instance.AddSpanEvent(
                id, 
                DemoConstants.CHILD_FAILURE_SPAN, 
                startTime, 
                GetAttributesBasedOnSpan(DemoConstants.CHILD_FAILURE_SPAN));

            StartCoroutine(GetRequest(id, EmbraceSpanErrorCode.FAILURE));
        }
        
        private void RecordAbandonChildSpan()
        {
            var startTime = GetCurrentMillisecondsPosix();
            var id = Embrace.Instance.StartSpan(DemoConstants.CHILD_USER_ABANDON_SPAN, startTime, _parentSpanId);
            Embrace.Instance.AddSpanAttribute(
                id,
                $"{DemoConstants.CHILD_USER_ABANDON_SPAN}-att-key1", 
                $"{DemoConstants.CHILD_USER_ABANDON_SPAN}-att-value1");
            Embrace.Instance.AddSpanEvent(
                id, 
                DemoConstants.CHILD_USER_ABANDON_SPAN, 
                startTime, 
                GetAttributesBasedOnSpan(DemoConstants.CHILD_USER_ABANDON_SPAN));

            StartCoroutine(GetRequest(id, EmbraceSpanErrorCode.USER_ABANDON));
        }
        
        private void RecordUnknownChildSpan()
        {
            var startTime = GetCurrentMillisecondsPosix();
            var id = Embrace.Instance.StartSpan(DemoConstants.CHILD_UNKNOWN_SPAN, startTime, _parentSpanId);
            Embrace.Instance.AddSpanAttribute(
                id,
                $"{DemoConstants.CHILD_UNKNOWN_SPAN}-att-key1", 
                $"{DemoConstants.CHILD_UNKNOWN_SPAN}-att-value1");
            Embrace.Instance.AddSpanEvent(
                id, 
                DemoConstants.CHILD_UNKNOWN_SPAN, 
                startTime, 
                GetAttributesBasedOnSpan(DemoConstants.CHILD_FAILURE_SPAN));

            StartCoroutine(GetRequest(id, EmbraceSpanErrorCode.UNKNOWN));
        }
        
        private void RecordCompletedSpan()
        {
            var startTime = GetCurrentMillisecondsPosix();
            var attributes = GetAttributesBasedOnSpan(DemoConstants.COMPLETED_SPAN);

            var spanEvent = new EmbraceSpanEvent(
                $"{DemoConstants.COMPLETED_SPAN}-event",
                GetCurrentMillisecondsPosix(),
                GetCurrentMillisecondsPosix(),
                attributes
                );
            
            var endTime = GetCurrentMillisecondsPosix();
            
            var result = Embrace.Instance.RecordCompletedSpan(
                DemoConstants.COMPLETED_SPAN, 
                startTime, 
                endTime, 
                0, 
                attributes, 
                spanEvent);
        }

        private long GetCurrentMillisecondsPosix()
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var currentTime = DateTime.UtcNow;
            return (long) (currentTime - unixEpoch).TotalMilliseconds;
        }
        
        private Dictionary<string, string> GetAttributesBasedOnSpan(string spanName)
        {
            var attributes = new Dictionary<string, string>
            {
                { $"{spanName}-att-key1", $"{spanName}-att-value1" },
                { $"{spanName}-att-key2", $"{spanName}-att-value2" },
            };
            return attributes;
        }
        
        private IEnumerator GetRequest(string spanId, EmbraceSpanErrorCode spanErrorCode)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://httpbin.org/image/jpeg"))
            {
                yield return webRequest.SendWebRequest();
                Embrace.Instance.StopSpan(spanId, GetCurrentMillisecondsPosix(), spanErrorCode);
            }
        }
    }
}
