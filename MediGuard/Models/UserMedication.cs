using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediGuard.API.Models
{
    public class UserMedication
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
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [MaxLength(100)]
        public string? DosageInstructions { get; set; }
        
        [MaxLength(50)]
        public string? Frequency { get; set; } // e.g., "Once daily", "Twice daily", "Every 8 hours"
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
