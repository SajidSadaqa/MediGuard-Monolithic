using MediGuard.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace MediGuard.API.Services
{
    public interface IPillRecognitionService
    {
        Task<PillRecognitionResult> RecognizePillAsync(string imageBase64);
    }

    public class PillRecognitionService : IPillRecognitionService
    {
        private readonly ILogger<PillRecognitionService> _logger;
        private readonly AppDbContext _context;

        public PillRecognitionService(ILogger<PillRecognitionService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<PillRecognitionResult> RecognizePillAsync(string imageBase64)
        {
            try
            {
                // In a real implementation, this would call a computer vision API or ML model
                // For this simulation, we'll randomly select a medication from the database
                
                // Log the recognition attempt
                _logger.LogInformation("Attempting to recognize pill from image");
                
                // Simulate processing delay
                await Task.Delay(1000);
                
                // Get a random medication from the database
                var medications = await _context.Medications.ToListAsync();
                if (!medications.Any())
                {
                    return new PillRecognitionResult
                    {
                        Success = false,
                        Message = "No medications found in database for recognition simulation"
                    };
                }
                
                var random = new Random();
                var medication = medications[random.Next(medications.Count)];
                
                // Simulate confidence score between 70% and 95%
                var confidence = random.Next(70, 96) / 100.0m;
                
                return new PillRecognitionResult
                {
                    Success = true,
                    MedicationId = medication.Id,
                    MedicationName = medication.Name,
                    ScientificName = medication.ScientificName,
                    Confidence = confidence,
                    Message = $"Pill recognized as {medication.Name} with {confidence:P0} confidence"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recognizing pill");
                return new PillRecognitionResult
                {
                    Success = false,
                    Message = "An error occurred during pill recognition"
                };
            }
        }
    }

    public class PillRecognitionResult
    {
        public bool Success { get; set; }
        public int? MedicationId { get; set; }
        public string? MedicationName { get; set; }
        public string? ScientificName { get; set; }
        public decimal? Confidence { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PillRecognitionAlternative>? Alternatives { get; set; }
    }

    public class PillRecognitionAlternative
    {
        public int MedicationId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
    }
}
