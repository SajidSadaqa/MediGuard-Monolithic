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
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationService recommendationService,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetUserRecommendations()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var recommendations = await _recommendationService.GetUserRecommendationsAsync(userId);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user recommendations");
                return StatusCode(500, new { message = "An error occurred while retrieving recommendations" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecommendationDto>> GetRecommendationById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var recommendation = await _recommendationService.GetRecommendationByIdAsync(id, userId);
                if (recommendation == null)
                {
                    return NotFound(new { message = $"Recommendation with ID {id} not found" });
                }

                return Ok(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendation with ID {RecommendationId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the recommendation" });
            }
        }

        [HttpPost("generate")]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GenerateRecommendations()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var recommendations = await _recommendationService.GenerateRecommendationsForUserAsync(userId);
                return Ok(recommendations);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating recommendations");
                return StatusCode(500, new { message = "An error occurred while generating recommendations" });
            }
        }

        [HttpPut("{id}/view")]
        public async Task<ActionResult> MarkRecommendationAsViewed(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _recommendationService.MarkRecommendationAsViewedAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = $"Recommendation with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking recommendation with ID {RecommendationId} as viewed", id);
                return StatusCode(500, new { message = "An error occurred while updating the recommendation" });
            }
        }

        [HttpPut("{id}/accept")]
        public async Task<ActionResult> AcceptRecommendation(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _recommendationService.AcceptRecommendationAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = $"Recommendation with ID {id} not found" });
                }

                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error accepting recommendation with ID {RecommendationId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error accepting recommendation with ID {RecommendationId}", id);
                return StatusCode(500, new { message = "An error occurred while accepting the recommendation" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRecommendation(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _recommendationService.DeleteRecommendationAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = $"Recommendation with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recommendation with ID {RecommendationId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the recommendation" });
            }
        }
    }
}
