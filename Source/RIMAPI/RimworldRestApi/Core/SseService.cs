using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RimWorld;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Core
{
    public class SseService : IDisposable
    {
        private readonly IGameDataService _gameDataService;
        private readonly List<SseClient> _connectedClients;
        private readonly object _clientsLock = new object();
        private bool _disposed = false;
        private int _lastBroadcastTick;
        private readonly Queue<Action> _broadcastQueue;
        private readonly object _queueLock = new object();

        public SseService(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
            _connectedClients = new List<SseClient>();
            _broadcastQueue = new Queue<Action>();
            _lastBroadcastTick = Find.TickManager?.TicksGame ?? 0;
        }

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
                // Set SSE headers
                response.StatusCode = 200;
                response.ContentType = "text/event-stream";
                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Connection", "keep-alive");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET");

                // Add client to connected list
                lock (_clientsLock)
                {
                    _connectedClients.Add(client);
                }

                DebugLogging.Info($"SSE connection established. Total clients: {_connectedClients.Count}");

                // Send initial connection message
                SendEventToClientInternal(client, "connected", new
                {
                    message = "SSE connection established",
                    timestamp = DateTime.UtcNow
                });

                // Send initial game state
                var gameState = _gameDataService.GetGameState();
                SendEventToClientInternal(client, "gameState", gameState);

                // Keep connection alive with heartbeat
                var lastHeartbeat = DateTime.UtcNow;
                var heartbeatInterval = TimeSpan.FromSeconds(30);

                while (!_disposed && client.IsConnected)
                {
                    try
                    {
                        if ((DateTime.UtcNow - lastHeartbeat).TotalSeconds >= 30)
                        {
                            SendEventToClientInternal(client, "heartbeat", new
                            {
                                timestamp = DateTime.UtcNow
                            });
                            lastHeartbeat = DateTime.UtcNow;
                        }

                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        DebugLogging.Info($"SSE client error - {ex.Message}");
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                DebugLogging.Error($"SSE connection error - {ex}");
            }
            finally
            {
                // Remove client from connected list and mark as disconnected
                client.MarkDisconnected();
                lock (_clientsLock)
                {
                    _connectedClients.Remove(client);
                }

                DebugLogging.Info($"SSE connection closed. Remaining clients: {_connectedClients.Count}");
            }
        }

        private async Task<bool> TestClientConnection(SseClient client)
        {
            try
            {
                if (!client.IsConnected || client.Response.OutputStream == null)
                    return false;

                // Send a proper SSE comment (starts with :) as a ping
                var pingMessage = ":ping\n\n";
                var buffer = System.Text.Encoding.UTF8.GetBytes(pingMessage);
                await client.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                await client.Response.OutputStream.FlushAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static SseService GetService()
        {
            return RIMAPI.RIMAPI_Mod.SseService;
        }

        public void BroadcastGameUpdate()
        {
            if (_disposed) return;

            var currentTick = Find.TickManager?.TicksGame ?? 0;
            if (currentTick - _lastBroadcastTick < 60) return; // Throttle to ~1 second

            try
            {
                var gameState = _gameDataService.GetGameState();

                // Queue the broadcast instead of awaiting it
                lock (_queueLock)
                {
                    _broadcastQueue.Enqueue(() => BroadcastEventInternal("gameUpdate", gameState));
                }

                _lastBroadcastTick = currentTick;
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"Error preparing game update broadcast - {ex}");
            }
        }

        public void ProcessBroadcastQueue()
        {
            if (_disposed || _broadcastQueue.Count == 0) return;

            List<Action> broadcastsToProcess;

            lock (_queueLock)
            {
                broadcastsToProcess = new List<Action>(_broadcastQueue);
                _broadcastQueue.Clear();
            }

            foreach (var broadcast in broadcastsToProcess)
            {
                try
                {
                    broadcast();
                }
                catch (Exception ex)
                {
                    DebugLogging.Error($"Error processing broadcast - {ex}");
                }
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
                    if (client == null || !client.IsConnected)
                    {
                        clientsToRemove.Add(client);
                        continue;
                    }

                    SendEventToClientInternal(client, eventType, data);

                    if (!client.IsConnected)
                    {
                        clientsToRemove.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    DebugLogging.Info($"[SSE] Error sending to client for event '{eventType}' - {ex.Message}");
                    if (client != null)
                    {
                        client.MarkDisconnected();
                        clientsToRemove.Add(client);
                    }
                }
            }

            if (clientsToRemove.Count > 0)
            {
                lock (_clientsLock)
                {
                    foreach (var client in clientsToRemove)
                    {
                        if (client == null) continue;
                        _connectedClients.Remove(client);
                        try
                        {
                            client.MarkDisconnected();
                            client.Response?.Close();
                        }
                        catch { }
                    }
                }
                DebugLogging.Info($"[SSE] Removed {clientsToRemove.Count} dead SSE connections");
            }
        }

        public void QueueEventBroadcast(string eventType, object data)
        {
            if (_disposed) return;

            lock (_queueLock)
            {
                _broadcastQueue.Enqueue(() => BroadcastEventInternal(eventType, data));
            }
        }

        private void SendEventToClientInternal(SseClient client, string eventType, object data)
        {
            if (client == null)
            {
                DebugLogging.Warning($"[SSE] SendEventToClientInternal: client is null for event '{eventType}'");
                return;
            }

            if (!client.IsConnected)
            {
                DebugLogging.Info($"[SSE] Client already disconnected, skipping event '{eventType}'");
                return;
            }

            if (client.Response == null || client.Response.OutputStream == null)
            {
                DebugLogging.Info($"[SSE] Client response/output is null, marking disconnected for event '{eventType}'");
                client.MarkDisconnected();
                return;
            }

            string json;
            try
            {
                // If caller already passed JSON string, don't double-serialize
                if (data is string s)
                {
                    json = s;
                }
                else
                {
                    json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.None
                    });
                }
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[SSE] Failed to serialize data for event '{eventType}': {ex}");
                return;
            }

            string message;
            try
            {
                message = $"id: {Guid.NewGuid()}\n" +
                          $"event: {eventType}\n" +
                          $"data: {json}\n" +
                          $"retry: 3000\n\n";
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[SSE] Failed to build SSE message for event '{eventType}': {ex}");
                return;
            }

            try
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);

                // SERIALIZE WRITES FOR THIS CLIENT
                lock (client.SendLock)
                {
                    if (!client.IsConnected || client.Response?.OutputStream == null)
                    {
                        DebugLogging.Info($"[SSE] Client disconnected before send for event '{eventType}'");
                        client.MarkDisconnected();
                        return;
                    }

                    client.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    client.Response.OutputStream.Flush();
                }

                DebugLogging.Message($"[SSE] Successfully sent event '{eventType}'", LoggingLevels.DEBUG);
                client.UpdateLastActivity();
            }
            catch (IOException ioEx)
            {
                // This is the “remote host forcibly closed connection” case — normal when client dies.
                DebugLogging.Error($"[SSE] IO error sending event '{eventType}' - {ioEx}");
                client.MarkDisconnected();
            }
            catch (Exception ex)
            {
                DebugLogging.Error($"[SSE] Error sending event '{eventType}' - {ex}");
                client.MarkDisconnected();
            }
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

            DebugLogging.Info($"Disposing SSE service with {clientsToDispose.Count} connected clients");

            foreach (var client in clientsToDispose)
            {
                try
                {
                    client.MarkDisconnected();
                    client.Response?.Close();
                }
                catch (Exception ex)
                {
                    DebugLogging.Info($"Error disposing client: {ex.Message}");
                }
            }
        }

        private class SseClient
        {
            public HttpListenerResponse Response { get; }
            public bool IsConnected { get; private set; }
            public DateTime LastActivity { get; private set; }
            public object SendLock { get; } = new object();

            public SseClient(HttpListenerResponse response)
            {
                Response = response;
                IsConnected = true;
                LastActivity = DateTime.UtcNow;
            }

            public void MarkDisconnected()
            {
                IsConnected = false;
                try
                {
                    Response?.Close();
                }
                catch
                {
                    // Ignore errors during close
                }
            }

            public void UpdateLastActivity()
            {
                LastActivity = DateTime.UtcNow;
            }
        }
    }
}