namespace SimpleToDoAPI.DTOs.Roles
{
    public class AssignPermissionDto
    {
        public string RoleId { get; set; } = string.Empty;

        public int PermissionId { get; set; }
    }
}