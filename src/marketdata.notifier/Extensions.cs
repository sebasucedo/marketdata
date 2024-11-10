using marketdata.domain;
using marketdata.infrastructure;
using marketdata.notifier.config;

namespace marketdata.notifier;

internal static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

        services.AddTransient<ITradeGateway, TradeNotifier>();
        services.AddTransient<IQuoteGateway, QuoteNotifier>();

        services.AddMassTransitAmazonSqsConsumers(config.Aws);

        services.Configure<CognitoConfig>(configuration.GetSection("Aws:Cognito"));

        return services;
    }
}
