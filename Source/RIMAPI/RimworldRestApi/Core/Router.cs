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

            foreach (var route in _routes)
            {
                if (route.Method != request.HttpMethod)
                    continue;

                var match = route.Pattern.Match(path);
                if (match.Success)
                {
                    // Extract route parameters
                    foreach (Group group in match.Groups)
                    {
                        if (group.Name.StartsWith("param_"))
                        {
                            var paramName = group.Name.Substring(6);
                            context.Request.QueryString[paramName] = group.Value;
                        }
                    }

                    await route.Handler(context);
                    return;
                }
            }

            // No route found
            await ResponseBuilder.Error(context.Response,
                HttpStatusCode.NotFound, "Endpoint not found");
        }

        private class Route
        {
            public string Method { get; }
            public Regex Pattern { get; }
            public Func<HttpListenerContext, Task> Handler { get; }

            public Route(string method, string path, Func<HttpListenerContext, Task> handler)
            {
                Method = method;
                Handler = handler;

                // Convert route pattern to regex
                var pattern = "^" + Regex.Escape(path)
                    .Replace("\\{", "(?<param_")
                    .Replace("\\}", ">[^/]+)") + "$";

                Pattern = new Regex(pattern, RegexOptions.IgnoreCase);
            }
        }
    }
}