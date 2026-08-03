using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        // Traceparent validation
        public const string EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS = nameof(EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS);
        private static readonly Regex TraceparentRegex = new("^(?<version>[0-9a-f]{2})-(?<traceId>[0-9a-f]{32})-(?<parentId>[0-9a-f]{16})-[0-9a-f]{2}$", RegexOptions.Compiled);
        private const string AllZeroTraceId = "00000000000000000000000000000000";
        private const string AllZeroParentId = "0000000000000000";

        // Traceparent generation
        private static readonly RandomNumberGenerator TraceparentRandom = RandomNumberGenerator.Create();
        private static readonly object TraceparentRandomLock = new object();
        private const int TraceIdBytes = 16;
        private const int ParentIdBytes = 8;
        private const int MaxTraceparentGenerationAttempts = 10;
        
        /// <summary>
        /// Validates that <paramref name="traceparent"/> is a well-formed W3C traceparent header value
        /// (https://www.w3.org/TR/trace-context/#traceparent-header). This is used both to sanity-check
        /// traceparent values generated internally and to guard against malformed or malicious values
        /// supplied by callers before they are attached as an HTTP header or forwarded to the native SDK.
        /// </summary>
        public static bool IsValidTraceparent(string traceparent)
        {
            if (string.IsNullOrEmpty(traceparent))
            {
                return false;
            }

            Match match = TraceparentRegex.Match(traceparent);
            
            if (!match.Success)
            {
                return false;
            }

            if (match.Groups["version"].Value == "ff")
            {
                return false;
            }

            if (match.Groups["traceId"].Value == AllZeroTraceId)
            {
                return false;
            }

            if (match.Groups["parentId"].Value == AllZeroParentId)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a new, valid W3C traceparent header value (https://www.w3.org/TR/trace-context/#traceparent-header)
        /// with a randomly generated trace id and parent (span) id, version "00" and trace-flags "01" (sampled).
        /// </summary>
        public static string GenerateTraceparent()
        {
            // GenerateHexId retries internally on the astronomically unlikely all-zero case, but we still guard
            // the assembled string with IsValidTraceparent so this can never hand back a value it wouldn't accept.
            // Bounded by MaxTraceparentGenerationAttempts so a persistently misbehaving RNG can't hang the caller.
            for (int attempt = 0; attempt < MaxTraceparentGenerationAttempts; attempt++)
            {
                string traceparent = $"00-{GenerateHexId(TraceIdBytes)}-{GenerateHexId(ParentIdBytes)}-01";

                if (IsValidTraceparent(traceparent))
                {
                    return traceparent;
                }
            }

            throw new InvalidOperationException($"Failed to generate a valid W3C traceparent after {MaxTraceparentGenerationAttempts} attempts.");
        }

        private static string GenerateHexId(int byteCount)
        {
            byte[] bytes = new byte[byteCount];

            lock (TraceparentRandomLock)
            {
                TraceparentRandom.GetBytes(bytes);
            }

            return BytesToHex(bytes);
        }

        private static string BytesToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        internal static bool IsNetworkSpanForwardingEnabled()
        {
            Embrace embrace = InternalEmbrace.GetExistingInstance();
            if (embrace == null || !embrace.IsStarted)
            {
                return false;
            }

            try
            {
                return embrace.Provider?.IsNetworkSpanForwardingEnabled() ?? false;
            }
            catch (Exception e)
            {
                EmbraceLogger.LogException(e);
                return false;
            }
        }

        private class PendingRequest<T>
        {
            public T requestOperation;
            public long startms;
            public string traceparent;
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

            string traceparent = null;
            if (IsNetworkSpanForwardingEnabled())
            {
                traceparent = GenerateTraceparent();
                request.SetRequestHeader("traceparent", traceparent);
            }

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            _pendingUnityWebRequests[request] = new PendingRequest<UnityWebRequestAsyncOperation>()
            {
                requestOperation = operation,
                startms = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                traceparent = traceparent,
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
                    Embrace.Instance.RecordIncompleteNetworkRequest(request.url, method, pendingRequest.startms, endms, error, pendingRequest.traceparent);
                }
                else
                {
                    Embrace.Instance.RecordCompleteNetworkRequest(request.url, method, pendingRequest.startms, endms, bytesin, bytesout, code, pendingRequest.traceparent);
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