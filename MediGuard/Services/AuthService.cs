using MediGuard.API.DTOs;
using MediGuard.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediGuard.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UserDto userDto);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user exists
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                throw new ApplicationException("User with this email already exists");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Conditions = registerDto.Conditions,
                Allergies = registerDto.Allergies,
                DateOfBirth = registerDto.DateOfBirth,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"User creation failed: {errors}");
            }

            // Add user to role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate JWT token
            var token = await GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Conditions = user.Conditions,
                    Allergies = user.Allergies,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new ApplicationException("Invalid email or password");
            }

            // Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                throw new ApplicationException("Invalid email or password");
            }

            // Generate JWT token
            var token = await GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Conditions = user.Conditions,
                    Allergies = user.Allergies,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<UserDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Conditions = user.Conditions,
                Allergies = user.Allergies,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UserDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Conditions = userDto.Conditions;
            user.Allergies = userDto.Allergies;
            user.DateOfBirth = userDto.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        private async Task<(string Token, DateTime Expiration)> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "MediGuardDefaultSecretKey123456789012345"));
            var expiryInMinutes = Convert.ToInt32(_configuration["JWT:ExpiryInMinutes"] ?? "60");
            var expiration = DateTime.UtcNow.AddMinutes(expiryInMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: expiration,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }
    }
}
