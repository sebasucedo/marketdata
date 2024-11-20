using marketdata.infrastructure;
using marketdata.socket;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6000);
    //options.ListenAnyIP(6001, listenOptions => listenOptions.UseHttps());
}); 

IConfigurationRoot configuration = await GetConfiguration(builder);
builder.Services.AddServices(configuration);

var app = builder.Build();

app.UseWebSockets();
app.AddEndpoints();

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