using CodeLeap.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodeLeap.API.Filters
{
    /// <summary>
    /// Global validation filter that validates model state and returns standardized error responses
    /// </summary>
    public class ModelValidationFilterAttribute : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                var errorMessages = string.Join("; ", errors.SelectMany(e => e.Value));

                var response = BaseResponseModel<object>.Failure(
                    ResponseMessage.GeneralMessage.ValidationFailed,
                    errorMessages
                );

                // Include detailed validation errors for development
                if (context.HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    response.Data = errors;
                }

                context.Result = new BadRequestObjectResult(response);
                return;
            }

            await next();
        }
    }
}
