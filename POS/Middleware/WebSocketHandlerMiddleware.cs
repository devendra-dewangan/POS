using System.Net.WebSockets;
using System.Text;
using POS.Services;

namespace POS.Middleware
{
    public class WebSocketHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketHandlerMiddleware> _logger;
        private readonly IConnectionManager _connectionManager;
        public WebSocketHandlerMiddleware(RequestDelegate next , ILogger<WebSocketHandlerMiddleware> logger, IConnectionManager connectionManager)
        {
            _next = next;
            _logger = logger;
            _connectionManager = connectionManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if(context.WebSockets.IsWebSocketRequest)
            {
                var connectionId = Guid.NewGuid().ToString();
                try
                {
                    _logger.LogInformation("WebSocket request received.");
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    await _connectionManager.AddConnection(connectionId, socket);
                    _logger.LogInformation($"WebSocket connection established: {connectionId}");
                    await HandleWebSocketAsync(connectionId, socket);
                    _logger.LogInformation($"WebSocket connection closed: {connectionId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while handling WebSocket request.");
                }
                finally
                {
                    await _connectionManager.RemoveConnection(connectionId);
                }
                return;
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleWebSocketAsync(string connectionId, WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation($"WebSocket connection {connectionId} requested close.");
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Received message from {connectionId}: {message}");
                        // Here you can handle incoming messages as needed
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error occurred while receiving message from {connectionId}.");
                    break;
                }
            }
        }
    }
}