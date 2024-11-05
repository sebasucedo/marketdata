using marketdata.infrastructure;
using marketdata.persister;

var builder = Host.CreateApplicationBuilder(args);

IConfigurationRoot configuration = await GetConfiguration(builder);
builder.Services.AddServices(configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

static async Task<IConfigurationRoot> GetConfiguration(HostApplicationBuilder builder)
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