using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RIMAPI.CameraStreamer;
using RIMAPI.Controllers;
using RIMAPI.Services;
using Verse;

namespace RIMAPI.Core
{
    public class ApiServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Router _router;
        private readonly SseService _sseService;
        public SseService SseService => _sseService;
        private readonly Queue<HttpListenerContext> _requestQueue;
        private readonly object _queueLock = new object();
        private bool _isRunning;
        private bool _disposed = false;

        public string ServerIP { get; private set; }
        public int Port { get; private set; }
        public string BaseUrl => $"http://{ServerIP}:{Port}/";

        private readonly ExtensionRegistry _extensionRegistry;
        private RIMAPI_Settings Settings;

        private readonly IServiceProvider _serviceProvider;
        private readonly AutoRouteRegistry _routeRegistry;
        private readonly IEventRegistry _eventRegistry;

        public ApiServer(RIMAPI_Settings settings, IServiceProvider serviceProvider = null)
        {
            try
            {
                LogApi.Info("Starting ApiServer constructor...");

                Settings = settings;
                ServerIP = settings.serverIP;
                Port = settings.serverPort;
                LogApi.Info($"Settings loaded - IP: {ServerIP}, Port: {Port}");

                // Initialize extension registry FIRST
                LogApi.Info("Initializing extension registry...");
                _extensionRegistry = new ExtensionRegistry();

                // Build service provider
                LogApi.Info("Creating service provider...");
                _serviceProvider = serviceProvider ?? CreateDefaultServiceProvider();
                LogApi.Info($"Service provider created: {_serviceProvider != null}");

                // Now resolve services from DI
                LogApi.Info("Resolving SseService...");
                _sseService = (SseService)_serviceProvider.GetService(typeof(SseService));
                LogApi.Info($"SseService resolved: {_sseService != null}");

                LogApi.Info("Resolving EventRegistry...");
                _eventRegistry = (EventRegistry)_serviceProvider.GetService(typeof(IEventRegistry));
                LogApi.Info($"EventRegistry resolved: {_eventRegistry != null}");

                LogApi.Info("Initializing HTTP components...");
                _listener = new HttpListener();

                string hostBinding = ServerIP == "0.0.0.0" ? "+" : ServerIP;
                string listenerPrefix = $"http://{hostBinding}:{Port}/";
                _listener.Prefixes.Add(listenerPrefix);

                _router = new Router();
                _requestQueue = new Queue<HttpListenerContext>();

                LogApi.Info("Initializing route registry...");
                _routeRegistry = new AutoRouteRegistry(_serviceProvider, _router);

                LogApi.Info("Initializing event publisher...");
                var eventPublisher = _serviceProvider.GetService<IEventPublisher>();
                LogApi.Info($"EventPublisher resolved: {eventPublisher != null}");
                EventPublisherAccess.Initialize(eventPublisher);

                LogApi.Info("Initializing extensions...");
                InitializeExtensions();

                LogApi.Info("Registering routes...");
                RegisterRoutes();

                LogApi.Info("ApiServer constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogApi.Error($"ApiServer constructor failed: {ex}");
                throw;
            }
        }

        private IServiceProvider CreateDefaultServiceProvider()
        {
            try
            {
                LogApi.Info("Creating default service provider...");

                var services = new ServiceCollection();

                // Register core services
                services.AddSingleton<RIMAPI_Settings>(Settings);

                var cachingService = new CachingService(Settings);
                services.AddSingleton<ICachingService>(cachingService);

                // Create and register ExtensionRegistry
                var extensionRegistry = new ExtensionRegistry();
                services.AddSingleton<ExtensionRegistry>(extensionRegistry);

                // Create instances first to avoid any DI issues
                var gameStateService = new GameStateService(cachingService);
                var sseService = new SseService(gameStateService);
                var eventRegistry = new EventRegistry(sseService);

                services.AddSingleton<ICameraStream, UdpCameraStream>();

                // Register the instances
                services.AddSingleton<SseService>(sseService);
                services.AddSingleton<IEventRegistry>(eventRegistry);
                services.AddSingleton<IEventPublisher, EventPublisher>();
                services.AddSingleton<ExtensionDocumentationService>();

                // Create DocumentationService
                services.AddSingleton<IDocumentationService, DocumentationService>();

                // Auto-discover and register all controllers and services
                RegisterDiscoveredComponents(services);

                // Register extension services
                RegisterExtensionServices(services);

                // Register controllers
                services.AddTransient<DocumentationController>();
                services.AddTransient<GameController>();
                services.AddTransient<MapController>();

                var serviceProvider = services.BuildServiceProvider();
                LogApi.Info("Default service provider created successfully");
                return serviceProvider;
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error creating default service provider: {ex}");
                throw;
            }
        }

        private void RegisterDiscoveredComponents(IServiceCollection services)
        {
            var assembly = typeof(ApiServer).Assembly;

            // Auto-register all controllers (by type, not generic)
            var controllerTypes = assembly
                .GetTypes()
                .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract && !t.IsInterface);

            foreach (var controllerType in controllerTypes)
            {
                services.AddTransient(controllerType, controllerType); // Use the non-generic overload
                LogApi.Message($"Registered controller: {controllerType.Name}");
            }

            // Auto-register all services
            var serviceTypes = assembly
                .GetTypes()
                .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Service"))
                .ToList();

            foreach (var serviceInterface in serviceTypes)
            {
                var implementationName = serviceInterface.Name.Substring(1); // Remove "I"
                var implementationType = assembly
                    .GetTypes()
                    .FirstOrDefault(t =>
                        t.Name == implementationName && !t.IsAbstract && !t.IsInterface
                    );

                if (
                    implementationType != null
                    && serviceInterface.IsAssignableFrom(implementationType)
                )
                {
                    services.AddSingleton(serviceInterface, implementationType); // Use the non-generic overload
                    LogApi.Message(
                        $"Registered service: {serviceInterface.Name} -> {implementationType.Name}"
                    );
                }
            }
        }

