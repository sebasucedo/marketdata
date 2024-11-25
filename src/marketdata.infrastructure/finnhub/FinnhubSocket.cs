using marketdata.domain;
using marketdata.infrastructure.externalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.finnhub;

public class FinnhubSocket(string url, string apiKey) : IMarketSocket
{
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
        var uriString = $"{_url}?token={_apiKey}";
        var webSocketUri = new Uri(uriString);

        await _webSocket.ConnectAsync(webSocketUri, stoppingToken);

    }

    public async Task Listen(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            var buffer = new byte[1024 * 4];

            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);

            switch(result.MessageType)
            {
                case WebSocketMessageType.Text:
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived(message);

                    Serilog.Log.Debug(message);
                    break;
                case WebSocketMessageType.Close:
                    break;
            }
        }
    }

    public async Task Subscribe(string[] symbols)
    {
        foreach (var symbol in symbols)
        {
            var subscription = new
            {
                type = "subscribe",
                symbol = symbol
            };
            var message = JsonSerializer.Serialize(subscription, serializeOptions);
            await _webSocket.SendMessage(message);
        }
    }

    protected virtual void OnMessageReceived(string message)
    {
        MessageReceived?.Invoke(this, message);
    }
}
