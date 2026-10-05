namespace SimpleToDoAPI.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        
        // You could also add OverdueTasks if TodoItem has a DueDate
        public List<EmployeeStatsDto> EmployeeStats { get; set; } = new List<EmployeeStatsDto>();
    }

    public class EmployeeStatsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
    }
}