        private void RegisterExtensionServices(ServiceCollection services)
        {
            // First discover extensions
            _extensionRegistry.DiscoverExtensions();

            // Then register their services
            foreach (var extension in _extensionRegistry.Extensions)
            {
                try
                {
                    extension.RegisterServices(services);
                    LogApi.Info($"Registered services for extension: {extension.ExtensionName}");
                }
                catch (Exception ex)
                {
                    LogApi.Error(
                        $"Failed to register services for {extension.ExtensionName}: {ex.Message}"
                    );
                }
            }
        }

        private void InitializeExtensions()
        {
            try
            {
                LogApi.Info("Initializing extensions...");

                // Discover extensions automatically via reflection
                _extensionRegistry.DiscoverExtensions();

                // Register extension events
                _extensionRegistry.RegisterExtensionEvents(_eventRegistry);

                // Register extension endpoints - FIXED: Use each extension's own ID
                foreach (var extension in _extensionRegistry.Extensions)
                {
                    try
                    {
                        var extensionRouter = new ExtensionRouter(
                            _router,
                            extension.ExtensionId,
                            _serviceProvider
                        );
                        extension.RegisterEndpoints(extensionRouter);
                        LogApi.Info(
                            $"Successfully registered endpoints for extension '{extension.ExtensionName}' with namespace '{extension.ExtensionId}'"
                        );
                    }
                    catch (Exception ex)
                    {
                        LogApi.Error(
                            $"Failed to register endpoints for extension '{extension.ExtensionId}': {ex}"
                        );
                    }
                }

                LogApi.Info(
                    $"Extension initialization complete. Found {_extensionRegistry.Extensions.Count} extensions."
                );
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error during extension initialization: {ex}");
            }
        }

        private void RegisterRoutes()
        {
            try
            {
                // Auto-register all controllers with Route attributes
                _routeRegistry.RegisterAllRoutes();

                // Manually register routes that don't use controllers or need special handling
                RegisterManualRoutes();

                LogApi.Info("Routes registered successfully");
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error registering routes: {ex}");
                throw;
            }
        }

        private void RegisterManualRoutes()
        {
            // SSE endpoint - special handling
            _router.AddRoute(
                "GET",
                "/api/v1/events",
                async context =>
                {
                    LogApi.Info("Handling /api/v1/events");
                    await _sseService.HandleSSEConnection(context);
                }
            );
        }

        public void Start()
        {
            if (_isRunning)
                return;

            try
            {
                _listener.Start();
                _isRunning = true;

                // Start background listener
                _ = ListenForRequestsAsync();

                LogApi.Info($"API Server listening on {BaseUrl}");

                if (_extensionRegistry.Extensions.Count > 0)
                {
                    LogApi.Info(
                        $"Loaded extensions: {string.Join(", ", _extensionRegistry.Extensions.Select(e => e.ExtensionName))}"
                    );
                }
            }
            catch (Exception ex)
            {
                LogApi.Error($"Failed to start API server - {ex.Message}");
                throw;
            }
        }

