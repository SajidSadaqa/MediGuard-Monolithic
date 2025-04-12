using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using System.Security.Claims;
using MediGuard.API.Helpers;

namespace MediGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserMedicationController : ControllerBase
    {
        private readonly IUserMedicationService _userMedicationService;
        private readonly IConflictChecker _conflictChecker;
        private readonly ILogger<UserMedicationController> _logger;

        public UserMedicationController(
            IUserMedicationService userMedicationService,
            IConflictChecker conflictChecker,
            ILogger<UserMedicationController> logger)
        {
            _userMedicationService = userMedicationService;
            _conflictChecker = conflictChecker;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserMedicationDto>>> GetUserMedications()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var medications = await _userMedicationService.GetUserMedicationsAsync(userId);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user medications");
                return StatusCode(500, new { message = "An error occurred while retrieving medications" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserMedicationDto>> GetUserMedicationById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var medication = await _userMedicationService.GetUserMedicationByIdAsync(id, userId);
                if (medication == null)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return Ok(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the medication" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserMedicationDto>> AddUserMedication([FromBody] CreateUserMedicationDto medicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var createdMedication = await _userMedicationService.AddUserMedicationAsync(userId, medicationDto);
                return CreatedAtAction(nameof(GetUserMedicationById), new { id = createdMedication.Id }, createdMedication);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error adding user medication");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding user medication");
                return StatusCode(500, new { message = "An error occurred while adding the medication" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUserMedication(int id, [FromBody] UpdateUserMedicationDto medicationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _userMedicationService.UpdateUserMedicationAsync(id, userId, medicationDto);
                if (!result)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the medication" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserMedication(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _userMedicationService.DeleteUserMedicationAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the medication" });
            }
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<ActionResult> ToggleUserMedicationStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _userMedicationService.ToggleUserMedicationStatusAsync(id, userId, isActive);
                if (!result)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user medication with ID {MedicationId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the medication status" });
            }
        }

        [HttpGet("conflicts")]
        public async Task<ActionResult<IEnumerable<MedicationConflictResult>>> CheckUserMedicationConflicts()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var conflicts = await _conflictChecker.CheckUserMedicationConflictsAsync(userId);
                return Ok(conflicts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user medication conflicts");
                return StatusCode(500, new { message = "An error occurred while checking medication conflicts" });
            }
        }

        [HttpGet("medication/{medicationId}/conflicts")]
        public async Task<ActionResult<IEnumerable<MedicationConflictResult>>> CheckMedicationConflicts(int medicationId)
        {
            try
            {
                var conflicts = await _conflictChecker.CheckMedicationConflictsAsync(medicationId);
                return Ok(conflicts);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error checking conflicts for medication with ID {MedicationId}", medicationId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error checking conflicts for medication with ID {MedicationId}", medicationId);
                return StatusCode(500, new { message = "An error occurred while checking medication conflicts" });
            }
        }
    }
}
