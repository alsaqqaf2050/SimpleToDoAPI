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
using SimpleToDoAPI.Authorization;
using SimpleToDoAPI.Services.Interfaces;
using System.Security.Cryptography;
using SimpleToDoAPI.Data;
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Services.Common;

namespace SimpleToDoAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly ICurrentUserService _currentUser;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings, IPermissionService permissionService, ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _permissionService = permissionService;
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);

            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "اسم المستخدم مستخدم مسبقاً"
                };
            }

            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);

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

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(x => x.Description))
                };
            }

            /// تسجيل الصلاحيات للمستخدم الذي تمت إضافته
            var roleResult = await _userManager.AddToRoleAsync(user,"User");

            //if (!roleResult.Succeeded)
            //    throw new Exception(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            if (!roleResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(x => x.Description))
                };
            }
            return await GenerateTokenAsync(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "بيانات الدخول غير صحيحة"
                };
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);

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

        /// <summary>
        /// دالة إنشاء ال Token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user)
        {
            var userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),

                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),

                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),

                new Claim(ClaimTypes.NameIdentifier, user.Id),

                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            //var roles = await _userManager.GetRolesAsync(user);

            //foreach (var role in roles)
            //{
            //    userClaims.Add(
            //        new Claim(ClaimTypes.Role, role));

            //    var permissions =
            //        PermissionStore.GetPermissions(role);

            //    foreach (var permission in permissions)
            //    {
            //        userClaims.Add(
            //            new Claim(
            //                "permission",
            //                permission));
            //    }
            //}

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var permissions = await _permissionService.GetPermissionsAsync(user.Id);

            foreach (var permission in permissions)
            {
                userClaims.Add(new Claim("permission", permission));
            }



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials =new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var expires =DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };

            var jwtToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: userClaims,
                expires: expires,
                signingCredentials: credentials);

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "تمت العملية بنجاح",
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                RefreshToken = refreshToken,
                Expiration = expires
            };
        }


        /// <summary>
        /// دالة إنشاء ال RefreshToken
        /// </summary>
        /// <returns></returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// دالة تحديث ال RefreshToken
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var refreshToken = await _context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (refreshToken == null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Refresh Token غير صالح"
                };
            }

            if (refreshToken.IsRevoked)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Refresh Token ملغي"
                };
            }

            if (refreshToken.IsUsed)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Refresh Token مستخدم مسبقاً"
                };
            }

            if (refreshToken.ExpiresOn <= DateTime.UtcNow)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Refresh Token منتهي الصلاحية"
                };
            }

            refreshToken.IsUsed = true;

            await _context.SaveChangesAsync();

            return await GenerateTokenAsync(refreshToken.User);
        }

        /// <summary>
        /// دالة إلغاء ال RefreshToken
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {

            // في هذه الحالة يتم تمرير الريفرش توكين وعرضه في المتصفح لادخال قيمته ولكن ليست امنه وبذلك سنعتمد على التالية التالية :LogoutAsync()

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null)
                return false;

            if (token.IsRevoked)
                return false;

            token.IsRevoked = true;

            await _context.SaveChangesAsync();

            return true;
        }


        /// <summary
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LogoutAsync()
        {
            // يتم جلب كل الريفريش توكينز للمستخدم الحالي والتي تكون فعالة
            var refreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == _currentUser.UserId && !x.IsRevoked).ToListAsync();

            if (!refreshTokens.Any())
                return false;

            // يتم إلغائها جمبعا إن وجدت
            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return true;
        }

    }
}