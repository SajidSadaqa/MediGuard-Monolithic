using MediGuard.API.Data;
using MediGuard.API.DTOs;
using MediGuard.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MediGuard.API.Services
{
    public interface IMedicationService
    {
        Task<List<MedicationDto>> GetAllMedicationsAsync();
        Task<MedicationDto?> GetMedicationByIdAsync(int id);
        Task<MedicationDto> CreateMedicationAsync(MedicationInputDto medicationDto);
        Task<bool> UpdateMedicationAsync(int id, MedicationInputDto medicationDto);
        Task<bool> DeleteMedicationAsync(int id);
        Task<List<MedicationDto>> SearchMedicationsAsync(string searchTerm);
    }

    public class MedicationService : IMedicationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(AppDbContext context, ILogger<MedicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MedicationDto>> GetAllMedicationsAsync()
        {
            var medications = await _context.Medications.ToListAsync();
            return medications.Select(MapToDto).ToList();
        }

        public async Task<MedicationDto?> GetMedicationByIdAsync(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            return medication != null ? MapToDto(medication) : null;
        }

        public async Task<MedicationDto> CreateMedicationAsync(MedicationInputDto medicationDto)
        {
            var medication = new Medication
            {
                Name = medicationDto.Name,
                ScientificName = medicationDto.ScientificName,
                Price = medicationDto.Price,
                Description = medicationDto.Description,
                Manufacturer = medicationDto.Manufacturer,
                DosageForm = medicationDto.DosageForm,
                Strength = medicationDto.Strength,
                ImageUrl = medicationDto.ImageUrl,
                RequiresPrescription = medicationDto.RequiresPrescription,
                IsAvailable = medicationDto.IsAvailable,
                ConflictsWith = medicationDto.ConflictsWith != null ? JsonSerializer.Serialize(medicationDto.ConflictsWith) : null
            };

            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            return MapToDto(medication);
        }

        public async Task<bool> UpdateMedicationAsync(int id, MedicationInputDto medicationDto)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return false;
            }

            medication.Name = medicationDto.Name;
            medication.ScientificName = medicationDto.ScientificName;
            medication.Price = medicationDto.Price;
            medication.Description = medicationDto.Description;
            medication.Manufacturer = medicationDto.Manufacturer;
            medication.DosageForm = medicationDto.DosageForm;
            medication.Strength = medicationDto.Strength;
            medication.ImageUrl = medicationDto.ImageUrl;
            medication.RequiresPrescription = medicationDto.RequiresPrescription;
            medication.IsAvailable = medicationDto.IsAvailable;
            medication.ConflictsWith = medicationDto.ConflictsWith != null ? JsonSerializer.Serialize(medicationDto.ConflictsWith) : null;

            _context.Medications.Update(medication);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteMedicationAsync(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return false;
            }

            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MedicationDto>> SearchMedicationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllMedicationsAsync();
            }

            searchTerm = searchTerm.ToLower();
            var medications = await _context.Medications
                .Where(m => m.Name.ToLower().Contains(searchTerm) || 
                            m.ScientificName.ToLower().Contains(searchTerm) ||
                            (m.Description != null && m.Description.ToLower().Contains(searchTerm)) ||
                            (m.Manufacturer != null && m.Manufacturer.ToLower().Contains(searchTerm)))
                .ToListAsync();

            return medications.Select(MapToDto).ToList();
        }

        private MedicationDto MapToDto(Medication medication)
        {
            List<string>? conflictsList = null;
            if (!string.IsNullOrEmpty(medication.ConflictsWith))
            {
                try
                {
                    conflictsList = JsonSerializer.Deserialize<List<string>>(medication.ConflictsWith);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing ConflictsWith for medication {MedicationId}", medication.Id);
                }
            }

            return new MedicationDto
            {
                Id = medication.Id,
                Name = medication.Name,
                ScientificName = medication.ScientificName,
                Price = medication.Price,
                Description = medication.Description,
                Manufacturer = medication.Manufacturer,
                DosageForm = medication.DosageForm,
                Strength = medication.Strength,
                ImageUrl = medication.ImageUrl,
                RequiresPrescription = medication.RequiresPrescription,
                IsAvailable = medication.IsAvailable,
                ConflictsWith = conflictsList
            };
        }
    }
}
