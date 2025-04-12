using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using System.Security.Claims;

namespace MediGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;
        private readonly ILogger<MedicationController> _logger;

        public MedicationController(IMedicationService medicationService, ILogger<MedicationController> logger)
        {
            _medicationService = medicationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicationDto>>> GetAllMedications()
        {
            try
            {
                var medications = await _medicationService.GetAllMedicationsAsync();
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medications");
                return StatusCode(500, new { message = "An error occurred while retrieving medications" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicationDto>> GetMedicationById(int id)
        {
            try
            {
                var medication = await _medicationService.GetMedicationByIdAsync(id);
                if (medication == null)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }
                return Ok(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the medication" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MedicationDto>> CreateMedication([FromBody] MedicationInputDto medicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdMedication = await _medicationService.CreateMedicationAsync(medicationDto);
                return CreatedAtAction(nameof(GetMedicationById), new { id = createdMedication.Id }, createdMedication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication");
                return StatusCode(500, new { message = "An error occurred while creating the medication" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateMedication(int id, [FromBody] MedicationInputDto medicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _medicationService.UpdateMedicationAsync(id, medicationDto);
                if (!result)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the medication" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteMedication(int id)
        {
            try
            {
                var result = await _medicationService.DeleteMedicationAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the medication" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MedicationDto>>> SearchMedications([FromQuery] string searchTerm)
        {
            try
            {
                var medications = await _medicationService.SearchMedicationsAsync(searchTerm);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching medications with term {SearchTerm}", searchTerm);
                return StatusCode(500, new { message = "An error occurred while searching for medications" });
            }
        }
    }
}
