#if !RIMWORLD_1_7
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RIMAPI.Core;

namespace RIMAPI.Http
{
    /// <summary>
    /// HTTP transport for RimWorld 1.5/1.6 (Unity Mono), where System.Net.HttpListener is a
    /// managed socket implementation that needs no URL reservation.
    /// </summary>
    public sealed class HttpListenerTransport : IRimApiHttpTransport
    {
        private readonly HttpListener _listener;
        private volatile bool _isRunning;

        public HttpListenerTransport(string serverIP, int port)
        {
            string hostBinding = serverIP == "0.0.0.0" ? "+" : serverIP;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{hostBinding}:{port}/");
        }

        public void Start(Action<RimApiContext> onRequest)
        {
            _listener.Start();
            _isRunning = true;
            _ = ListenAsync(onRequest);
        }

        private async Task ListenAsync(Action<RimApiContext> onRequest)
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    onRequest(new HttpListenerApiContext(context));
                }
                catch (HttpListenerException)
                {
                    // Expected when stopping
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        LogApi.Error($"Error accepting request - {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            try
            {
                if (_listener.IsListening)
                    _listener.Stop();
                _listener.Close();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error stopping HttpListener - {ex.Message}");
            }
        }

        private sealed class HttpListenerApiContext : RimApiContext
        {
            private readonly ListenerRequest _request;
            private readonly ListenerResponse _response;

            public HttpListenerApiContext(HttpListenerContext context)
            {
                _request = new ListenerRequest(context.Request);
                _response = new ListenerResponse(context.Response);
            }

            public override RimApiRequest Request => _request;
            public override RimApiResponse Response => _response;
        }

        private sealed class ListenerRequest : RimApiRequest
        {
            private readonly HttpListenerRequest _request;

            public ListenerRequest(HttpListenerRequest request)
            {
                _request = request;
            }

            public override string HttpMethod => _request.HttpMethod;
            public override Uri Url => _request.Url;
            public override NameValueCollection QueryString => _request.QueryString;
            public override NameValueCollection Headers => _request.Headers;
            public override bool HasEntityBody => _request.HasEntityBody;

            public override async Task<byte[]> ReadBodyBytesAsync()
            {
                if (!_request.HasEntityBody)
                    return new byte[0];

                using (var buffer = new MemoryStream())
                {
                    await _request.InputStream.CopyToAsync(buffer);
                    return buffer.ToArray();
                }
            }
            public override Encoding ContentEncoding => _request.ContentEncoding;
        }

        private sealed class ListenerResponse : RimApiResponse
        {
            private readonly HttpListenerResponse _response;

            public ListenerResponse(HttpListenerResponse response)
            {
                _response = response;
            }

            public override int StatusCode
            {
                get => _response.StatusCode;
                set => _response.StatusCode = value;
            }

            public override string StatusDescription
            {
                get => _response.StatusDescription;
                set => _response.StatusDescription = value;
            }

            public override string ContentType
            {
                get => _response.ContentType;
                set => _response.ContentType = value;
            }

            public override NameValueCollection Headers => _response.Headers;

            public override bool SendChunked
            {
                get => _response.SendChunked;
                set => _response.SendChunked = value;
            }

            public override async Task SendAsync(byte[] body)
            {
                try
                {
                    _response.ContentLength64 = body.Length;
                    await _response.OutputStream.WriteAsync(body, 0, body.Length);
                    _response.Close();
                }
                finally
                {
                    MarkCompleted();
                }
            }

            public override async Task WriteChunkAsync(byte[] chunk)
            {
                await _response.OutputStream.WriteAsync(chunk, 0, chunk.Length);
                await _response.OutputStream.FlushAsync();
            }

            public override void Close()
            {
                try
                {
                    _response.Close();
                }
                finally
                {
                    MarkCompleted();
                }
            }

            public override void Abort()
            {
                try
                {
                    _response.Abort();
                }
                finally
                {
                    MarkCompleted();
                }
            }
        }
    }
}
#endif
