namespace SimpleToDoAPI.DTOs.Auth
{
    // DTO مستقل لا يعتمد على جدول
    public class RevokeTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
