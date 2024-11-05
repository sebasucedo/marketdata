namespace marketdata.notifier;

internal class Config(IConfiguration config) : infrastructure.configs.Config(config)
{
}

internal static class ConfigExtensions
{
    internal static Config Get(this IConfiguration configuration) => new(configuration);
}
