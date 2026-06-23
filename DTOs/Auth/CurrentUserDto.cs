namespace SimpleToDoAPI.DTOs.Auth
{
    public class CurrentUserDto
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public List<string> Permissions { get; set; } = new();
    }
}