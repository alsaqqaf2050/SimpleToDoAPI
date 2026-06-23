using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.Services.Interfaces;
using System.Collections.Concurrent;

namespace SimpleToDoAPI.Services.Common
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly ApplicationDbContext _context;

        private static readonly ConcurrentDictionary<string, List<string>> _cache = new();

        public PermissionCacheService(ApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task<List<string>> GetUserPermissionsAsync(string userId)
        //{
        //    if (_cache.TryGetValue(userId, out var cached))
        //        return cached;

        //    var permissions = await _context.RolePermissions
        //        //.Where(x => x.Role.Users.Any(u => u.UserId == userId))
        //        .Select(x => x.Permission.Name)
        //        .Distinct()
        //        .ToListAsync();

        //    _cache[userId] = permissions;

        //    return permissions;
        //}


        //public async Task<List<string>> GetUserPermissionsAsync(string userId)
        //{
        //    if (_cache.TryGetValue(userId, out var cached))
        //        return cached;

        //    var permissions = await
        //    (
        //        from userRole in _context.UserRoles

        //        join rolePermission in _context.RolePermissions
        //            on userRole.RoleId equals rolePermission.RoleId

        //        join permission in _context.Permissions
        //            on rolePermission.PermissionId equals permission.Id

        //        where userRole.UserId == userId

        //        select permission.Name
        //    )
        //    .Distinct()
        //    .ToListAsync();

        //    _cache[userId] = permissions;

        //    return permissions;
        //}


        //        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        //        {
        //            if (_cache.TryGetValue(userId, out var cached))
        //                return cached;


        //            var permissions = await
        //            (
        //                from userRole in _context.UserRoles

        //                join rolePermission in _context.RolePermissions
        //                    on userRole.RoleId equals rolePermission.RoleId

        //                join permission in _context.Permissions
        //                    on rolePermission.PermissionId equals permission.Id

        //                where userRole.UserId == userId

        //                select permission.Name
        //            )
        //            .Distinct()
        //            .ToListAsync();

        //                        _cache[userId] = permissions;

        //                        return permissions;
        //}




        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            if (_cache.TryGetValue(userId, out var cached))
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

            _cache[userId] = permissions;

            return permissions;
        }



        public Task RefreshUserPermissionsAsync(string userId)
        {
            _cache.TryRemove(userId, out _);
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
                        _cache.TryRemove(userId, out _);
                    }
         }       

    }
}