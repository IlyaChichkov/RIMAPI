using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Verse;

namespace RimworldRestApi.Core
{
    public class Router
    {
        private readonly List<Route> _routes;

        public Router()
        {
            _routes = new List<Route>();
        }

        public void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler)
        {
            _routes.Add(new Route(method, path, handler));
        }

        public async Task RouteRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var path = request.Url.AbsolutePath;
            var method = request.HttpMethod;

            Log.Message($"RIMAPI: Routing {method} {path}");

            foreach (var route in _routes)
            {
                if (route.Method != method)
                    continue;

                var match = route.Pattern.Match(path);
                if (match.Success)
                {
                    Log.Message($"RIMAPI: Route matched: {route.Method} {route.PathPattern}");

                    // Extract route parameters
                    foreach (Group group in match.Groups)
                    {
                        if (group.Name.StartsWith("param_"))
                        {
                            var paramName = group.Name.Substring(6);
                            // Store parameters in a way we can access them later
                            if (context.Request.QueryString == null)
                            {
                                // This shouldn't happen, but just in case
                                continue;
                            }

                            // Use reflection to add to query string or use custom storage
                            // For now, we'll use a simpler approach - parse from URL directly
                        }
                    }

                    try
                    {
                        await route.Handler(context);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"RIMAPI: Error in route handler: {ex}");
                        await ResponseBuilder.Error(context.Response,
                            HttpStatusCode.InternalServerError, "Handler error");
                        return;
                    }
                }
            }

            // No route found
            Log.Warning($"RIMAPI: No route found for {method} {path}");
            await ResponseBuilder.Error(context.Response,
                HttpStatusCode.NotFound, $"Endpoint not found: {path}");
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