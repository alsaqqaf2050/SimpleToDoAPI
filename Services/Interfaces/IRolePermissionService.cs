//namespace SimpleToDoAPI.Services.Interfaces
//{
//    public interface IRolePermissionService
//    {
//        Task<List<string>> GetRolePermissionsAsync(string roleId);

//        Task AssignPermissionAsync(string roleId, int permissionId);

//        Task RemovePermissionAsync(string roleId, int permissionId);

//        Task SyncPermissionsAsync(string roleId, List<int> permissionIds);
//    }
//}


using SimpleToDoAPI.DTOs.Permissions;
using SimpleToDoAPI.DTOs.Roles;
using System.Collections.Generic;

namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IRolePermissionService
    {
        Task<RolePermissionsResponseDto?> GetRolePermissionsAsync(string roleId);

        Task<bool> AssignPermissionAsync(string roleId,int permissionId);

        Task<bool> RemovePermissionAsync(string roleId,int permissionId);

        Task<List<RolePermissionMatrixDto>> GetPermissionMatrixAsync();

    }
}
