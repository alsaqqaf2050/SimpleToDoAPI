//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SimpleToDoAPI.Constants;
//using SimpleToDoAPI.DTOs.Roles;
//using SimpleToDoAPI.Services.Interfaces;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize(Policy = Policies.AdminOnly)]
//public class RolePermissionsController : ControllerBase
//{
//    private readonly IRolePermissionService _service;

//    public RolePermissionsController(IRolePermissionService service)
//    {
//        _service = service;
//    }

//    /// <summary>
//    /// Get Role Permissions
//    /// </summary>
//    /// <param name="roleId"></param>
//    /// <returns></returns>
//    [HttpGet("{roleId}")]
//    public async Task<IActionResult> Get(string roleId)
//    {
//        var permissions = await _service.GetRolePermissionsAsync(roleId);
//        return Ok(permissions);
//    }

//    /// <summary>
//    /// Assign Permission
//    /// </summary>
//    /// <param name="dto"></param>
//    /// <returns></returns>
//    [HttpPost("assign")]
//    public async Task<IActionResult> Assign(AssignPermissionDto dto)
//    {
//        await _service.AssignPermissionAsync(dto.RoleId, dto.PermissionId);
//        return Ok();
//    }

//    /// <summary>
//    /// Remove Permission
//    /// </summary>
//    /// <param name="dto"></param>
//    /// <returns></returns>
//    [HttpDelete]
//    public async Task<IActionResult> Remove(AssignPermissionDto dto)
//    {
//        await _service.RemovePermissionAsync(dto.RoleId, dto.PermissionId);
//        return Ok();
//    }

//    /// <summary>
//    /// Sync Permissions
//    /// </summary>
//    /// <param name="roleId"></param>
//    /// <param name="permissionIds"></param>
//    /// <returns></returns>
//    [HttpPost("sync")]
//    public async Task<IActionResult> Sync(string roleId, List<int> permissionIds)
//    {
//        await _service.SyncPermissionsAsync(roleId, permissionIds);
//        return Ok();
//    }
//}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.DTOs.Permissions;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = "Admin")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _service;

        public RolePermissionsController(
            IRolePermissionService service)
        {
            _service = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}/permissions")]
        [Authorize(Policy = Policies.RoleView)]
        public async Task<IActionResult>
            GetPermissions(string roleId)
        {
            var result =
                await _service.GetRolePermissionsAsync(roleId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("{roleId}/permissions")]
        [Authorize(Policy = Policies.RoleUpdate)]
        public async Task<IActionResult>
            AssignPermission(
                string roleId,
                AssignPermissionDto dto)
        {
            var result =
                await _service.AssignPermissionAsync(
                    roleId,
                    dto.PermissionId);

            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        [Authorize(Policy = Policies.RoleUpdate)]
        public async Task<IActionResult>
            RemovePermission(
                string roleId,
                int permissionId)
        {
            var result =
                await _service.RemovePermissionAsync(
                    roleId,
                    permissionId);

            return Ok(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("permissions/matrix")]
        [Authorize(Policy = Policies.RoleView)]
        public async Task<IActionResult>
    GetPermissionMatrix()
        {
            var result =
                await _service.GetPermissionMatrixAsync();

            return Ok(
                new ApiResponse<
                    IEnumerable<PermissionMatrixDto>>
                (
                    true,
                    "تم جلب مصفوفة الصلاحيات بنجاح",
                    null
                ));
        }
    }
}