using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediGuard.API.Services;
using System.Security.Claims;

namespace MediGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PillRecognitionController : ControllerBase
    {
        private readonly IPillRecognitionService _pillRecognitionService;
        private readonly ILogger<PillRecognitionController> _logger;

        public PillRecognitionController(
            IPillRecognitionService pillRecognitionService,
            ILogger<PillRecognitionController> logger)
        {
            _pillRecognitionService = pillRecognitionService;
            _logger = logger;
        }

        [HttpPost("recognize")]
        public async Task<ActionResult<PillRecognitionResult>> RecognizePill([FromBody] PillRecognitionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ImageBase64))
                {
                    return BadRequest(new { message = "Image data is required" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _pillRecognitionService.RecognizePillAsync(request.ImageBase64);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recognizing pill");
                return StatusCode(500, new { message = "An error occurred while recognizing the pill" });
            }
        }
    }

    public class PillRecognitionRequest
    {
        public string ImageBase64 { get; set; } = string.Empty;
    }
}
