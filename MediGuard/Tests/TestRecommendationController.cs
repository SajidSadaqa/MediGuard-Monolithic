using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MediGuard.API.Controllers;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using System.Security.Claims;
using Xunit;

namespace MediGuard.Tests
{
    public class TestRecommendationController
    {
        private readonly Mock<IRecommendationService> _mockRecommendationService;
        private readonly Mock<ILogger<RecommendationController>> _mockLogger;
        private readonly RecommendationController _controller;

        public TestRecommendationController()
        {
            _mockRecommendationService = new Mock<IRecommendationService>();
            _mockLogger = new Mock<ILogger<RecommendationController>>();
            _controller = new RecommendationController(_mockRecommendationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserRecommendations_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var recommendations = new List<RecommendationDto>
            {
                new RecommendationDto
                {
                    Id = 1,
                    MedicationId = 1,
                    MedicationName = "Advil",
                    RecommendationText = "Effective for headache relief",
                    RecommendationScore = 85,
                    RecommendationReason = "Common first-line treatment for headaches",
                    IsViewed = false,
                    IsAccepted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new RecommendationDto
                {
                    Id = 2,
                    MedicationId = 2,
                    MedicationName = "Tylenol",
                    RecommendationText = "Alternative for headache relief",
                    RecommendationScore = 80,
                    RecommendationReason = "Good option if you have stomach sensitivity",
                    IsViewed = true,
                    IsAccepted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _mockRecommendationService.Setup(service => service.GetUserRecommendationsAsync(userId))
                .ReturnsAsync(recommendations);

            // Mock the User.FindFirstValue to return the userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GetUserRecommendations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<RecommendationDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GenerateRecommendations_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var recommendations = new List<RecommendationDto>
            {
                new RecommendationDto
                {
                    Id = 3,
                    MedicationId = 3,
                    MedicationName = "Aspirin",
                    RecommendationText = "Consider taking Aspirin for your health needs",
                    RecommendationScore = 75,
                    RecommendationReason = "General health recommendation",
                    IsViewed = false,
                    IsAccepted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new RecommendationDto
                {
                    Id = 4,
                    MedicationId = 4,
                    MedicationName = "Lipitor",
                    RecommendationText = "Consider taking Lipitor for your health needs",
                    RecommendationScore = 70,
                    RecommendationReason = "General health recommendation",
                    IsViewed = false,
                    IsAccepted = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockRecommendationService.Setup(service => service.GenerateRecommendationsForUserAsync(userId))
                .ReturnsAsync(recommendations);

            // Mock the User.FindFirstValue to return the userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.GenerateRecommendations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<RecommendationDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task AcceptRecommendation_ValidId_ReturnsNoContent()
        {
            // Arrange
            var userId = "user-id";
            var recommendationId = 1;

            _mockRecommendationService.Setup(service => service.AcceptRecommendationAsync(recommendationId, userId))
                .ReturnsAsync(true);

            // Mock the User.FindFirstValue to return the userId
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.AcceptRecommendation(recommendationId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
