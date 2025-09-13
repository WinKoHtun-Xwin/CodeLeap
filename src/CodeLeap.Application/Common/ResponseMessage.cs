namespace CodeLeap.Application.Common
{
    public static class ResponseMessage
    {
        public static class GeneralMessage
        {
            public const string Success = "Operation completed successfully";
            public const string ValidationFailed = "Validation failed";
            public const string NotFound = "Resource not found";
            public const string Unauthorized = "Unauthorized access";
            public const string InternalServerError = "An internal server error occurred";
            public const string BadRequest = "Bad request";
        }

        public static class RegisterNewUserMessage
        {
            public const string Success = "User registered successfully";
            public const string UserAlreadyExists = "User with the same username already exists";
            public const string RegistrationFailed = "User registration failed";
        }
        public static class LoginMessage
        {

            public const string Success = "Login successful";
            public const string InvalidCredentials = "Invalid username or password";
            public const string UserNotFound = "User not found";
            public const string TokenGenerationFailed = "Failed to generate authentication token";
        }

        public static class UserMessage
        {
            public const string Success = "Operation completed successfully";
            public const string CreatedSuccess = "User created successfully";
            public const string CreatedFail = "Failed to create user";
            public const string UpdatedSuccess = "User updated successfully";
            public const string UpdatedFail = "Failed to update user";
            public const string DeletedSuccess = "User deleted successfully";
            public const string DeletedFail = "Failed to delete user";
            public const string NotFound = "User not found";
            public const string GetSuccess = "User retrieved successfully";
            public const string GetAllSuccess = "Users retrieved successfully";
        }

        public static class ProductMessage
        {
            public const string GetSuccess = "Product retrieved successfully";
            public const string GetAllSuccess = "Products retrieved successfully";
            public const string Success = "Operation completed successfully";
            public const string CreatedSuccess = "Product created successfully";
            public const string CreatedFail = "Failed to create product";
            public const string UpdatedSuccess = "Product updated successfully";
            public const string UpdatedFail = "Failed to update product";
            public const string DeletedSuccess = "Product deleted successfully";
            public const string DeletedFail = "Failed to delete product";
            public const string NotFound = "Product not found";
            public const string AlreadyExists = "Product already exists";
        }
    }
}
