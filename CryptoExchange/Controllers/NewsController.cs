using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CryptoExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("token_bucket")]
    public class NewsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetNews()
        {
            return Ok(new
            {
                Headline = "Bitcoin reaches new highs!",
                Source = "CryptoDaily",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
