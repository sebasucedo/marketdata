namespace marketdata.api;

public class Config(IConfiguration config)
{
    public ConnectionStrings ConnectionStrings { get; set; } = config.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>()!;
    public Alpaca Alpaca { get; set; } = config.GetSection(nameof(Alpaca)).Get<Alpaca>()!;
    public Polygon Polygon { get; set; } = config.GetSection(nameof(Polygon)).Get<Polygon>()!;
    public Yahoo Yahoo { get; set; } = config.GetSection(nameof(Yahoo)).Get<Yahoo>()!;
}

public class ConnectionStrings
{
    public required string DefaultConnection { get; set; }
}
public class Alpaca
{
    public required string Url { get; set; }
    public required string Key { get; set; }
    public required string Secret { get; set; }
}

public class Polygon
{
    public required string Url { get; set; }
    public required string ApiKey { get; set; }
}

public class Yahoo
{
    public required string Url { get; set; }

}

internal static class ConfigExtensions
{
    internal static Config Get(this IConfiguration configuration) => new(configuration);
}