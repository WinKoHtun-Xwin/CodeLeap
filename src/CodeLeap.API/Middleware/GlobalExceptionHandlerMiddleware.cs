using CodeLeap.Application.Common;
using System.Net;
using System.Text.Json;

namespace CodeLeap.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred");
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

            var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
