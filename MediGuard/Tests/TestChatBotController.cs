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
    public class TestChatBotController
    {
        private readonly Mock<IChatBotService> _mockChatBotService;
        private readonly Mock<ILogger<ChatBotController>> _mockLogger;
        private readonly ChatBotController _controller;

        public TestChatBotController()
        {
            _mockChatBotService = new Mock<IChatBotService>();
            _mockLogger = new Mock<ILogger<ChatBotController>>();
            _controller = new ChatBotController(_mockChatBotService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetChatResponse_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var chatRequest = new ChatRequestDto
            {
                Message = "I have a headache, what should I take?",
                Context = "medical advice"
            };

            var chatResponse = new ChatResponseDto
            {
                Response = "For headaches, over-the-counter pain relievers like acetaminophen (Tylenol) or ibuprofen (Advil) can be effective. Make sure to follow the recommended dosage. If headaches are severe or persistent, please consult with your healthcare provider.",
                Timestamp = DateTime.UtcNow,
                Recommendations = new List<RecommendationDto>
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
                        CreatedAt = DateTime.UtcNow
                    },
                    new RecommendationDto
                    {
                        Id = 2,
                        MedicationId = 2,
                        MedicationName = "Tylenol",
                        RecommendationText = "Alternative for headache relief",
                        RecommendationScore = 80,
                        RecommendationReason = "Good option if you have stomach sensitivity",
                        IsViewed = false,
                        IsAccepted = false,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            _mockChatBotService.Setup(service => service.GetChatResponseAsync(userId, It.IsAny<ChatRequestDto>()))
                .ReturnsAsync(chatResponse);

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
            var result = await _controller.GetChatResponse(chatRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ChatResponseDto>(okResult.Value);
            Assert.Contains("headache", returnValue.Response);
            Assert.Equal(2, returnValue.Recommendations?.Count);
        }

        [Fact]
        public async Task GetChatHistory_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var chatHistory = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    Id = 1,
                    Message = "I have a headache, what should I take?",
                    IsFromUser = true,
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    Context = "medical advice"
                },
                new ChatMessageDto
                {
                    Id = 2,
                    Message = "For headaches, over-the-counter pain relievers like acetaminophen (Tylenol) or ibuprofen (Advil) can be effective. Make sure to follow the recommended dosage. If headaches are severe or persistent, please consult with your healthcare provider.",
                    IsFromUser = false,
                    Timestamp = DateTime.UtcNow.AddMinutes(-4),
                    Context = "medical advice"
                },
                new ChatMessageDto
                {
                    Id = 3,
                    Message = "Thank you, I'll try Tylenol.",
                    IsFromUser = true,
                    Timestamp = DateTime.UtcNow.AddMinutes(-3),
                    Context = "medical advice"
                },
                new ChatMessageDto
                {
                    Id = 4,
                    Message = "You're welcome. Remember to follow the dosage instructions on the package. Let me know if you have any other questions.",
                    IsFromUser = false,
                    Timestamp = DateTime.UtcNow.AddMinutes(-2),
                    Context = "medical advice"
                }
            };

            _mockChatBotService.Setup(service => service.GetUserChatHistoryAsync(userId))
                .ReturnsAsync(chatHistory);

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
            var result = await _controller.GetChatHistory();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<ChatMessageDto>>(okResult.Value);
            Assert.Equal(4, returnValue.Count());
        }
    }
}
