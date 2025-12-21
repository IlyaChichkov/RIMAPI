using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
            if (Current.ProgramState == ProgramState.Playing)
            {
                _lastBroadcastTick = Find.TickManager.TicksGame;
            }
            else
            {
                _lastBroadcastTick = 0;
            }

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

        // Synchronous connection handling - no async!
        public void HandleSSEConnection(HttpListenerContext context)
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
                // Set SSE headers
                response.StatusCode = 200;
                response.ContentType = "text/event-stream";
                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Connection", "keep-alive");
                response.SendChunked = true;

                // Add client to connected list
                lock (_clientsLock)
                {
                    _connectedClients.Add(client);
                }

                LogApi.Info(
                    $"[SSE] Connection established. Total clients: {_connectedClients.Count}"
                );

                // Send initial connection message
                SendEventToClient(
                    client,
                    "connected",
                    new
                    {
                        message = "SSE connection established",
                        timestamp = DateTime.UtcNow,
                        registeredEvents = GetRegisteredEventTypes(),
                    }
                );

                // Send initial game state
                var gameStateResult = _gameStateService.GetGameState();
                if (gameStateResult.Success)
                {
                    SendEventToClient(client, "gameState", gameStateResult.Data);
                }
                else
                {
                    SendEventToClient(
                        client,
                        "error",
                        new
                        {
                            message = "Failed to get initial game state",
                            errors = gameStateResult.Errors,
                        }
                    );
                }

                // Note: We don't keep the connection open here - that's handled by the HTTP listener
                // The client will remain in our list and receive broadcasts
            }
            catch (Exception ex)
            {
                LogApi.Error($"[SSE] Connection setup error - {ex}");
                RemoveClient(client);
            }
        }

        public void BroadcastEvent(string eventType, object data)
        {
            if (_disposed)
                return;

            if (!IsEventTypeRegistered(eventType))
            {
                LogApi.Warning(
                    $"[SSE] Attempted to broadcast unregistered event type: {eventType}"
                );
                return;
            }

            lock (_queueLock)
            {
                _broadcastQueue.Enqueue(new SseEvent { Type = eventType, Data = data });
            }

            LogApi.Message($"[SSE] Queued event '{eventType}' for broadcast", LoggingLevels.DEBUG);
        }

        // Call this from your main game tick
        public void ProcessTick()
        {
            if (_disposed)
                return;

            ProcessBroadcastQueue();
            CheckClientConnections();
            SendHeartbeatsIfNeeded();
        }

        private void ProcessBroadcastQueue()
        {
            if (_broadcastQueue.Count == 0)
                return;

            List<SseEvent> eventsToProcess;
            lock (_queueLock)
            {
                eventsToProcess = new List<SseEvent>(_broadcastQueue);
                _broadcastQueue.Clear();
            }

            foreach (var sseEvent in eventsToProcess)
            {
                try
                {
                    BroadcastEventInternal(sseEvent.Type, sseEvent.Data);
                }
                catch (Exception ex)
                {
                    LogApi.Error($"[SSE] Error processing broadcast '{sseEvent.Type}' - {ex}");
                }
            }
        }

        private void CheckClientConnections()
        {
            List<SseClient> clientsToRemove = new List<SseClient>();
            List<SseClient> currentClients;

            lock (_clientsLock)
            {
                currentClients = new List<SseClient>(_connectedClients);
            }

            foreach (var client in currentClients)
            {
                try
                {
                    // Test connection by trying to write a comment (safe operation)
                    if (!TestClientConnection(client))
                    {
                        clientsToRemove.Add(client);
                    }
                }
                catch
                {
                    clientsToRemove.Add(client);
                }
            }

            if (clientsToRemove.Count > 0)
            {
                RemoveClients(clientsToRemove);
            }
        }

        private void SendHeartbeatsIfNeeded()
        {
            var currentTick = Find.TickManager?.TicksGame ?? 0;
            if (currentTick - _lastBroadcastTick < 180)
                return; // Every ~3 seconds

            try
            {
                BroadcastEventInternal(
                    "heartbeat",
                    new { timestamp = DateTime.UtcNow, tick = currentTick }
                );

                _lastBroadcastTick = currentTick;
            }
            catch (Exception ex)
            {
                LogApi.Error($"[SSE] Error sending heartbeat - {ex}");
            }
        }

        public void BroadcastGameUpdate()
        {
            if (_disposed)
                return;

            var currentTick = Find.TickManager?.TicksGame ?? 0;
            if (currentTick - _lastBroadcastTick < 60)
                return;

            try
            {
                var gameStateResult = _gameStateService.GetGameState();
                if (gameStateResult.Success)
                {
                    BroadcastEvent("gameUpdate", gameStateResult.Data);
                }
                else
                {
                    BroadcastEvent(
                        "error",
                        new
                        {
                            message = "Failed to get game state update",
                            errors = gameStateResult.Errors,
                        }
                    );
                }

                _lastBroadcastTick = currentTick;
            }
            catch (Exception ex)
            {
                LogApi.Error($"[SSE] Error preparing game update broadcast - {ex}");
                BroadcastEvent(
                    "error",
                    new { message = "Error preparing game update", error = ex.Message }
                );
            }
        }

        private void BroadcastEventInternal(string eventType, object data)
        {
            List<SseClient> clientsToRemove = new List<SseClient>();
            List<SseClient> currentClients;

            lock (_clientsLock)
            {
                currentClients = new List<SseClient>(_connectedClients);
            }

            foreach (var client in currentClients)
            {
                try
                {
                    if (!client.IsConnected)
                    {
                        clientsToRemove.Add(client);
                        continue;
                    }

                    SendEventToClient(client, eventType, data);
                }
                catch (Exception ex)
                {
                    LogApi.Info(
                        $"[SSE] Error sending to client for event '{eventType}' - {ex.Message}"
                    );
                    clientsToRemove.Add(client);
                }
            }

            if (clientsToRemove.Count > 0)
            {
                RemoveClients(clientsToRemove);
            }
        }

        private void SendEventToClient(SseClient client, string eventType, object data)
        {
            if (client == null || !client.IsConnected)
                return;

            string json;
            try
            {
                json = data is string s
                    ? s
                    : JsonConvert.SerializeObject(
                        data,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.None,
                        }
                    );
            }
            catch (Exception ex)
            {
                LogApi.Error($"[SSE] Failed to serialize data for event '{eventType}': {ex}");
                return;
            }

            string message = $"event: {eventType}\ndata: {json}\n\n";

            try
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);

                lock (client.SendLock)
                {
                    if (!client.IsConnected || client.Response?.OutputStream == null)
                    {
                        client.MarkDisconnected();
                        return;
                    }

                    client.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    client.Response.OutputStream.Flush();
                }

                client.UpdateLastActivity();
                LogApi.Message($"[SSE] Sent event '{eventType}' to client", LoggingLevels.DEBUG);
            }
            catch (IOException ioEx)
            {
                LogApi.Info(
                    $"[SSE] Client disconnected during send '{eventType}' - {ioEx.Message}"
                );
                client.MarkDisconnected();
            }
            catch (Exception ex)
            {
                LogApi.Error($"[SSE] Error sending event '{eventType}' - {ex}");
                client.MarkDisconnected();
            }
        }

        private bool TestClientConnection(SseClient client)
        {
            try
            {
                if (!client.IsConnected || client.Response?.OutputStream == null)
                    return false;

                // Send a simple SSE comment to test the connection
                var pingMessage = ":ping\n\n";
                var buffer = System.Text.Encoding.UTF8.GetBytes(pingMessage);

                lock (client.SendLock)
                {
                    client.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    client.Response.OutputStream.Flush();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RemoveClient(SseClient client)
        {
            if (client == null)
                return;

            lock (_clientsLock)
            {
                _connectedClients.Remove(client);
            }
            client.MarkDisconnected();
        }

        private void RemoveClients(List<SseClient> clients)
        {
            if (clients.Count == 0)
                return;

            lock (_clientsLock)
            {
                foreach (var client in clients)
                {
                    if (client != null)
                    {
                        _connectedClients.Remove(client);
                        client.MarkDisconnected();
                    }
                }
            }

            LogApi.Info($"[SSE] Removed {clients.Count} dead SSE connections");
        }

        public bool IsEventTypeRegistered(string eventType)
        {
            lock (_eventsLock)
            {
                return true;
            }
        }

        public IReadOnlyList<string> GetRegisteredEventTypes()
        {
            lock (_eventsLock)
            {
                return new List<string>(_registeredEventTypes);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            List<SseClient> clientsToDispose;
            lock (_clientsLock)
            {
                clientsToDispose = new List<SseClient>(_connectedClients);
                _connectedClients.Clear();
            }

            LogApi.Info(
                $"[SSE] Disposing SSE service with {clientsToDispose.Count} connected clients"
            );

            foreach (var client in clientsToDispose)
            {
                try
                {
                    client.MarkDisconnected();
                    client.Response?.Close();
                }
                catch
                { /* Ignore */
                }
            }
        }

        private class SseEvent
        {
            public string Type { get; set; }
            public object Data { get; set; }
        }

        private class SseClient : IDisposable
        {
            public HttpListenerResponse Response { get; }
            public bool IsConnected { get; private set; }
            public DateTime LastActivity { get; private set; }
            public object SendLock { get; } = new object();
            private bool _disposed = false;

            public SseClient(HttpListenerResponse response)
            {
                Response = response;
                IsConnected = true;
                LastActivity = DateTime.UtcNow;
            }

            public void MarkDisconnected()
            {
                if (_disposed)
                    return;
                IsConnected = false;
            }

            public void UpdateLastActivity()
            {
                LastActivity = DateTime.UtcNow;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                MarkDisconnected();
                try
                {
                    Response?.Close();
                }
                catch
                { /* Ignore */
                }

                _disposed = true;
            }
        }
    }
}
