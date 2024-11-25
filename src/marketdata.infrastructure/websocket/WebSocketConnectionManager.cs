using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.websocket;

public class WebSocketConnectionManager
{
    private readonly Dictionary<Guid, WebSocketHandler> _connections = [];

    private readonly object _lock = new();

    public void AddConnection(Guid connectionId, WebSocketHandler handler)
    {
        lock (_lock)
        {
            if (!_connections.ContainsKey(connectionId))
            {
                _connections[connectionId] = handler;
            }
        }
    }

    public void RemoveConnection(Guid connectionId)
    {
        lock (_lock)
        {
            _connections.Remove(connectionId);
        }
    }

    public WebSocketHandler? GetConnection(Guid connectionId)
    {
        lock (_lock)
        {
            return _connections.TryGetValue(connectionId, out var handler) ? handler : null;
        }
    }

    public IEnumerable<WebSocketHandler> GetAllConnections()
    {
        lock (_lock)
        {
            return [.. _connections.Values];
        }
    }

    public async Task BroadcastMessage(string message)
    {
        var tasks = new List<Task>();
        lock (_lock)
        {
            tasks = _connections.Values
                    .Where(handler => handler.IsConnected)
                    .Select(handler => handler.SendMessage(message))
                    .ToList();
        }
        await Task.WhenAll(tasks);
    }

    public int GetActiveConnectionsCount()
    {
        lock (_lock)
        {
            return _connections.Count;
        }
    }
}
