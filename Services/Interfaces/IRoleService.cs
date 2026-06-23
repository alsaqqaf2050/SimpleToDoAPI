using SimpleToDoAPI.DTOs.Roles;
using SimpleToDoAPI.Helpers;

namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IRoleService
    {
        Task<PagedResult<RoleResponseDto>>
            GetAllAsync(RoleQueryParametersDto parameters);

        Task<RoleResponseDto?>
            GetByIdAsync(string id);

        Task<RoleResponseDto>
            CreateAsync(CreateRoleDto dto);

        Task<RoleResponseDto?>
            UpdateAsync(
                string id,
                UpdateRoleDto dto);

        Task<bool>
            DeleteAsync(string id);

        Task<bool>
            AssignUserAsync(
                string userId,
                string roleName);

        Task<bool>
            RemoveUserAsync(
                string userId,
                string roleName);

        Task<List<RolePermissionResponseDto>>GetPermissionsAsync(string roleId);

        Task<bool> AssignPermissionAsync(
                string roleId,
                int permissionId);

        Task<bool>RemovePermissionAsync(
                string roleId,
                int permissionId);
    }


}