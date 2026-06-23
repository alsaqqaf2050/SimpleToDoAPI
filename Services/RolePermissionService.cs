//using Microsoft.EntityFrameworkCore;
//using SimpleToDoAPI.Data;
//using SimpleToDoAPI.Models;
//using SimpleToDoAPI.Services.Interfaces;

//namespace SimpleToDoAPI.Services
//{
//    public class RolePermissionService : IRolePermissionService
//    {
//        private readonly ApplicationDbContext _context;

//        private readonly IPermissionCacheService _cache;

//        public RolePermissionService(ApplicationDbContext context, IPermissionCacheService cache)
//        {
//            _context = context;
//            _cache = cache;
//        }

//        public async Task<List<string>> GetRolePermissionsAsync(string roleId)
//        {
//            return await _context.RolePermissions
//                .Where(x => x.RoleId == roleId)
//                .Select(x => x.Permission.Name)
//                .ToListAsync();
//        }

//        public async Task AssignPermissionAsync(string roleId, int permissionId)
//        {
//            var exists = await _context.RolePermissions
//                .AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);

//            if (exists) return;

//            _context.RolePermissions.Add(new RolePermission
//            {
//                RoleId = roleId,
//                PermissionId = permissionId
//            });

//            await _context.SaveChangesAsync();
//        }

//        public async Task RemovePermissionAsync(string roleId, int permissionId)
//        {
//            var entity = await _context.RolePermissions
//                .FirstOrDefaultAsync(x =>
//                    x.RoleId == roleId &&
//                    x.PermissionId == permissionId);

//            if (entity == null) return;

//            _context.RolePermissions.Remove(entity);
//            await _context.SaveChangesAsync();
//        }

//        public async Task SyncPermissionsAsync(string roleId, List<int> permissionIds)
//        {
//            var existing = await _context.RolePermissions
//                .Where(x => x.RoleId == roleId)
//                .ToListAsync();

//            _context.RolePermissions.RemoveRange(existing);

//            var newPermissions = permissionIds.Select(pid => new RolePermission
//            {
//                RoleId = roleId,
//                PermissionId = pid
//            });

//            _context.RolePermissions.AddRange(newPermissions);

//            await _context.SaveChangesAsync();
//        }
//    }
//}


using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.DTOs.Permissions;
using SimpleToDoAPI.DTOs.Roles;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermissionCacheService _permissionCache;

        public RolePermissionService(ApplicationDbContext context, IPermissionCacheService permissionCache)
        {
            _context = context;
            _permissionCache = permissionCache;
        }

        public async Task<RolePermissionsResponseDto?>
            GetRolePermissionsAsync(string roleId)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == roleId);

            if (role == null)
                return null;

            var permissions = await _context.RolePermissions
                .Where(x => x.RoleId == roleId)
                .Include(x => x.Permission)
                .Select(x => new PermissionResponseDto
                {
                    Id = x.Permission.Id,
                    Name = x.Permission.Name
                })
                .ToListAsync();

            return new RolePermissionsResponseDto
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "",
                Permissions = permissions
            };
        }

        public async Task<bool> AssignPermissionAsync(
            string roleId,
            int permissionId)
        {
            var exists = await _context.RolePermissions
                .AnyAsync(x =>
                    x.RoleId == roleId &&
                    x.PermissionId == permissionId);

            if (exists)
                return true;

            _context.RolePermissions.Add(
                new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });

            await _context.SaveChangesAsync();

            // تحدسث الكاش 
            await _permissionCache.RefreshRolePermissionsAsync(roleId);

            await InvalidateRoleUsersCache(roleId);

            return true;
        }

        public async Task<bool> RemovePermissionAsync(
            string roleId,
            int permissionId)
        {
            var entity = await _context.RolePermissions
                .FirstOrDefaultAsync(x =>
                    x.RoleId == roleId &&
                    x.PermissionId == permissionId);

            if (entity == null)
                return false;

            _context.RolePermissions.Remove(entity);

            await _context.SaveChangesAsync();

            // تحدسث الكاش 
            await _permissionCache.RefreshRolePermissionsAsync(roleId);

            await InvalidateRoleUsersCache(roleId);

            return true;
        }


        //        public async Task<List<PermissionMatrixDto>> GetPermissionMatrixAsync()
        //        {
        //            var matrix = await _context.Permissions
        //            .Select(permission => new PermissionMatrixDto
        //            {
        //                PermissionId = permission.Id,

        //                    PermissionName = permission.Name,

        //                Roles = permission.RolePermissions
        //                        .Select(rp => rp.Role.Name!)
        //                        .OrderBy(x => x)
        //                        .ToList()
        //            })
        //                .OrderBy(x => x.PermissionName)
        //                .ToListAsync();

        //            return matrix;

        //}

        //    public async Task<List<RolePermissionMatrixDto>>
        //GetPermissionMatrixAsync()
        //    {
        //        var matrix =
        //            await (
        //                from role in _context.Roles

        //                select new RolePermissionMatrixDto
        //                {
        //                    RoleId = role.Id,

        //                    RoleName = role.Name!,

        //                    Permissions =
        //                        (
        //                            from rp in _context.RolePermissions

        //                            join p in _context.Permissions
        //                                on rp.PermissionId equals p.Id

        //                            where rp.RoleId == role.Id

        //                            select p.Name
        //                        )
        //                        .ToList()
        //                }
        //            )
        //            .ToListAsync();

        //        return matrix;
        //    }





        // سيسجل 

        //{
        //  "roleId": "1",
        //  "roleName": "Admin",
        //  "permissions": [
        //    {
        //      "permissionId": 1,
        //      "permissionName": "Permissions.Todos.View",
        //      "assigned": true
        //    },
        //    {
        //      "permissionId": 2,
        //      "permissionName": "Permissions.Todos.Create",
        //      "assigned": true
        //    },
        //    {
        //    "permissionId": 3,
        //      "permissionName": "Permissions.Todos.Update",
        //      "assigned": true
        //    },
        //    {
        //    "permissionId": 4,
        //      "permissionName": "Permissions.Todos.Delete",
        //      "assigned": false
        //    }
        //  ]
        //}

        public async Task<List<RolePermissionMatrixDto>>
            GetPermissionMatrixAsync()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .ToListAsync();

            var permissions = await _context.Permissions
                .AsNoTracking()
                .ToListAsync();

            var rolePermissions = await _context.RolePermissions
                .AsNoTracking()
                .ToListAsync();

            var assignedLookup =
                rolePermissions
                    .Select(x =>
                        $"{x.RoleId}_{x.PermissionId}")
                    .ToHashSet();

            return roles
                .Select(role =>
                    new RolePermissionMatrixDto
                    {
                        RoleId = role.Id,

                        RoleName = role.Name ?? "",

                        Permissions = permissions
                            .Select(permission =>
                                new PermissionMatrixItemDto
                                {
                                    PermissionId =
                                        permission.Id,

                                    PermissionName =
                                        permission.Name,

                                    Assigned =
                                        assignedLookup.Contains(
                                            $"{role.Id}_{permission.Id}")
                                })
                            .ToList()
                    })
                .ToList();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private async Task InvalidateRoleUsersCache(
    string roleId)
        {
            var userIds = await _context.UserRoles
                .Where(x => x.RoleId == roleId)
                .Select(x => x.UserId)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await _permissionCache
                    .RefreshUserPermissionsAsync(userId);
            }
        }


    }
}