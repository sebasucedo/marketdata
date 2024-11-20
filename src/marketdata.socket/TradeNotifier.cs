using marketdata.domain;
using marketdata.domain.entities;
using System.Text.Json;

namespace marketdata.socket;

public class TradeNotifier(infrastructure.websocket.WebSocketHandler webSocketManager) : ITradeGateway
{
    private readonly infrastructure.websocket.WebSocketHandler _webSocketManager = webSocketManager;

    public Task<IEnumerable<Trade>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Save(Trade trade)
    {
        var message = JsonSerializer.Serialize(trade);
        await _webSocketManager.SendMessage(message);
        return true;
    }
}
