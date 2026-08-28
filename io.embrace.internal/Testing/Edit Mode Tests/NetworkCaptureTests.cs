using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EmbraceSDK.Internal;
using EmbraceSDK.Networking;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    [EmbraceWeaverExclude]
#if UNITY_EDITOR_OSX
    // CPU lightmapping is not supported on macOS arm64, and recompiling
    // scripts seems to trigger this to happen, causing an error which causes
    // test failures on CI (which has no GPU).
    [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
    public class NetworkCaptureTests
    {
        private const string GET_URL = "https://embrace-io.github.io/embrace-unity-sdk/index.html";
        private const string PROTOCOL_ERROR_URL = "https://data.emb-api.com/error";
        private const string INVALID_URL = "https://not-a-valid-url/";

        [UnitySetUp]
        [UsedImplicitly]
        public IEnumerator SetUp()
        {
            Embrace.Create();
            Embrace.Instance.provider = Substitute.For<IEmbraceProvider>();
            yield return null;
        }

        // NetworkCapture does not log requests on iOS because the native SDK captures them
#if !UNITY_IOS
        [Test]
        public void NetworkCapture_SendWebRequest_ThrowIfRequestIsNull()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                NetworkCapture.SendWebRequest(null);
            });
        }

        [UnityTest]
        public IEnumerator NetworkCapture_SendWebRequest_And_DisposeWebRequest_LogRequestIfSdkStarted()
        {
            UnityWebRequest preStartRequest = UnityWebRequest.Get(GET_URL);
            yield return NetworkCapture.SendWebRequest(preStartRequest);
            NetworkCapture.DisposeWebRequest(preStartRequest);

            Embrace.Instance.provider.DidNotReceiveWithAnyArgs()
                .RecordCompletedNetworkRequest(default, default, default, default, default, default, default, default);
            Embrace.Instance.provider.DidNotReceiveWithAnyArgs()
                .RecordIncompleteNetworkRequest(default, default, default, default, default, default);

            Embrace.Instance.StartSDK();
            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long bytesin, bytesout;
            int statusCode;
            string error;

            UnityWebRequest postStartRequest = UnityWebRequest.Get(GET_URL);
            yield return NetworkCapture.SendWebRequest(postStartRequest);
            bytesin = (long)postStartRequest.downloadedBytes;
            bytesout = (long)postStartRequest.uploadedBytes;
            statusCode = (int)postStartRequest.responseCode;
            error = GetExpectedErrorMessage(postStartRequest);
            NetworkCapture.DisposeWebRequest(postStartRequest);

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            AssertAgainstNetworkRequestProvider(GET_URL, HTTPMethod.GET, startTime, endTime, bytesin, bytesout, statusCode, error);
        }

        [UnityTest]
        public IEnumerator NetworkCapture_LogsOnlyFromOnCompleteEvent_IfFiredBeforeRequestDisposed()
        {
            Embrace.Instance.StartSDK();
            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long bytesin, bytesout;
            int statusCode;
            string error;

            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return NetworkCapture.SendWebRequest(request);

                bytesin = (long)request.downloadedBytes;
                bytesout = (long)request.uploadedBytes;
                statusCode = (int)request.responseCode;
                error = GetExpectedErrorMessage(request);

                yield return new WaitForSecondsRealtime(1f);
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            AssertAgainstNetworkRequestProvider(GET_URL, HTTPMethod.GET, startTime, endTime, bytesin, bytesout, statusCode, error);
        }

        [UnityTest]
        public IEnumerator NetworkCapture_OmitsErrorMessage_OnProtocolErrors()
        {
            Embrace.Instance.StartSDK();

            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long bytesin, bytesout;
            int statusCode;
            string error = string.Empty;

            using (UnityWebRequest request = UnityWebRequest.Get(PROTOCOL_ERROR_URL))
            {
                yield return NetworkCapture.SendWebRequest(request);

                bytesin = (long)request.downloadedBytes;
                bytesout = (long)request.uploadedBytes;
                statusCode = (int)request.responseCode;

                yield return new WaitForSecondsRealtime(1f);
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            AssertAgainstNetworkRequestProvider(PROTOCOL_ERROR_URL, HTTPMethod.GET, startTime, endTime, bytesin, bytesout, statusCode, error);
        }

        [UnityTest]
        public IEnumerator NetworkCapture_IncludesErrorMessage_OnConnectionErrors()
        {
            Embrace.Instance.StartSDK();

            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long bytesin, bytesout;
            int statusCode;
            string error;

            using (UnityWebRequest request = UnityWebRequest.Get(INVALID_URL))
            {
                yield return NetworkCapture.SendWebRequest(request);

                bytesin = (long)request.downloadedBytes;
                bytesout = (long)request.uploadedBytes;
                statusCode = (int)request.responseCode;
                error = request.error;

                yield return new WaitForSecondsRealtime(1f);
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            AssertAgainstNetworkRequestProvider(INVALID_URL, HTTPMethod.GET, startTime, endTime, bytesin, bytesout, statusCode, error);
        }

        [UnityTest]
        public IEnumerator NetworkCapture_AttachesTraceparentHeaderAndForwardsSameValue_WhenNetworkSpanForwardingIsEnabled()
        {
            Embrace.Instance.provider.IsNetworkSpanForwardingEnabled().Returns(true);
            Embrace.Instance.StartSDK();

            string attachedTraceparent;
            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return NetworkCapture.SendWebRequest(request);
                attachedTraceparent = request.GetRequestHeader("traceparent");
            }

            Assert.IsTrue(NetworkCapture.IsValidTraceparent(attachedTraceparent));

            Embrace.Instance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>(),
                    attachedTraceparent);
        }

        private void AssertAgainstNetworkRequestProvider(string url, HTTPMethod method, long startTime, long endTime, long bytesin, long bytesout, int statusCode, string error)
        {
            if (error != string.Empty)
            {
                Embrace.Instance.provider.Received()
                    .RecordIncompleteNetworkRequest(url,
                        method,
                        Arg.Is<long>(t => t >= startTime && t <= endTime),
                        Arg.Is<long>(t => t >= startTime && t <= endTime),
                        error,
                        null);
            }
            else
            {
                Embrace.Instance.provider.Received()
                    .RecordCompletedNetworkRequest(url,
                        method,
                        Arg.Is<long>(t => t >= startTime && t <= endTime),
                        Arg.Is<long>(t => t >= startTime && t <= endTime),
                        Arg.Is(bytesin),
                        Arg.Is(bytesout),
                        statusCode,
                        null);
            }
        }
