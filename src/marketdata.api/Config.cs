namespace marketdata.api;

public class Config(IConfiguration config)
{
    public Alpaca Alpaca { get; set; } = config.GetSection(nameof(Alpaca)).Get<Alpaca>()!;
}

public class Alpaca
{
    public required string Url { get; set; }
    public required string Key { get; set; }
    public required string Secret { get; set; }
}

internal static class ConfigExtensions
{
    internal static Config Get(this IConfiguration configuration) => new(configuration);
}