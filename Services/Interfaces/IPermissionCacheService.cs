namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IPermissionCacheService
    {
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task RefreshUserPermissionsAsync(string userId);
        Task RefreshRolePermissionsAsync(string roleId);
    }
}
