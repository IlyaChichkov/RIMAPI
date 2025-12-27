using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks; // Required for Task
using Newtonsoft.Json;
using RIMAPI.Services;
using Verse;

namespace RIMAPI.Core
{
    public class SseService : IDisposable
    {
        private readonly IGameStateService _gameStateService;
        private readonly List<SseClient> _connectedClients;
        private readonly object _clientsLock = new object();
        private bool _disposed = false;
        private int _lastBroadcastTick;
        private readonly Queue<SseEvent> _broadcastQueue;
        private readonly object _queueLock = new object();
        private readonly HashSet<string> _registeredEventTypes;
        private readonly object _eventsLock = new object();

        public SseService(IGameStateService gameStateService)
        {
            _gameStateService = gameStateService;
            _connectedClients = new List<SseClient>();
            _broadcastQueue = new Queue<SseEvent>();
            _registeredEventTypes = new HashSet<string>();
            _lastBroadcastTick = Current.ProgramState == ProgramState.Playing ? Find.TickManager.TicksGame : 0;
            RegisterCoreEventTypes();
        }

        private void RegisterCoreEventTypes()
        {
            lock (_eventsLock)
            {
                _registeredEventTypes.Add("connected");
                _registeredEventTypes.Add("gameState");
                _registeredEventTypes.Add("gameUpdate");
                _registeredEventTypes.Add("heartbeat");
                _registeredEventTypes.Add("error");
            }
        }

        public void RegisterEventType(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
            {
                LogApi.Warning("[SSE] Attempted to register null or empty event type");
                return;
            }

            lock (_eventsLock)
            {
                if (_registeredEventTypes.Contains(eventType))
                {
                    LogApi.Warning($"[SSE] Event type '{eventType}' is already registered");
                    return;
                }

                _registeredEventTypes.Add(eventType);
                LogApi.Info($"[SSE] Registered SSE event type: {eventType}");
            }
        }

        // --- UPDATED: Method signature changed from void to async Task ---
        public async Task HandleSSEConnection(HttpListenerContext context)
        {
            if (_disposed)
            {
                context.Response.StatusCode = 503;
                context.Response.Close();
                return;
            }

            var response = context.Response;
            var client = new SseClient(response);

            try
            {
                // 1. Set Headers
                response.StatusCode = 200;
                response.ContentType = "text/event-stream";
                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Connection", "keep-alive");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.SendChunked = true;

                // 2. Register Client
                lock (_clientsLock)
                {
                    _connectedClients.Add(client);
                }

                LogApi.Info($"[SSE] Connection established. Total clients: {_connectedClients.Count}");

                // 3. Send Initial Events
                SendEventToClient(client, "connected", new
                {
                    message = "SSE connection established",
                    timestamp = DateTime.UtcNow,
                    registeredEvents = GetRegisteredEventTypes(),
                });

                var gameStateResult = _gameStateService.GetGameState();
                if (gameStateResult.Success)
                    SendEventToClient(client, "gameState", gameStateResult.Data);
                else
                    SendEventToClient(client, "error", new { message = "Failed to get initial game state", errors = gameStateResult.Errors });

                // 4. CRITICAL: Wait here until the client disconnects.
                // This keeps the HttpListenerContext alive.
                await client.WaitForDisconnect();
            }
            catch (Exception ex)
            {
                // Only log real errors, not normal disconnections
                if (!(ex is HttpListenerException))
                    LogApi.Error($"[SSE] Connection error - {ex.Message}");
            }
            finally
            {
                // Ensure cleanup happens when the loop breaks
                RemoveClient(client);
            }
        }

        public void BroadcastEvent(string eventType, object data)
        {
            if (_disposed) return;

            // Optional: Strict check removed for stability, or keep it if you want strict typing
            // if (!IsEventTypeRegistered(eventType)) ...

            lock (_queueLock)
            {
                _broadcastQueue.Enqueue(new SseEvent { Type = eventType, Data = data });
            }
        }

