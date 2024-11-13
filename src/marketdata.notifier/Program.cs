using marketdata.infrastructure;
using marketdata.notifier;
using marketdata.notifier.hubs;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configuration = await GetConfiguration(builder);
builder.Services.AddServices(configuration);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddAuthentication(configuration);
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    builder.WebHost.UseSetting("browserlink", "false");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<TradeHub>("/trade-hub");

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