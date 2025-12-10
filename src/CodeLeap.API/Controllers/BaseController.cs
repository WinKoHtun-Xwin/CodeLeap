using Microsoft.AspNetCore.Mvc;

namespace CodeLeap.API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Converts a BaseResponseModel with a status code to the appropriate HTTP response
    /// </summary>
    /// <typeparam name="T">The type of data in the response model</typeparam>
    /// <param name="result">The response model containing status code and data</param>
    /// <returns>An IActionResult with the appropriate HTTP status code</returns>
    protected IActionResult ConvertToHttpResponse<T>(Application.Common.BaseResponseModel<T> result)
    {
        return result.StatusCode switch
        {
            200 => Ok(result),
            201 => Created("", result),
            400 => BadRequest(result),
            401 => Unauthorized(result),
            403 => StatusCode(403, result), // Forbid() doesn't accept a value
            404 => NotFound(result),
            409 => Conflict(result),
            500 => StatusCode(500, result),
            _ => StatusCode(result.StatusCode, result),
        };
    }
}
