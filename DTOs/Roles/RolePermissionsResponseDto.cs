using SimpleToDoAPI.DTOs.Permissions;

namespace SimpleToDoAPI.DTOs.Roles
{
    public class RolePermissionsResponseDto
    {
        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public List<PermissionResponseDto> Permissions { get; set; } = new();
    }
}