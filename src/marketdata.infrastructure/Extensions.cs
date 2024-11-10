using marketdata.infrastructure.configs;
using marketdata.infrastructure.consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure;

public static class Extensions
{
    public static IServiceCollection AddMassTransitAmazonSqs(this IServiceCollection services, AwsConfig awsConfig)
    {
        services.AddMassTransit(x =>
        {
            x.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host(awsConfig.Region, h => {
                    h.AccessKey(awsConfig.AccessKey);
                    h.SecretKey(awsConfig.SecretKey);

                    h.Scope(awsConfig.Sqs.ScopePrefix, true);
                });

                cfg.ConfigureEndpoints(context, new DefaultEndpointNameFormatter($"{awsConfig.Sqs.ScopePrefix}-", false));
            });
        });

        return services;
    }

    public static IServiceCollection AddMassTransitAmazonSqsConsumers(this IServiceCollection services, AwsConfig awsConfig)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TradeEventConsumer>();
            x.AddConsumer<TradeEventErrorConsumer>();
            x.AddConsumer<QuoteEventConsumer>();

            x.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host(awsConfig.Region, h => {
                    h.AccessKey(awsConfig.AccessKey);
                    h.SecretKey(awsConfig.SecretKey);

                    h.Scope(awsConfig.Sqs.ScopePrefix, true);
                });

                cfg.ConfigureEndpoints(context, new DefaultEndpointNameFormatter($"{awsConfig.Sqs.ScopePrefix}-", false));
            });
        });

        return services;
    }
}
