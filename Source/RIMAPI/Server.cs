using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Verse;

namespace RIMAPI
{
    public static class Server
    {
        private static HttpListener _listener;
        private static int Port => RIMAPI_Mod.Settings?.serverPort ?? 8765;
        private static readonly string Prefix = $"http://localhost:{Port}/";
        private static Thread _thread;
        private static readonly ApiHandler _apiHandler;

        // Cached JSON responses refreshed on game load
        private static string _cacheColonyInfo = "{}";
        private static string _cacheLetters = "[]";
        private static string _cacheColonists = "[]";
        private static readonly Dictionary<int, string> _cacheColonistsById = new Dictionary<int, string>();
        private static readonly object _cacheLock = new object();

        // Request queue for main thread processing
        private static readonly Queue<HttpListenerContext> _requestQueue = new Queue<HttpListenerContext>();
        private static readonly object _queueLock = new object();

        static Server()
        {
            _apiHandler = new ApiHandler();
        }

        public static void Stop()
        {
            _listener?.Close();
            _listener = null;
            _thread = null;
        }

        public static void RefreshCache()
        {
            lock (_cacheLock)
            {
                try
                {
                    // Refresh basic caches
                    _cacheColonyInfo = _apiHandler.GetColonyInfo();
                    _cacheLetters = _apiHandler.GetLetters();
                    _cacheColonists = _apiHandler.GetColonists();

                    _cacheColonistsById.Clear();
                    var colonists = (Find.CurrentMap?.mapPawns?.FreeColonists ?? Enumerable.Empty<Pawn>()).ToList();

                    foreach (var pawn in colonists)
                    {
                        _cacheColonistsById[pawn.thingIDNumber] = _apiHandler.GetColonistById(pawn.thingIDNumber.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[RIMAPI] Critical error during cache refresh: {ex}");
                }
            }
        }

        public static void Start()
        {
            if (_listener != null) return;

            _listener = new HttpListener();
            _listener.Prefixes.Add(Prefix);
            _listener.Start();

            _thread = new Thread(Loop) { IsBackground = true, Name = "RIMAPI_Server" };
            _thread.Start();
            Log.Message($"[RIMAPI] REST API listening on {Prefix}");
            Log.Message($"[RIMAPI] Server thread state: {_thread.ThreadState}");
        }

        private static void Loop()
        {
            Log.Message("[RIMAPI] Starting async listener");

            try
            {
                // Start the async listening
                _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
                Log.Message("[RIMAPI] Async listening started successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Failed to start async listening: {ex}");
            }
        }

        private static void GetContextCallback(IAsyncResult result)
        {
            Log.Message("[RIMAPI] GetContextCallback invoked");

            try
            {
                if (_listener?.IsListening != true)
                {
                    Log.Message("[RIMAPI] Listener not active, exiting callback");
                    return;
                }

                // Get the context
                var ctx = _listener.EndGetContext(result);
                Log.Message($"[RIMAPI] Request received: {ctx.Request.Url}");

                // Immediately start listening for the next request
                _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);

                // Queue the request for main thread processing instead of handling immediately
                lock (_queueLock)
                {
                    _requestQueue.Enqueue(ctx);
                }
                Log.Message("[RIMAPI] Request queued for main thread processing");
            }
            catch (HttpListenerException ex)
            {
                Log.Message($"[RIMAPI] HttpListenerException in callback: {ex.ErrorCode}");
            }
            catch (ObjectDisposedException)
            {
                Log.Message("[RIMAPI] Listener was disposed");
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Error in callback: {ex}");

                // Try to restart listening if still valid
                if (_listener?.IsListening == true)
                {
                    try
                    {
                        _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
                    }
                    catch (Exception ex2)
                    {
                        Log.Error($"[RIMAPI] Failed to restart listening: {ex2}");
                    }
                }
            }
        }

        // New method to process queued requests from main thread
        public static void ProcessQueuedRequests()
        {
            if (_requestQueue.Count == 0) return;

            List<HttpListenerContext> requestsToProcess;
            lock (_queueLock)
            {
                // Take up to 5 requests per tick to avoid performance issues
                requestsToProcess = new List<HttpListenerContext>();
                for (int i = 0; i < 5 && _requestQueue.Count > 0; i++)
                {
                    requestsToProcess.Add(_requestQueue.Dequeue());
                }
            }

            foreach (var ctx in requestsToProcess)
            {
                try
                {
                    HandleRequestOnMainThread(ctx);
                }
                catch (Exception ex)
                {
                    Log.Error($"[RIMAPI] Error processing queued request: {ex}");
                }
            }
        }

        private static void HandleRequestOnMainThread(HttpListenerContext ctx)
        {
            string json = "{}";
            byte[] data = null;
            string path = ctx.Request.Url.AbsolutePath.Trim(new char[] { '/' }).ToLowerInvariant();

            try
            {
                // Handle POST requests
                if (ctx.Request.HttpMethod == "POST")
                {
                    json = ProcessPostRequest(ctx, path);
                }
                else
                {
                    // Process GET request directly on main thread
                    json = ProcessRequest(ctx, path);
                }

                // Send response
                data = Encoding.UTF8.GetBytes(json);
                ctx.Response.ContentType = "application/json";
                ctx.Response.ContentEncoding = Encoding.UTF8;
                ctx.Response.ContentLength64 = data.Length;
                ctx.Response.OutputStream.Write(data, 0, data.Length);
                ctx.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] Handle error for {path}: {ex}");

                try
                {
                    string errorJson = "{\"error\": \"Internal server error\"}";
                    data = Encoding.UTF8.GetBytes(errorJson);
                    ctx.Response.StatusCode = 500;
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentEncoding = Encoding.UTF8;
                    ctx.Response.ContentLength64 = data.Length;
                    ctx.Response.OutputStream.Write(data, 0, data.Length);
                    ctx.Response.OutputStream.Close();
                }
                catch (Exception ex2)
                {
                    Log.Error($"[RIMAPI] Failed to send error response: {ex2}");
                }
            }
        }

