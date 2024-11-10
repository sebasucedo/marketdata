using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure;
using marketdata.infrastructure.alpaca;
using MassTransit.Monitoring;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
        services.AddTransient<IQuoteGateway, QuoteGateway>();
        services.AddTransient<IMessageHandler, AlpacaMessageHandler>();

        services.AddHostedService<listener.Worker>();

        services.AddMassTransitAmazonSqs(config.Aws);

        return services;
    }

    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

        void ConfigureResource(ResourceBuilder r)
        {
            r.AddService(config.Jaeger.ServiceName,
                serviceVersion: config.Jaeger.ServiceVersion,
                serviceInstanceId: Environment.MachineName);
        }

        services.AddOpenTelemetry()
            .ConfigureResource(ConfigureResource)
            .WithMetrics(b =>
                b.AddMeter(InstrumentationOptions.MeterName)
                 .AddOtlpExporter(opts => { opts.Endpoint = new Uri(config.Jaeger.Endpoint); })
            )
            .WithTracing(b =>
            {
                b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(config.Jaeger.ServiceName))
                 .AddAspNetCoreInstrumentation()
                 .AddOtlpExporter(opts => { opts.Endpoint = new Uri(config.Jaeger.Endpoint); });
            });

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

        app.MapPost("/test", async (ITradeGateway gateway, IClockWrapper clockWrapper) =>
        {
            Random rnd = new();
            decimal max = 100000m;
            Trade trade = new()
            {
                Symbol = "AAPL",
                Timestamp = clockWrapper.UtcNow,
                TradeId = rnd.Next(),
                Price = Math.Round((decimal)(rnd.NextDouble() * (double)max), 2),
                Quantity = Math.Round((decimal)(rnd.NextDouble() * (double)max), 2),
                Tape = "A",
                VolumeWeightedAveragePrice = rnd.Next(),
            };
            await gateway.Save(trade);

            return Results.Ok();
        });
    }
}
