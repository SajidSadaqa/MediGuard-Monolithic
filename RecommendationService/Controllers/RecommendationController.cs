using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RecommendationService.Models;
using RecommendationService.Services;
using RecommendationService.Data;
using Microsoft.EntityFrameworkCore;

namespace RecommendationService.Controllers
{
    [ApiController]
    [Route("recommendation")]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationEngine _recommendationEngine;
        private readonly RecommendationDbContext _dbContext;

        public RecommendationController(
            IRecommendationEngine recommendationEngine,
            RecommendationDbContext dbContext)
        {
            _recommendationEngine = recommendationEngine;
            _dbContext = dbContext;
        }

        /// <summary>
        /// GET /recommendation/user/{userId}
        /// Returns recommended medications or a dosing schedule for a given user.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRecommendationsForUser(string userId)
        {
            var existingRecommendation = await _dbContext.Recommendations
                .Include(r => r.MedicationRecommendations)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (existingRecommendation != null)
            {
                return Ok(existingRecommendation);
            }

            var recommendation = await _recommendationEngine.GenerateRecommendationsAsync(userId);
            if (recommendation == null)
            {
                return NotFound(new { Message = $"No recommendations available for user: {userId}" });
            }

            _dbContext.Recommendations.Add(recommendation);
            await _dbContext.SaveChangesAsync();

            return Ok(recommendation);
        }

        /// <summary>
        /// POST /recommendation
        /// Adds or updates a recommendation for a user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateRecommendation([FromBody] Recommendation recommendation)
        {
            if (recommendation == null || string.IsNullOrWhiteSpace(recommendation.UserId))
            {
                return BadRequest(new { Message = "Invalid recommendation data." });
            }

            var existing = await _dbContext.Recommendations
                .Include(r => r.MedicationRecommendations)
                .FirstOrDefaultAsync(r => r.UserId == recommendation.UserId);

            if (existing != null)
            {
                _dbContext.MedicationRecommendations.RemoveRange(existing.MedicationRecommendations);
                existing.RecommendationSummary = recommendation.RecommendationSummary;
                existing.MedicationRecommendations = recommendation.MedicationRecommendations;
            }
            else
            {
                _dbContext.Recommendations.Add(recommendation);
            }

            await _dbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecommendationsForUser), new { userId = recommendation.UserId }, recommendation);
        }
    }
}