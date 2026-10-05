using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Services.Common
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public PermissionCacheService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            var cacheKey = $"permissions_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
                return cached;

            var permissions = await
            (
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

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(cacheKey, permissions, cacheOptions);

            return permissions;
        }

        public Task RefreshUserPermissionsAsync(string userId)
        {
            _cache.Remove($"permissions_{userId}");
            return Task.CompletedTask;
        }

        public async Task RefreshRolePermissionsAsync(string roleId)
        {
            var userIds = await _context.UserRoles
                .Where(x => x.RoleId == roleId)
                .Select(x => x.UserId)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                _cache.Remove($"permissions_{userId}");
            }
        }
    }
}