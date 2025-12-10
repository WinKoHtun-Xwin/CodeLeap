using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.Common;

namespace CodeLeap.API.Filters
{
    /// <summary>
    /// Global authorization filter that validates authenticated users have a valid UserId
    /// Returns 401 if user is authenticated but UserId is null/empty
    /// </summary>
    public class UserAuthorizationFilter(
        IKeycloakUserinfoService userService,
        ILogger<UserAuthorizationFilter> logger) : IAsyncActionFilter
    {

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the endpoint allows anonymous access
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

            if (allowAnonymous)
            {
                // Skip validation for anonymous endpoints
                await next();
                return;
            }

            // Check if user is authenticated
            if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userId = userService.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Authenticated user has no UserId claim. Returning 401 Unauthorized.");

                    context.Result = new UnauthorizedObjectResult(new BaseResponseModel<object>
                    {
                        Success = false,
                        Message = ResponseMessage.GeneralMessage.Unauthorized,
                        Error = "User ID not found in token claims"
                    });
                    return;
                }
            }

            await next();
        }
    }
}
