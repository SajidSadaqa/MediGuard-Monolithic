using MediGuard.API.Data;
using MediGuard.API.DTOs;
using MediGuard.API.Helpers;
using MediGuard.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuard.API.Services
{
    public interface IUserMedicationService
    {
        Task<List<UserMedicationDto>> GetUserMedicationsAsync(string userId);
        Task<UserMedicationDto?> GetUserMedicationByIdAsync(int id, string userId);
        Task<UserMedicationDto> AddUserMedicationAsync(string userId, CreateUserMedicationDto medicationDto);
        Task<bool> UpdateUserMedicationAsync(int id, string userId, UpdateUserMedicationDto medicationDto);
        Task<bool> DeleteUserMedicationAsync(int id, string userId);
        Task<bool> ToggleUserMedicationStatusAsync(int id, string userId, bool isActive);
    }

    public class UserMedicationService : IUserMedicationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserMedicationService> _logger;
        private readonly IConflictChecker _conflictChecker;

        public UserMedicationService(
            AppDbContext context, 
            ILogger<UserMedicationService> logger,
            IConflictChecker conflictChecker)
        {
            _context = context;
            _logger = logger;
            _conflictChecker = conflictChecker;
        }

        public async Task<List<UserMedicationDto>> GetUserMedicationsAsync(string userId)
        {
            var userMedications = await _context.UserMedications
                .Where(um => um.UserId == userId)
                .Include(um => um.Medication)
                .OrderByDescending(um => um.IsActive)
                .ThenByDescending(um => um.CreatedAt)
                .ToListAsync();

            return userMedications.Select(MapToDto).ToList();
        }

        public async Task<UserMedicationDto?> GetUserMedicationByIdAsync(int id, string userId)
        {
            var userMedication = await _context.UserMedications
                .Include(um => um.Medication)
                .FirstOrDefaultAsync(um => um.Id == id && um.UserId == userId);

            return userMedication != null ? MapToDto(userMedication) : null;
        }

        public async Task<UserMedicationDto> AddUserMedicationAsync(string userId, CreateUserMedicationDto medicationDto)
        {
            // Check if medication exists
            var medication = await _context.Medications.FindAsync(medicationDto.MedicationId);
            if (medication == null)
            {
                throw new ApplicationException($"Medication with ID {medicationDto.MedicationId} not found");
            }

            // Check for conflicts with existing medications
            var hasConflict = await _conflictChecker.CheckConflictAsync(medicationDto.MedicationId, userId);
            if (hasConflict)
            {
                throw new ApplicationException($"Adding {medication.Name} would create a conflict with your existing medications");
            }

            var userMedication = new UserMedication
            {
                UserId = userId,
                MedicationId = medicationDto.MedicationId,
                StartDate = medicationDto.StartDate,
                EndDate = medicationDto.EndDate,
                DosageInstructions = medicationDto.DosageInstructions,
                Frequency = medicationDto.Frequency,
                IsActive = true,
                Notes = medicationDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserMedications.Add(userMedication);
            await _context.SaveChangesAsync();

            // Reload with medication included
            userMedication = await _context.UserMedications
                .Include(um => um.Medication)
                .FirstOrDefaultAsync(um => um.Id == userMedication.Id);

            return MapToDto(userMedication!);
        }

        public async Task<bool> UpdateUserMedicationAsync(int id, string userId, UpdateUserMedicationDto medicationDto)
        {
            var userMedication = await _context.UserMedications
                .FirstOrDefaultAsync(um => um.Id == id && um.UserId == userId);

            if (userMedication == null)
            {
                return false;
            }

            userMedication.StartDate = medicationDto.StartDate;
            userMedication.EndDate = medicationDto.EndDate;
            userMedication.DosageInstructions = medicationDto.DosageInstructions;
            userMedication.Frequency = medicationDto.Frequency;
            userMedication.Notes = medicationDto.Notes;
            userMedication.UpdatedAt = DateTime.UtcNow;

            _context.UserMedications.Update(userMedication);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserMedicationAsync(int id, string userId)
        {
            var userMedication = await _context.UserMedications
                .FirstOrDefaultAsync(um => um.Id == id && um.UserId == userId);

            if (userMedication == null)
            {
                return false;
            }

            _context.UserMedications.Remove(userMedication);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleUserMedicationStatusAsync(int id, string userId, bool isActive)
        {
            var userMedication = await _context.UserMedications
                .FirstOrDefaultAsync(um => um.Id == id && um.UserId == userId);

            if (userMedication == null)
            {
                return false;
            }

            userMedication.IsActive = isActive;
            userMedication.UpdatedAt = DateTime.UtcNow;

            _context.UserMedications.Update(userMedication);
            await _context.SaveChangesAsync();

            return true;
        }

        private UserMedicationDto MapToDto(UserMedication userMedication)
        {
            return new UserMedicationDto
            {
                Id = userMedication.Id,
                UserId = userMedication.UserId,
                MedicationId = userMedication.MedicationId,
                MedicationName = userMedication.Medication?.Name ?? "Unknown",
                StartDate = userMedication.StartDate,
                EndDate = userMedication.EndDate,
                DosageInstructions = userMedication.DosageInstructions,
                Frequency = userMedication.Frequency,
                IsActive = userMedication.IsActive,
                Notes = userMedication.Notes,
                CreatedAt = userMedication.CreatedAt,
                UpdatedAt = userMedication.UpdatedAt
            };
        }
    }
}
