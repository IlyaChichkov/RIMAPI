using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace RIMAPI.Core
{
    public class AutoRouteRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Router _router;

        public AutoRouteRegistry(IServiceProvider serviceProvider, Router router)
        {
            _serviceProvider = serviceProvider;
            _router = router;
        }

        public void RegisterAllRoutes()
        {
            try
            {
                LogApi.Info("Starting auto-route registration...");

                // Find all controller types in the current assembly
                var controllerTypes = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface)
                    .ToList();

                LogApi.Info($"Found {controllerTypes.Count} controller types");

                int totalRoutes = 0;
                foreach (var controllerType in controllerTypes)
                {
                    totalRoutes += RegisterControllerRoutes(controllerType);
                }

                LogApi.Info(
                    $"Auto-route registration complete. Registered {totalRoutes} routes across {controllerTypes.Count} controllers"
                );
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error during auto-route registration: {ex}");
                throw;
            }
        }

        public void RegisterController<TController>()
            where TController : class
        {
            RegisterControllerRoutes(typeof(TController));
        }

        private int RegisterControllerRoutes(Type controllerType)
        {
            var routeCount = 0;

            try
            {
                LogApi.Message($"Registering routes for controller: {controllerType.Name}");

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
                    var routeAttrs = method.GetCustomAttributes<RouteAttribute>();
                    foreach (var routeAttr in routeAttrs)
                    {
                        RegisterRoute(controllerType, method, routeAttr);
                        routeCount++;
                    }

                    // Also check for specific HTTP method attributes
                    var getAttr = method.GetCustomAttribute<GetAttribute>();
                    if (getAttr != null)
                    {
                        RegisterRoute(controllerType, method, getAttr);
                        routeCount++;
                    }

                    var postAttr = method.GetCustomAttribute<PostAttribute>();
                    if (postAttr != null)
                    {
                        RegisterRoute(controllerType, method, postAttr);
                        routeCount++;
                    }

                    var putAttr = method.GetCustomAttribute<PutAttribute>();
                    if (putAttr != null)
                    {
                        RegisterRoute(controllerType, method, putAttr);
                        routeCount++;
                    }

                    var deleteAttr = method.GetCustomAttribute<DeleteAttribute>();
                    if (deleteAttr != null)
                    {
                        RegisterRoute(controllerType, method, deleteAttr);
                        routeCount++;
                    }
                }

                if (routeCount == 0)
                {
                    LogApi.Warning($"No routes found for controller: {controllerType.Name}");
                }
            }
            catch (Exception ex)
            {
                LogApi.Error(
                    $"Error registering routes for controller {controllerType.Name}: {ex}"
                );
            }

            return routeCount;
        }

        private void RegisterRoute(Type controllerType, MethodInfo method, RouteAttribute routeAttr)
        {
            try
            {
                // Create a generic handler that resolves the controller from DI
                var handler = CreateRouteHandler(controllerType, method);

                // Use AddRoute instead of Register to match your Router API
                _router.AddRoute(routeAttr.Method, routeAttr.Pattern, handler);

                LogApi.Message(
                    $"Auto-registered: {routeAttr.Method} {routeAttr.Pattern} -> {controllerType.Name}.{method.Name}"
                    , LoggingLevels.DEBUG
                );
            }
            catch (Exception ex)
            {
                LogApi.Error(
                    $"Failed to register route {routeAttr.Method} {routeAttr.Pattern} for {controllerType.Name}.{method.Name}: {ex}"
                );
                throw;
            }
        }

        private Func<HttpListenerContext, Task> CreateRouteHandler(
            Type controllerType,
            MethodInfo method
        )
        {
            return async context =>
            {
                try
                {
                    // Resolve controller from DI container
                    var controller = _serviceProvider.GetService(controllerType);
                    if (controller == null)
                    {
                        LogApi.Error(
                            $"Controller {controllerType.Name} not registered in DI container"
                        );
                        await context.SendError(
                            HttpStatusCode.InternalServerError,
                            $"Service unavailable: {controllerType.Name} not registered"
                        );
                        return;
                    }

                    // Invoke the method
                    var task = (Task)method.Invoke(controller, new object[] { context });
                    if (task != null)
                    {
                        await task;
                    }
                    else
                    {
                        LogApi.Error(
                            $"Method {controllerType.Name}.{method.Name} returned null Task"
                        );
                        await context.SendError(
                            HttpStatusCode.InternalServerError,
                            "Internal server error: handler returned null"
                        );
                    }
                }
                catch (TargetInvocationException ex)
                {
                    // Unwrap the actual exception from reflection
                    var innerEx = ex.InnerException ?? ex;
                    LogApi.Error(
                        $"Error in auto-routed endpoint {controllerType.Name}.{method.Name}: {innerEx}"
                    );
                    await context.SendError(
                        HttpStatusCode.InternalServerError,
                        $"Internal server error: {innerEx.Message}"
                    );
                }
                catch (Exception ex)
                {
                    LogApi.Error(
                        $"Error in auto-routed endpoint {controllerType.Name}.{method.Name}: {ex}"
                    );
                    await context.SendError(
                        HttpStatusCode.InternalServerError,
                        $"Internal server error: {ex.Message}"
                    );
                }
            };
        }

        // Method to get all registered routes for debugging
        public IEnumerable<(
            string Controller,
            string Method,
            string HttpMethod,
            string Pattern
        )> GetRegisteredRoutesInfo()
        {
            var routes = new List<(string, string, string, string)>();

            var controllerTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface);

            foreach (var controllerType in controllerTypes)
            {
                var methods = controllerType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m =>
                        m.ReturnType == typeof(Task)
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == typeof(HttpListenerContext)
                    );

                foreach (var method in methods)
                {
                    var routeAttrs = method.GetCustomAttributes<RouteAttribute>();
                    foreach (var routeAttr in routeAttrs)
                    {
                        routes.Add(
                            (controllerType.Name, method.Name, routeAttr.Method, routeAttr.Pattern)
                        );
                    }

                    // Check specific HTTP method attributes
                    var getAttr = method.GetCustomAttribute<GetAttribute>();
                    if (getAttr != null)
                    {
                        routes.Add((controllerType.Name, method.Name, "GET", getAttr.Pattern));
                    }

                    var postAttr = method.GetCustomAttribute<PostAttribute>();
                    if (postAttr != null)
                    {
                        routes.Add((controllerType.Name, method.Name, "POST", postAttr.Pattern));
                    }

                    var putAttr = method.GetCustomAttribute<PutAttribute>();
                    if (putAttr != null)
                    {
                        routes.Add((controllerType.Name, method.Name, "PUT", putAttr.Pattern));
                    }

                    var deleteAttr = method.GetCustomAttribute<DeleteAttribute>();
                    if (deleteAttr != null)
                    {
                        routes.Add(
                            (controllerType.Name, method.Name, "DELETE", deleteAttr.Pattern)
                        );
                    }
                }
            }

            return routes;
        }

        // Method to validate that all controllers can be resolved
        public Dictionary<string, bool> ValidateControllerRegistration()
        {
            var results = new Dictionary<string, bool>();

            var controllerTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface);

            foreach (var controllerType in controllerTypes)
            {
                try
                {
                    var controller = _serviceProvider.GetService(controllerType);
                    results[controllerType.Name] = controller != null;

                    if (controller == null)
                    {
                        LogApi.Warning(
                            $"Controller {controllerType.Name} cannot be resolved from DI container"
                        );
                    }
                }
                catch (Exception ex)
                {
                    LogApi.Error($"Error validating controller {controllerType.Name}: {ex}");
                    results[controllerType.Name] = false;
                }
            }

            return results;
        }
    }
}
