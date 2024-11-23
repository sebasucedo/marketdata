using marketdata.domain.security;
using marketdata.infrastructure.configs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.security;

public class CognitoTokenValidator(IOptions<AwsConfig> options) : ITokenValidator
{
    private readonly string _awsRegion = options.Value.Region;
    private readonly string _userPoolId = options.Value.Cognito!.UserPoolId;
    private readonly string _clientId = options.Value.Cognito!.ClientId;

    public async Task<bool> Validate(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        string validIssuer = $"https://cognito-idp.{_awsRegion}.amazonaws.com/{_userPoolId}";
        string validAudience = _clientId;

        var jwks = await GetCognitoJwks(_awsRegion, _userPoolId);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var signingKey = ResolveSigningKey(jwks, kid);
                return signingKey != null ? new[] { signingKey } : [];
            },
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            ValidateIssuer = true,
            ValidateAudience = true,
        };

        try
        {
            var claims = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return true;
        }
        catch (SecurityTokenException ex)
        {
            Serilog.Log.Debug($"Token validation failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"Unexpected error during token validation: {ex.Message}");
            return false;
        }
    }

    public static SecurityKey? ResolveSigningKey(JsonWebKeySet jwks, string kid)
    {
        var matchingKey = jwks.Keys.FirstOrDefault(k => k.Kid == kid);
        if (matchingKey is null)
        {
            Serilog.Log.Error($"No matching key found for kid: {kid}");
            return null;
        }

        return matchingKey;
    }

    public static async Task<JsonWebKeySet> GetCognitoJwks(string region, string userPoolId)
    {
        var jwksUri = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/jwks.json";
        using var httpClient = new HttpClient();

        try
        {
            var response = await httpClient.GetStringAsync(jwksUri);
            return new JsonWebKeySet(response);
        }
        catch (HttpRequestException ex)
        {
            Serilog.Log.Error($"Error fetching JWKs: {ex.Message}");
            throw new Exception("Unable to fetch JWKs from Cognito.", ex);
        }
        catch (JsonException ex)
        {
            Serilog.Log.Error($"Error parsing JWKs: {ex.Message}");
            throw new Exception("Invalid JWKs format received.", ex);
        }
    }
}
