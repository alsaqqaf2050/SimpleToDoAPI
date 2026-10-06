using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleToDoAPI.Middlewares
{
    public static class MaintenanceState
    {
        public static bool IsMaintenanceMode { get; set; } = false;
    }

    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (MaintenanceState.IsMaintenanceMode)
            {
                // Allow admins to login, and allow the system controller to toggle maintenance off
                var path = context.Request.Path.Value?.ToLower();
                if (path != null && !path.StartsWith("/api/auth/login") && !path.StartsWith("/api/system"))
                {
                    context.Response.StatusCode = 503;
                    context.Response.ContentType = "application/json";
                    
                    var response = new { 
                        isSuccess = false, 
                        message = "النظام حالياً تحت الصيانة. يرجى المحاولة لاحقاً." 
                    };
                    
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }
            }

            await _next(context);
        }
    }
}
