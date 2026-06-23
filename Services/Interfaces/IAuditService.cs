using SimpleToDoAPI.DTOs.Audit;
using SimpleToDoAPI.Helpers;

namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityName, string entityId,object? oldValues = null, object? newValues = null);

        Task<PagedResult<AuditLogResponseDto>>GetLogsAsync(AuditQueryParametersDto parameters);

        Task<AuditLogResponseDto?> GetByIdAsync(long id);
    }
}
