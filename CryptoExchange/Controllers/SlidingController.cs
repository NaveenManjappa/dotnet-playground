using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CryptoExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("sliding")]
    public class SlidingController : ControllerBase
    {
        [HttpGet("price/{ticker}")]
        public IActionResult GetPrice(string ticker)
        {
            var price = Random.Shared.Next(20000, 70000);
            return Ok(new { Ticker = ticker.ToUpper(), Price = price, TimeStamp = DateTime.UtcNow });
        }
    }
}
