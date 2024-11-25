using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using marketdata.domain.security;
using marketdata.infrastructure.configs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.security;

public class CognitoIdentityService(AmazonCognitoIdentityProviderClient cognitoClient, IOptions<AwsConfig> options) : IIdentityService
{
    private readonly AmazonCognitoIdentityProviderClient _cognitoClient = cognitoClient;
    private readonly string _awsRegion = options.Value.Region;
    private readonly string _userPoolId = options.Value.Cognito!.UserPoolId;
    private readonly string _clientId = options.Value.Cognito!.ClientId;
    private readonly string _clientSecret = options.Value.Cognito!.ClientSecret;

    public async Task<AuthenticationResponse?> AuthenticateUser(string username, string password)
    {
        var secretHash = CalculateSecretHash(username, _clientId, _clientSecret);

        var authRequest = new AdminInitiateAuthRequest
        {
            UserPoolId = _userPoolId,
            ClientId = _clientId,
            AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                { Constants.Keys.USERNAME, username },
                { Constants.Keys.PASSWORD, password },
                { Constants.Keys.SECRET_HASH, secretHash }
            }
        };

        try
        {
            var response = await _cognitoClient.AdminInitiateAuthAsync(authRequest);

            var authenticationResult = response.AuthenticationResult;
            var result = new AuthenticationResponse(authenticationResult.AccessToken,
                                                    authenticationResult.IdToken,
                                                    authenticationResult.RefreshToken,
                                                    authenticationResult.ExpiresIn,
                                                    null,
                                                    null);

            return result;
        }
        catch (NotAuthorizedException ex)
        {
            Serilog.Log.Debug(ex, "Authentication failed.");
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Unexpected error during authentication.");
        }
        return null;
    }

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
            Serilog.Log.Debug(ex, "Token validation failed: {ErrorMessage}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Unexpected error during token validation: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    private static JsonWebKey? ResolveSigningKey(JsonWebKeySet jwks, string kid)
    {
        var matchingKey = jwks.Keys.FirstOrDefault(k => k.Kid == kid);
        if (matchingKey is null)
        {
            Serilog.Log.Error("No matching key found for kid: {kid}", kid);
            return null;
        }

        return matchingKey;
    }

    private static async Task<JsonWebKeySet> GetCognitoJwks(string region, string userPoolId)
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

    private static string CalculateSecretHash(string username, string clientId, string clientSecret)
    {
        var message = $"{username}{clientId}";
        var keyBytes = Encoding.UTF8.GetBytes(clientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
