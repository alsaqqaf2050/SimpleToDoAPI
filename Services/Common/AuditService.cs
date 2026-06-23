using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.DTOs.Audit;
using SimpleToDoAPI.Helpers;
using SimpleToDoAPI.Models;
using SimpleToDoAPI.Services.Interfaces;

namespace SimpleToDoAPI.Services.Common
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        private readonly ICurrentUserService _currentUser;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string action,
            string entityName,
            string entityId,
            object? oldValues = null,
            object? newValues = null)
        {
            var log = new AuditLog
            {
                UserId = _currentUser.UserId,

                UserName =
                    _httpContextAccessor
                        .HttpContext?
                        .User?
                        .Identity?
                        .Name
                    ?? "",

                Action = action,

                EntityName = entityName,

                EntityId = entityId,

                OldValues =
                    oldValues == null
                    ? null
                    : JsonSerializer.Serialize(oldValues),

                NewValues =
                    newValues == null
                    ? null
                    : JsonSerializer.Serialize(newValues),

                IpAddress =
                    _httpContextAccessor
                        .HttpContext?
                        .Connection?
                        .RemoteIpAddress?
                        .ToString()
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }



        public async Task<PagedResult<AuditLogResponseDto>> GetLogsAsync(AuditQueryParametersDto parameters)
        {
            var query =
                _context.AuditLogs
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.UserId))
            {
                query = query.Where(x =>
                    x.UserId == parameters.UserId);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Action))
            {
                query = query.Where(x =>
                    x.Action.Contains(parameters.Action));
            }

            if (!string.IsNullOrWhiteSpace(parameters.EntityName))
            {
                query = query.Where(x =>
                    x.EntityName.Contains(
                        parameters.EntityName));
            }

            if (parameters.FromDate.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedOn >= parameters.FromDate);
            }

            if (parameters.ToDate.HasValue)
            {
                query = query.Where(x =>
                    x.CreatedOn <= parameters.ToDate);
            }

            var totalRecords =
                await query.CountAsync();

            var logs = await query
                .OrderByDescending(x => x.CreatedOn)
                .Skip(
                    (parameters.PageNumber - 1)
                    * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(x =>
                    new AuditLogResponseDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Action = x.Action,
                        EntityName = x.EntityName,
                        EntityId = x.EntityId,
                        OldValues = x.OldValues,
                        NewValues = x.NewValues,
                        CreatedOn = x.CreatedOn
                    })
                .ToListAsync();

            return new PagedResult<AuditLogResponseDto>
            {
                Items = logs,
                TotalRecords = totalRecords,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }



        public async Task<AuditLogResponseDto?> GetByIdAsync(long id)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AuditLogResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Action = x.Action,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    OldValues = x.OldValues,
                    NewValues = x.NewValues,
                    IpAddress = x.IpAddress,
                    CreatedOn = x.CreatedOn
                })
                .FirstOrDefaultAsync();
        }


    }

}
