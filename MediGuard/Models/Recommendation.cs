using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuard.API.Models
{
    public class Recommendation
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        [Required]
        public int MedicationId { get; set; }
        
        [ForeignKey("MedicationId")]
        public virtual Medication? Medication { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string RecommendationText { get; set; } = string.Empty;
        
        [Required]
        public decimal RecommendationScore { get; set; } // 0-100 score indicating strength of recommendation
        
        [MaxLength(200)]
        public string? RecommendationReason { get; set; }
        
        public bool IsViewed { get; set; } = false;
        
        public bool IsAccepted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ViewedAt { get; set; }
        
        public DateTime? AcceptedAt { get; set; }
    }
}
