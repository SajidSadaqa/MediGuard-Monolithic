using System.Text.Json.Serialization;

namespace MediGuard.API.DTOs
{
    public class MedicationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public string? Strength { get; set; }
        public string? ImageUrl { get; set; }
        public bool RequiresPrescription { get; set; }
        public bool IsAvailable { get; set; }
        public List<string>? ConflictsWith { get; set; }
    }

    public class MedicationInputDto
    {
        public string Name { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public string? Strength { get; set; }
        public string? ImageUrl { get; set; }
        public bool RequiresPrescription { get; set; }
        public bool IsAvailable { get; set; } = true;
        public List<string>? ConflictsWith { get; set; }
    }
}
