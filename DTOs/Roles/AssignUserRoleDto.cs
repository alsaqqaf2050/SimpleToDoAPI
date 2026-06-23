namespace SimpleToDoAPI.DTOs.Roles
{
    public class AssignUserRoleDto
    {
        public string UserId { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;
    }
}