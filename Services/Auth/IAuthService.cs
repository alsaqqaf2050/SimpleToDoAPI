
using SimpleToDoAPI.DTOs.Auth;

namespace SimpleToDoAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        Task<AuthResponseDto?> LoginAsync(LoginDto dto);

        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);

        Task<bool> RevokeTokenAsync(string refreshToken);

        Task<bool> LogoutAsync();

        // دالة لجلب صلاحيات المستخدم الحالي
        Task<CurrentUserDto?> GetCurrentUserAsync();
    }
}