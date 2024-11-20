using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.websocket;

public class WebSocketHandler()
{
    private WebSocket? _webSocket;

    public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;
    
    public void SetWebSocket(WebSocket webSocket)
    {
        _webSocket = webSocket;
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

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken);
    }

    private async Task ProcessMessage(string message)
    {
        Serilog.Log.Debug(message);

        await Task.CompletedTask;
    }

}
