using Amazon;
using marketdata.domain;
using marketdata.infrastructure.externalServices;
using marketdata.infrastructure.persistance;
using marketdata.workerservice;
using Microsoft.AspNetCore.Mvc;

namespace marketdata.api;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

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
    }
}
