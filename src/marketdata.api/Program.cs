using marketdata.api;

var builder = WebApplication.CreateBuilder(args);

var configurationBuilder = new ConfigurationBuilder()
                               .SetBasePath(AppContext.BaseDirectory)
                               .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                               .AddEnvironmentVariables();
IConfiguration configuration = configurationBuilder.Build();
var config = configuration.Get();

builder.Services.ConfigureServices(config);

var app = builder.Build();

app.ConfigureRoutes();
app.Run();
