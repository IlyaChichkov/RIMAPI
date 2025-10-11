using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.Core
{
    public class SseService : IDisposable
    {
        private readonly IGameDataService _gameDataService;
        private readonly List<HttpListenerResponse> _connectedClients;
        private readonly object _clientsLock = new object();
        private bool _disposed = false;
        private int _lastBroadcastTick;
        private readonly Queue<Action> _broadcastQueue;
        private readonly object _queueLock = new object();

        public SseService(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
            _connectedClients = new List<HttpListenerResponse>();
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

            try
            {
                // Set SSE headers
                response.StatusCode = 200;
                response.ContentType = "text/event-stream";
                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Connection", "keep-alive");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET");
                response.Headers.Add("Access-Control-Expose-Headers", "*");

                // Add client to connected list
                lock (_clientsLock)
                {
                    _connectedClients.Add(response);
                }

                Log.Message($"RIMAPI: SSE connection established. Total clients: {_connectedClients.Count}");

                // Send initial connection message
                await SendEventToClient(response, "connected", new
                {
                    message = "SSE connection established",
                    timestamp = DateTime.UtcNow
                });

                // Send initial game state
                var gameState = _gameDataService.GetGameState();
                await SendEventToClient(response, "gameState", gameState);

                // Keep connection alive with heartbeat
                var lastHeartbeat = DateTime.UtcNow;
                var clientDisconnected = false;

                while (!_disposed && !clientDisconnected && response.OutputStream.CanWrite)
                {
                    try
                    {
                        // Send heartbeat every 30 seconds
                        if ((DateTime.UtcNow - lastHeartbeat).TotalSeconds >= 30)
                        {
                            await SendEventToClient(response, "heartbeat", new
                            {
                                timestamp = DateTime.UtcNow
                            });
                            lastHeartbeat = DateTime.UtcNow;
                        }

                        // Check if client is still connected by attempting a flush
                        await response.OutputStream.FlushAsync();

                        // Small delay to prevent tight looping
                        await Task.Delay(1000); // 1 second delay
                    }
                    catch (Exception ex)
                    {
                        Log.Message($"RIMAPI: SSE client disconnected - {ex.Message}");
                        clientDisconnected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: SSE connection error - {ex}");
            }
            finally
            {
                // Remove client from connected list
                lock (_clientsLock)
                {
                    _connectedClients.Remove(response);
                }

                try
                {
                    response.Close();
                }
                catch { }

                Log.Message($"RIMAPI: SSE connection closed. Remaining clients: {_connectedClients.Count}");
            }
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
                    _broadcastQueue.Enqueue(() => _ = BroadcastEventInternal("gameUpdate", gameState));
                }

                _lastBroadcastTick = currentTick;
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error preparing game update broadcast - {ex}");
            }
        }

        // Process any queued broadcasts (call this from GameComponentTick)
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
                    broadcast(); // This will fire-and-forget the async operation
                }
                catch (Exception ex)
                {
                    Log.Error($"RIMAPI: Error processing broadcast - {ex}");
                }
            }
        }

        private async Task BroadcastEventInternal(string eventType, object data)
        {
            List<HttpListenerResponse> clientsToRemove = new List<HttpListenerResponse>();
            List<HttpListenerResponse> currentClients;

            lock (_clientsLock)
            {
                currentClients = new List<HttpListenerResponse>(_connectedClients);
            }

            foreach (var client in currentClients)
            {
                try
                {
                    if (client.OutputStream.CanWrite)
                    {
                        await SendEventToClient(client, eventType, data);
                    }
                    else
                    {
                        clientsToRemove.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    Log.Message($"RIMAPI: Error sending to SSE client - {ex.Message}");
                    clientsToRemove.Add(client);
                }
            }

            // Clean up dead connections
            if (clientsToRemove.Count > 0)
            {
                lock (_clientsLock)
                {
                    foreach (var client in clientsToRemove)
                    {
                        _connectedClients.Remove(client);
                        try
                        {
                            client.Close();
                        }
                        catch { }
                    }
                }
                Log.Message($"RIMAPI: Removed {clientsToRemove.Count} dead SSE connections");
            }
        }


        private async Task SendEventToClient(HttpListenerResponse client, string eventType, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var message = $"event: {eventType}\ndata: {json}\n\n";
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);

                await client.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                await client.OutputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: Error sending SSE event '{eventType}' - {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            lock (_clientsLock)
            {
                Log.Message($"RIMAPI: Disposing SSE service with {_connectedClients.Count} connected clients");

                foreach (var client in _connectedClients)
                {
                    try
                    {
                        client.Close();
                    }
                    catch { }
                }
                _connectedClients.Clear();
            }
        }
    }
}