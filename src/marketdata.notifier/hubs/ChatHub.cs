﻿using Microsoft.AspNetCore.SignalR;

namespace marketdata.notifier.hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
