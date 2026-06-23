namespace SimpleToDoAPI.DTOs.Permissions
{
    public class PermissionMatrixItemDto
    {
        public int PermissionId { get; set; }

        public string PermissionName { get; set; }
            = string.Empty;

        public bool Assigned { get; set; }
    }
}