using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EmbraceSDK.EditorView;
using EmbraceSDK.Internal;
using JetBrains.Annotations;
using NSubstitute;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EmbraceSDK.Tests
{
#if UNITY_EDITOR_OSX
    // CPU lightmapping is not supported on macOS arm64, and recompiling
    // scripts seems to trigger this to happen, causing an error which causes
    // test failures on CI (which has no GPU).
    [ConditionalIgnore(EmbraceTesting.REQUIRE_GRAPHICS_DEVICE, EmbraceTesting.REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION)]
#endif
    public class AutomaticNetworkCaptureTests
    {
        private bool _waitingForRequest;
        private bool _hasRecompiled;
        private Embrace _embraceInstance;

        private const string GET_URL = "https://embrace-io.github.io/embrace-unity-sdk-internal/index.html";
        private const string SVG_URL = "https://embrace.io/docs/images/embrace_logo_black-text_transparent-bg_400x200.svg";
        private const string PNG_URL = "https://embrace-io.github.io/embrace-unity-sdk-internal/images/logo.png";

        private static string[] _urlSource = new string[] { GET_URL };

        [OneTimeSetUp]
        public void Init()
        {
            _hasRecompiled = false;
        }

        [UnitySetUp]
        [UsedImplicitly]
        public IEnumerator RecompileBeforeTests()
        {
            if (!_hasRecompiled)
            {
                EmbraceLogger.Log(EmbraceTestMessages.RECOMPILE_BEFORE_TESTS);

                AssetDatabaseUtil.ForceRecompileScripts();
                yield return new RecompileScripts();
                _hasRecompiled = true;
            }

            _embraceInstance = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };
            
            _embraceInstance.StartSDK(null, false);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _embraceInstance.provider.ClearReceivedCalls();
            _embraceInstance.StopSDK();
            yield return null;
        }

        #region UnityWebRequest
        // Data processing errors were treated as exceptions in 2019 and earlier
#if UNITY_2020_1_OR_NEWER
        [UnityTest]
        public IEnumerator UnityWebRequestAssetBundle_LogsDataProcessingErrors_SeparatelyFromSuccessfulRequest()
        {
            string expectedErrorMessage = null;
            long bytesIn = 0, bytesOut = 0;
            Dictionary<string, string> properties = new Dictionary<string, string>();
            UnityWebRequest.Result result = UnityWebRequest.Result.InProgress;
            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(SVG_URL))
            {
                yield return request.SendWebRequest();

                expectedErrorMessage = request.downloadHandler.error;
                bytesIn = (long)request.downloadedBytes;
                bytesOut = (long)request.uploadedBytes;
                properties.Add("Download Handler", request.downloadHandler.GetType().Name);
                properties.Add("URL", request.url);
                properties.Add("Response Code", request.responseCode.ToString());
                properties.Add("Bytes In", request.downloadedBytes.ToString());
                properties.Add("Bytes Out", request.uploadedBytes.ToString());
                result = request.result;
            }

            // On iOS the native SDK captures UnityWebRequests, so we wouldn't expect to capture it here
#if !UNITY_IOS
            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(SVG_URL,
                    HTTPMethod.GET,
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Is(bytesIn),
                    Arg.Is(bytesOut),
                    200);
#endif

#if EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS
            if (result == UnityWebRequest.Result.DataProcessingError)
            {
                _embraceInstance.provider.Received(1)
                    .LogMessage(expectedErrorMessage,
                        EMBSeverity.Error,
                        Arg.Is<Dictionary<string, string>>(val => DictionariesAreEqual(properties, val)));
            }
            else
#endif
            {
                _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                    .LogMessage(expectedErrorMessage,
                        EMBSeverity.Error,
                        Arg.Is<Dictionary<string, string>>(val => DictionariesAreEqual(properties, val)));
            }
        }

        [UnityTest]
        public IEnumerator UnityWebRequestTexture_LogsDataProcessingErrors_SeparatelyFromSuccessfulRequest()
        {
            string expectedErrorMessage = null;
            long bytesIn = 0, bytesOut = 0;
            Dictionary<string, string> properties = new Dictionary<string, string>();
            UnityWebRequest.Result result = UnityWebRequest.Result.InProgress;
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(SVG_URL))
            {
                yield return request.SendWebRequest();

                expectedErrorMessage = request.downloadHandler.error;
                bytesIn = (long)request.downloadedBytes;
                bytesOut = (long)request.uploadedBytes;
                properties.Add("Download Handler", request.downloadHandler.GetType().Name);
                properties.Add("URL", request.url);
                properties.Add("Response Code", request.responseCode.ToString());
                properties.Add("Bytes In", request.downloadedBytes.ToString());
                properties.Add("Bytes Out", request.uploadedBytes.ToString());
                result = request.result;
            }

            // On iOS the native SDK captures UnityWebRequests, so we wouldn't expect to capture it here
#if !UNITY_IOS
            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(SVG_URL,
                    HTTPMethod.GET,
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Is(bytesIn),
                    Arg.Is(bytesOut),
                    200);
#endif

