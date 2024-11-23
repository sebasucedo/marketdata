using marketdata.domain;
using marketdata.domain.entities;
using marketdata.notifier.hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace marketdata.notifier;

public class TradeNotifier(IHubContext<TradeHub> hubContext) : ITradeGateway
{
    private readonly IHubContext<TradeHub> _hubContext = hubContext;

    public Task<IEnumerable<Trade>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Process(Trade trade)
    {
        var message = JsonSerializer.Serialize(trade);

        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "");
        }
        return false;
    }
}
