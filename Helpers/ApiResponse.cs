namespace SimpleTodoAPI.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public PaginationMetadata? Pagination { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(
            bool success,
            string message,
            T? data,
            PaginationMetadata? pagination = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Pagination = pagination;
        }
    }
}