#if EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS
            if (result == UnityWebRequest.Result.DataProcessingError)
            {
                _embraceInstance.provider.Received(1)
                    .LogMessage(expectedErrorMessage,
                        EMBSeverity.Error,
                        Arg.Is<Dictionary<string, string>>(val => DictionariesAreEqual(properties, val)));
            }
            else
#endif
            {
                _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                    .LogMessage(expectedErrorMessage,
                        EMBSeverity.Error,
                        Arg.Is<Dictionary<string, string>>(val => DictionariesAreEqual(properties, val)));
            }
        }

        [UnityTest]
        public IEnumerator UnityWebRequestTexture_DoesNotLogError_WhenTextureDecodingSucceeds()
        {
            long bytesIn = 0, bytesOut = 0;
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(PNG_URL))
            {
                yield return request.SendWebRequest();

                Assert.AreEqual(UnityWebRequest.Result.Success, request.result);

                bytesIn = (long)request.downloadedBytes;
                bytesOut = (long)request.uploadedBytes;
            }

            // On iOS the native SDK captures UnityWebRequests, so we wouldn't expect to capture it here
#if !UNITY_IOS
            _embraceInstance.provider.Received(1)
                .RecordCompletedNetworkRequest(PNG_URL,
                    HTTPMethod.GET,
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Is(bytesIn),
                    Arg.Is(bytesOut),
                    200);
#endif

            _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                .LogMessage(default, default, default);
        }

        private bool DictionariesAreEqual<TKey, TValue>(Dictionary<TKey, TValue> expected,
            Dictionary<TKey, TValue> actual)
        {
            if (actual.Count != expected.Count)
            {
                return false;
            }

            foreach (KeyValuePair<TKey, TValue> entry in expected)
            {
                if (!actual.TryGetValue(entry.Key, out TValue value) || !value.Equals(entry.Value))
                {
                    return false;
                }
            }

            return true;
        }
#endif

        // UnityWebRequests are capture by the native SDK on iOS, so we do not capture them in the Unity SDK on that platform.
