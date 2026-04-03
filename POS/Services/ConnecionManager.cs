using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace POS.Services;

public class ConnectionManager(ILogger<ConnectionManager> logger) : IConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<ConnectionManager> _logger = logger;

    public Task AddConnection(string connectionId, WebSocket socket)
    {
        _connections.TryAdd(connectionId, socket);
        return Task.CompletedTask;
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);

        var tasks = _connections.Select(async pair =>
        {
            var socket = pair.Value;
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch
            {
                // remove broken connection
                _connections.TryRemove(pair.Key, out _);
                _logger.LogWarning($"Connection {pair.Key} removed due to error during broadcast.");
            }
        });
        await Task.WhenAll(tasks);
    }

    public Task RemoveConnection(string connectionId)
    {
        if (_connections.ContainsKey(connectionId))
        {
            _connections.TryRemove(connectionId, out _);
        }
        return Task.CompletedTask;
    }

    public async Task<bool> SendMessageAsync(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var socket))
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);

                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);

                    return true;
                }
            }
            catch
            {
                _connections.TryRemove(connectionId, out _);
                _logger.LogWarning($"Connection {connectionId} removed due to error during send.");
            }
        }

        return false;
    }

}