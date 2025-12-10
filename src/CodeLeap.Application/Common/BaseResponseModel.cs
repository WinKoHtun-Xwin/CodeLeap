namespace CodeLeap.Application.Common
{
    public class BaseResponseModel<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public required string Message { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }

        /// <summary>
        /// Creates a success response with HTTP 200 OK status
        /// </summary>
        public static BaseResponseModel<T> SuccessResponse(T data, string message = "")
            => new() { Success = true, StatusCode = HttpStatusCodes.Ok, Data = data, Message = message, Error = "" };

        /// <summary>
        /// Creates a success response with a custom status code (e.g., 201 for Created)
        /// </summary>
        public static BaseResponseModel<T> SuccessResponse(T data, int statusCode, string message = "")
            => new() { Success = true, StatusCode = statusCode, Data = data, Message = message, Error = "" };

        /// <summary>
        /// Creates a failure response with a specified status code
        /// </summary>
        public static BaseResponseModel<T> Failure(int statusCode, string message, string error = "")
            => new() { Success = false, StatusCode = statusCode, Message = message, Error = error };

        /// <summary>
        /// Creates a 404 Not Found response
        /// </summary>
        public static BaseResponseModel<T> NotFoundResponse(string message)
            => new() { Success = false, StatusCode = HttpStatusCodes.NotFound, Message = message, Error = "" };

        /// <summary>
        /// Creates a 409 Conflict response
        /// </summary>
        public static BaseResponseModel<T> ConflictResponse(string message)
            => new() { Success = false, StatusCode = HttpStatusCodes.Conflict, Message = message, Error = "" };

        /// <summary>
        /// Creates a 500 Internal Server Error response
        /// </summary>
        public static BaseResponseModel<T> ServerErrorResponse(string message, string error = "")
            => new() { Success = false, StatusCode = HttpStatusCodes.InternalServerError, Message = message, Error = error };
    }

    public class BaseResponseModelPagination<T> : BaseResponseModel<T>
    {
        public PaginationDto Pagination { get; set; } = new PaginationDto();
    }

    public class PaginationDto
    {
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public int TotalPages { get; set; } = 0;
        public int TotalItems { get; set; } = 0;
    }
}