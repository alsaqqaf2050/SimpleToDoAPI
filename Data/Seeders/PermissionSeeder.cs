using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Models;

namespace SimpleToDoAPI.Data.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedPermissionsAsync(
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            var adminRole = await roleManager.FindByNameAsync("Admin");
            var userRole = await roleManager.FindByNameAsync("User");

            if (adminRole == null || userRole == null)
                throw new Exception("Roles not found.");

            // إذا تم الربط مسبقاً لا نكرر الإدخال
            if (await context.RolePermissions.AnyAsync())
                return;

            var permissions = await context.Permissions.ToListAsync();

            // Admin يحصل على جميع الصلاحيات
            foreach (var permission in permissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // User يحصل على صلاحيات محددة فقط
            var userPermissions = permissions
                .Where(x =>
                    x.Name == "Permissions.Todos.View" ||
                    x.Name == "Permissions.Todos.Create" ||
                    x.Name == "Permissions.Todos.Update")
                .ToList();

            foreach (var permission in userPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = permission.Id
                });
            }

            await context.SaveChangesAsync();
        }
    }
}