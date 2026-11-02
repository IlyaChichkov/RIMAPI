using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace RIMAPI.Http
{
    /// <summary>
    /// Server-agnostic HTTP context passed to controllers, routers and extensions.
    /// <para>Concrete implementations wrap the underlying HTTP server (System.Net.HttpListener on
    /// RimWorld 1.5/1.6, Watson on RimWorld 1.7), so the rest of RIMAPI never depends on it.</para>
    /// </summary>
    public abstract class RimApiContext
    {
        public abstract RimApiRequest Request { get; }
        public abstract RimApiResponse Response { get; }
    }

    /// <summary>
    /// Incoming HTTP request. Member names mirror System.Net.HttpListenerRequest.
    /// </summary>
    public abstract class RimApiRequest
    {
        /// <summary>HTTP method in upper case, e.g. "GET".</summary>
        public abstract string HttpMethod { get; }

        /// <summary>Absolute request URL including the query string.</summary>
        public abstract Uri Url { get; }

        public abstract NameValueCollection QueryString { get; }

        public abstract NameValueCollection Headers { get; }

        public abstract bool HasEntityBody { get; }

        /// <summary>Read the whole request body (Content-Length or chunked); empty if there is none.</summary>
        public abstract Task<byte[]> ReadBodyBytesAsync();

        public abstract Encoding ContentEncoding { get; }
    }

    /// <summary>
    /// Outgoing HTTP response. Either send a whole body once with <see cref="SendAsync"/>,
    /// or set <see cref="SendChunked"/> and stream with <see cref="WriteChunkAsync"/> until <see cref="Close"/>.
    /// </summary>
    public abstract class RimApiResponse
    {
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public abstract int StatusCode { get; set; }

        public abstract string StatusDescription { get; set; }

        public abstract string ContentType { get; set; }

        /// <summary>Response headers; must be set before anything is sent.</summary>
        public abstract NameValueCollection Headers { get; }

        /// <summary>Use chunked transfer encoding (required for server-sent events).</summary>
        public abstract bool SendChunked { get; set; }

        /// <summary>Completes once the response has been sent, closed or aborted.</summary>
        public Task Completion => _completion.Task;

        public bool IsCompleted => _completion.Task.IsCompleted;

        /// <summary>Send the whole body (Content-Length is set automatically) and finish the response.</summary>
        public abstract Task SendAsync(byte[] body);

        /// <summary>Write one chunk of a chunked response; throws IOException if the client is gone.</summary>
        public abstract Task WriteChunkAsync(byte[] chunk);

        /// <summary>Finish the response; sends an empty body if nothing was sent yet.</summary>
        public abstract void Close();

        /// <summary>Drop the connection without finishing the response.</summary>
        public abstract void Abort();

        protected void MarkCompleted()
        {
            _completion.TrySetResult(true);
        }
    }

    /// <summary>
    /// An HTTP server that hands every incoming request to RIMAPI as a <see cref="RimApiContext"/>.
    /// </summary>
    public interface IRimApiHttpTransport : IDisposable
    {
        /// <summary>Start listening; <paramref name="onRequest"/> is invoked on a background thread.</summary>
        void Start(Action<RimApiContext> onRequest);
    }
}
