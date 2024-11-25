using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using marketdata.domain;
using marketdata.domain.security;

namespace marketdata.notifier.Pages.Account;

[AllowAnonymous]
public class LoginModel(IIdentityService identityService) : PageModel
{
    private readonly IIdentityService _identityService = identityService;

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
            var authResponse = await _identityService.AuthenticateUser(Username, Password);

            if (authResponse is not null &&
                authResponse.AccessToken is not null &&
                authResponse.IdToken is not null &&
                authResponse.RefreshToken is not null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, Username),
                    new(Constants.ClaimTypes.ACCESS_TOKEN, authResponse.AccessToken),
                    new(Constants.ClaimTypes.ID_TOKEN, authResponse.IdToken),
                    new(Constants.ClaimTypes.REFRESH_TOKEN, authResponse.RefreshToken)
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

}
