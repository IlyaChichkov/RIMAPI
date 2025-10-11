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

        public ApiServer(int port, IGameDataService gameDataService)
        {
            Port = port;
            _gameDataService = gameDataService;
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _router = new Router();
            _requestQueue = new Queue<HttpListenerContext>();
            _sseService = new SseService(_gameDataService);

            RegisterRoutes();
        }


        private void RegisterRoutes()
        {
            try
            {
                // Clear any existing routes
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

                // Server-Sent Events endpoint for real-time updates
                _router.AddRoute("GET", "/api/v1/events", async context =>
                {
                    Log.Message("RIMAPI: Handling /api/v1/events");
                    await _sseService.HandleSSEConnection(context);
                });

                // Note: WebSocket broadcasting is disabled due to Mono limitations
                // Use SSE instead for real-time updates
                // _router.AddRoute("GET", "/api/v1/events/stream", async context =>
                // {
                //     Log.Message("RIMAPI: Handling /api/v1/events/stream");
                //     await _webSocketManager.HandleWebSocketRequest(context);
                // });

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