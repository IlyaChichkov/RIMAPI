using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RimworldRestApi.Core
{
    public class Router
    {
        private readonly List<Route> _routes;

        private readonly HashSet<string> _allowedOrigins;

        public Router()
        {
            _routes = new List<Route>();
            _allowedOrigins = null;
        }

        public void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler)
        {
            _routes.Add(new Route(method, path, handler));
        }

        public async Task RouteRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var path = NormalizePath(request.Url.AbsolutePath);
            var method = request.HttpMethod?.ToUpperInvariant() ?? "GET";

            DebugLogging.Info($"Routing {method} {path}");

            // Handle CORS preflight request (OPTIONS)
            if (method == "OPTIONS")
            {
                CorsUtil.WritePreflight(context); // Handles OPTIONS preflight
                return;
            }

            // Ensure CORS headers are applied before response (check for valid origin)
            CorsUtil.WriteCors(request, response, _allowedOrigins);

            // 1) HEAD -> treat as GET but don't send a body (controller can decide what to do)
            var matchMethod = method == "HEAD" ? "GET" : method;

            foreach (var route in _routes)
            {
                if (route.Method != matchMethod)
                    continue;

                var m = route.Pattern.Match(path);
                if (!m.Success) continue;

                DebugLogging.Info($"Route matched: {route.Method} {route.PathPattern}");

                try
                {
                    await route.Handler(context);
                    return;
                }
                catch (Exception ex)
                {
                    DebugLogging.Error($"Error in route handler: {ex}");
                    // Ensure CORS is set before responding with error
                    CorsUtil.WriteCors(request, response, _allowedOrigins);  // Set CORS before error response
                    await ResponseBuilder.Error(response, HttpStatusCode.InternalServerError, "Handler error");
                    return;
                }
            }

            // No route found
            DebugLogging.Warning($"No route found for {method} {path}");
            CorsUtil.WriteCors(request, response, _allowedOrigins);  // Set CORS before error response
            await ResponseBuilder.Error(response, HttpStatusCode.NotFound, $"Endpoint not found: {path}");
        }


        private static string NormalizePath(string p)
        {
            if (string.IsNullOrEmpty(p)) return "/";
            // Trim trailing slash except root to avoid double route registration for /foo vs /foo/
            if (p.Length > 1 && p.EndsWith("/")) p = p.TrimEnd('/');
            return p;
        }

        private class Route
        {
            public string Method { get; }
            public Regex Pattern { get; }
            public string PathPattern { get; }
            public Func<HttpListenerContext, Task> Handler { get; }

            public Route(string method, string path, Func<HttpListenerContext, Task> handler)
            {
                Method = method;
                PathPattern = path;
                Handler = handler;

                // Convert route pattern to regex with named groups
                var regexPattern = "^" + Regex.Escape(path)
                    .Replace("\\{", "(?<param_")
                    .Replace("\\}", ">[^/]+)") + "$";

                Pattern = new Regex(regexPattern, RegexOptions.IgnoreCase);
            }
        }
    }
}