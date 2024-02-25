using marketdata.domain;
using marketdata.infraestructure.externalServices;
using marketdata.infraestructure.persistance;
using marketdata.workerservice;
using Microsoft.AspNetCore.Mvc;

namespace marketdata.api;

public static class Extensions
{
    public static void ConfigureServices(this IServiceCollection services, Config config)
    {
        var sharedCts = new CancellationTokenSource();
        services.AddSingleton(sharedCts);
        services.AddSingleton<IMarketSocket>(provider => new AlpacaSocket(
            provider.GetRequiredService<ILogger<AlpacaSocket>>(),
            config.Alpaca.Url,
            config.Alpaca.Key,
            config.Alpaca.Secret
            ));

        services.AddTransient<ITradeGateway>(provider => new TradeDAO(config.ConnectionStrings.DefaultConnection));
        services.AddTransient<MessageHandlerInteractor>();

        services.AddHostedService<Worker>();
    }

    public static void ConfigureRoutes(this WebApplication app)
    {
        app.MapGet("/", () => "Marketdata API!");

        app.MapGet("/stop", (CancellationTokenSource sharedCts) =>
        {
            sharedCts.Cancel();
            return Results.Ok("Cancel signal sent.");
        });

        app.MapPost("/subscribe", async ([FromServices] IMarketSocket socket,[FromBody] SubscrtiptionRequest request) =>
        {
            await socket.Subscribe(request.Symbols);
            return Results.Ok();
        });
    }
}
