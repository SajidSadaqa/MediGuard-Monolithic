using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using System.Security.Claims;

namespace MediGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatBotController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly ILogger<ChatBotController> _logger;

        public ChatBotController(IChatBotService chatBotService, ILogger<ChatBotController> logger)
        {
            _chatBotService = chatBotService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> GetChatResponse([FromBody] ChatRequestDto chatRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var response = await _chatBotService.GetChatResponseAsync(userId, chatRequest);
                return Ok(response);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error getting chat response");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting chat response");
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var chatHistory = await _chatBotService.GetUserChatHistoryAsync(userId);
                return Ok(chatHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat history");
                return StatusCode(500, new { message = "An error occurred while retrieving chat history" });
            }
        }
    }
}
