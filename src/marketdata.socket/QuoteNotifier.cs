using marketdata.domain;
using marketdata.domain.entities;
using System.Text.Json;

namespace marketdata.socket;

public class QuoteNotifier(infrastructure.websocket.WebSocketHandler webSocketManager) : IQuoteGateway
{
    private readonly infrastructure.websocket.WebSocketHandler _webSocketManager = webSocketManager;

    public async Task<bool> Process(Quote quote)
    {
        var message = JsonSerializer.Serialize(quote);
        await _webSocketManager.SendMessage(message);
        return true;
    }
}
