using marketdata.domain;
using marketdata.domain.entities;
using marketdata.notifier.hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace marketdata.notifier;

public class QuoteNotifier(IHubContext<TradeHub> hubContext) : IQuoteGateway
{
    private readonly IHubContext<TradeHub> _hubContext = hubContext;

    public async Task<bool> Process(Quote quote)
    {
        var message = JsonSerializer.Serialize(quote);

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
