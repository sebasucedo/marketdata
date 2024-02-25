using marketdata.domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infraestructure.externalServices;

public class PolygonSocket(ILogger<AlpacaSocket> logger, string url, string apiKey) : IMarketSocket
{
    private readonly ILogger<AlpacaSocket> _logger = logger;
    private readonly string _url = url;
    private readonly string _apiKey = apiKey;
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

        var auth = new
        {
            action = "auth",
            @params = _apiKey,
        };
        var message = JsonSerializer.Serialize(auth, serializeOptions);
        await SendMessage(message);
    }

    public Task Listen(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }

    public Task Subscribe(string[] symbols)
    {
        throw new NotImplementedException();
    }

    private async Task SendMessage(string message)
    {
        var buffer = new ArraySegment<byte>(
            array: Encoding.ASCII.GetBytes(message),
            offset: 0,
        count: message.Length);

        await _webSocket.SendAsync(
            buffer: buffer,
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None);
    }

}
