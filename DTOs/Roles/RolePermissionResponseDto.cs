namespace SimpleToDoAPI.DTOs.Roles
{
    public class RolePermissionResponseDto
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}