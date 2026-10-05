
using SimpleToDoAPI.Configurations;
using SimpleToDoAPI.Models;

namespace SimpleToDoAPI.Seeders
{
    public static class AuditSeeder
    {
        public static void Seed()
        {
            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.PasswordHash));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.SecurityStamp));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.ConcurrencyStamp));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.NormalizedEmail));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.NormalizedUserName));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.AccessFailedCount));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.LockoutEnd));

            AuditConfiguration.IgnoreProperty<ApplicationUser>(nameof(ApplicationUser.TwoFactorEnabled));
        }
    }
}