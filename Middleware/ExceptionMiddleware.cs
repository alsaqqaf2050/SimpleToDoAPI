using System.Net;
using System.Text.Json;
using SimpleToDoAPI.Exceptions;
using SimpleToDoAPI.Helpers;

namespace SimpleToDoAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware( RequestDelegate next,ILogger<ExceptionMiddleware> logger,IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        //private async Task HandleExceptionAsync(
        //    HttpContext context,
        //    Exception exception)
        //{
        //    context.Response.ContentType = "application/json";

        //    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        //    //var response = new
        //    //{
        //    //    success = false,
        //    //    message = "حدث خطأ داخلي في الخادم",

        //    //    // إظهار التفاصيل فقط في بيئة التطوير
        //    //    details = _environment.IsDevelopment()
        //    //        ? exception.Message
        //    //        : null
        //    //};

        //    var response = new ApiErrorResponse("حدث خطأ داخلي في الخادم", _environment.IsDevelopment() ? exception.Message: null);

        //    var jsonResponse = JsonSerializer.Serialize(response);

        //    await context.Response.WriteAsync(jsonResponse);
        //}



        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                BusinessException => HttpStatusCode.BadRequest,

                KeyNotFoundException => HttpStatusCode.NotFound,

                UnauthorizedAccessException => HttpStatusCode.Unauthorized, _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            //var response =
            //    new ApiErrorResponse(
            //        exception is BusinessException
            //            ? exception.Message
            //            : "حدث خطأ داخلي في الخادم",

            //        _environment.IsDevelopment()
            //            ? exception.Message
            //            : null);

            var details = _environment.IsDevelopment() ? exception.InnerException?.Message ?? exception.Message : null;

            var response = new ApiErrorResponse(
                "حدث خطأ داخلي في الخادم",
                details);

            var jsonResponse =
                JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}