using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.DTOs;
using SimpleToDoAPI.DTOs.Permissions;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _service;

        public PermissionsController(
            IRolePermissionService service)
        {
            _service = service;
        }

        [HttpGet("matrix")]
        public async Task<IActionResult> GetMatrix()
        {
            var result =
                await _service
                    .GetPermissionMatrixAsync();

            //return Ok(
            //    new ApiResponse<
            //        IEnumerable<RolePermissionMatrixDto>>
            //    (
            //        true,
            //        "تم جلب مصفوفة الصلاحيات بنجاح",
            //        result
            //    ));

            return Ok(new ApiResponse<IEnumerable<RolePermissionMatrixDto>>(true, "تم جلب المهام بنجاح",null));
        }
    }
}