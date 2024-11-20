using marketdata.domain;
using marketdata.infrastructure;
using marketdata.infrastructure.websocket;

namespace marketdata.socket;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

        services.AddSingleton<WebSocketHandler>();
        services.AddTransient<ITradeGateway, TradeNotifier>();
        services.AddTransient<IQuoteGateway, QuoteNotifier>();

        services.AddMassTransitAmazonSqsConsumers(config.Aws);

        return services;
    }

    public static void AddEndpoints(this WebApplication app)
    {
        app.Map("/", async (HttpContext context, WebSocketHandler webSocketManager) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                webSocketManager.SetWebSocket(webSocket);
                Console.WriteLine("WebSocket connected");

                await webSocketManager.ReceiveMessages(CancellationToken.None);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

    }
}
