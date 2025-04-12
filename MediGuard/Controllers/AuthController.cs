using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediGuard.API.DTOs;
using MediGuard.API.Services;
using System.Security.Claims;

namespace MediGuard.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Registration failed");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Login failed");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var userProfile = await _authService.GetUserProfileAsync(userId);
                return Ok(userProfile);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Failed to get user profile");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting user profile");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult> UpdateProfile([FromBody] UserDto userDto)
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

                var result = await _authService.UpdateUserProfileAsync(userId, userDto);
                if (result)
                {
                    return Ok(new { message = "Profile updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update profile" });
                }
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Failed to update user profile");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating user profile");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
}
