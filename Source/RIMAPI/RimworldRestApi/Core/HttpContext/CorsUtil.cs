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

            string allowOrigin = string.IsNullOrEmpty(origin) ? "*" : origin;

            res.Headers.Set("Access-Control-Allow-Origin", allowOrigin);

            if (allowOrigin != "*")
            {
                res.Headers.Set("Access-Control-Allow-Credentials", "true");
            }

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
            var req = ctx.Request;
            var res = ctx.Response;

            res.Headers.Set(
                "Access-Control-Allow-Methods",
                methods != null
                    ? string.Join(", ", methods)
                    : "GET, POST, PUT, PATCH, DELETE, OPTIONS"
            );

            var requested = req.Headers["Access-Control-Request-Headers"];
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

            var origin = req.Headers["Origin"];
            string allowOrigin = string.IsNullOrEmpty(origin) ? "*" : origin;

            res.Headers.Set("Access-Control-Allow-Origin", allowOrigin);

            if (allowOrigin != "*")
            {
                res.Headers.Set("Access-Control-Allow-Credentials", "true");
            }

            res.Headers.Set("Access-Control-Max-Age", "86400"); // Cache preflight for 24 hours
            res.StatusCode = (int)HttpStatusCode.NoContent; // 204 No Content
            res.StatusDescription = "No Content";
            res.Close();
        }
    }
}