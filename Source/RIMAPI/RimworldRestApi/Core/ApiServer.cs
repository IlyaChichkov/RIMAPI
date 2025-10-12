using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RimworldRestApi.Services;
using RimworldRestApi.Controllers;
using Verse;

namespace RimworldRestApi.Core
{
    public class ApiServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Router _router;
        private readonly SseService _sseService;
        private readonly Queue<HttpListenerContext> _requestQueue;
        private readonly object _queueLock = new object();
        private bool _isRunning;
        private readonly IGameDataService _gameDataService;
        private bool _disposed = false;

        public int Port { get; private set; }
        public string BaseUrl => $"http://localhost:{Port}/";
        private readonly ExtensionRegistry _extensionRegistry;

        public ApiServer(int port, IGameDataService gameDataService)
        {
            Port = port;
            _gameDataService = gameDataService;
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _router = new Router();
            _requestQueue = new Queue<HttpListenerContext>();
            _sseService = new SseService(_gameDataService);
            _extensionRegistry = new ExtensionRegistry(); // NEW

            RegisterRoutes();
            RegisterExtensions(); // NEW
        }



        private void RegisterRoutes()
        {
            try
            {
                #region Basic
                // Version endpoint - simple static route
                _router.AddRoute("GET", "/api/v1/version", async context =>
                {
                    Log.Message("RIMAPI: Handling /api/v1/version");
                    await new VersionController().GetVersion(context);
                });

                // Game state endpoints
                _router.AddRoute("GET", "/api/v1/game/state", async context =>
                {
                    Log.Message("RIMAPI: Handling /api/v1/game/state");
                    await new GameController(_gameDataService).GetGameState(context);
                });
                #endregion

                #region Map
                _router.AddRoute("GET", "/api/v1/map", async context =>
                {
                    Log.Message("RIMAPI: Handling /api/v1/maps");
                    await new MapController(_gameDataService).GetMaps(context);
                });

                _router.AddRoute("GET", "/api/v1/map/power/info", async context =>
                {
                    Log.Message("RIMAPI: Handling /api/v1/maps");
                    await new MapController(_gameDataService).GetMapPowerInfo(context);
                });
                #endregion

                #region Colonists
                _router.AddRoute("GET", "/api/v1/colonists", async context =>
                {
                    await new GameController(_gameDataService).GetColonists(context);
                });

                _router.AddRoute("GET", "/api/v1/colonist", async context =>
                {
                    await new GameController(_gameDataService).GetColonistDetailed(context);
                });

                _router.AddRoute("GET", "/api/v1/colonists/detailed", async context =>
                {
                    await new GameController(_gameDataService).GetColonistsDetailed(context);
                });

                _router.AddRoute("GET", "/api/v1/colonist/detailed", async context =>
                {
                    await new GameController(_gameDataService).GetColonistDetailed(context);
                });

                _router.AddRoute("GET", "/api/v1/colonist/inventory", async context =>
                {
                    await new GameController(_gameDataService).GetColonistInventory(context);
                });
                #endregion

                _router.AddRoute("GET", "/api/v1/item/image", async context =>
                {
                    await new GameController(_gameDataService).GetItemImage(context);
                });

                _router.AddRoute("GET", "/api/v1/colonist/body/image", async context =>
                {
                    await new GameController(_gameDataService).GetColonistBody(context);
                });

                _router.AddRoute("GET", "/api/v1/datetime", async context =>
                {
                    await new GameController(_gameDataService).GetMapTime(context);
                });

                // Server-Sent Events endpoint for real-time updates
                _router.AddRoute("GET", "/api/v1/events", async context =>
                {
                    Log.Message("RIMAPI: Handling /api/v1/events");
                    await _sseService.HandleSSEConnection(context);
                });

                Log.Message("RIMAPI: Routes registered successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error registering routes: {ex}");
                throw;
            }
        }

        // ... (rest of the class remains same as Phase 1)
        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _listener.Start();
                _isRunning = true;

                // Start background listener
                _ = ListenForRequestsAsync();

                Log.Message($"RIMAPI: API Server listening on {BaseUrl}");
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Failed to start API server - {ex.Message}");
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
                        Log.Error($"RIMAPI: Error accepting request - {ex.Message}");
                }
            }
        }

        public void ProcessQueuedRequests()
        {
            List<HttpListenerContext> requestsToProcess;

            lock (_queueLock)
            {
                if (_requestQueue.Count == 0) return;

                requestsToProcess = new List<HttpListenerContext>(_requestQueue);
                _requestQueue.Clear();
            }

            // Process up to 10 requests per tick to avoid blocking
            var maxRequests = Math.Min(10, requestsToProcess.Count);

            for (int i = 0; i < maxRequests; i++)
            {
                var context = requestsToProcess[i];
                _ = ProcessRequestAsync(context);
            }

            // Requeue any unprocessed requests
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
                Log.Error($"RIMAPI: Error processing request - {ex.Message}");
                await ResponseBuilder.Error(context.Response,
                    System.Net.HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        public void RefreshDataCache()
        {
            // Refresh game data cache and notify WebSocket clients
            _gameDataService.RefreshCache();
            _sseService.BroadcastGameUpdate();
        }

        public void ProcessBroadcastQueue()
        {
            _sseService?.ProcessBroadcastQueue();
        }

        private void RegisterExtensions()
        {
            try
            {
                Log.Message("RIMAPI: Initializing extensions...");

                // Discover extensions automatically via reflection
                _extensionRegistry.DiscoverExtensions();

                // Register endpoints for each extension
                var extensions = _extensionRegistry.GetExtensions();
                foreach (var extension in extensions)
                {
                    try
                    {
                        var extensionRouter = new ExtensionRouter(_router, extension.ExtensionId);
                        extension.RegisterEndpoints(extensionRouter);
                        Log.Message($"RIMAPI: Successfully registered endpoints for extension '{extension.ExtensionName}'");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"RIMAPI: Failed to register endpoints for extension '{extension.ExtensionId}': {ex}");
                    }
                }

                Log.Message($"RIMAPI: Extension registration complete. {extensions.Count} extensions loaded.");
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error during extension registration: {ex}");
            }
        }

        // Add method for manual extension registration (for mods that can't use reflection)
        public void RegisterExtension(IRimApiExtension extension)
        {
            _extensionRegistry.RegisterExtension(extension);

            // If server is already running, register endpoints immediately
            if (_isRunning)
            {
                try
                {
                    var extensionRouter = new ExtensionRouter(_router, extension.ExtensionId);
                    extension.RegisterEndpoints(extensionRouter);
                    Log.Message($"RIMAPI: Dynamically registered endpoints for extension '{extension.ExtensionName}'");
                }
                catch (Exception ex)
                {
                    Log.Error($"RIMAPI: Failed to dynamically register endpoints for extension '{extension.ExtensionId}': {ex}");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _isRunning = false;
            _disposed = true;

            try
            {
                Log.Message("RIMAPI: Disposing API server...");

                // Stop listening first
                if (_listener?.IsListening == true)
                {
                    _listener.Stop();
                }

                _listener?.Close();
                _sseService?.Dispose();

                // Clear request queue
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

                Log.Message("RIMAPI: API server disposed successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error disposing server - {ex.Message}");
            }
        }
    }
}