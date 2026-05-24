namespace SimpleTodoAPI.Helpers
{
    public class ApiErrorResponse
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public object? Errors { get; set; }

        public ApiErrorResponse()
        {
        }

        public ApiErrorResponse(
            string message,
            object? errors = null)
        {
            Message = message;
            Errors = errors;
        }
    }
}