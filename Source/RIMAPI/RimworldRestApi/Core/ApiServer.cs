using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Verse;

namespace RimworldRestApi.Core
{
    public class ApiServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Router _router;
        private readonly WebSocketManager _webSocketManager;
        private readonly Queue<HttpListenerContext> _requestQueue;
        private readonly object _queueLock = new object();
        private bool _isRunning;
        private readonly IGameDataService _gameDataService;

        public int Port { get; private set; }
        public string BaseUrl => $"http://localhost:{Port}/";

        public ApiServer(int port = 8765)
        {
            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _router = new Router();
            _requestQueue = new Queue<HttpListenerContext>();
            _gameDataService = new GameDataService();
            _webSocketManager = new WebSocketManager();

            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Version endpoint
            _router.AddRoute("GET", "/api/v1/version", context =>
                new VersionController().GetVersion(context));

            // Game state endpoints
            _router.AddRoute("GET", "/api/v1/game/state", context =>
                new GameController(_gameDataService).GetGameState(context));

            _router.AddRoute("GET", "/api/v1/colonists", context =>
                new ColonistsController(_gameDataService).GetColonists(context));

            _router.AddRoute("GET", "/api/v1/colonists/{id}", context =>
                new ColonistsController(_gameDataService).GetColonist(context));

            // WebSocket upgrade endpoint
            _router.AddRoute("GET", "/api/v1/events/stream", context =>
                _webSocketManager.HandleWebSocketRequest(context));
        }

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
            _webSocketManager.BroadcastGameUpdate();
        }

        public void Dispose()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
                _webSocketManager?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error disposing server - {ex.Message}");
            }
        }
    }
}