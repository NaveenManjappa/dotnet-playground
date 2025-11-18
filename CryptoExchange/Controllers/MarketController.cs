using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CryptoExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("tiered_policy")]
    public class MarketController : ControllerBase
    {
        [HttpGet("price/{ticker}")]
        public IActionResult GetPrice(string ticker)
        {
            var price = Random.Shared.Next(20000, 70000);
            return Ok(new { Ticker = ticker.ToUpper(), Price = price, TimeStamp = DateTime.UtcNow });
        }


        [HttpGet("heavy-report")]
        [EnableRateLimiting("concurrency_policy")]
        public async Task<IActionResult> GetHeavyReport()
        {
            await Task.Delay(3000);
            return Ok(new { Message = "Report generated", TimeStamp = DateTime.UtcNow });
        }
    }
}
