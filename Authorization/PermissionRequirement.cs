using Microsoft.AspNetCore.Authorization;

namespace SimpleToDoAPI.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        // وظيفة هذا الكلاس هو حمل الصلاحية المطلوبة
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}