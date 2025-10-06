using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EmbraceSDK;
using EmbraceSDK.Demo;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace EmbraceSDK.Internal
{
    public class ValidationBehavior : MonoBehaviour
    {
#if UNITY_IOS || UNITY_TVOS
        [DllImport("__Internal")]
        private static extern void _embrace_basic_open_web_view(string url);
#endif

        private string _startSpanId = "";

        public string appID = "3Ynor";
        public Button crashButton;

        void Awake()
        {
            EmbraceStartupArgs args = new EmbraceStartupArgs(appID);
            Embrace.Instance.StartSDK(args);

            Embrace.Instance.SetUsername("test_username");
            Embrace.Instance.SetUserEmail("test_email@example.com");
            Embrace.Instance.SetUserIdentifier("test_user_id");
            Embrace.Instance.AddUserPersona("test_persona");
            Embrace.Instance.SetUserAsPayer();

            Embrace.Instance.LogInfo("test log info");
            Embrace.Instance.LogWarning("test log warning");
            Embrace.Instance.LogError("test log error");
            Embrace.Instance.LogMessage(
                "test log message",
                EMBSeverity.Info,
                new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } }
            );
            _startSpanId = Embrace.Instance.StartSpan(
                "start",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );

            crashButton.onClick.AddListener(DoCrash);
        }

        void Start()
        {
            if (string.IsNullOrEmpty(_startSpanId) == false)
            {
                Embrace.Instance.StopSpan(_startSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
            
            StartCoroutine(DoBreadcrumb());
            StartCoroutine(DoRequests());
            StartCoroutine(DoSpans());
            StartCoroutine(DoView());
            DoPushNotification();
            DoGetLastRunEndState();
            DoFileAttachment();
            DoLogException();
        }

        private IEnumerator DoBreadcrumb()
        {
            yield return new WaitForSeconds(Random.Range(1f, 5f));
            Embrace.Instance.AddBreadcrumb("test");
        }

        private void DoCrash()
        {
            Utils.ForceCrash(ForcedCrashCategory.FatalError);
        }

        private void DoLogException()
        {
            try
            {
                NestedFunction1();
            }
            catch (Exception e)
            {
                Embrace.Instance.LogUnhandledUnityException(e);
                Embrace.Instance.LogHandledUnityException(e);
            }
        }

        private void NestedFunction1()
        {
            NestedFunction2();
        }

        private void NestedFunction2()
        {
            throw new Exception("test exception");
        }

        private void DoPushNotification()
        {
#if UNITY_ANDROID
            var androidArgs = new AndroidPushNotificationArgs(
                DemoConstants.TEST_PN_TITLE,
                DemoConstants.TEST_PN_BODY,
                DemoConstants.TEST_PN_TOPIC,
                DemoConstants.TEST_ID,
                DemoConstants.TEST_PN_NOTIFICATION_PRIORITY,
                DemoConstants.TEST_PN_MESSAGE_DELIVERED_PRIORITY,
                DemoConstants.TEST_PN_IS_NOTIFICATION,
                DemoConstants.TEST_PN_HAS_DATA
            );
            Embrace.Instance.RecordPushNotification(androidArgs: androidArgs);
#elif UNITY_IOS || UNITY_TVOS
            var iosArgs = new iOSPushNotificationArgs(
                DemoConstants.TEST_PN_TITLE,
                DemoConstants.TEST_PN_BODY,
                DemoConstants.TEST_PN_SUBTITLE,
                DemoConstants.TEST_PN_CATEGORY,
                DemoConstants.TEST_PN_BADGE
            );
            Embrace.Instance.RecordPushNotification(iosArgs);
#endif
        }

        private IEnumerator DoRequests()
        {
            using (var webRequest = UnityWebRequest.Get("https://httpbin.org/image/jpeg"))
            {
                yield return webRequest.SendWebRequest();
            }

            var form = new WWWForm();
            form.AddField("myField", "myData");
            using (var webRequest = UnityWebRequest.Post("https://httpbin.org/post", form))
            {
                yield return webRequest.SendWebRequest();
            }

            var data = System.Text.Encoding.UTF8.GetBytes("This is some test data");
            using (var webRequest = UnityWebRequest.Put("https://httpbin.org/put", data))
            {
                yield return webRequest.SendWebRequest();
            }

            using (var webRequest = UnityWebRequest.Delete("https://httpbin.org/delete"))
            {
                yield return webRequest.SendWebRequest();
            }

            using (var webRequest = UnityWebRequest.Get("https://httpbin.org/status/403"))
            {
                yield return webRequest.SendWebRequest();
            }
        }

        private IEnumerator DoSpans()
        {
            var spanId = Embrace.Instance.StartSpan(
                "test",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );
            yield return new WaitForSeconds(1);
            Embrace.Instance.AddSpanEvent(
                spanId,
                "testEvent",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } }
            );

            var childSpanId = Embrace.Instance.StartSpan(
                "child",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                spanId
            );
            Embrace.Instance.AddSpanAttribute(childSpanId, "key1", "value1");
            Embrace.Instance.AddSpanAttribute(childSpanId, "key2", "value2");

            yield return new WaitForSeconds(1);
            var failureSpanId = Embrace.Instance.StartSpan(
                "failure",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                spanId
            );
            yield return new WaitForSeconds(0.1f);
            Embrace.Instance.StopSpan(
                failureSpanId,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                EmbraceSpanErrorCode.FAILURE
            );

            Embrace.Instance.StopSpan(childSpanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            var userAbandonSpanId = Embrace.Instance.StartSpan(
                "userAbandon",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                spanId
            );
            yield return new WaitForSeconds(0.1f);
            Embrace.Instance.StopSpan(
                userAbandonSpanId,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                EmbraceSpanErrorCode.USER_ABANDON
            );

            var unknownSpanId = Embrace.Instance.StartSpan(
                "unknown",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            );
            yield return new WaitForSeconds(0.1f);
            Embrace.Instance.StopSpan(
                unknownSpanId,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                EmbraceSpanErrorCode.UNKNOWN
            );

            var startTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            yield return new WaitForSeconds(0.1f);
            var endTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Embrace.Instance.RecordCompletedSpan(
                "completed",
                startTimeMs,
                endTimeMs,
                EmbraceSpanErrorCode.NONE,
                new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } },
                null,
                spanId
            );

            Embrace.Instance.StopSpan(spanId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        private IEnumerator DoView()
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            Embrace.Instance.StartView("test");
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            Embrace.Instance.EndView("test");
#if UNITY_IOS || UNITY_TVOS
            _embrace_basic_open_web_view("https://www.google.com");
#endif
        }

        private void DoGetLastRunEndState()
        {
            var state = Embrace.Instance.GetLastRunEndState();
            EmbraceLogger.Log($"Last run end state: {state}");
        }

        private void DoFileAttachment()
        {
            int size = 1024 * 1024;
#if UNITY_ANDROID
            var blob = new sbyte[size];
            for (int i = 0; i < size; i++)
            {
                blob[i] = (sbyte)Random.Range(0, int.MaxValue);
            }
#elif UNITY_IOS || UNITY_TVOS
            var blob = new byte[size];
            for (int i = 0; i < size; i++)
            {
                blob[i] = (byte)Random.Range(0, int.MaxValue);
            }
#endif

            Embrace.Instance.LogMessage("binary blob", EMBSeverity.Info, null, blob);
            Embrace.Instance.LogMessage("external attachment", EMBSeverity.Info, null,
                (new Guid()).ToString(),
                "https://archive.org/download/sample-video-1280x-720-1mb_202102/SampleVideo_1280x720_1mb.mp4");
        }
    }
}
