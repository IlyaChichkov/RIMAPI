#if RIMWORLD_1_7
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RIMAPI.Core;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace RIMAPI.Http
{
    /// <summary>
    /// HTTP transport for RimWorld 1.7 (Waylith, .NET 10). System.Net.HttpListener there is backed by
    /// HTTP.sys on Windows and fails with "Access denied" without a URL reservation, so RIMAPI uses
    /// Watson, which runs on its own TcpListener transport.
    /// </summary>
    public sealed class WatsonTransport : IRimApiHttpTransport
    {
        private readonly Webserver _server;
        private Action<RimApiContext> _onRequest;

        public WatsonTransport(string serverIP, int port)
        {
            string hostname = serverIP == "0.0.0.0" || serverIP == "+" ? "*" : serverIP;
            var settings = new WebserverSettings(hostname, port, false);
            // Watson's defaults add CORS, "Connection: close" and "Host" headers to every response;
            // RIMAPI writes its own CORS headers, so start from an empty set.
            settings.Headers.DefaultHeaders = new Dictionary<string, string>();
            settings.IO.EnableKeepAlive = true;
            _server = new Webserver(settings, HandleRequestAsync);
        }

        public void Start(Action<RimApiContext> onRequest)
        {
            _onRequest = onRequest;
            _server.Start();
        }

        /// <summary>
        /// Watson keeps the connection open only while this handler runs, so hand the request to
        /// RIMAPI's main-thread queue and wait until RIMAPI finishes the response.
        /// </summary>
        private async Task HandleRequestAsync(HttpContextBase ctx)
        {
            var context = new WatsonApiContext(ctx);
            _onRequest(context);
            await context.Response.Completion.ConfigureAwait(false);
        }

        public void Dispose()
        {
            try
            {
                _server.Stop();
                _server.Dispose();
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error stopping Watson server - {ex.Message}");
            }
        }

        private sealed class WatsonApiContext : RimApiContext
        {
            private readonly WatsonRequest _request;
            private readonly WatsonResponse _response;

            public WatsonApiContext(HttpContextBase ctx)
            {
                _request = new WatsonRequest(ctx.Request);
                _response = new WatsonResponse(ctx);
            }

            public override RimApiRequest Request => _request;
            public override RimApiResponse Response => _response;
        }

        private sealed class WatsonRequest : RimApiRequest
        {
            private readonly HttpRequestBase _request;
            private readonly Uri _url;

            public WatsonRequest(HttpRequestBase request)
            {
                _request = request;
                _url = request.Url.Uri;
                if (_url == null)
                {
                    var host = request.Headers?["Host"];
                    _url = new Uri(
                        $"http://{(string.IsNullOrEmpty(host) ? "localhost" : host)}{request.Url.RawWithQuery}"
                    );
                }
            }

            public override string HttpMethod => _request.MethodRaw?.ToUpperInvariant() ?? _request.Method.ToString();
            public override Uri Url => _url;
            public override NameValueCollection QueryString => _request.Query.Elements;
            public override NameValueCollection Headers => _request.Headers;
            public override bool HasEntityBody => _request.ContentLength > 0 || _request.ChunkedTransfer;

            public override async Task<byte[]> ReadBodyBytesAsync()
            {
                // Watson's Data is the raw connection stream; ReadBodyAsync honours
                // Content-Length and decodes chunked bodies
                return await _request.ReadBodyAsync().ConfigureAwait(false) ?? Array.Empty<byte>();
            }

            public override Encoding ContentEncoding
            {
                get
                {
                    var contentType = _request.ContentType;
                    var index = contentType?.IndexOf("charset=", StringComparison.OrdinalIgnoreCase) ?? -1;
                    if (index < 0)
                        return Encoding.UTF8;
                    try
                    {
                        var charset = contentType.Substring(index + 8).Split(';')[0].Trim().Trim('"');
                        return Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException)
                    {
                        return Encoding.UTF8;
                    }
                }
            }
        }

        private sealed class WatsonResponse : RimApiResponse
        {
            private readonly HttpContextBase _ctx;
            private HttpResponseBase Response => _ctx.Response;

            public WatsonResponse(HttpContextBase ctx)
            {
                _ctx = ctx;
            }

            public override int StatusCode
            {
                get => Response.StatusCode;
                set => Response.StatusCode = value;
            }

            // Watson derives the reason phrase from StatusCode
            public override string StatusDescription
            {
                get => Response.StatusDescription;
                set { }
            }

            public override string ContentType
            {
                get => Response.ContentType;
                set => Response.ContentType = value;
            }

            public override NameValueCollection Headers => Response.Headers;

            public override bool SendChunked
            {
                get => Response.ChunkedTransfer;
                set => Response.ChunkedTransfer = value;
            }

            public override async Task SendAsync(byte[] body)
            {
                try
                {
                    if (!await Response.Send(body).ConfigureAwait(false))
                        throw new IOException("Failed to send response; the client may have disconnected.");
                }
                finally
                {
                    MarkCompleted();
                }
            }

            public override async Task WriteChunkAsync(byte[] chunk)
            {
                if (!await Response.SendChunk(chunk, false).ConfigureAwait(false))
                    throw new IOException("Failed to send chunk; the client may have disconnected.");
            }

            public override void Close()
            {
                if (IsCompleted)
                    return;
                _ = FinishAsync();
            }

            private async Task FinishAsync()
            {
                try
                {
                    if (!Response.ResponseSent || Response.ChunkedTransfer)
                    {
                        if (Response.ChunkedTransfer)
                            await Response.SendChunk(Array.Empty<byte>(), true).ConfigureAwait(false);
                        else
                            await Response.Send().ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {
                    // Client already gone; nothing left to finish
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
                    _ctx.TokenSource?.Cancel();
                }
                catch (Exception)
                {
                    // Already disposed
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
