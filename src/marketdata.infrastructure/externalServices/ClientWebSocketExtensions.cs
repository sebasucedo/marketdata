using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.externalServices;

public static class ClientWebSocketExtensions
{
    public static async Task SendMessage(this ClientWebSocket clientWebSocket, string message)
    {
        var buffer = new ArraySegment<byte>(
            array: Encoding.ASCII.GetBytes(message),
            offset: 0,
        count: message.Length);

        await clientWebSocket.SendAsync(
            buffer: buffer,
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None);
    }

    public static async Task<WebSocketReceiveResult> ReceiveMessage(this ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4]; // Buffer 

        var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

        return result;
    }

}
