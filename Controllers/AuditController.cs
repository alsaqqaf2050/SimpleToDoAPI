using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.DTOs.Audit;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.AuditView)]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(
            IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLogs(
            [FromQuery]
            AuditQueryParametersDto parameters)
        {
            var result =
                await _auditService
                    .GetLogsAsync(parameters);

            var pagination =
                new PaginationMetadata
                {
                    PageNumber =
                        result.PageNumber,

                    PageSize =
                        result.PageSize,

                    TotalPages =
                        result.TotalPages,

                    TotalRecords =
                        result.TotalRecords
                };

            return Ok(
                new ApiResponse<
                    IEnumerable<
                        AuditLogResponseDto>>
                (
                    true,
                    "تم جلب السجلات بنجاح",
                    result.Items,
                    pagination
                ));
        }




        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var item =
                await _auditService.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound(
                    new ApiErrorResponse(
                        $"السجل رقم {id} غير موجود"));
            }

            return Ok(
                new ApiResponse<AuditLogResponseDto>
                (
                    true,
                    "تم جلب السجل بنجاح",
                    item
                ));
        }



    }
}