using MediGuard.API.DTOs;

namespace MediGuard.API.DTOs
{
    public class UserMedicationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DosageInstructions { get; set; }
        public string? Frequency { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateUserMedicationDto
    {
        public int MedicationId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public string? DosageInstructions { get; set; }
        public string? Frequency { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateUserMedicationDto
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DosageInstructions { get; set; }
        public string? Frequency { get; set; }
        public string? Notes { get; set; }
    }
}
