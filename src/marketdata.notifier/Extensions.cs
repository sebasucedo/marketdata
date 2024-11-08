using System.Data;
using marketdata.domain;
using marketdata.infrastructure;
using marketdata.notifier.hubs;

namespace marketdata.notifier;

internal static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

        services.AddTransient<ITradeGateway, TradeNotifier>();

        services.AddMassTransitAmazonSqsConsumers(config.Aws);

        return services;
    }
}
