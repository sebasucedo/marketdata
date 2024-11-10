using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace marketdata.notifier.hubs;

[Authorize]
public class TradeHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        //if (true)
        //    Context.Abort();

        await base.OnConnectedAsync();
    }
    ////Para llamar desde cliente
    //public async Task SendMessage(string user, string message)
    //{
    //    await Clients.All.SendAsync("ReceiveMessage", message);
    //}
}
