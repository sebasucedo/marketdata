using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure.websocket;
using System.Text.Json;

namespace marketdata.socket;

public class QuoteNotifier(WebSocketConnectionManager webSocketConnectionManager) : IQuoteGateway
{
    private readonly WebSocketConnectionManager _manager = webSocketConnectionManager;

    public async Task<bool> Process(Quote quote)
    {
        var message = JsonSerializer.Serialize(quote);
        bool predicate(WebSocketHandler x) => x.IsConnected && x.Symbols.Any(s => s.Equals(quote.Symbol, StringComparison.OrdinalIgnoreCase));
        await _manager.BroadcastMessage(predicate, message);
        return true;
    }
}
