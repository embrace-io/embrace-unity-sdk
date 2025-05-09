using System;
using System.Collections.Generic;
using System.Net.Http;
using EmbraceSDK.Internal;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Networking;

namespace EmbraceSDK.Networking
{
    /// <summary>
    /// This type is used by the Embrace weaver to wrap network request calls for automatic logging. It is not
    /// intended to be used directly.
    /// </summary>
    public static class NetworkCapture
    {
        public const string EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS = nameof(EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS);

        private class PendingRequest<T>
        {
            public T requestOperation;
            public long startms;
        }

        #if EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS
        // Used for log properties on UnityWebRequests with data processing errors. We cache this instance to avoid
        // allocating a new Dictionary for each error.
        private static Dictionary<string, string> _dataProcessingErrorProperties = new Dictionary<string, string>(5);
        #endif

        private static readonly Dictionary<UnityWebRequest, PendingRequest<UnityWebRequestAsyncOperation>> _pendingUnityWebRequests =
            new Dictionary<UnityWebRequest, PendingRequest<UnityWebRequestAsyncOperation>>();

        /// <summary>
        /// Caches the current time as the start time for the request and subscribes to the async operations completed event
        /// for eventual logging of the request.
        /// </summary>
        /// <returns>The UnityWebRequestAsyncOperation returned by calling SendWebRequest on the request.</returns>
        /// <exception cref="NullReferenceException">Throws a NullReferenceException when the request parameter is null.</exception>
        [Preserve]
        public static UnityWebRequestAsyncOperation SendWebRequest(UnityWebRequest request)
        {
            if (request == null)
            {
                // This static function is used by the weaver to wrap the UnityWebRequest.SendWebRequest instance method.
                // Therefore, if the UnityWebRequest argument is null parameter is null, the unwrapped behavior would have
                // attempted to call SendWebRequest on a null reference. We replicate that NullReferenceException here.

                throw new NullReferenceException();
            }

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            _pendingUnityWebRequests[request] = new PendingRequest<UnityWebRequestAsyncOperation>()
            {
                requestOperation = operation,
                startms = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            operation.completed += OnUnityWebRequestAsyncOperationComplete;

            return operation;
        }

        /// <summary>
        /// Disposes the parameter and logs it if it is a pending UnityWebRequest that has not yet been logged.
        /// </summary>
        [Preserve]
        public static void DisposeWebRequest(IDisposable disposable)
        {
            // Because the weaver cannot always determine the type of the object being disposed, we can't safely assume
            // that disposable is a reference to a UnityWebRequest.
            if (disposable is UnityWebRequest request)
            {
                LogAndRemoveUnityWebRequest(request);
            }

            disposable.Dispose();
        }

        private static void OnUnityWebRequestAsyncOperationComplete(AsyncOperation asyncOperation)
        {
            asyncOperation.completed -= OnUnityWebRequestAsyncOperationComplete;

            if (!(asyncOperation is UnityWebRequestAsyncOperation unityWebRequestAsyncOperation))
            {
                return;
            }

            UnityWebRequest operationRequest = unityWebRequestAsyncOperation.webRequest;

            LogAndRemoveUnityWebRequest(operationRequest);
        }

        private static void LogAndRemoveUnityWebRequest(UnityWebRequest request)
        {
            // If the request isn't in our dictionary of pending requests, its probably already been logged.
            if (!_pendingUnityWebRequests.TryGetValue(request, out PendingRequest<UnityWebRequestAsyncOperation> pendingRequest))
            {
                return;
            }

            pendingRequest.requestOperation.completed -= OnUnityWebRequestAsyncOperationComplete;
            _pendingUnityWebRequests.Remove(request);

            if(!InternalEmbrace.GetExistingInstance()?.IsStarted ?? true)
            {
                EmbraceLogger.LogWarning("Attempted to log a network request before the Embrace SDK was started.");
                return;
            }

            try
            {
                // If the UnityWebRequest has been disposed at this point, accessing any of its properties will throw an
                // ArgumentNullException. We protect against this by wrapping all calls to UnityWebRequest.Dispose (and
                // IDisposable.Dispose when UnityWebRequest may be the concrete type of the IDisposable reference),
                // but we will still wrap this inside a try-catch just in case the dispose escaped our weaver.

                // The iOS SDK automatically captures UnityWebRequest, so we can skip this on that platform
                #if !UNITY_IOS
                long endms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long bytesin = (long)request.downloadedBytes;
                long bytesout = (long)request.uploadedBytes;
                int code = (int)request.responseCode;

                string error;
                #if UNITY_2020_1_OR_NEWER
                switch (request.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        error = request.error ?? string.Empty;
                        break;

                    case UnityWebRequest.Result.Success:
                    case UnityWebRequest.Result.ProtocolError:
                    default:
                        error = string.Empty;
                        break;
                }
                #else
                error = request.isNetworkError ? (request.error ?? string.Empty) : string.Empty;
                #endif


                if (!HTTPMethod.TryParse(request.method, out HTTPMethod method))
                {
                    method = HTTPMethod.OTHER;
                }
                
                if (error != string.Empty)
                {
                    Embrace.Instance?.RecordIncompleteNetworkRequest(request.url, method, pendingRequest.startms, endms, error);
                }
                else
                {
                    Embrace.Instance?.RecordCompleteNetworkRequest(request.url, method, pendingRequest.startms, endms, bytesin, bytesout, code);
                }
                #endif

                #if UNITY_2020_1_OR_NEWER && EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS
                // If the web request was using a download handler that expects the downloaded data to be in a certain
                // format (ie UnityWebRequestTexture, UnityWebRequestAssetBundle), its possible that the request was
                // successful but the "result" of the request is an error. In those cases, we log the error separately
                // here. Some information about the request is included as properties of the log so that it can be
                // correlated back to the request.
                if (request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    _dataProcessingErrorProperties.Clear();
                    _dataProcessingErrorProperties.Add("Download Handler", request.downloadHandler.GetType().Name);
                    _dataProcessingErrorProperties.Add("URL", request.url);
                    _dataProcessingErrorProperties.Add("Response Code", request.responseCode.ToString());
                    _dataProcessingErrorProperties.Add("Bytes In", request.downloadedBytes.ToString());
                    _dataProcessingErrorProperties.Add("Bytes Out", request.uploadedBytes.ToString());

                    Embrace.Instance.LogMessage(request.downloadHandler?.error ?? request.error ?? "UnityWebRequest data processing error", EMBSeverity.Error, _dataProcessingErrorProperties);
                }
                #endif
            }
            catch (ArgumentNullException) { }
        }

        /// <summary>
        /// Returns an instance of HttpClient that uses the EmbraceLoggingHttpMessageHandler to log all requests
        /// to Embrace.
        /// </summary>
        [Preserve]
        public static HttpClient GetHttpClientWithLoggingHandler()
            => GetHttpClientWithLoggingHandler(new HttpClientHandler());

        /// <summary>
        /// Returns an instance of HttpClient that uses the EmbraceLoggingHttpMessageHandler to log all requests
        /// to Embrace.
        /// </summary>
        /// <param name="innerHandler">The EmbraceLoggingHttpMessageHandler is a DelegatingHandler, so you can
        /// provide an innerHandler for it to delegate to.</param>
        [Preserve]
        public static HttpClient GetHttpClientWithLoggingHandler(HttpMessageHandler innerHandler)
        {
            HttpMessageHandler wrappedHandler = innerHandler is EmbraceLoggingHttpMessageHandler
                ? innerHandler
                : new EmbraceLoggingHttpMessageHandler(innerHandler);
            return new HttpClient(wrappedHandler);
        }

        /// <summary>
        /// Returns an instance of HttpClient that uses the EmbraceLoggingHttpMessageHandler to log all requests
        /// to Embrace.
        /// </summary>
        /// <param name="innerHandler">The EmbraceLoggingHttpMessageHandler is a DelegatingHandler, so you can
        /// provide an innerHandler for it to delegate to.</param>
        /// <param name="disposeHandler">The value to pass to the disposeHandler parameter in the HttpClient
        /// constructor.</param>
        [Preserve]
        public static HttpClient GetHttpClientWithLoggingHandler(HttpMessageHandler innerHandler, bool disposeHandler)
        {
            HttpMessageHandler wrappedHandler = innerHandler is EmbraceLoggingHttpMessageHandler
                ? innerHandler
                : new EmbraceLoggingHttpMessageHandler(innerHandler);
            return new HttpClient(wrappedHandler, disposeHandler);
        }

    }
}