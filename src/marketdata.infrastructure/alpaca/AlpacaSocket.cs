using marketdata.domain;
using marketdata.infrastructure.externalServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.alpaca;

public class AlpacaSocket(ILogger<AlpacaSocket> logger, string url, string key, string secret) : IMarketSocket
{
    private readonly ILogger<AlpacaSocket> _logger = logger;
    private readonly string _url = url;
    private readonly string _key = key;
    private readonly string _secret = secret;
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
            action = Constants.Actions.AUTH,
            key = _key,
            secret = _secret
        };
        var message = JsonSerializer.Serialize(auth, serializeOptions);
        await _webSocket.SendMessage(message);
    }

    public async Task Subscribe(string[] symbols)
    {
        var subscription = new
        {
            action = Constants.Actions.SUBSCRIBE,
            trades = symbols,
            quotes = symbols,
            bars = symbols,
        };
        var message = JsonSerializer.Serialize(subscription, serializeOptions);
        await _webSocket.SendMessage(message);
    }

    public async Task Listen(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 4];

            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogInformation("Message received: {message}", message);

                OnMessageReceived(message);
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                _logger.LogInformation("WebSocket connection closed.");
                break;
            }
        }

        _logger.LogInformation("Bye!");
    }

    protected virtual void OnMessageReceived(string message)
    {
        MessageReceived?.Invoke(this, message);
    }

}
