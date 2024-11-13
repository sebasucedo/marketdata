using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using marketdata.domain;
using marketdata.infrastructure;
using marketdata.notifier.config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace marketdata.notifier;

internal static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();

        services.AddTransient<ITradeGateway, TradeNotifier>();
        services.AddTransient<IQuoteGateway, QuoteNotifier>();

        services.AddMassTransitAmazonSqsConsumers(config.Aws);

        services.Configure<CognitoConfig>(configuration.GetSection("Aws:Cognito"));

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var config = configuration.Get();
        if (config.Aws.Cognito is null)
            throw new InvalidOperationException("AWS Cognito config missing");

        string userPoolUrl = $"https://cognito-idp.us-east-1.amazonaws.com/{config.Aws.Cognito.UserPoolId}";

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = userPoolUrl;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidIssuer = userPoolUrl,
                };
            });

        services.AddSingleton(provider => 
            {
                var credentials = new BasicAWSCredentials(config.Aws.AccessKey, config.Aws.SecretKey);
                var region = RegionEndpoint.GetBySystemName(config.Aws.Region);
                return new AmazonCognitoIdentityProviderClient(credentials, region); 
            });

        return services;
    }
}
