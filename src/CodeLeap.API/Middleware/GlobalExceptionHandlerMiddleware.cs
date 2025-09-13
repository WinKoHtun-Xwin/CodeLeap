using CodeLeap.Application.Common;
using System.Net;
using System.Text.Json;

namespace CodeLeap.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = exception switch
            {
                ArgumentException => BaseResponseModel<object>.Failure(
                    ResponseMessage.GeneralMessage.ValidationFailed,
                    exception.Message),
                KeyNotFoundException => BaseResponseModel<object>.Failure(
                    ResponseMessage.GeneralMessage.NotFound,
                    exception.Message),
                UnauthorizedAccessException => BaseResponseModel<object>.Failure(
                    ResponseMessage.GeneralMessage.Unauthorized,
                    exception.Message),
                _ => BaseResponseModel<object>.Failure(
                    ResponseMessage.GeneralMessage.InternalServerError,
                    "An unexpected error occurred")
            };

            context.Response.StatusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
