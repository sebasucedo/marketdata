using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure;
using marketdata.infrastructure.alpaca;
using marketdata.listener;
using Microsoft.AspNetCore.Mvc;

namespace marketdata.api;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddSingleton<IClockWrapper, SystemClockWrapper>();

        var config = configuration.Get();

        var sharedCts = new CancellationTokenSource();
        services.AddSingleton(sharedCts);
        services.AddSingleton<IMarketSocket>(provider => new AlpacaSocket(
            provider.GetRequiredService<ILogger<AlpacaSocket>>(),
            config.Alpaca.Url,
            config.Alpaca.Key,
            config.Alpaca.Secret
            ));


        services.AddTransient<ITradeGateway, TradeGateway>();

        services.AddTransient<IMessageHandler, AlpacaMessageHandler>();

        services.AddHostedService<Worker>();

        services.AddMassTransitAmazonSqs(config.Aws);

        return services;
    }

    public static void ConfigureRoutes(this WebApplication app)
    {
        app.MapGet("/", () => "Marketdata API!");

        app.MapPost("/stop", (CancellationTokenSource sharedCts) =>
        {
            sharedCts.Cancel();
            return Results.Ok("Cancel signal sent.");
        });

        app.MapPost("/subscribe", async ([FromServices] IMarketSocket socket, [FromBody] SubscrtiptionRequest request) =>
        {
            await socket.Subscribe(request.Symbols);
            return Results.Ok();
        });

        app.MapPost("/test", async (ITradeGateway gateway) =>
        {
            Trade trade = new()
            {
                Symbol = "AAPL",
                Timestamp = DateTime.UtcNow,
                TradeId = 1,
                Price = 2m,
                Quantity = 3m,
                Tape = "A",
                VolumeWeightedAveragePrice = 4m,
            };
            await gateway.Save(trade);

            return Results.Ok();
        });
    }
}
