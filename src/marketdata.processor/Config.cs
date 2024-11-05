using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.processor;

internal class Config(IConfiguration config) : infrastructure.configs.Config(config)
{
    public ConnectionStrings ConnectionStrings { get; set; } = config.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>()!;
}

internal class ConnectionStrings
{
    public required string DefaultConnection { get; set; }
}

internal static class ConfigExtensions
{
    internal static Config Get(this IConfiguration configuration) => new(configuration);
}