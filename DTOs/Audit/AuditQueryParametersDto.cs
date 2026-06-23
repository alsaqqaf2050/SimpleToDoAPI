namespace SimpleToDoAPI.DTOs.Audit
{
    public class AuditQueryParametersDto
    {
        const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 20;

        public int PageSize
        {
            get => _pageSize;

            set => _pageSize =
                value > MaxPageSize
                ? MaxPageSize
                : value;
        }

        public string? UserId { get; set; }

        public string? Action { get; set; }

        public string? EntityName { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}