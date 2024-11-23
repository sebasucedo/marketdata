using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure.websocket;
using System.Text.Json;

namespace marketdata.socket;

public class TradeNotifier(WebSocketHandler webSocketManager) : ITradeGateway
{
    private readonly WebSocketHandler _webSocketManager = webSocketManager;

    public Task<IEnumerable<Trade>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Process(Trade trade)
    {
        var message = JsonSerializer.Serialize(trade);
        await _webSocketManager.SendMessage(message);
        return true;
    }
}
