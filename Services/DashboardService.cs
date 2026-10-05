using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SimpleToDoAPI.Constants;
using SimpleToDoAPI.Data;
using SimpleToDoAPI.DTOs.Dashboard;
using SimpleToDoAPI.Services.Interfaces;
using SimpleToDoAPI.Services.Common;

namespace SimpleToDoAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;

        public DashboardService(ApplicationDbContext context, ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor, IMemoryCache cache)
        {
            _context = context;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<DashboardResponseDto> GetDashboardStatsAsync()
        {
            var isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole(Roles.Admin) ?? false;
            var userId = _currentUserService.UserId;

            var cacheKey = isAdmin ? "dashboard_stats_admin" : $"dashboard_stats_{userId}";

            if (_cache.TryGetValue(cacheKey, out DashboardResponseDto? cachedStats) && cachedStats != null)
            {
                return cachedStats;
            }

            var query = _context.TodoItems.AsQueryable();

            if (!isAdmin)
            {
                // Normal user only sees their own tasks (created by them or assigned to them)
                query = query.Where(t => t.UserId == userId || t.AssignedToUserId == userId);
            }

            var allTasks = await query.ToListAsync();

            var response = new DashboardResponseDto
            {
                TotalTasks = allTasks.Count,
                CompletedTasks = allTasks.Count(t => t.IsCompleted),
                PendingTasks = allTasks.Count(t => !t.IsCompleted)
            };

            // Calculate Employee Stats
            // If the user is admin, they can see everyone's stats.
            // If they are a normal user, they only see their own stats.
            var usersQuery = _context.Users.AsQueryable();
            if (!isAdmin)
            {
                usersQuery = usersQuery.Where(u => u.Id == userId);
            }

            var users = await usersQuery.ToListAsync();

            foreach (var u in users)
            {
                // Tasks associated with this user
                var userTasks = allTasks.Where(t => t.UserId == u.Id || t.AssignedToUserId == u.Id).ToList();
                
                // Let's count tasks they are responsible for (either assigned to them, or created by them if unassigned)
                var relevantTasks = userTasks; 

                // Only add to list if they have tasks, or if they are admin viewing all users
                if (isAdmin || relevantTasks.Any())
                {
                    response.EmployeeStats.Add(new EmployeeStatsDto
                    {
                        UserId = u.Id,
                        FullName = u.FullName ?? u.UserName ?? "Unknown",
                        TotalTasks = relevantTasks.Count,
                        CompletedTasks = relevantTasks.Count(t => t.IsCompleted),
                        PendingTasks = relevantTasks.Count(t => !t.IsCompleted)
                    });
                }
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(cacheKey, response, cacheOptions);

            return response;
        }
    }
}
