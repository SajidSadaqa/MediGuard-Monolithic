using MediGuard.API.Data;
using MediGuard.API.DTOs;
using MediGuard.API.Models;
using MediGuard.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MediGuard.API.Services
{
    public interface IRecommendationService
    {
        Task<List<RecommendationDto>> GetUserRecommendationsAsync(string userId);
        Task<RecommendationDto?> GetRecommendationByIdAsync(int id, string userId);
        Task<RecommendationDto> CreateRecommendationAsync(string userId, CreateRecommendationDto recommendationDto);
        Task<bool> MarkRecommendationAsViewedAsync(int id, string userId);
        Task<bool> AcceptRecommendationAsync(int id, string userId);
        Task<bool> DeleteRecommendationAsync(int id, string userId);
        Task<List<RecommendationDto>> GenerateRecommendationsForUserAsync(string userId);
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecommendationService> _logger;
        private readonly IConflictChecker _conflictChecker;

        public RecommendationService(
            AppDbContext context,
            ILogger<RecommendationService> logger,
            IConflictChecker conflictChecker)
        {
            _context = context;
            _logger = logger;
            _conflictChecker = conflictChecker;
        }

        public async Task<List<RecommendationDto>> GetUserRecommendationsAsync(string userId)
        {
            var recommendations = await _context.Recommendations
                .Where(r => r.UserId == userId)
                .Include(r => r.Medication)
                .OrderByDescending(r => r.RecommendationScore)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            return recommendations.Select(MapToDto).ToList();
        }

        public async Task<RecommendationDto?> GetRecommendationByIdAsync(int id, string userId)
        {
            var recommendation = await _context.Recommendations
                .Include(r => r.Medication)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            return recommendation != null ? MapToDto(recommendation) : null;
        }

        public async Task<RecommendationDto> CreateRecommendationAsync(string userId, CreateRecommendationDto recommendationDto)
        {
            // Check if medication exists
            var medication = await _context.Medications.FindAsync(recommendationDto.MedicationId);
            if (medication == null)
            {
                throw new ApplicationException($"Medication with ID {recommendationDto.MedicationId} not found");
            }

            var recommendation = new Recommendation
            {
                UserId = userId,
                MedicationId = recommendationDto.MedicationId,
                RecommendationText = recommendationDto.RecommendationText,
                RecommendationScore = recommendationDto.RecommendationScore,
                RecommendationReason = recommendationDto.RecommendationReason,
                IsViewed = false,
                IsAccepted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Recommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            // Reload with medication included
            recommendation = await _context.Recommendations
                .Include(r => r.Medication)
                .FirstOrDefaultAsync(r => r.Id == recommendation.Id);

            return MapToDto(recommendation!);
        }

        public async Task<bool> MarkRecommendationAsViewedAsync(int id, string userId)
        {
            var recommendation = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation == null)
            {
                return false;
            }

            recommendation.IsViewed = true;
            recommendation.ViewedAt = DateTime.UtcNow;

            _context.Recommendations.Update(recommendation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AcceptRecommendationAsync(int id, string userId)
        {
            var recommendation = await _context.Recommendations
                .Include(r => r.Medication)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation == null || recommendation.Medication == null)
            {
                return false;
            }

            // Check for conflicts before accepting
            var hasConflict = await _conflictChecker.CheckConflictAsync(recommendation.MedicationId, userId);
            if (hasConflict)
            {
                throw new ApplicationException($"Cannot accept recommendation for {recommendation.Medication.Name} due to conflicts with your existing medications");
            }

            // Mark recommendation as accepted
            recommendation.IsAccepted = true;
            recommendation.AcceptedAt = DateTime.UtcNow;

            // Add medication to user's medications
            var userMedication = new UserMedication
            {
                UserId = userId,
                MedicationId = recommendation.MedicationId,
                StartDate = DateTime.UtcNow,
                DosageInstructions = "As recommended by your healthcare provider",
                Frequency = "As needed",
                IsActive = true,
                Notes = $"Added from recommendation: {recommendation.RecommendationText}",
                CreatedAt = DateTime.UtcNow
            };

            _context.UserMedications.Add(userMedication);
            _context.Recommendations.Update(recommendation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteRecommendationAsync(int id, string userId)
        {
            var recommendation = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation == null)
            {
                return false;
            }

            _context.Recommendations.Remove(recommendation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<RecommendationDto>> GenerateRecommendationsForUserAsync(string userId)
        {
            // Get user information
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            // Get user's current medications
            var userMedications = await _context.UserMedications
                .Where(um => um.UserId == userId && um.IsActive)
                .Include(um => um.Medication)
                .ToListAsync();

            var userMedicationIds = userMedications.Select(um => um.MedicationId).ToList();

            // Get all medications that the user is not currently taking
            var availableMedications = await _context.Medications
                .Where(m => !userMedicationIds.Contains(m.Id) && m.IsAvailable)
                .ToListAsync();

            // Generate recommendations based on user conditions and current medications
            var recommendations = new List<Recommendation>();

            // This is a simplified recommendation algorithm
            // In a real application, this would be more sophisticated and possibly use machine learning
            if (user.Conditions != null)
            {
                var conditions = user.Conditions.ToLower();

                foreach (var medication in availableMedications)
                {
                    // Skip medications that would conflict with user's current medications
                    bool hasConflict = false;
                    foreach (var userMed in userMedications)
                    {
                        if (userMed.Medication == null) continue;
                        
                        if (HasConflict(medication, userMed.Medication) || HasConflict(userMed.Medication, medication))
                        {
                            hasConflict = true;
                            break;
                        }
                    }

                    if (hasConflict) continue;

                    // Simple condition-based recommendations
                    decimal score = 0;
                    string? reason = null;

                    if (conditions.Contains("asthma") && 
                        (medication.Name.Contains("Inhaler") || medication.Description?.Contains("asthma") == true))
                    {
                        score = 90;
                        reason = "Recommended for asthma management";
                    }
                    else if (conditions.Contains("diabetes") && 
                             (medication.Name.Contains("Insulin") || medication.Description?.Contains("diabetes") == true))
                    {
                        score = 85;
                        reason = "Recommended for diabetes management";
                    }
                    else if (conditions.Contains("hypertension") && 
                             (medication.Name.Contains("Pressure") || medication.Description?.Contains("blood pressure") == true))
                    {
                        score = 80;
                        reason = "Recommended for blood pressure management";
                    }
                    else if (conditions.Contains("pain") && 
                             (medication.Name.Contains("Pain") || medication.Description?.Contains("pain") == true))
                    {
                        score = 75;
                        reason = "Recommended for pain management";
                    }
                    else
                    {
                        // General recommendations with lower scores
                        score = 50;
                        reason = "General health recommendation";
                    }

                    // Only add recommendations with a score above a threshold
                    if (score >= 50)
                    {
                        recommendations.Add(new Recommendation
                        {
                            UserId = userId,
                            MedicationId = medication.Id,
                            RecommendationText = $"Consider taking {medication.Name} for your health needs",
                            RecommendationScore = score,
                            RecommendationReason = reason,
                            IsViewed = false,
                            IsAccepted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // Sort by score and take top 5
            var topRecommendations = recommendations
                .OrderByDescending(r => r.RecommendationScore)
                .Take(5)
                .ToList();

            // Save recommendations to database
            _context.Recommendations.AddRange(topRecommendations);
            await _context.SaveChangesAsync();

            // Load medications for the recommendations
            foreach (var recommendation in topRecommendations)
            {
                await _context.Entry(recommendation)
                    .Reference(r => r.Medication)
                    .LoadAsync();
            }

            return topRecommendations.Select(MapToDto).ToList();
        }

        private RecommendationDto MapToDto(Recommendation recommendation)
        {
            return new RecommendationDto
            {
                Id = recommendation.Id,
                MedicationId = recommendation.MedicationId,
                MedicationName = recommendation.Medication?.Name ?? "Unknown",
                RecommendationText = recommendation.RecommendationText,
                RecommendationScore = recommendation.RecommendationScore,
                RecommendationReason = recommendation.RecommendationReason,
                IsViewed = recommendation.IsViewed,
                IsAccepted = recommendation.IsAccepted,
                CreatedAt = recommendation.CreatedAt
            };
        }

        private bool HasConflict(Medication medication1, Medication medication2)
        {
            if (string.IsNullOrEmpty(medication1.ConflictsWith))
            {
                return false;
            }

            try
            {
                var conflictsList = JsonSerializer.Deserialize<List<string>>(medication1.ConflictsWith);
                if (conflictsList == null)
                {
                    return false;
                }

                return conflictsList.Contains(medication2.ScientificName, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing ConflictsWith for medication {MedicationId}", medication1.Id);
                return false;
            }
        }
    }
}
