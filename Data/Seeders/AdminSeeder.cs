using Microsoft.AspNetCore.Identity;
using SimpleToDoAPI.Models;

namespace SimpleToDoAPI.Data.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "admin@todo.com";
            const string password = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(email);

            if (adminUser != null) return;

            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = email,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, password);

            if (!result.Succeeded) throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

            // الأدمن يكون لديه صلاحييات الأدمن مع صلاحية المستخدم العادي أيضأ
            await userManager.AddToRoleAsync(adminUser,"Admin");
            await userManager.AddToRoleAsync(adminUser,"User");
        }
    }
}