using SimpleToDoAPI.Infrastructure.Audit;

namespace SimpleToDoAPI.DTOs.Auth
{
    [AuditIgnore]
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
