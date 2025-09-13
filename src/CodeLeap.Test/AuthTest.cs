using Moq;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Services;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Core.Entities;

namespace CodeLeap.Test
{
    public class AuthTest
    {
        private readonly Mock<IUserRepository> userRepo;
        private readonly Mock<IPasswordService> passwordService;
        private readonly Mock<IJwtService> jwtService;
        private readonly Mock<ILoggerService<AuthService>> logger;
        private readonly AuthService authService;

        public AuthTest()
        {
            userRepo = new Mock<IUserRepository>();
            passwordService = new Mock<IPasswordService>();
            jwtService = new Mock<IJwtService>();
            logger = new Mock<ILoggerService<AuthService>>();
            authService = new AuthService(userRepo.Object, passwordService.Object, jwtService.Object, logger.Object);
        }

        #region RefreshTokenAsync Tests

        [Fact]
        public async Task TestRefreshTokenAsync_Success()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var authDto = new GetAuthDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            jwtService.Setup(x => x.RefreshTokenAsync(refreshToken)).ReturnsAsync(authDto);

            // Act
            var result = await authService.RefreshTokenAsync(refreshToken);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful", result.Message);
            Assert.Equal("new-access-token", result?.Data?.AccessToken);
        }

        [Fact]
        public async Task TestRefreshTokenAsync_NullToken()
        {
            // Act
            var result = await authService.RefreshTokenAsync(null!);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Bad request", result.Message);
        }

        [Fact]
        public async Task TestRefreshTokenAsync_EmptyToken()
        {
            // Act
            var result = await authService.RefreshTokenAsync("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Bad request", result.Message);
        }

        [Fact]
        public async Task TestRefreshTokenAsync_InvalidToken()
        {
            // Arrange
            var refreshToken = "invalid-refresh-token";
            jwtService.Setup(x => x.RefreshTokenAsync(refreshToken)).ReturnsAsync((GetAuthDto?)null);

            // Act
            var result = await authService.RefreshTokenAsync(refreshToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.Message);
        }

        [Fact]
        public async Task TestRefreshTokenAsync_ServiceThrowsException()
        {
            // Arrange
            var refreshToken = "test-token";
            jwtService.Setup(x => x.RefreshTokenAsync(refreshToken)).ThrowsAsync(new Exception("JWT service error"));

            // Act
            var result = await authService.RefreshTokenAsync(refreshToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An internal server error occurred", result.Message);
            Assert.Equal("JWT service error", result.Error);
        }

        #endregion

        #region LoginUser Tests

        [Fact]
        public async Task TestLoginUser_Success()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            };

            var user = new UserEntity
            {
                Id = "user-id",
                Username = "testuser",
                Password = "hashed-password",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            var authDto = new GetAuthDto
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            userRepo.Setup(x => x.GetByUserNameAsync("testuser")).ReturnsAsync(user);
            passwordService.Setup(x => x.VerifyPassword("hashed-password", "password123")).Returns(true);
            jwtService.Setup(x => x.GenerateToken("user-id", "testuser", "User")).ReturnsAsync(authDto);

            // Act
            var result = await authService.LoginUser(loginRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful", result.Message);
            Assert.Equal("access-token", result?.Data?.AccessToken);
        }

        [Fact]
        public async Task TestLoginUser_UserNotFound()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistent",
                Password = "password123"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("nonexistent")).ReturnsAsync((UserEntity?)null);

            // Act
            var result = await authService.LoginUser(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task TestLoginUser_InvalidPassword()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var user = new UserEntity
            {
                Id = "user-id",
                Username = "testuser",
                Password = "hashed-password",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("testuser")).ReturnsAsync(user);
            passwordService.Setup(x => x.VerifyPassword("hashed-password", "wrongpassword")).Returns(false);

            // Act
            var result = await authService.LoginUser(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password", result.Message);
        }

        [Fact]
        public async Task TestLoginUser_TokenGenerationFails()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            };

            var user = new UserEntity
            {
                Id = "user-id",
                Username = "testuser",
                Password = "hashed-password",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("testuser")).ReturnsAsync(user);
            passwordService.Setup(x => x.VerifyPassword("hashed-password", "password123")).Returns(true);
            jwtService.Setup(x => x.GenerateToken("user-id", "testuser", "User")).ReturnsAsync((GetAuthDto?)null);

            // Act
            var result = await authService.LoginUser(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to generate authentication token", result.Message);
        }

        [Fact]
        public async Task TestLoginUser_ServiceThrowsException()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("testuser")).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await authService.LoginUser(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An internal server error occurred", result.Message);
            Assert.Equal("Database error", result.Error);
        }

        #endregion

        #region RegisterNewUser Tests

        [Fact]
        public async Task TestRegisterNewUser_Success()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "newuser",
                Password = "password123"
            };

            var createdUser = new UserEntity
            {
                Id = "new-user-id",
                Username = "newuser",
                Password = "hashed-password",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "new-user-id"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("newuser")).ReturnsAsync((UserEntity?)null);
            passwordService.Setup(x => x.HashPassword("password123")).Returns("hashed-password");
            userRepo.Setup(x => x.CreateUserAsync(It.IsAny<UserEntity>())).ReturnsAsync(createdUser);

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task TestRegisterNewUser_UserAlreadyExists()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "existinguser",
                Password = "password123"
            };

            var existingUser = new UserEntity
            {
                Id = "existing-id",
                Username = "existinguser",
                Password = "existing-password",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("existinguser")).ReturnsAsync(existingUser);

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User with the same username already exists", result.Message);
        }

        [Fact]
        public async Task TestRegisterNewUser_CreationFails()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "newuser",
                Password = "password123"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("newuser")).ReturnsAsync((UserEntity?)null);
            passwordService.Setup(x => x.HashPassword("password123")).Returns("hashed-password");
            userRepo.Setup(x => x.CreateUserAsync(It.IsAny<UserEntity>())).ThrowsAsync(new Exception("User creation failed"));

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An internal server error occurred", result.Message);
            Assert.Equal("User creation failed", result.Error);
        }

        [Fact]
        public async Task TestRegisterNewUser_WithWhitespaceUsername()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "  newuser  ",
                Password = "  password123  "
            };

            var createdUser = new UserEntity
            {
                Id = "new-user-id",
                Username = "newuser",
                Password = "hashed-password",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "new-user-id"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("newuser")).ReturnsAsync((UserEntity?)null);
            passwordService.Setup(x => x.HashPassword("password123")).Returns("hashed-password");
            userRepo.Setup(x => x.CreateUserAsync(It.IsAny<UserEntity>())).ReturnsAsync(createdUser);

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
            
            // Verify that username was trimmed
            userRepo.Verify(x => x.GetByUserNameAsync("newuser"), Times.Once);
        }

        [Fact]
        public async Task TestRegisterNewUser_ServiceThrowsException()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "newuser",
                Password = "password123"
            };

            userRepo.Setup(x => x.GetByUserNameAsync("newuser")).ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An internal server error occurred", result.Message);
            Assert.Equal("Database connection failed", result.Error);
        }

        #endregion
    }
}