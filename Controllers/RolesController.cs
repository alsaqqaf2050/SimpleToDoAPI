using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.DTOs.Roles;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleService roleService,
            ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.RoleView)]
        [HttpGet]
        public async Task<IActionResult> GetAll(
    [FromQuery] RoleQueryParametersDto parameters)
        {
            var result =
                await _roleService.GetAllAsync(parameters);

            var pagination =
                new PaginationMetadata
                {
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalRecords = result.TotalRecords,
                    TotalPages = result.TotalPages
                };

            return Ok(
                new ApiResponse<IEnumerable<RoleResponseDto>>(
                    true,
                    "تم جلب الأدوار بنجاح",
                    result.Items,
                    pagination));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.RoleView)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
    string id)
        {
            var role =
                await _roleService.GetByIdAsync(id);

            if (role == null)
            {
                return NotFound(
                    new ApiErrorResponse(
                        $"الدور بالمعرف {id} غير موجود"));
            }

            return Ok(
                new ApiResponse<RoleResponseDto>(
                    true,
                    "تم جلب الدور بنجاح",
                    role));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.RoleCreate)]
        [HttpPost]
        public async Task<IActionResult> Create(
    [FromBody] CreateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ApiErrorResponse(
                        "البيانات غير صحيحة",
                        ModelState));
            }

            var role =
                await _roleService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = role.Id },
                new ApiResponse<RoleResponseDto>(
                    true,
                    "تم إنشاء الدور بنجاح",
                    role));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.RoleUpdate)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    string id,
    [FromBody] UpdateRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ApiErrorResponse(
                        "البيانات غير صحيحة",
                        ModelState));
            }

            var role =
                await _roleService.UpdateAsync(id, dto);

            if (role == null)
            {
                return NotFound(
                    new ApiErrorResponse(
                        $"الدور بالمعرف {id} غير موجود"));
            }

            return Ok(
                new ApiResponse<RoleResponseDto>(
                    true,
                    "تم تحديث الدور بنجاح",
                    role));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.RoleDelete)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
    string id)
        {
            var result =
                await _roleService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(
                    new ApiErrorResponse(
                        $"الدور بالمعرف {id} غير موجود"));
            }

            return Ok(
                new ApiResponse<object>(
                    true,
                    "تم حذف الدور بنجاح",
                    null));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.UserCreate)]
        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUser(
    [FromBody] AssignUserRoleDto dto)
        {
            var result =
                await _roleService.AssignUserAsync(
                    dto.UserId,
                    dto.RoleName);

            if (!result)
            {
                return BadRequest(
                    new ApiErrorResponse(
                        "فشل إسناد الدور للمستخدم"));
            }

            return Ok(
                new ApiResponse<object>(
                    true,
                    "تم إسناد الدور للمستخدم بنجاح",
                    null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.UserDelete)]
        [HttpPost("remove-user")]
        public async Task<IActionResult> RemoveUser(
    [FromBody] AssignUserRoleDto dto)
        {
            var result =
                await _roleService.RemoveUserAsync(
                    dto.UserId,
                    dto.RoleName);

            if (!result)
            {
                return BadRequest(
                    new ApiErrorResponse(
                        "فشل إزالة الدور من المستخدم"));
            }

            return Ok(
                new ApiResponse<object>(
                    true,
                    "تم إزالة الدور من المستخدم بنجاح",
                    null));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}/permissions")]
        [Authorize(Policy = Policies.RoleUpdate)]
        public async Task<IActionResult> GetPermissions(
    string roleId)
        {
            var permissions =
                await _roleService
                    .GetPermissionsAsync(roleId);

            return Ok(
                new ApiResponse<
                    List<RolePermissionResponseDto>>
                (
                    true,
                    "تم جلب الصلاحيات بنجاح",
                    permissions
                ));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        [HttpPost("{roleId}/permissions/{permissionId}")]
        [Authorize(Policy = Policies.RoleUpdate)]
        public async Task<IActionResult> AssignPermission(
    string roleId,
    int permissionId)
        {
            var result =
                await _roleService
                    .AssignPermissionAsync(
                        roleId,
                        permissionId);

            if (!result)
            {
                return BadRequest(
                    new ApiErrorResponse(
                        "فشل إسناد الصلاحية"));
            }

            return Ok(
                new ApiResponse<object>(
                    true,
                    "تم إسناد الصلاحية بنجاح",
                    null));
        }



        [Authorize(Policy = Policies.AdminOnly)]
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermission(
    string roleId,
    int permissionId)
        {
            var result =
                await _roleService
                    .RemovePermissionAsync(
                        roleId,
                        permissionId);

            if (!result)
            {
                return BadRequest(
                    new ApiErrorResponse(
                        "فشل إزالة الصلاحية"));
            }

            return Ok(
                new ApiResponse<object>(
                    true,
                    "تم إزالة الصلاحية بنجاح",
                    null));
        }



    }
}