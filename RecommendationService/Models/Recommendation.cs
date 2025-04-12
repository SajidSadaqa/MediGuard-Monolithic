using System.Collections.Generic;

namespace RecommendationService.Models
{
    public class Recommendation
    {
        public string UserId { get; set; }
        public string RecommendationSummary { get; set; }
        public List<MedicationRecommendation> MedicationRecommendations { get; set; } = new();
    }

    public class MedicationRecommendation
    {
        public string MedicationId { get; set; }
        public string RecommendationUserId { get; set; } // Foreign key to Recommendation
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Timing { get; set; }
        public string RecommendationReason { get; set; }
    }
}