        public void ProcessTick()
        {
            if (_disposed) return;
            ProcessBroadcastQueue();
            // CheckClientConnections(); // Removed: Redundant now that we handle lifetime via async/await
            SendHeartbeatsIfNeeded();
        }

        private void ProcessBroadcastQueue()
        {
            if (_broadcastQueue.Count == 0) return;

            List<SseEvent> eventsToProcess;
            lock (_queueLock)
            {
                eventsToProcess = new List<SseEvent>(_broadcastQueue);
                _broadcastQueue.Clear();
            }

            foreach (var sseEvent in eventsToProcess)
            {
                BroadcastEventInternal(sseEvent.Type, sseEvent.Data);
            }
        }

        private void SendHeartbeatsIfNeeded()
        {
            var currentTick = Find.TickManager?.TicksGame ?? 0;
            if (currentTick - _lastBroadcastTick < 180) return;

            BroadcastEventInternal("heartbeat", new { timestamp = DateTime.UtcNow, tick = currentTick });
            _lastBroadcastTick = currentTick;
        }

        private void BroadcastEventInternal(string eventType, object data)
        {
            List<SseClient> currentClients;
            lock (_clientsLock)
            {
                currentClients = new List<SseClient>(_connectedClients);
            }

            foreach (var client in currentClients)
            {
                // If client is dead, just signal it (the awaiter in HandleSSEConnection will wake up)
                if (!client.IsConnected)
                {
                    client.SignalDisconnect();
                    continue;
                }
                SendEventToClient(client, eventType, data);
            }
        }

        private void SendEventToClient(SseClient client, string eventType, object data)
        {
            if (client == null || !client.IsConnected) return;

            try
            {
                string json = data is string s ? s : JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                });

                string message = $"event: {eventType}\ndata: {json}\n\n";
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);

                lock (client.SendLock)
                {
                    if (!client.IsConnected) return;
                    client.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    client.Response.OutputStream.Flush();
                }
                client.UpdateLastActivity();
            }
            catch
            {
                // If write fails, signal disconnect. 
                // This wakes up HandleSSEConnection, which triggers RemoveClient in finally block.
                client.SignalDisconnect();
            }
        }

        private void RemoveClient(SseClient client)
        {
            if (client == null) return;
            lock (_clientsLock)
            {
                if (_connectedClients.Contains(client))
                {
                    _connectedClients.Remove(client);
                    LogApi.Info($"[SSE] Client disconnected. Remaining: {_connectedClients.Count}");
                }
            }
            client.SignalDisconnect(); // Ensure task completes
            try { client.Response.Close(); } catch { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            List<SseClient> clientsToDispose;
            lock (_clientsLock)
            {
                clientsToDispose = new List<SseClient>(_connectedClients);
                _connectedClients.Clear();
            }

            foreach (var client in clientsToDispose)
            {
                client.SignalDisconnect();
                try { client.Response.Close(); } catch { }
            }
        }

        public IReadOnlyList<string> GetRegisteredEventTypes()
        {
            lock (_eventsLock) return new List<string>(_registeredEventTypes);
        }

        private class SseEvent
        {
            public string Type { get; set; }
            public object Data { get; set; }
        }

        // --- UPDATED SseClient with Async Support ---
        private class SseClient
        {
            public HttpListenerResponse Response { get; }
            public bool IsConnected { get; private set; }
            public DateTime LastActivity { get; private set; }
            public object SendLock { get; } = new object();

            // This allows HandleSSEConnection to "await" the disconnection
            private readonly TaskCompletionSource<bool> _disconnectTcs = new TaskCompletionSource<bool>();

            public SseClient(HttpListenerResponse response)
            {
                Response = response;
                IsConnected = true;
                LastActivity = DateTime.UtcNow;
            }

            public Task WaitForDisconnect()
            {
                return _disconnectTcs.Task;
            }

            public void SignalDisconnect()
            {
                IsConnected = false;
                _disconnectTcs.TrySetResult(true);
            }

            public void UpdateLastActivity()
            {
                LastActivity = DateTime.UtcNow;
            }
        }
    }
}