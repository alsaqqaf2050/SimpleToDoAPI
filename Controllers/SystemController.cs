using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Middlewares;

namespace SimpleToDoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SystemController : ControllerBase
    {
        [HttpPost("maintenance/toggle")]
        public IActionResult ToggleMaintenanceMode([FromBody] MaintenanceToggleRequest request)
        {
            MaintenanceState.IsMaintenanceMode = request.IsMaintenanceMode;
            return Ok(new { isSuccess = true, message = $"تم تغيير وضع الصيانة إلى {request.IsMaintenanceMode}", isMaintenanceMode = MaintenanceState.IsMaintenanceMode });
        }

        [HttpGet("maintenance/status")]
        [AllowAnonymous]
        public IActionResult GetMaintenanceStatus()
        {
            return Ok(new { isSuccess = true, isMaintenanceMode = MaintenanceState.IsMaintenanceMode });
        }
    }

    public class MaintenanceToggleRequest
    {
        public bool IsMaintenanceMode { get; set; }
    }
}