        // Remove MainThreadDispatcher.Invoke from these methods since they're already on main thread
        private static string ProcessRequest(HttpListenerContext ctx, string path)
        {
            Log.Message($"[RIMAPI] ProcessRequest: {path}");

            try
            {
                // Handle cached endpoints
                if (path == "colony")
                {
                    return _cacheColonyInfo;
                }
                if (path == "letters")
                {
                    return _cacheLetters;
                }
                if (path == "colonists")
                {
                    return _cacheColonists;
                }
                if (path == "map/info")
                {
                    return _apiHandler.GetMapInfo();
                }
                if (path == "map/plants")
                {
                    return _apiHandler.GetMapPlants();
                }
                if (path == "map/terrain/hard")
                {
                    return _apiHandler.GetHardTerrainTiles();
                }
                if (path == "ping")
                {
                    return _apiHandler.Ping();
                }
                if (path == "mods")
                {
                    return _apiHandler.ModsList();
                }

                // NEW GET ENDPOINTS - Add these
                if (path == "items/spawnable")
                {
                    return _apiHandler.GetSpawnableItems();
                }

                // NEW RESOURCE MANAGEMENT ENDPOINTS
                if (path == "items/forbidden")
                {
                    return _apiHandler.GetForbiddenItems();
                }
                if (path == "resources/summary")
                {
                    return _apiHandler.GetResourceSummary();
                }

                if (path == "camera")
                {
                    return _apiHandler.GetCameraInfo();
                }
                if (path == "camera/zoom")
                {
                    return _apiHandler.GetCameraZoom();
                }
                if (path == "power")
                {
                    return _apiHandler.GetPowerInfo();
                }
                if (path == "buildings")
                {
                    return _apiHandler.GetColonyBuildings();
                }

                if (path.StartsWith("map/tiles/"))
                {
                    var parts = path.Split(new char[] { '/' });
                    if (parts.Length >= 5 && int.TryParse(parts[3], out int tx) && int.TryParse(parts[4], out int ty))
                        return _apiHandler.GetMapTile(tx, ty);
                    return "{}";
                }
                if (path.StartsWith("item/image/"))
                {
                    var parts = path.Split(new char[] { '/' });
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int thingId))
                    {
                        return _apiHandler.GetItemImage(thingId);
                    }
                    return "{\"error\": \"Invalid item ID\"}";
                }


                if (path.StartsWith("colonists/"))
                {
                    var parts = path.Split(new char[] { '/' });
                    string idPart = parts[parts.Length - 1];
                    if (int.TryParse(idPart, out int cid) && _cacheColonistsById.TryGetValue(cid, out string json))
                        return json;
                    return "{}";
                }
                ctx.Response.StatusCode = 404;
                return $"\"Invalid request\"";
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] ProcessRequest error for {path}: {ex}");
                throw;
            }
        }

        private static string ProcessPostRequest(HttpListenerContext ctx, string path)
        {
            try
            {
                string requestBody;
                using (var reader = new System.IO.StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }

                if (path == "zones/create")
                {
                    return _apiHandler.CreateZone(requestBody);
                }
                if (path == "camera/move")
                {
                    return _apiHandler.MoveCamera(requestBody);
                }
                if (path == "camera/zoom")
                {
                    return _apiHandler.SetCameraZoom(requestBody);
                }
                if (path == "item/set-forbidden")
                {
                    return _apiHandler.SetItemForbidden(requestBody);
                }
                if (path == "items/set-multiple-forbidden")
                {
                    return _apiHandler.SetMultipleItemsForbidden(requestBody);
                }
                if (path == "pawn/spawn")
                {
                    return _apiHandler.SpawnPawn(requestBody);
                }
                if (path == "item/spawn")
                {
                    return _apiHandler.SpawnItem(requestBody);
                }
                if (path == "event/trigger")
                {
                    return _apiHandler.TriggerEvent(requestBody);
                }
                if (path == "zones/create")
                {
                    return _apiHandler.CreateStorageZone(requestBody);
                }
                if (path == "zones/create/basic")
                {
                    return _apiHandler.CreateBasicStorageZone(requestBody);
                }

                ctx.Response.StatusCode = 404;
                return "{\"error\": \"POST endpoint not found\"}";
            }
            catch (Exception ex)
            {
                Log.Error($"[RIMAPI] ProcessPostRequest error for {path}: {ex}");
                ctx.Response.StatusCode = 500;
                return "{\"error\": \"Internal server error processing POST request\"}";
            }
        }
    }
}