namespace SimpleToDoAPI.DTOs.Permissions
{
    public class PermissionMatrixDto
    {
        public int PermissionId { get; set; }

    public string PermissionName { get; set; }
        = string.Empty;

        public List<string> Roles { get; set; }
            = new();
    }

}
