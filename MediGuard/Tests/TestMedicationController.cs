using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MediGuard.API.Controllers;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using MediGuard.API.Helpers;
using System.Security.Claims;
using Xunit;

namespace MediGuard.Tests
{
    public class TestMedicationController
    {
        private readonly Mock<IMedicationService> _mockMedicationService;
        private readonly Mock<ILogger<MedicationController>> _mockLogger;
        private readonly MedicationController _controller;

        public TestMedicationController()
        {
            _mockMedicationService = new Mock<IMedicationService>();
            _mockLogger = new Mock<ILogger<MedicationController>>();
            _controller = new MedicationController(_mockMedicationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllMedications_ReturnsOkResult()
        {
            // Arrange
            var medications = new List<MedicationDto>
            {
                new MedicationDto
                {
                    Id = 1,
                    Name = "Advil",
                    ScientificName = "Ibuprofen",
                    Price = 5.99m,
                    Description = "Used for treating pain, fever, and inflammation.",
                    Manufacturer = "Pfizer",
                    DosageForm = "Tablet",
                    Strength = "200mg",
                    RequiresPrescription = false,
                    IsAvailable = true,
                    ConflictsWith = new List<string> { "Warfarin", "Aspirin" }
                },
                new MedicationDto
                {
                    Id = 2,
                    Name = "Tylenol",
                    ScientificName = "Acetaminophen",
                    Price = 4.99m,
                    Description = "Used for treating pain and fever.",
                    Manufacturer = "Johnson & Johnson",
                    DosageForm = "Tablet",
                    Strength = "500mg",
                    RequiresPrescription = false,
                    IsAvailable = true,
                    ConflictsWith = new List<string>()
                }
            };

            _mockMedicationService.Setup(service => service.GetAllMedicationsAsync())
                .ReturnsAsync(medications);

            // Act
            var result = await _controller.GetAllMedications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<MedicationDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GetMedicationById_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var medicationId = 1;
            var medication = new MedicationDto
            {
                Id = medicationId,
                Name = "Advil",
                ScientificName = "Ibuprofen",
                Price = 5.99m,
                Description = "Used for treating pain, fever, and inflammation.",
                Manufacturer = "Pfizer",
                DosageForm = "Tablet",
                Strength = "200mg",
                RequiresPrescription = false,
                IsAvailable = true,
                ConflictsWith = new List<string> { "Warfarin", "Aspirin" }
            };

            _mockMedicationService.Setup(service => service.GetMedicationByIdAsync(medicationId))
                .ReturnsAsync(medication);

            // Act
            var result = await _controller.GetMedicationById(medicationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<MedicationDto>(okResult.Value);
            Assert.Equal(medicationId, returnValue.Id);
            Assert.Equal("Advil", returnValue.Name);
        }

        [Fact]
        public async Task GetMedicationById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var medicationId = 999;
            _mockMedicationService.Setup(service => service.GetMedicationByIdAsync(medicationId))
                .ReturnsAsync((MedicationDto)null);

            // Act
            var result = await _controller.GetMedicationById(medicationId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task SearchMedications_ReturnsOkResult()
        {
            // Arrange
            var searchTerm = "pain";
            var medications = new List<MedicationDto>
            {
                new MedicationDto
                {
                    Id = 1,
                    Name = "Advil",
                    ScientificName = "Ibuprofen",
                    Price = 5.99m,
                    Description = "Used for treating pain, fever, and inflammation.",
                    Manufacturer = "Pfizer",
                    DosageForm = "Tablet",
                    Strength = "200mg",
                    RequiresPrescription = false,
                    IsAvailable = true,
                    ConflictsWith = new List<string> { "Warfarin", "Aspirin" }
                },
                new MedicationDto
                {
                    Id = 2,
                    Name = "Tylenol",
                    ScientificName = "Acetaminophen",
                    Price = 4.99m,
                    Description = "Used for treating pain and fever.",
                    Manufacturer = "Johnson & Johnson",
                    DosageForm = "Tablet",
                    Strength = "500mg",
                    RequiresPrescription = false,
                    IsAvailable = true,
                    ConflictsWith = new List<string>()
                }
            };

            _mockMedicationService.Setup(service => service.SearchMedicationsAsync(searchTerm))
                .ReturnsAsync(medications);

            // Act
            var result = await _controller.SearchMedications(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<MedicationDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }
    }
}
