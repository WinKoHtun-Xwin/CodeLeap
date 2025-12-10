namespace CodeLeap.Application.Common
{
    /// <summary>
    /// HTTP status code constants for consistent use across the application
    /// </summary>
    public static class HttpStatusCodes
    {
        /// <summary>
        /// 200 OK - The request succeeded
        /// </summary>
        public const int Ok = 200;

        /// <summary>
        /// 201 Created - The request succeeded and a new resource was created
        /// </summary>
        public const int Created = 201;

        /// <summary>
        /// 400 Bad Request - The server cannot process the request due to client error
        /// </summary>
        public const int BadRequest = 400;

        /// <summary>
        /// 401 Unauthorized - Authentication is required and has failed or not been provided
        /// </summary>
        public const int Unauthorized = 401;

        /// <summary>
        /// 403 Forbidden - The client does not have access rights to the content
        /// </summary>
        public const int Forbidden = 403;

        /// <summary>
        /// 404 Not Found - The server cannot find the requested resource
        /// </summary>
        public const int NotFound = 404;

        /// <summary>
        /// 409 Conflict - The request conflicts with the current state of the server
        /// </summary>
        public const int Conflict = 409;

        /// <summary>
        /// 500 Internal Server Error - The server encountered an unexpected condition
        /// </summary>
        public const int InternalServerError = 500;
    }
}
