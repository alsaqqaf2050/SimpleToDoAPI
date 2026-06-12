using System.Security.Claims;

namespace SimpleToDoAPI.Security
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // تجاهل المسارات العامة
            if (context.Request.Path.StartsWithSegments("/auth"))
            {
                await _next(context);
                return;
            }

            // قراءة المستخدم
            var user = context.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // استخراج الصلاحيات من JWT
            var permissions = user.FindAll("permission")
                                  .Select(x => x.Value)
                                  .ToList();

            // مثال: استخراج endpoint الحالي
            var endpoint = context.GetEndpoint();

            var requiredPermission = endpoint?
                .Metadata
                .GetMetadata<RequiredPermissionAttribute>()?
                .Permission;

            // إذا لا يوجد شرط → أكمل
            if (requiredPermission == null)
            {
                await _next(context);
                return;
            }

            // التحقق
            if (!permissions.Contains(requiredPermission))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access Denied: Missing Permission");
                return;
            }

            await _next(context);
        }
    }
}