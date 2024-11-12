using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace marketdata.notifier.controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpGet("token")]
        public IActionResult GetAccessToken()
        {
            var accessToken = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Access token not found");
         
            return Ok(new { AccessToken = accessToken });
        }
    }
}
