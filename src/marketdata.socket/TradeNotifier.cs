using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure.websocket;
using System.Text.Json;

namespace marketdata.socket;

public class TradeNotifier(WebSocketConnectionManager webSocketConnectionManager) : ITradeGateway
{
    private readonly WebSocketConnectionManager _manager = webSocketConnectionManager;

    public Task<IEnumerable<Trade>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Process(Trade trade)
    {
        var message = JsonSerializer.Serialize(trade);
        bool predicate(WebSocketHandler x) => x.IsConnected && x.Symbols.Any(s => s.Equals(trade.Symbol, StringComparison.OrdinalIgnoreCase));
        await _manager.BroadcastMessage(predicate, message);
        return true;
    }
}
