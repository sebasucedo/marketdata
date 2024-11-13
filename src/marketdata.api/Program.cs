using marketdata.api;
using marketdata.infrastructure;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configuration = await GetConfiguration(builder);
builder.Services.AddServices(configuration);
builder.Services.AddOpenTelemetry(configuration);
builder.Services.AddLogger(configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.ConfigureRoutes();
app.Run();

static async Task<IConfigurationRoot> GetConfiguration(WebApplicationBuilder builder)
{
    IConfigurationRoot configuration;
    if (builder.Environment.IsDevelopment())
        configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build() ?? throw new Exception("Configuration is null");
    else
        configuration = await SecretsManagerHelper.GetConfigurationFromPlainText();

    return configuration;
}
