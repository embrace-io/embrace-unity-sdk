using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EmbraceSDK.Networking
{
    /// <summary>
    /// When attached to an HttpClient, this message handler will log all network requests sent by the client
    /// to Embrace. This handler is automatically added to all HttpClients when automatic network capture is enabled,
    /// but can also be used manually by passing an instance of the handler to the HttpClient constructor.
    /// </summary>
    public class EmbraceLoggingHttpMessageHandler : DelegatingHandler
    {
        public EmbraceLoggingHttpMessageHandler() : base(new HttpClientHandler()) { }

        public EmbraceLoggingHttpMessageHandler(HttpMessageHandler inner) : base(inner) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            long startms = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            HttpResponseMessage response = null;
            string error = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (AggregateException aggregateException)
            {
                Exception e = aggregateException.InnerException;
                error = e != null ? $"{e.GetType().Name}: {e.Message}" : string.Empty;
            }
            catch (Exception e)
            {
                error = $"{e.GetType().Name}: {e.Message}";
                throw;
            }
            finally
            {
                if (Embrace.GetExistingInstance()?.IsStarted ?? false)
                {
                    string uri = request.RequestUri?.ToString() ?? string.Empty;
                    if (!HTTPMethod.TryParse(request.Method?.ToString(), out HTTPMethod method))
                    {
                        method = HTTPMethod.OTHER;
                    }
                    long endms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    long bytesin = response?.Content?.Headers?.ContentLength ?? 0;
                    long bytesout = request.Content?.Headers?.ContentLength ?? 0;
                    int code = (int)(response?.StatusCode ?? 0);
                    
                    if (error != null)
                    {
                        Embrace.Instance.RecordIncompleteNetworkRequest(uri, method, startms, endms, error);
                    }
                    else
                    {
                        Embrace.Instance.RecordCompleteNetworkRequest(uri, method, startms, endms, bytesin, bytesout, code);
                    }
                }
            }

            return response;
        }
    }
}