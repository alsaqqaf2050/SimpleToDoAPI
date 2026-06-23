namespace SimpleToDoAPI.DTOs.Permissions
{
    public class RolePermissionMatrixDto
    {
        public string RoleId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public List<PermissionMatrixItemDto> Permissions { get; set; } = new();
    }
}