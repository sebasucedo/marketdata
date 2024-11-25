using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure.websocket;
using System.Text.Json;

namespace marketdata.socket;

public class QuoteNotifier(WebSocketConnectionManager connectionManager) : IQuoteGateway
{
    private readonly WebSocketConnectionManager _connectionManager = connectionManager;

    public async Task<bool> Process(Quote quote)
    {
        var message = JsonSerializer.Serialize(quote);
        await _connectionManager.BroadcastMessage(message);
        return true;
    }
}
