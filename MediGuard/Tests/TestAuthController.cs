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
    public class TestAuthController
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public TestAuthController()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Register_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "Test",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var authResponse = new AuthResponseDto
            {
                Token = "test-token",
                Expiration = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = "user-id",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockAuthService.Setup(service => service.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal(authResponse.Token, returnValue.Token);
            Assert.Equal(authResponse.User.Email, returnValue.User.Email);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var authResponse = new AuthResponseDto
            {
                Token = "test-token",
                Expiration = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = "user-id",
                    Email = "test@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockAuthService.Setup(service => service.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal(authResponse.Token, returnValue.Token);
            Assert.Equal(authResponse.User.Email, returnValue.User.Email);
        }

        [Fact]
        public async Task GetProfile_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            var userId = "user-id";
            var userDto = new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };

            _mockAuthService.Setup(service => service.GetUserProfileAsync(userId))
                .ReturnsAsync(userDto);

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
            var result = await _controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userDto.Id, returnValue.Id);
            Assert.Equal(userDto.Email, returnValue.Email);
        }
    }
}
