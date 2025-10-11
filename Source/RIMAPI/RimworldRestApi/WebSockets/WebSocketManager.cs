using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RimworldRestApi.Services;
using Verse;

namespace RimworldRestApi.WebSockets
{
    public class WebSocketManager : IDisposable
    {
        private readonly List<WebSocket> _sockets;
        private readonly IGameDataService _gameDataService;
        private readonly object _lock = new object();
        private int _lastBroadcastTick;

        public WebSocketManager(IGameDataService gameDataService)
        {
            _sockets = new List<WebSocket>();
            _gameDataService = gameDataService;
            _lastBroadcastTick = Find.TickManager?.TicksGame ?? 0;
        }

        public async Task HandleWebSocketRequest(HttpListenerContext context)
        {
            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                return;
            }

            var webSocketContext = await context.AcceptWebSocketAsync(null);
            var webSocket = webSocketContext.WebSocket;

            lock (_lock)
            {
                _sockets.Add(webSocket);
            }

            Log.Message("RIMAPI: WebSocket connection established");
            await HandleWebSocketConnection(webSocket);
        }

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                            "Closed by client", CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RIMAPI: WebSocket error - {ex.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _sockets.Remove(webSocket);
                }
                webSocket?.Dispose();
            }
        }

        public void BroadcastGameUpdate()
        {
            var currentTick = Find.TickManager?.TicksGame ?? 0;
            if (currentTick - _lastBroadcastTick < 30) return; // Throttle broadcasts

            var gameState = _gameDataService.GetGameState();
            var message = new
            {
                type = "gameUpdate",
                data = gameState,
                timestamp = DateTime.UtcNow
            };

            _ = BroadcastMessageAsync(message); // Fire and forget
            _lastBroadcastTick = currentTick;
        }

        private async Task BroadcastMessageAsync(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            List<WebSocket> socketsToRemove = new List<WebSocket>();
            List<WebSocket> currentSockets;

            // Copy the sockets list to avoid holding the lock during async operations
            lock (_lock)
            {
                currentSockets = new List<WebSocket>(_sockets);
            }

            // Send messages to all sockets without holding the lock
            foreach (var socket in currentSockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(segment, WebSocketMessageType.Text,
                            true, CancellationToken.None);
                    }
                    catch
                    {
                        socketsToRemove.Add(socket);
                    }
                }
                else
                {
                    socketsToRemove.Add(socket);
                }
            }

            // Clean up dead connections (with lock)
            if (socketsToRemove.Count > 0)
            {
                lock (_lock)
                {
                    foreach (var socket in socketsToRemove)
                    {
                        _sockets.Remove(socket);
                        socket.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            List<WebSocket> socketsToDispose;

            lock (_lock)
            {
                socketsToDispose = new List<WebSocket>(_sockets);
                _sockets.Clear();
            }

            foreach (var socket in socketsToDispose)
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                            "Server shutting down", CancellationToken.None).Wait(1000);
                    }
                    socket.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error($"RIMAPI: Error disposing WebSocket - {ex.Message}");
                }
            }
        }
    }
}