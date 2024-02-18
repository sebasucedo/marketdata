using marketdata.workerservice;
using marketdata.api;
using marketdata.domain;
using marketdata.infraestructure;

var builder = WebApplication.CreateBuilder(args);

var configurationBuilder = new ConfigurationBuilder()
                               .SetBasePath(AppContext.BaseDirectory)
                               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                               .AddEnvironmentVariables();
IConfiguration configuration = configurationBuilder.Build();
var config = configuration.Get();

var sharedCts = new CancellationTokenSource();
builder.Services.AddSingleton(sharedCts);
builder.Services.AddSingleton<IMarketSocket>(provider => new AlpacaSocket(
    provider.GetRequiredService<ILogger<AlpacaSocket>>(),
    config.Alpaca.Url,
    config.Alpaca.Key,
    config.Alpaca.Secret
    ));

builder.Services.AddTransient<ITradeGateway, TradeDAO>();
builder.Services.AddTransient<MessageHandlerInteractor>();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/", () => "Marketdata API!");

app.MapGet("/stop", (CancellationTokenSource sharedCts) =>
{
    sharedCts.Cancel();
    return Results.Ok("Cancel signal sent.");
});

app.MapGet("/subscribe", async (IMarketSocket socket) =>
{
    string[] symbols = { "MSFT", "SPY" };
    await socket.Subscribe(symbols);
    return Results.Ok();
});

app.Run();
