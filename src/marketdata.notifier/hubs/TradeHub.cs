using Microsoft.AspNetCore.SignalR;

namespace marketdata.notifier.hubs;

public class TradeHub : Hub
{
    ////Para llamar desde cliente
    //public async Task SendMessage(string user, string message)
    //{
    //    await Clients.All.SendAsync("ReceiveMessage", message);
    //}
}
