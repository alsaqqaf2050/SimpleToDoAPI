using SimpleToDoAPI.DTOs.Dashboard;

namespace SimpleToDoAPI.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardStatsAsync();
    }
}