        private async Task ListenForRequestsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    lock (_queueLock)
                    {
                        _requestQueue.Enqueue(context);
                    }
                }
                catch (HttpListenerException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                        LogApi.Error($"Error accepting request - {ex.Message}");
                }
            }
        }

        public void ProcessQueuedRequests()
        {
            List<HttpListenerContext> requestsToProcess;

            lock (_queueLock)
            {
                if (_requestQueue.Count == 0)
                    return;

                requestsToProcess = new List<HttpListenerContext>(_requestQueue);
                _requestQueue.Clear();
            }

            var maxRequests = Math.Min(10, requestsToProcess.Count);

            for (int i = 0; i < maxRequests; i++)
            {
                var context = requestsToProcess[i];
                _ = ProcessRequestAsync(context);
            }

            if (maxRequests < requestsToProcess.Count)
            {
                lock (_queueLock)
                {
                    for (int i = maxRequests; i < requestsToProcess.Count; i++)
                    {
                        _requestQueue.Enqueue(requestsToProcess[i]);
                    }
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            try
            {
                await _router.RouteRequest(context);
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error processing request - {ex.Message}");
                await ResponseBuilder.SendError(
                    context.Response,
                    System.Net.HttpStatusCode.InternalServerError,
                    "Internal server error"
                );
            }
        }

        public void RefreshDataCache()
        {
            // Optional: Refresh extension data caches if needed
        }

        public void ProcessBroadcastQueue()
        {
            _sseService?.ProcessTick();
        }

        public void RegisterExtension(IRimApiExtension extension)
        {
            _extensionRegistry.RegisterExtension(extension);

            if (_isRunning)
            {
                try
                {
                    // For dynamic registration, we can't easily add to existing DI container
                    // So we'll just register events and endpoints
                    extension.RegisterEvents(_eventRegistry);

                    var extensionRouter = new ExtensionRouter(
                        _router,
                        extension.ExtensionId,
                        _serviceProvider
                    );
                    extension.RegisterEndpoints(extensionRouter);

                    LogApi.Info(
                        $"Dynamically registered extension '{extension.ExtensionName}' (endpoints and events only)"
                    );
                }
                catch (Exception ex)
                {
                    LogApi.Error(
                        $"Failed to dynamically register extension '{extension.ExtensionId}': {ex}"
                    );
                }
            }
        }

        public IRimApiExtension GetExtension(string extensionId)
        {
            return _extensionRegistry.Extensions.FirstOrDefault(e => e.ExtensionId == extensionId);
        }

        public IReadOnlyList<IRimApiExtension> GetExtensions()
        {
            return _extensionRegistry.Extensions;
        }

        public void RegisterController<TController>()
            where TController : class
        {
            _routeRegistry.RegisterController<TController>();
        }

        public void RefreshRoutes()
        {
            _router.ClearRoutes();
            RegisterRoutes();
            _extensionRegistry.InitializeExtensionEndpoints(
                new ExtensionRouter(_router, "core", _serviceProvider)
            );
            LogApi.Info("Routes and extension endpoints refreshed");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _isRunning = false;
            _disposed = true;

            try
            {
                LogApi.Info("Disposing API server...");

                if (_listener?.IsListening == true)
                {
                    _listener.Stop();
                }

                _listener?.Close();
                _sseService?.Dispose();

                lock (_queueLock)
                {
                    while (_requestQueue.Count > 0)
                    {
                        var context = _requestQueue.Dequeue();
                        try
                        {
                            context.Response?.Close();
                        }
                        catch { }
                    }
                }

                LogApi.Info("API server disposed successfully");
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error disposing server - {ex.Message}");
            }
        }

        // Add this method to your ApiServer class
        public void DebugEndpoints()
        {
            try
            {
                LogApi.Info("=== RIMAPI ENDPOINT DEBUG INFORMATION ===");

                // 1. Core routes from AutoRouteRegistry
                LogApi.Info("CORE ROUTES:");
                var coreRoutes = _routeRegistry.GetRegisteredRoutesInfo();
                foreach (var route in coreRoutes)
                {
                    LogApi.Info(
                        $"  {route.HttpMethod} {route.Pattern} -> {route.Controller}.{route.Method}"
                    );
                }

                // 2. Extension routes
                LogApi.Info("EXTENSION ROUTES:");
                var extensions = _extensionRegistry.GetExtensions();
                foreach (var extension in extensions)
                {
                    LogApi.Info(
                        $"  Extension: {extension.ExtensionName} ({extension.ExtensionId})"
                    );
                    // Note: You might need to track extension routes separately
                }

                // 3. Manual routes (SSE, etc.)
                LogApi.Info("MANUAL ROUTES:");
                // You'll need to add tracking for manual routes or inspect the router directly

                // 4. SSE Events
                LogApi.Info("SSE EVENTS:");
                var sseEvents = _sseService?.GetRegisteredEventTypes() ?? new List<string>();
                foreach (var eventType in sseEvents)
                {
                    LogApi.Info($"  {eventType}");
                }

                LogApi.Info($"Total Core Routes: {coreRoutes.Count()}");
                LogApi.Info($"Total Extensions: {extensions.Count}");
                LogApi.Info($"Total SSE Events: {sseEvents.Count}");
                LogApi.Info("=== END DEBUG INFORMATION ===");
            }
            catch (Exception ex)
            {
                LogApi.Error($"Error in DebugEndpoints: {ex}");
            }
        }
    }
}
