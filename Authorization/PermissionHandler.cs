using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SimpleToDoAPI.Authorization
{
    // يقرأ جميع Claims من نوع permission  ثم يتحقق هل الصلاحية المطلوبة موجودة؟
    public class PermissionHandler: AuthorizationHandler<PermissionRequirement>
    {
        // نحن لا نستدعي Handler بنفسك وإنما نكتب فقط [Authorize(Policy = "...")] ويتم إستدعائه مباشرا من النظام
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionRequirement requirement)
        {
            var permissions = context.User.FindAll("permission").Select(x => x.Value);

            // هل داخل التوكن يوجد:Permissions.Todos.View ؟ مثلا
            if (permissions.Contains(requirement.Permission))
            {
                // إذا كانت الصلاحية موجودة:
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}