#endif

        #region Traceparent

        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", true, TestName = "Valid traceparent with sampled flag")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00", true, TestName = "Valid traceparent with unsampled flag")]
        [TestCase(null, false, TestName = "Null is invalid")]
        [TestCase("", false, TestName = "Empty string is invalid")]
        [TestCase("00-00000000000000000000000000000000-00f067aa0ba902b7-01", false, TestName = "All-zero trace id is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-0000000000000000-01", false, TestName = "All-zero parent id is invalid")]
        [TestCase("ff-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", false, TestName = "Reserved version ff is invalid")]
        [TestCase("00-4BF92F3577B34DA6A3CE929D0E0E4736-00f067aa0ba902b7-01", false, TestName = "Uppercase trace id is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7", false, TestName = "Missing trace-flags segment is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e473-00f067aa0ba902b7-01", false, TestName = "Trace id too short is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e47366-00f067aa0ba902b7-01", false, TestName = "Trace id too long is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b-01", false, TestName = "Parent id too short is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b70-01", false, TestName = "Parent id too long is invalid")]
        [TestCase("00_4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01", false, TestName = "Wrong delimiter is invalid")]
        [TestCase("00-4bf92f3577b34da6a3ce929d0e0e473g-00f067aa0ba902b7-01", false, TestName = "Non-hex character in trace id is invalid")]
        
        public void IsValidTraceparent_ValidatesFormat(string traceparent, bool expectedValid)
        {
            Assert.AreEqual(expectedValid, NetworkCapture.IsValidTraceparent(traceparent));
        }

        [Test]
        public void GenerateTraceparent_AlwaysProducesAValueThatPassesValidation()
        {
            for (int i = 0; i < 1000; i++)
            {
                string traceparent = NetworkCapture.GenerateTraceparent();
                Assert.IsTrue(NetworkCapture.IsValidTraceparent(traceparent), $"Generated traceparent '{traceparent}' failed validation.");
            }
        }

        [Test]
        public void GenerateTraceparent_ProducesDifferentValuesOnEachCall()
        {
            string first = NetworkCapture.GenerateTraceparent();
            string second = NetworkCapture.GenerateTraceparent();

            Assert.AreNotEqual(first, second);
        }

        #endregion Traceparent

        [UnityTest]
        public IEnumerator EmbraceLoggingHttpMessageHandler_LogsOnException()
        {
            HttpClient client = NetworkCapture.GetHttpClientWithLoggingHandler();

            Embrace.Instance.StartSDK();

            string badUrl = "http://fakurl.html/";
            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var task = client.GetAsync(badUrl);

            while (!task.IsCompleted)
            {
                yield return null;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Embrace.Instance.provider.Received()
                .RecordIncompleteNetworkRequest(
                    Arg.Is<string>(badUrl),
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Is<string>(GetExpectedErrorMessage(task)),
                    Arg.Any<string>());
        }

        [UnityTest]
        public IEnumerator EmbraceLoggingHttpMessageHandler_OmitsErrorMessage_OnProtocolErrors()
        {
            HttpClient client = NetworkCapture.GetHttpClientWithLoggingHandler();

            Embrace.Instance.StartSDK();

            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var task = client.GetAsync(PROTOCOL_ERROR_URL);

            while (!task.IsCompleted)
            {
                yield return null;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Embrace.Instance.provider.Received()
                .RecordCompletedNetworkRequest(
                    Arg.Is<string>(PROTOCOL_ERROR_URL),
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Is<int>(404),
                    Arg.Any<string>());
        }

        [UnityTest]
        public IEnumerator EmbraceLoggingHttpMessageHandler_AttachesTraceparentHeaderAndForwardsSameValue_WhenNetworkSpanForwardingIsEnabled()
        {
            Embrace.Instance.provider.IsNetworkSpanForwardingEnabled().Returns(true);
            Embrace.Instance.StartSDK();

            HttpClient client = NetworkCapture.GetHttpClientWithLoggingHandler();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GET_URL);

            var task = client.SendAsync(request);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(request.Headers.TryGetValues("traceparent", out var values));
            string attachedTraceparent = values.FirstOrDefault();

            Assert.IsTrue(NetworkCapture.IsValidTraceparent(attachedTraceparent));

            Embrace.Instance.provider.Received()
                .RecordCompletedNetworkRequest(
                    Arg.Is<string>(GET_URL),
                    HTTPMethod.GET,
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>(),
                    Arg.Is(attachedTraceparent));
        }

        [UnityTest]
        public IEnumerator EmbraceLoggingHttpMessageHandler_DoesNotLogIfEmbraceHasNotStarted()
        {
            HttpClient client = NetworkCapture.GetHttpClientWithLoggingHandler();

            var task = client.GetAsync(GET_URL);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            Embrace.Instance.provider.DidNotReceiveWithAnyArgs().RecordCompletedNetworkRequest(default, default, default, default, default, default, default, default);
            Embrace.Instance.provider.DidNotReceiveWithAnyArgs().RecordIncompleteNetworkRequest(default, default, default, default, default, default);
        }

        [UnityTest]
        public IEnumerator NetworkCapture_GetHttpClientWithLoggingHandler_DoesNotDuplicateWhenPassedALoggingHandler()
        {
            HttpClient client = NetworkCapture.GetHttpClientWithLoggingHandler(new EmbraceLoggingHttpMessageHandler());

            Embrace.Instance.StartSDK();

            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var task = client.GetAsync(GET_URL);

            while (!task.IsCompleted)
            {
                yield return null;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Embrace.Instance.provider.Received(1)
                .RecordCompletedNetworkRequest(
                    Arg.Is<string>(GET_URL),
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>(),
                    Arg.Any<string>());
        }

        [UnityTest]
        public IEnumerator NetworkCapture_GetHttpClientWithLoggingHandler_DoesNotDuplicateWhenPassedALoggingHandlerAndBoolParameter()
        {
            HttpClient client = NetworkCapture.GetHttpClientWithLoggingHandler(new EmbraceLoggingHttpMessageHandler(), true);

            Embrace.Instance.StartSDK();

            long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var task = client.GetAsync(GET_URL);

            while (!task.IsCompleted)
            {
                yield return null;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            Embrace.Instance.provider.Received(1)
                .RecordCompletedNetworkRequest(
                    Arg.Is<string>(GET_URL),
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Is<long>(t => t >= startTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>(),
                    Arg.Any<string>());
        }

        private string GetExpectedErrorMessage(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    return request.error ?? string.Empty;

                default: return string.Empty;
            }
#else
            return request.isNetworkError ? (request.error ?? string.Empty) : string.Empty;
#endif
        }

        private string GetExpectedErrorMessage(Task<HttpResponseMessage> task)
        {
            Exception e = task.Exception?.InnerException;
            if (task.IsFaulted && e != null)
            {
                return $"{e.GetType().Name}: {e.Message}";
            }

            return string.Empty;
        }
    }
}
