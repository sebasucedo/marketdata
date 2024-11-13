using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amazon.CognitoIdentityProvider;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Authorization;
using marketdata.domain;
using Microsoft.Extensions.Options;
using marketdata.notifier.config;

namespace marketdata.notifier.Pages.Account;

[AllowAnonymous]
public class LoginModel(AmazonCognitoIdentityProviderClient cognitoClient, IOptions<CognitoConfig> cognitoConfig) : PageModel
{
    private readonly AmazonCognitoIdentityProviderClient _cognitoClient = cognitoClient;
    private readonly string _userPoolId = cognitoConfig.Value.UserPoolId;
    private readonly string _clientId = cognitoConfig.Value.ClientId;
    private readonly string _clientSecret = cognitoConfig.Value.ClientSecret;

    [BindProperty]
    public required string Username { get; set; }

    [BindProperty]
    public required string Password { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ModelState.AddModelError(string.Empty, "Username and password are required.");
            return Page();
        }

        try
        {
            var authResponse = await AuthenticateUserAsync(Username, Password);

            if (authResponse.AuthenticationResult != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, Username),
                    new(Constants.ClaimTypes.ACCESS_TOKEN, authResponse.AuthenticationResult.AccessToken),
                    new(Constants.ClaimTypes.ID_TOKEN, authResponse.AuthenticationResult.IdToken),
                    new(Constants.ClaimTypes.REFRESH_TOKEN, authResponse.AuthenticationResult.RefreshToken)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme, 
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                        });

                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Authentication failed.");
                return Page();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
            return Page();
        }
    }

    private async Task<AdminInitiateAuthResponse> AuthenticateUserAsync(string username, string password)
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

        var response = await _cognitoClient.AdminInitiateAuthAsync(authRequest);
        return response;
    }

    private static string CalculateSecretHash(string username, string clientId, string clientSecret)
    {
        var message = $"{username}{clientId}";
        var keyBytes = Encoding.UTF8.GetBytes(clientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

}
