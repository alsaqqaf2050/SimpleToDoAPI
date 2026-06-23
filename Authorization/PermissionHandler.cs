//using Microsoft.AspNetCore.Authorization;
//using System.Security.Claims;

//namespace SimpleToDoAPI.Authorization
//{
//    // يقرأ جميع Claims من نوع permission  ثم يتحقق هل الصلاحية المطلوبة موجودة؟
//    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
//    {
//        // نحن لا نستدعي Handler بنفسك وإنما نكتب فقط [Authorize(Policy = "...")] ويتم إستدعائه مباشرا من النظام
//        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
//        {
//            var permissions = context.User.FindAll("permission").Select(x => x.Value);

//            // هل داخل التوكن يوجد:Permissions.Todos.View ؟ مثلا
//            if (permissions.Contains(requirement.Permission))
//            {
//                // إذا كانت الصلاحية موجودة:
//                context.Succeed(requirement);
//            }

//            return Task.CompletedTask;
//        }
//    }
//}



using Microsoft.AspNetCore.Authorization;
using SimpleToDoAPI.Services.Interfaces;
using System.Security.Claims;

namespace SimpleToDoAPI.Authorization
{
    public class PermissionHandler
    : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionCacheService _permissionCache;


    public PermissionHandler(
        IPermissionCacheService permissionCache)
        {
            _permissionCache = permissionCache;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userId = context.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return;

            var permissions =
                await _permissionCache
                    .GetUserPermissionsAsync(userId);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }

}



//using Microsoft.AspNetCore.Authorization;
//using SimpleToDoAPI.Services.Common;
//using SimpleToDoAPI.Services.Interfaces;
//using System.Security.Claims;

//namespace SimpleToDoAPI.Authorization
//{
//    public class PermissionHandler
//        : AuthorizationHandler<PermissionRequirement>
//    {
//        private readonly IPermissionCacheService _permissionCache;

//        public PermissionHandler(
//            IPermissionCacheService permissionCache)
//        {
//            _permissionCache = permissionCache;
//        }

//        protected override async Task HandleRequirementAsync(
//            AuthorizationHandlerContext context,
//            PermissionRequirement requirement)
//        {
//            var userId = context.User
//                .FindFirstValue(ClaimTypes.NameIdentifier);

//            if (string.IsNullOrWhiteSpace(userId))
//                return;

//            var permissions =
//                await _permissionCache
//                    .GetUserPermissionsAsync(userId);

//            if (permissions.Contains(requirement.Permission))
//            {
//                context.Succeed(requirement);
//            }
//        }
//    }
//}