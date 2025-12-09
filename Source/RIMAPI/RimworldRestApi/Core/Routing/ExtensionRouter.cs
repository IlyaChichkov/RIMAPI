using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Verse;

namespace RIMAPI.Core
{
    /// <summary>
    /// Implementation of IExtensionRouter that routes to the main router with namespacing
    /// </summary>
    public class ExtensionRouter : IExtensionRouter
    {
        private readonly Router _mainRouter;
        private readonly string _extensionNamespace;
        private readonly IServiceProvider _serviceProvider;
        private readonly ExtensionDocumentationService _docExportService;

        public ExtensionRouter(
            Router mainRouter,
            string extensionNamespace,
            IServiceProvider serviceProvider,
            ExtensionDocumentationService docExportService = null
        )
        {
            _mainRouter = mainRouter;
            _extensionNamespace = extensionNamespace.ToLowerInvariant();
            _serviceProvider = serviceProvider;
            _docExportService = docExportService;
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
                LogApi.Error($"Extension '{_extensionNamespace}' attempted to register empty path");
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
                    LogApi.Info($"Handling extension endpoint {method} {fullPath}");
                    await handler(context);
                }
                catch (Exception ex)
                {
                    LogApi.Error(
                        $"Error in extension '{_extensionNamespace}' endpoint {path}: {ex}"
                    );
                    await ResponseBuilder.SendError(
                        context.Response,
                        HttpStatusCode.InternalServerError,
                        $"Extension '{_extensionNamespace}' error: {ex.Message}"
                    );
                }
            }

