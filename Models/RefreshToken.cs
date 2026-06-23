//namespace SimpleToDoAPI.Models
//{
//    public class RefreshToken
//    {
//        public int Id { get; set; }

//        public string Token { get; set; } = string.Empty;

//        public DateTime ExpiresOn { get; set; }

//        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

//        public bool IsRevoked { get; set; }

//        public bool IsUsed { get; set; }

//        public string UserId { get; set; } = string.Empty;

//        public ApplicationUser User { get; set; } = null!;
//    }
//}

namespace SimpleToDoAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresOn { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? RevokedOn { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;

        public bool IsActive => RevokedOn == null && !IsExpired;

        public string? ReplacedByToken { get; set; }

        // علاقة المستخدم
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
    }
}