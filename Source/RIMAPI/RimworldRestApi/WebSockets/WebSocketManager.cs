using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace RimworldRestApi.WebSockets
{
    public class WebSocketManager : IDisposable
    {
        private readonly List<WebSocket> _sockets;
        private readonly GameEventBroadcaster _broadcaster;
        private readonly object _lock = new object();

        public WebSocketManager()
        {
            _sockets = new List<WebSocket>();
            _broadcaster = new GameEventBroadcaster();
            _broadcaster.OnEvent += BroadcastToAll;
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

            Log.Message("WebSocket connection established");

            // Keep connection alive and handle messages
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
                Log.Error($"WebSocket error: {ex.Message}");
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

        private async void BroadcastToAll(string eventType, object data)
        {
            var message = System.Text.Json.JsonSerializer.Serialize(new
            {
                type = eventType,
                data = data,
                timestamp = DateTime.UtcNow
            });

            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            List<WebSocket> socketsToRemove = new List<WebSocket>();

            lock (_lock)
            {
                foreach (var socket in _sockets)
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

                // Clean up dead connections
                foreach (var socket in socketsToRemove)
                {
                    _sockets.Remove(socket);
                    socket.Dispose();
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var socket in _sockets)
                {
                    socket?.Dispose();
                }
                _sockets.Clear();
            }
            _broadcaster?.Dispose();
        }
    }
}