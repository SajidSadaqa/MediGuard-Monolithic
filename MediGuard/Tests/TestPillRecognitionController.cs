using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MediGuard.API.Controllers;
using MediGuard.API.Services;
using Xunit;
using System.Security.Claims;

namespace MediGuard.Tests
{
    public class TestPillRecognitionController
    {
        private readonly Mock<IPillRecognitionService> _mockPillRecognitionService;
        private readonly Mock<ILogger<PillRecognitionController>> _mockLogger;
        private readonly PillRecognitionController _controller;

        public TestPillRecognitionController()
        {
            _mockPillRecognitionService = new Mock<IPillRecognitionService>();
            _mockLogger = new Mock<ILogger<PillRecognitionController>>();
            _controller = new PillRecognitionController(_mockPillRecognitionService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RecognizePill_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var request = new PillRecognitionRequest
            {
                ImageBase64 = "base64encodedimagedata"
            };

            var recognitionResult = new PillRecognitionResult
            {
                Success = true,
                MedicationId = 1,
                MedicationName = "Advil",
                ScientificName = "Ibuprofen",
                Confidence = 0.85m,
                Message = "Pill recognized as Advil with 85% confidence"
            };

            _mockPillRecognitionService.Setup(service => service.RecognizePillAsync(request.ImageBase64))
                .ReturnsAsync(recognitionResult);

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
            var result = await _controller.RecognizePill(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<PillRecognitionResult>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal("Advil", returnValue.MedicationName);
            Assert.Equal(0.85m, returnValue.Confidence);
        }

        [Fact]
        public async Task RecognizePill_EmptyImage_ReturnsBadRequest()
        {
            // Arrange
            var userId = "user-id";
            var request = new PillRecognitionRequest
            {
                ImageBase64 = ""
            };

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
            var result = await _controller.RecognizePill(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
