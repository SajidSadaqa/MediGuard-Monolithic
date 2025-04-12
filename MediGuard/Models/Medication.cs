using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MediGuard.API.Models
{
    public class Medication
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string ScientificName { get; set; } = string.Empty;
        
        [Required]
        public decimal Price { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(200)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(50)]
        public string? DosageForm { get; set; } // e.g., tablet, capsule, liquid
        
        [MaxLength(50)]
        public string? Strength { get; set; } // e.g., 500mg, 10mg/ml
        
        public string? ImageUrl { get; set; }
        
        public bool RequiresPrescription { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        // Stored as JSON string in the database
        public string? ConflictsWith { get; set; }
        
        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        [JsonIgnore]
        public virtual ICollection<UserMedication> UserMedications { get; set; } = new List<UserMedication>();
    }
}
