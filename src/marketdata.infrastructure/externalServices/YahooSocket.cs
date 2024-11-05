using marketdata.domain;
using marketdata.infrastructure.alpaca;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.externalServices;

public class YahooSocket(ILogger<AlpacaSocket> logger, string url) : IMarketSocket
{
    private readonly ILogger<AlpacaSocket> _logger = logger;
    private readonly string _url = url;
    private readonly ClientWebSocket _webSocket = new();

    public event EventHandler<string>? MessageReceived;

    static readonly JsonSerializerOptions serializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task Connect(CancellationToken stoppingToken)
    {
        var webSocketUri = new Uri(_url);

        await _webSocket.ConnectAsync(webSocketUri, stoppingToken);
    }

    public Task Listen(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public async Task Subscribe(string[] symbols)
    {
        var subscription = new
        {
            subscribe = symbols,
        };
        var message = JsonSerializer.Serialize(subscription, serializeOptions);
        await _webSocket.SendMessage(message);
    }
}
