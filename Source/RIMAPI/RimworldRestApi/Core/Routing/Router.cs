using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RIMAPI.Core
{
    public class Router
    {
        private readonly List<Route> _routes;
        private readonly HashSet<string> _allowedOrigins;

        // Store route parameters per request
        private readonly Dictionary<
            HttpListenerContext,
            Dictionary<string, string>
        > _routeParameters;

        public Router()
        {
            _routes = new List<Route>();
            _allowedOrigins = null;
            _routeParameters = new Dictionary<HttpListenerContext, Dictionary<string, string>>();
        }

        public void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler)
        {
            _routes.Add(new Route(method, path, handler));
            LogApi.Message($"Registered route: {method} {path}");
        }

        public void ClearRoutes()
        {
            _routes.Clear();
            LogApi.Message("All routes cleared");
        }

        public IEnumerable<(string Method, string Path)> GetRegisteredRoutes()
        {
            return _routes.Select(r => (r.Method, r.PathPattern));
        }

        public async Task RouteRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var path = NormalizePath(request.Url.AbsolutePath);
            var method = request.HttpMethod?.ToUpperInvariant() ?? "GET";

            LogApi.Info($"Routing {method} {path}");

            // Handle CORS preflight request (OPTIONS)
            if (method == "OPTIONS")
            {
                CorsUtil.WritePreflight(context);
                return;
            }

            // Ensure CORS headers are applied before response
            CorsUtil.WriteCors(request, response, _allowedOrigins);

            // 1) HEAD -> treat as GET but don't send a body
            var matchMethod = method == "HEAD" ? "GET" : method;

            // Try to find matching route
            var routeMatch = FindMatchingRoute(matchMethod, path);

            if (routeMatch != null)
            {
                await ExecuteRouteHandler(routeMatch, context);
                return;
            }

            // No route found
            await HandleNoRouteFound(context, method, path); // Fixed: added await
        }

        private RouteMatch FindMatchingRoute(string method, string path)
        {
            foreach (var route in _routes)
            {
                if (route.Method != method)
                    continue;

                var match = route.Pattern.Match(path);
                if (match.Success)
                {
                    // Extract route parameters
                    var parameters = new Dictionary<string, string>();
                    foreach (var groupName in route.Pattern.GetGroupNames())
                    {
                        if (groupName.StartsWith("param_") && match.Groups[groupName].Success)
                        {
                            var paramName = groupName.Substring(6); // Remove "param_" prefix
                            parameters[paramName] = match.Groups[groupName].Value;
                        }
                    }

                    return new RouteMatch(route, parameters);
                }
            }

            return null;
        }

        private async Task ExecuteRouteHandler(RouteMatch routeMatch, HttpListenerContext context)
        {
            var route = routeMatch.Route;
            LogApi.Info($"Route matched: {route.Method} {route.PathPattern}");

            try
            {
                // Store route parameters for this request
                lock (_routeParameters)
                {
                    _routeParameters[context] = routeMatch.Parameters;
                }

                await route.Handler(context);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error in route handler {route.Method} {route.PathPattern}: {ex}");

                // Ensure CORS is set before responding with error
                CorsUtil.WriteCors(context.Request, context.Response, _allowedOrigins);
                await ResponseBuilder.SendError(
                    context.Response,
                    HttpStatusCode.InternalServerError,
                    $"Handler error: {ex.Message}"
                );
            }
            finally
            {
                // Clean up route parameters
                lock (_routeParameters)
                {
                    _routeParameters.Remove(context);
                }
            }
        }

        private async Task HandleNoRouteFound(
            HttpListenerContext context,
            string method,
            string path
        ) // Fixed: added async Task
        {
            LogApi.Warning($"No route found for {method} {path}");

            // Log available routes for debugging
            var availableRoutes = _routes
                .Where(r => r.Method == method)
                .Select(r => r.PathPattern)
                .ToList();

            if (availableRoutes.Any())
            {
                LogApi.Info($"Available {method} routes: {string.Join(", ", availableRoutes)}");
            }

            CorsUtil.WriteCors(context.Request, context.Response, _allowedOrigins);
            await ResponseBuilder.SendError(
                context.Response,
                HttpStatusCode.NotFound,
                $"Endpoint not found: {method} {path}"
            );
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "/";

            // Trim trailing slash except for root path
            if (path.Length > 1 && path.EndsWith("/"))
                path = path.TrimEnd('/');

            return path;
        }

        // Get route parameters from context
        public Dictionary<string, string> GetRouteParameters(HttpListenerContext context)
        {
            lock (_routeParameters)
            {
                return _routeParameters.TryGetValue(context, out var parameters)
                    ? parameters
                    : new Dictionary<string, string>();
            }
        }

        // Get specific route parameter
        public string GetRouteParameter(HttpListenerContext context, string paramName)
        {
            var parameters = GetRouteParameters(context);
            return parameters.TryGetValue(paramName, out var value) ? value : null;
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
                // Supports both {param} and :param syntax
                var regexPattern =
                    "^"
                    + Regex
                        .Escape(path)
                        .Replace("\\{", "(?<param_")
                        .Replace("\\}", ">[^/]+)")
                        .Replace("\\:", "(?<param_")
                    + "$";

                Pattern = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        }

        private class RouteMatch
        {
            public Route Route { get; }
            public Dictionary<string, string> Parameters { get; }

            public RouteMatch(Route route, Dictionary<string, string> parameters)
            {
                Route = route;
                Parameters = parameters;
            }
        }
    }

    // Extension methods for easier route parameter access
    public static class RouterExtensions
    {
        public static Dictionary<string, string> GetRouteParams(
            this HttpListenerContext context,
            Router router
        )
        {
            return router.GetRouteParameters(context);
        }

        public static string GetRouteParam(
            this HttpListenerContext context,
            Router router,
            string paramName
        )
        {
            return router.GetRouteParameter(context, paramName);
        }

        public static string GetRouteParam(
            this HttpListenerContext context,
            Router router,
            string paramName,
            string defaultValue
        )
        {
            return router.GetRouteParameter(context, paramName) ?? defaultValue;
        }

        public static int GetRouteParamInt(
            this HttpListenerContext context,
            Router router,
            string paramName,
            int defaultValue = 0
        )
        {
            var value = router.GetRouteParameter(context, paramName);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public static bool GetRouteParamBool(
            this HttpListenerContext context,
            Router router,
            string paramName,
            bool defaultValue = false
        )
        {
            var value = router.GetRouteParameter(context, paramName);
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }
    }
}
