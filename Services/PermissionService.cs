// Services/PermissionService.cs

using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetPermissionsAsync(string userId)
        {
            var permissions = await
            (
            // إستخدمنا ال UserRoles لأن IdentityDbContext يحتوي تلقائياً على  _context.UserRoles  
            // وهو يمثل AspNetUserRoles  لذلك لسنا بحاجة لإنشاء Entity جديد له.

                from userRole in _context.UserRoles
                join rolePermission in _context.RolePermissions
                    on userRole.RoleId equals rolePermission.RoleId
                join permission in _context.Permissions
                    on rolePermission.PermissionId equals permission.Id
                where userRole.UserId == userId
                select permission.Name
            )
            .Distinct()
            .ToListAsync();

            return permissions;
        }
    }
}