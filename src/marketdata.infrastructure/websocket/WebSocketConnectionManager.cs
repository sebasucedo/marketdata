using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.websocket;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<Guid, WebSocketHandler> _connections = [];

    public void AddConnection(Guid connectionId, WebSocketHandler handler)
    {
        _connections.TryAdd(connectionId, handler);
    }

    public void RemoveConnection(Guid connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public WebSocketHandler? GetConnection(Guid connectionId)
    {
        return _connections.TryGetValue(connectionId, out var handler) ? handler : null;
    }

    public IEnumerable<WebSocketHandler> GetAllConnections()
    {
        return [.. _connections.Values];
    }

    public async Task BroadcastMessage(string message) => await BroadcastMessage(_ => true, message);

    public async Task BroadcastMessage(Func<WebSocketHandler, bool> predicate, string message)
    {
        var connections = _connections.Values
                                      .Where(predicate)
                                      .ToList();
        if (connections.Count == 0)
            return;

        List<Task> tasks = connections
                           .Select(async handler =>
                           {
                               try
                               {
                                   await handler.SendMessage(message);
                               }
                               catch (Exception ex)
                               {
                                   Serilog.Log.Error(ex, "An error occurred while sending a message. Exception details: {message}", ex.Message);
                               }
                           })
                           .ToList();
        await Task.WhenAll(tasks);
    }

    public int GetActiveConnectionsCount()
    {
        return _connections.Count;
    }
}