#if !UNITY_IOS && !UNITY_TVOS
        [EmbraceWeaverExclude]
        [UnityTest]
        public IEnumerator UnityWebRequest_IsNotCapturedWhenExcludedFromWeaving()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return request.SendWebRequest();
            }

            _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                .RecordCompletedNetworkRequest(default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default);

            _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                .RecordIncompleteNetworkRequest(default,
                    default,
                    default,
                    default,
                    default);

        }

        [UnityTest]
        public IEnumerator UnityWebRequest_IsCapturedWhenMethodOverloadsAnExcludedMethodButIsNotExcludedItself([ValueSource(nameof(_urlSource))] string url)
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_PassesAnEmptyString_AsErrorMessage_WhenStatusIsSuccess()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            bool isSuccess = false;
            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return request.SendWebRequest();
                isSuccess = (request.responseCode / 100) == 2;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (isSuccess)
            {
                _embraceInstance.provider.Received()
                    .RecordCompletedNetworkRequest(GET_URL,
                        HTTPMethod.GET,
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Any<long>(),
                        Arg.Any<long>(),
                        Arg.Any<int>());
            }
            else
            {
                _embraceInstance.provider.Received()
                    .RecordIncompleteNetworkRequest(GET_URL,
                        HTTPMethod.GET,
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Is<string>(s => !string.IsNullOrEmpty(s)));
            }
        }

        // Not a test, but an excluded method to enable the above test
        [EmbraceWeaverExclude]
        public IEnumerator UnityWebRequest_IsCapturedWhenMethodOverloadsAnExcludedMethodButIsNotExcludedItself()
        {
            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineUsingStatement()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return request.SendWebRequest();
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineUsingStatementAsIDisposableField()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            yield return request.SendWebRequest();

            IDisposable disposable = request;
            disposable.Dispose();
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineUsingStatementAsIDisposableParameter()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            yield return request.SendWebRequest();

            DisposeParameter(request);
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DisposeParameter(IDisposable disposable)
        {
            disposable.Dispose();
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineUsingStatementAsIDisposableLocalVariable()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            yield return request.SendWebRequest();

            DisposeAfterLoadingFromLocalVariable(request);
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DisposeAfterLoadingFromLocalVariable(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest localVariable1 = unityWebRequest;
            IDisposable localVariable2 = localVariable1;
            localVariable2.Dispose();
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_TextureInCoroutineUsingStatement()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(GET_URL))
            {
                yield return request.SendWebRequest();
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineUsingStatementWithAsyncOpVariable()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                yield return operation;
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineManuallyDisposed()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            yield return request.SendWebRequest();
            request.Dispose();
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_TextureInCoroutineManuallyDisposed()
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            yield return request.SendWebRequest();
            request.Dispose();
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_InCoroutineWithAsyncOpVariableManuallyDisposed()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;
            request.Dispose();
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_WithLambdaOnComplete()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            request.SendWebRequest().completed += operation =>
            {
                request.Dispose();
                _waitingForRequest = false;
            };

            _waitingForRequest = true;
            while (_waitingForRequest)
            {
                yield return null;
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            yield return new WaitForSecondsRealtime(0.2f);

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_WithAsyncOperationVariableWithLambdaOnComplete()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ =>
            {
                request.Dispose();
                _waitingForRequest = false;
            };

            _waitingForRequest = true;
            while (_waitingForRequest)
            {
                yield return null;
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_WithOnCompleteDelegate()
        {
            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            request.SendWebRequest().completed += FakeOnComplete;

            _waitingForRequest = true;
            while (_waitingForRequest)
            {
                yield return null;
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_WithAsyncOpVariableWithOnCompleteDelegate()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            UnityWebRequest request = UnityWebRequest.Get(GET_URL);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += FakeOnComplete;

            _waitingForRequest = true;
            while (_waitingForRequest)
            {
                yield return null;
            }

            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_LoggedBeforeDisposeInLongRunningCoroutine()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long endTime = 0;
            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return request.SendWebRequest();
                yield return null;
                endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                yield return new WaitForSecondsRealtime(2f);
            }

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [UnityTest]
        public IEnumerator UnityWebRequest_NotLoggedIfSDKHasNotStarted()
        {
            Embrace embrace = new Embrace
            {
                provider = Substitute.For<IEmbraceProvider>()
            };

            using (UnityWebRequest request = UnityWebRequest.Get(GET_URL))
            {
                yield return request.SendWebRequest();
                yield return null;
            }

            embrace.provider.DidNotReceiveWithAnyArgs().RecordCompletedNetworkRequest(default, default, default, default, default, default, default);
            embrace.provider.DidNotReceiveWithAnyArgs().RecordIncompleteNetworkRequest(default, default, default, default, default);
        }

        private void FakeOnComplete(AsyncOperation obj)
        {
            (obj as UnityWebRequestAsyncOperation)?.webRequest.Dispose();
            _waitingForRequest = false;
        }
#endif
        #endregion UnityWebRequest

        #region HttpClient

        [Test]
        public async Task HttpClient_ConstructedWithNoParams_GetAsyncIsLogged()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int statusCode;
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(GET_URL);

                statusCode = (int)response.StatusCode;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    statusCode);
        }

        [Test]
        public async Task HttpClient_PassesEmptyString_AsErrorMessage_WhenStatusIsSuccess()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int statusCode;
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(GET_URL);
                statusCode = (int)response.StatusCode;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            bool isSuccess = (statusCode / 100) == 2;

            if (isSuccess)
            {
                _embraceInstance.provider.Received()
                    .RecordCompletedNetworkRequest(GET_URL,
                        HTTPMethod.GET,
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Any<long>(),
                        Arg.Any<long>(),
                        statusCode);
            }
            else
            {
                _embraceInstance.provider.Received()
                    .RecordIncompleteNetworkRequest(GET_URL,
                        HTTPMethod.GET,
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Is<long>(t => t >= sendTime && t <= endTime),
                        Arg.Is<string>(s => !string.IsNullOrEmpty(s)));
            }
        }

        [Test]
        public async Task HttpClient_ConstructedWithHandlerParam_GetAsyncIsLogged()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int statusCode;
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var response = await client.GetAsync(GET_URL);
                statusCode = (int)response.StatusCode;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    statusCode);
        }

        [Test]
        public async Task HttpClient_ConstructedWithHandlerAndBoolParams_GetAsyncIsLogged()
        {
            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int statusCode;
            using (HttpClient client = new HttpClient(new HttpClientHandler(), true))
            {
                var response = await client.GetAsync(GET_URL);
                statusCode = (int)response.StatusCode;
            }
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received()
                .RecordCompletedNetworkRequest(GET_URL,
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    statusCode);
        }

        [Test]
        public async Task HttpClient_IsNotCapturedWhenExcludedFromWeaving()
        {
            await SendHttpClientAsync();
            _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                .RecordCompletedNetworkRequest(default, default, default, default, default, default, default);
            _embraceInstance.provider.DidNotReceiveWithAnyArgs()
                .RecordIncompleteNetworkRequest(default, default, default, default, default);
        }

        [Test]
        public async Task HttpClient_IsCapturedWhenMethodOverloadsAnExcludedMethodButNotExcludedItself([ValueSource(nameof(_urlSource))] string url)
        {
            await SendHttpClientAsync();
            Embrace.Instance.provider.DidNotReceiveWithAnyArgs().RecordCompletedNetworkRequest(default, default, default, default, default, default, default);
            Embrace.Instance.provider.DidNotReceiveWithAnyArgs().RecordIncompleteNetworkRequest(default, default, default, default, default);

            long sendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            await SendHttpClientAsync(GET_URL);
            long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _embraceInstance.provider.Received(1)
                .RecordCompletedNetworkRequest(Arg.Is(GET_URL),
                    HTTPMethod.GET,
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Is<long>(t => t >= sendTime && t <= endTime),
                    Arg.Any<long>(),
                    Arg.Any<long>(),
                    Arg.Any<int>());
        }

        [EmbraceWeaverExclude]
        private async Task<HttpResponseMessage> SendHttpClientAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetAsync(GET_URL);
            }
        }

        private async Task<HttpResponseMessage> SendHttpClientAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetAsync(url);
            }
        }

        #endregion HttpClient
    }
}
