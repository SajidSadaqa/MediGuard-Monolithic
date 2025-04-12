using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MedicationService.Services;
using MedicationService.Models;
using MedicationService.Data;
using Microsoft.EntityFrameworkCore;

namespace MediGuard.MedicationService.Controllers
{
    [ApiController]
    [Route("medication")]
    public class MedicationController : ControllerBase
    {
        private readonly IExternalMedicationApiService _externalMedicationApiService;
        private readonly IAiConflictCheckService _aiConflictCheckService;
        private readonly MedicationDbContext _dbContext;

        public MedicationController(
            IExternalMedicationApiService externalMedicationApiService,
            IAiConflictCheckService aiConflictCheckService,
            MedicationDbContext dbContext)
        {
            _externalMedicationApiService = externalMedicationApiService;
            _aiConflictCheckService = aiConflictCheckService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// GET /medication/{id}
        /// Retrieves a single medication by its id. Fetches from DB first, then external API if not found.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedication(string id)
        {
            // Check local database first
            var medication = await _dbContext.Medications
                .FirstOrDefaultAsync(m => m.MedicationId == id);

            if (medication == null)
            {
                // Fetch from external API if not in DB
                medication = await _externalMedicationApiService.GetMedicationByIdAsync(id);
                if (medication == null)
                {
                    return NotFound(new { Message = $"Medication with id '{id}' was not found." });
                }

                // Save to local DB for future requests
                _dbContext.Medications.Add(medication);
                await _dbContext.SaveChangesAsync();
            }

            return Ok(medication);
        }

        /// <summary>
        /// POST /medication
        /// Adds a new medication to the local database.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddMedication([FromBody] Medication medication)
        {
            if (medication == null || string.IsNullOrWhiteSpace(medication.MedicationId))
            {
                return BadRequest(new { Message = "Medication data or ID is required." });
            }

            if (await _dbContext.Medications.AnyAsync(m => m.MedicationId == medication.MedicationId))
            {
                return Conflict(new { Message = $"Medication with ID '{medication.MedicationId}' already exists." });
            }

            _dbContext.Medications.Add(medication);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMedication), new { id = medication.MedicationId }, medication);
        }

        /// <summary>
        /// GET /medication/conflicts?meds=med1,med2,med3
        /// Checks for conflicts among the listed medications by calling an AI API.
        /// </summary>
        [HttpGet("conflicts")]
        public async Task<IActionResult> GetConflicts([FromQuery] string meds)
        {
            if (string.IsNullOrWhiteSpace(meds))
            {
                return BadRequest(new { Message = "Query parameter 'meds' is required." });
            }

            var medicationList = meds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var conflictResults = await _aiConflictCheckService.CheckConflictsAsync(medicationList);
            return Ok(conflictResults);
        }

        /// <summary>
        /// GET /medication/alternatives?drugId=...
        /// Suggests possible alternative medications for the provided drugId.
        /// </summary>
        [HttpGet("alternatives")]
        public async Task<IActionResult> GetAlternatives([FromQuery] string drugId)
        {
            if (string.IsNullOrWhiteSpace(drugId))
            {
                return BadRequest(new { Message = "Query parameter 'drugId' is required." });
            }

            var alternatives = await _externalMedicationApiService.GetAlternativesAsync(drugId);
            if (alternatives == null)
            {
                return NotFound(new { Message = $"No alternatives found for medication with id '{drugId}'." });
            }

            return Ok(alternatives);
        }
    }
}