﻿using marketdata.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using marketdata.infrastructure.persistance;
using MySql.Data.MySqlClient;
using System.Data;
using marketdata.domain;


namespace marketdata.processor;

internal static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

        services.AddTransient<IDbConnection>(provider => new MySqlConnection(config.ConnectionStrings.DefaultConnection));
        services.AddTransient<ITradeGateway, TradeDAO>();

        services.AddMassTransitAmazonSqsConsumers(config.Aws);

        return services;
    }
}
