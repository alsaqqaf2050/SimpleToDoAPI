using Microsoft.AspNetCore.Identity;
using SimpleToDoAPI.Infrastructure.Audit;

namespace SimpleToDoAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        [AuditIgnore]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [AuditIgnore]
        public override string? ConcurrencyStamp { get; set; }

        [AuditIgnore]
        public override string? SecurityStamp { get; set; }

        [AuditIgnore]
        public override string? PasswordHash { get; set; }


        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public ICollection<TodoItem> Todos { get; set; } = new List<TodoItem>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}