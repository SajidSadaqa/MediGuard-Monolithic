// In AuthService.Controllers/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthService.Models;
using MediGuard.AuthService.Services;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager,
            IConfiguration configuration,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new UserProfile
            {
                UserName = model.Email,
                Email = model.Email,
                FullName=model.FullName,
                EmailConfirmed = true // Set to false if you want email confirmation
                
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Optionally add user to a default role
                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new { Message = "User registered successfully" });
            }

            return BadRequest(new
            {
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid login attempt" });
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var token = _tokenService.GenerateToken(user, out DateTime expiration);
                return Ok(new
                {
                    Token = token,
                    Expiration = expiration,
                    User = new
                    {
                        user.Id,
                        user.Email,
                        user.FullName
                    }
                });
            }

            if (result.IsLockedOut)
            {
                return StatusCode(423, new { Message = "Account is locked out" });
            }

            return Unauthorized(new { Message = "Invalid login attempt" });
        }

        // Optional: Add a profile endpoint
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.DateOfBirth
            });
        }
    }
}