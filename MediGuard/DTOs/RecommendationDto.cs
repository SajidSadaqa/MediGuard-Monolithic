namespace MediGuard.API.DTOs
{
    public class RecommendationDto
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string RecommendationText { get; set; } = string.Empty;
        public decimal RecommendationScore { get; set; }
        public string? RecommendationReason { get; set; }
        public bool IsViewed { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRecommendationDto
    {
        public int MedicationId { get; set; }
        public string RecommendationText { get; set; } = string.Empty;
        public decimal RecommendationScore { get; set; }
        public string? RecommendationReason { get; set; }
    }
}
