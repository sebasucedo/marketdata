
namespace marketdata.api;

internal class Config(IConfiguration config) : infrastructure.configs.Config(config)
{
    public Alpaca Alpaca { get; set; } = config.GetSection(nameof(Alpaca)).Get<Alpaca>()!;
    public Polygon Polygon { get; set; } = config.GetSection(nameof(Polygon)).Get<Polygon>()!;
    public Yahoo Yahoo { get; set; } = config.GetSection(nameof(Yahoo)).Get<Yahoo>()!;
}

internal class Alpaca
{
    public required string Url { get; set; }
    public required string Key { get; set; }
    public required string Secret { get; set; }
}

internal class Polygon
{
    public required string Url { get; set; }
    public required string ApiKey { get; set; }
}

internal class Yahoo
{
    public required string Url { get; set; }

}

internal static class ConfigExtensions
{
    internal static Config Get(this IConfiguration configuration) => new(configuration);
}