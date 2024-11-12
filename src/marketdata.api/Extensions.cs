using Amazon.CloudWatchLogs;
using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure;
using marketdata.infrastructure.alpaca;
using marketdata.infrastructure.configs;
using MassTransit.Monitoring;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog.Sinks.AwsCloudWatch;
using Serilog;
using Amazon;

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

    public static IServiceCollection AddLogger(this IServiceCollection services, IConfiguration configuration)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var awsConfig = serviceProvider.GetRequiredService<IOptions<AwsConfig>>().Value;

        if (awsConfig.CloudWatch is not null)
        {
            var logClient = new AmazonCloudWatchLogsClient(awsConfig.AccessKey, awsConfig.SecretKey, RegionEndpoint.GetBySystemName(awsConfig.Region));

            Log.Logger = new LoggerConfiguration()
                             .ReadFrom.Configuration(configuration)
                             .Enrich.FromLogContext()
                             .WriteTo.AmazonCloudWatch(
                                 logGroup: awsConfig.CloudWatch.LogGroupName,
                                 logStreamPrefix: DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                                 batchSizeLimit: 100,
                                 queueSizeLimit: 10000,
                                 batchUploadPeriodInSeconds: 15,
                                 createLogGroup: true,
                                 maxRetryAttempts: 3,
                                 logGroupRetentionPolicy: LogGroupRetentionPolicy.OneMonth,
                                 cloudWatchClient: logClient,
                                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                             .CreateLogger();
        }

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
