using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleToDoAPI.Configurations;
using SimpleToDoAPI.DTOs.Auth;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleToDoAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser =
                await _userManager.FindByNameAsync(dto.UserName);

            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "اسم المستخدم مستخدم مسبقاً"
                };
            }

            var existingEmail =
                await _userManager.FindByEmailAsync(dto.Email);

            if (existingEmail != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "البريد الإلكتروني مستخدم مسبقاً"
                };
            }

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                CreatedDate = DateTime.UtcNow
            };

            var result =
                await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ",
                        result.Errors.Select(x => x.Description))
                };
            }

            return await GenerateTokenAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user =
                await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "بيانات الدخول غير صحيحة"
                };
            }

            var validPassword =
                await _userManager.CheckPasswordAsync(
                    user,
                    dto.Password);

            if (!validPassword)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "بيانات الدخول غير صحيحة"
                };
            }

            return await GenerateTokenAsync(user);
        }

        private async Task<AuthResponseDto> GenerateTokenAsync(
            ApplicationUser user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),

                new Claim(
                    JwtRegisteredClaimNames.UniqueName,
                    user.UserName ?? ""),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    user.Email ?? ""),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName ?? "")
            };

            var roles =
                await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                userClaims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            var expires =
                DateTime.UtcNow.AddMinutes(
                    _jwtSettings.DurationInMinutes);

            var jwtToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: userClaims,
                expires: expires,
                signingCredentials: credentials);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "تمت العملية بنجاح",
                Token = new JwtSecurityTokenHandler()
                    .WriteToken(jwtToken),
                Expiration = expires
            };
        }
    }
}