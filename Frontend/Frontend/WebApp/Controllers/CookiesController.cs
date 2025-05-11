using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("cookies")]
    public class CookiesController : Controller
    {
        [HttpPost("setcookies")]
        public IActionResult SetCookies([FromBody] CookieConsent consent)
        {
            var jsonConsent = System.Text.Json.JsonSerializer.Serialize(consent);
            Response.Cookies.Append("cookieConsent", jsonConsent, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

            return Ok();
        }
    }
}