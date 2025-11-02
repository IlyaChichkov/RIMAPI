using System;
using System.Threading.Tasks;
using System.Net;
using Verse;

namespace RimworldRestApi.Core
{
    /// <summary>
    /// Implementation of IExtensionRouter that routes to the main router with namespacing
    /// </summary>
    public class ExtensionRouter : IExtensionRouter
    {
        private readonly Router _mainRouter;
        private readonly string _extensionNamespace;

        public ExtensionRouter(Router mainRouter, string extensionNamespace)
        {
            _mainRouter = mainRouter;
            _extensionNamespace = extensionNamespace.ToLowerInvariant();
        }

        public void Get(string path, Func<HttpListenerContext, Task> handler)
        {
            AddRoute("GET", path, handler);
        }

        public void Post(string path, Func<HttpListenerContext, Task> handler)
        {
            AddRoute("POST", path, handler);
        }

        public void Put(string path, Func<HttpListenerContext, Task> handler)
        {
            AddRoute("PUT", path, handler);
        }

        public void Delete(string path, Func<HttpListenerContext, Task> handler)
        {
            AddRoute("DELETE", path, handler);
        }

        public void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler)
        {
            if (string.IsNullOrEmpty(path))
            {
                DebugLogging.Error($"Extension '{_extensionNamespace}' attempted to register empty path");
                return;
            }

            // Normalize path - ensure it starts without slash
            var normalizedPath = path.TrimStart('/');

            // Create full path with extension namespace
            var fullPath = $"/api/v1/{_extensionNamespace}/{normalizedPath}";

            // Add logging wrapper for error handling
            async Task WrappedHandler(HttpListenerContext context)
            {
                try
                {
                    DebugLogging.Info($"Handling extension endpoint {method} {fullPath}");
                    await handler(context);
                }
                catch (Exception ex)
                {
                    DebugLogging.Error($"Error in extension '{_extensionNamespace}' endpoint {path}: {ex}");
                    await ResponseBuilder.Error(context.Response,
                        HttpStatusCode.InternalServerError,
                        $"Extension '{_extensionNamespace}' error: {ex.Message}");
                }
            }

            _mainRouter.AddRoute(method, fullPath, WrappedHandler);
            DebugLogging.Info($"Registered extension endpoint: {method} {fullPath}");
        }
    }
}