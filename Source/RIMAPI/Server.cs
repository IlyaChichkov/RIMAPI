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

                // Process the request SYNCHRONOUSLY in the callback
                Log.Message("[RIMAPI] Processing request synchronously");
                Handle(ctx);
                Log.Message("[RIMAPI] Synchronous processing completed");
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


        private static void Handle(HttpListenerContext ctx)
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
                    // Send response (existing code remains the same)
                    data = Encoding.UTF8.GetBytes(json);
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.ContentEncoding = Encoding.UTF8;
                    ctx.Response.ContentLength64 = data.Length;
                    ctx.Response.OutputStream.Write(data, 0, data.Length);
                    ctx.Response.OutputStream.Close();
                    return;
                }

                // Process the request
                json = ProcessRequest(ctx, path);

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
                if (path == "map/tiles")
                {
                    return _apiHandler.GetAllMapTiles();
                }
                if (path == "ping")
                {
                    return _apiHandler.Ping();
                }
                if (path == "mods")
                {
                    return _apiHandler.ModsList();
                }

                if (path.StartsWith("map/tiles/"))
                {
                    var parts = path.Split(new char[] { '/' });
                    if (parts.Length >= 5 && int.TryParse(parts[3], out int tx) && int.TryParse(parts[4], out int ty))
                        return _apiHandler.GetMapTile(tx, ty);
                    return "{}";
                }

                if (path.StartsWith("colonists/"))
                {
                    var parts = path.Split(new char[] { '/' });
                    string idPart = parts[parts.Length - 1]; // Manual .Last() equivalent
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
                // Read request body
                string requestBody;
                using (var reader = new System.IO.StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }

                // Route POST requests
                if (path == "zones/create")
                {
                    return _apiHandler.CreateZone(requestBody);
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