            _mainRouter.AddRoute(method, fullPath, WrappedHandler);
            LogApi.Info($"Registered extension endpoint: {method} {fullPath}");
        }

        public void RegisterController<TController>()
            where TController : class
        {
            RegisterController(typeof(TController));
        }

        public void RegisterControllers(Assembly assembly)
        {
            try
            {
                var controllerTypes = assembly
                    .GetTypes()
                    .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface)
                    .ToList();

                LogApi.Info(
                    $"Found {controllerTypes.Count} controller types in extension '{_extensionNamespace}'"
                );

                foreach (var controllerType in controllerTypes)
                {
                    RegisterController(controllerType);
                }
            }
            catch (Exception ex)
            {
                LogApi.Error(
                    $"Error registering controllers for extension '{_extensionNamespace}': {ex}"
                );
            }
        }

        private void RegisterController(Type controllerType)
        {
            try
            {
                LogApi.Info(
                    $"Registering controller {controllerType.Name} for extension '{_extensionNamespace}'"
                );

                var methods = controllerType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m =>
                        m.ReturnType == typeof(Task)
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == typeof(HttpListenerContext)
                    )
                    .ToList();

                foreach (var method in methods)
                {
                    // Check for HTTP method attributes
                    var getAttr = method.GetCustomAttribute<GetAttribute>();
                    if (getAttr != null)
                    {
                        RegisterControllerRoute(controllerType, method, "GET", getAttr.Pattern); // FIXED: Pattern instead of Path
                        continue;
                    }

                    var postAttr = method.GetCustomAttribute<PostAttribute>();
                    if (postAttr != null)
                    {
                        RegisterControllerRoute(controllerType, method, "POST", postAttr.Pattern); // FIXED: Pattern instead of Path
                        continue;
                    }

                    var putAttr = method.GetCustomAttribute<PutAttribute>();
                    if (putAttr != null)
                    {
                        RegisterControllerRoute(controllerType, method, "PUT", putAttr.Pattern); // FIXED: Pattern instead of Path
                        continue;
                    }

                    var deleteAttr = method.GetCustomAttribute<DeleteAttribute>();
                    if (deleteAttr != null)
                    {
                        RegisterControllerRoute(
                            controllerType,
                            method,
                            "DELETE",
                            deleteAttr.Pattern
                        ); // FIXED: Pattern instead of Path
                        continue;
                    }

                    // Check for generic Route attribute
                    var routeAttr = method.GetCustomAttribute<RouteAttribute>();
                    if (routeAttr != null)
                    {
                        RegisterControllerRoute(
                            controllerType,
                            method,
                            routeAttr.Method,
                            routeAttr.Pattern
                        );
                    }
                }

                LogApi.Info(
                    $"Registered {methods.Count} methods from controller {controllerType.Name}"
                );
            }
            catch (Exception ex)
            {
                LogApi.Error(
                    $"Error registering controller {controllerType.Name} for extension '{_extensionNamespace}': {ex}"
                );
            }
        }

        private void RegisterControllerRoute(
            Type controllerType,
            MethodInfo method,
            string httpMethod,
            string path
        )
        {
            if (string.IsNullOrEmpty(path))
            {
                LogApi.Warning($"Empty path in controller {controllerType.Name}.{method.Name}");
                return;
            }

            // Normalize path
            var normalizedPath = path.TrimStart('/');
            var fullPath = $"/api/v1/{_extensionNamespace}/{normalizedPath}";

            // Create handler that resolves controller from DI
            Func<HttpListenerContext, Task> handler = async context =>
            {
                try
                {
                    // Try to resolve controller from DI if available
                    object controller = null;
                    if (_serviceProvider != null)
                    {
                        controller = _serviceProvider.GetService(controllerType);
                    }

                    // Fallback: create instance if not in DI
                    if (controller == null)
                    {
                        controller = Activator.CreateInstance(controllerType);
                    }

                    if (controller == null)
                    {
                        LogApi.Error(
                            $"Failed to create controller instance: {controllerType.Name}"
                        );
                        await ResponseBuilder.SendError(
                            context.Response,
                            HttpStatusCode.InternalServerError,
                            $"Extension service unavailable: {controllerType.Name}"
                        );
                        return;
                    }

                    // Invoke the method
                    var task = (Task)method.Invoke(controller, new object[] { context });
                    if (task != null)
                    {
                        await task;
                    }
                }
                catch (TargetInvocationException ex)
                {
                    var innerEx = ex.InnerException ?? ex;
                    LogApi.Error(
                        $"Error in extension controller {controllerType.Name}.{method.Name}: {innerEx}"
                    );
                    await ResponseBuilder.SendError(
                        context.Response,
                        HttpStatusCode.InternalServerError,
                        $"Extension error: {innerEx.Message}"
                    );
                }
                catch (Exception ex)
                {
                    LogApi.Error(
                        $"Error in extension controller {controllerType.Name}.{method.Name}: {ex}"
                    );
                    await ResponseBuilder.SendError(
                        context.Response,
                        HttpStatusCode.InternalServerError,
                        $"Extension error: {ex.Message}"
                    );
                }
            };

            _mainRouter.AddRoute(httpMethod, fullPath, handler);
            LogApi.Info(
                $"Registered extension controller route: {httpMethod} {fullPath} -> {controllerType.Name}.{method.Name}"
            );
        }

        public void RegisterDocumentationExport()
        {
            _mainRouter.AddRoute(
                "GET",
                $"/api/v1/{_extensionNamespace}/docs/export",
                async context =>
                {
                    try
                    {
                        if (_docExportService == null)
                        {
                            await ResponseBuilder.SendError(
                                context.Response,
                                HttpStatusCode.ServiceUnavailable,
                                "Documentation export service not available for this extension"
                            );
                            return;
                        }

                        var format = context.Request.QueryString["format"] ?? "markdown";
                        var success = _docExportService.TryExportExtensionDocumentation(
                            _extensionNamespace,
                            format
                        );

                        if (success)
                        {
                            await ResponseBuilder.SendSuccess(
                                context.Response,
                                new
                                {
                                    message = $"Documentation exported for extension '{_extensionNamespace}'",
                                    extensionId = _extensionNamespace,
                                    format = format,
                                    timestamp = DateTime.UtcNow,
                                }
                            );
                        }
                        else
                        {
                            await ResponseBuilder.SendError(
                                context.Response,
                                HttpStatusCode.InternalServerError,
                                $"Failed to export documentation for extension '{_extensionNamespace}'"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Error in documentation export for {_extensionNamespace}: {ex}"
                        );
                        await ResponseBuilder.SendError(
                            context.Response,
                            HttpStatusCode.InternalServerError,
                            $"Export failed: {ex.Message}"
                        );
                    }
                }
            );

            LogApi.Info($"Registered documentation export for extension: {_extensionNamespace}");
        }
    }
}
