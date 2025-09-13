namespace CodeLeap.Application.Common
{
    public class BaseResponseModel<T>
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }

        public static BaseResponseModel<T> SuccessResponse(T data, string message = "")
            => new() { Success = true, Data = data, Message = message , Error = "" };

        public static BaseResponseModel<T> Failure(string message , string error="")
            => new() { Success = false, Message = message , Error = error };
    }
}