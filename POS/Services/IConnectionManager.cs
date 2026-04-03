using System.Net.WebSockets;

namespace POS.Services
{
    public interface IConnectionManager
    {
        Task AddConnection(string connectionId, WebSocket socket);
        Task RemoveConnection(string connectionId);
        Task<bool> SendMessageAsync(string connectionId, string message);
        Task BroadcastMessageAsync(string message);
    }
}