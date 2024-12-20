﻿using marketdata.domain.security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.websocket;

public class WebSocketHandler(WebSocketConnectionManager connectionManager, IIdentityService tokenValidator)
{
    private WebSocket? _webSocket;
    private readonly WebSocketConnectionManager _connectionManager = connectionManager;
    private readonly IIdentityService _tokenValidator = tokenValidator;

    public Guid ConnectionId { get; set; } = Guid.NewGuid();
    public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;
    public bool IsAuthenticated { get; private set; }
    public string[] Symbols { get; private set; } = [];

    public void SetWebSocket(WebSocket webSocket)
    {
        _webSocket = webSocket;
        _connectionManager.AddConnection(ConnectionId, this);
    }

    public async Task SendMessage(string message)
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task ReceiveMessages(CancellationToken cancellationToken)
    {
        if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            return;

        var buffer = new byte[1024 * 4];
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                Serilog.Log.Information("WebSocket disconnected");
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Serilog.Log.Information("Message received: {message}", message);

                await ProcessMessage(message);
            }
        }

        _connectionManager.RemoveConnection(ConnectionId);

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
    }

    private async Task ProcessMessage(string message)
    {
        Serilog.Log.Debug(message);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(message);
            var element = doc.RootElement;
            if (element.TryGetProperty("action", out JsonElement typeElement))
            {
                string? type = typeElement.GetString();
                switch (type)
                {
                    case domain.Constants.Actions.AUTHORIZE:
                        await Authenticate(element);
                        break;
                    case domain.Constants.Actions.SUBSCRIBE:
                        Subscribe(element);
                        break;
                    case domain.Constants.Actions.UNSUBSCRIBE:
                        Unsubscribe(element);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Error processing websocket incoming message");

            await SendMessage($"Echo {message}");
        }
    }

    private async Task Authenticate(JsonElement element)
    {
        if (IsAuthenticated)
        {
            await SendMessage("Already authenticated");
            return;
        }
        if (element.TryGetProperty(domain.Constants.Properties.TOKEN, out JsonElement tokenElement))
        {
            string? token = tokenElement.GetString();
            if (token is not null)
            {
                IsAuthenticated = await _tokenValidator.Validate(token);
                await SendMessage(IsAuthenticated ? "Authentication successful." : "Authentication failed.");
            }
        }
    }

    private async void Subscribe(JsonElement element)
    {
        if (!IsAuthenticated)
            return;

        if (element.TryGetProperty(domain.Constants.Properties.SYMBOLS, out JsonElement symbolsElement))
        {
            string[] symbols = symbolsElement.GetString()?.Split(',').Select(val => val.Trim().ToLower()).ToArray() ?? [];
            Symbols = Symbols.Concat(symbols)
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .ToArray();
            await SendMessage($"Subscribed to {string.Join(", ", Symbols)}");
        }
    }

    private async void Unsubscribe(JsonElement element)
    {
        if (!IsAuthenticated)
            return;

        if (element.TryGetProperty(domain.Constants.Properties.SYMBOLS, out JsonElement symbolsElement))
        {
            string[] symbols = symbolsElement.GetString()?.Split(',').Select(val => val.Trim().ToLower()).ToArray() ?? [];
            string[] symbolsToRemove = Symbols.Intersect(symbols).ToArray();
            Symbols = Symbols.Except(symbols).ToArray();
            await SendMessage(symbolsToRemove.Length > 0
                              ? $"Unsubscribed from {string.Join(", ", symbolsToRemove)}"
                              : "No symbols to unsubscribe.");
        }
    }

}
