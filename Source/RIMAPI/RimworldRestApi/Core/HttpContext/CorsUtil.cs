using System.Collections.Generic;
using System.Net;

namespace RIMAPI.Core
{
    public static class CorsUtil
    {
        public static void WriteCors(
            HttpListenerRequest req,
            HttpListenerResponse res,
            ISet<string> allowedOrigins = null
        )
        {
            var origin = req.Headers["Origin"];
            string allowOrigin = null;

            if (string.IsNullOrEmpty(origin))
            {
                // Non-CORS or same-origin XHR; choose * or leave unset.
                allowOrigin = "*"; // Allow all origins for non-CORS requests
            }
            else if (allowedOrigins != null && allowedOrigins.Contains(origin))
            {
                allowOrigin = origin;
            }
            else
            {
                allowOrigin = "*"; // Or you can explicitly allow specific origins
            }

            res.Headers.Set("Access-Control-Allow-Origin", allowOrigin);
            res.Headers.Set("Access-Control-Allow-Credentials", "true");
            res.Headers.Set(
                "Access-Control-Allow-Methods",
                "GET, POST, PUT, PATCH, DELETE, OPTIONS"
            );
            res.Headers.Set(
                "Access-Control-Allow-Headers",
                "Content-Type, Accept, Authorization, ETag, If-None-Match"
            );
        }

        public static void WritePreflight(
            HttpListenerContext ctx,
            IEnumerable<string> methods = null,
            IEnumerable<string> extraHeaders = null
        )
        {
            LogApi.Info($"WritePreflight");
            var res = ctx.Response;

            res.Headers.Set(
                "Access-Control-Allow-Methods",
                methods != null
                    ? string.Join(", ", methods)
                    : "GET, POST, PUT, PATCH, DELETE, OPTIONS"
            );

            var requested = ctx.Request.Headers["Access-Control-Request-Headers"];
            res.Headers.Set(
                "Access-Control-Allow-Headers",
                !string.IsNullOrEmpty(requested)
                    ? requested
                    : (
                        extraHeaders != null
                            ? string.Join(", ", extraHeaders)
                            : "Content-Type, Accept, Authorization, ETag, If-None-Match"
                    )
            );

            res.Headers.Set("Access-Control-Allow-Origin", "*");
            res.Headers.Set("Access-Control-Max-Age", "86400"); // Cache preflight response for 24 hours
            res.StatusCode = (int)HttpStatusCode.NoContent; // 204 No Content
            res.StatusDescription = "No Content"; // Set appropriate status description
            res.Close(); // Close after writing the headers
        }
    }
}
