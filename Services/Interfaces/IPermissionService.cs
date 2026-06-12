namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<string>> GetPermissionsAsync(string userId);
    }
}