using Microsoft.AspNetCore.Mvc;
using AdherenceService.Models;
using AdherenceService.Services;
using AdherenceService.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AdherenceService.Controllers
{
    [ApiController]
    [Route("adherence")]
    public class AdherenceController : ControllerBase
    {
        private readonly IAdherencePredictionService _predictionService;
        private readonly AdherenceDbContext _dbContext;

        public AdherenceController(
            IAdherencePredictionService predictionService,
            AdherenceDbContext dbContext)
        {
            _predictionService = predictionService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// GET /adherence/user/{userId}
        /// Retrieves adherence records (and predictions) for a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAdherenceRecords(string userId)
        {
            var records = await _dbContext.AdherenceRecords
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (records == null || records.Count == 0)
            {
                return NotFound(new { Message = $"No adherence records found for user: {userId}" });
            }

            // Enhance with predictions
            var predictedRecords = await _predictionService.GetAdherenceRecordsAsync(userId);
            return Ok(predictedRecords);
        }

        /// <summary>
        /// POST /adherence
        /// Adds a new adherence record.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddAdherenceRecord([FromBody] AdherenceRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.UserId) || string.IsNullOrWhiteSpace(record.MedicationId))
            {
                return BadRequest(new { Message = "Invalid adherence record data." });
            }

            if (await _dbContext.AdherenceRecords.AnyAsync(r =>
                r.UserId == record.UserId &&
                r.MedicationId == record.MedicationId &&
                r.ScheduledDoseTime == record.ScheduledDoseTime))
            {
                return Conflict(new { Message = "Adherence record already exists." });
            }

            _dbContext.AdherenceRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdherenceRecords), new { userId = record.UserId }, record);
        }

        /// <summary>
        /// POST /adherence/alert
        /// Triggers an adherence alert for a given record.
        /// </summary>
        [HttpPost("alert")]
        public async Task<IActionResult> TriggerAdherenceAlert([FromBody] AdherenceRecord alertRequest)
        {
            if (alertRequest == null || string.IsNullOrWhiteSpace(alertRequest.UserId))
            {
                return BadRequest(new { Message = "Invalid alert request data." });
            }

            var record = await _dbContext.AdherenceRecords
                .FirstOrDefaultAsync(r =>
                    r.UserId == alertRequest.UserId &&
                    r.MedicationId == alertRequest.MedicationId &&
                    r.ScheduledDoseTime == alertRequest.ScheduledDoseTime);

            if (record == null)
            {
                return NotFound(new { Message = "Adherence record not found." });
            }

            bool result = await _predictionService.TriggerAlertAsync(record);
            if (!result)
            {
                return StatusCode(500, new { Message = "Failed to trigger adherence alert." });
            }

            record.AlertTriggered = true;
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Adherence alert triggered successfully." });
        }
    }
}