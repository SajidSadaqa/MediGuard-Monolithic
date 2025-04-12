using Microsoft.AspNetCore.Mvc;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GatewayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Gateway/Status
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            // You can extend this endpoint to provide additional gateway diagnostics if needed.
            return Ok(new
            {
                status = "API Gateway is running",
                environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown"
            });
        }
    }
}
