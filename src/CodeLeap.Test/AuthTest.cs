using Moq;
using CodeLeap.Application.Services;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.Auth;
using CodeLeap.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace CodeLeap.Test
{
    public class AuthTest
    {
        private readonly Mock<UserManager<UserEntity>> userManager;
        private readonly Mock<IJwtService> jwtService;
        private readonly Mock<ILoggerService<AuthService>> logger;
        private readonly AuthService authService;

        public AuthTest()
        {
            // Mock UserManager (complex setup required)
            var userStoreMock = new Mock<IUserStore<UserEntity>>();
            userManager = new Mock<UserManager<UserEntity>>(
                userStoreMock.Object,
                null!, null!, null!, null!, null!, null!, null!, null!);

            jwtService = new Mock<IJwtService>();
            logger = new Mock<ILoggerService<AuthService>>();
            var refreshTokenRepository = new Mock<CodeLeap.Core.IRepositories.IRefreshTokenRepository>();
            authService = new AuthService(userManager.Object, jwtService.Object, logger.Object, refreshTokenRepository.Object);
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
                UserId = "user-id",
                Username = "testuser",
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
            Assert.Equal("Invalid or expired refresh token. Please login again.", result.Message);
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
                EmailOrUsername = "testuser",
                Password = "password123"
            };

            var user = new UserEntity
            {
                Id = "user-id",
                UserName = "testuser",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            var authDto = new GetAuthDto
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                UserId = "user-id",
                Username = "testuser",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            userManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, "password123")).ReturnsAsync(true);
            userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["User"]);
            jwtService.Setup(x => x.GenerateToken("user-id", "testuser", It.IsAny<List<string>>())).ReturnsAsync(authDto);

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
                EmailOrUsername = "nonexistent",
                Password = "password123"
            };

            userManager.Setup(x => x.FindByNameAsync("nonexistent")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.FindByEmailAsync("nonexistent")).ReturnsAsync((UserEntity?)null);

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
                EmailOrUsername = "testuser",
                Password = "wrongpassword"
            };

            var user = new UserEntity
            {
                Id = "user-id",
                UserName = "testuser",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            userManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, "wrongpassword")).ReturnsAsync(false);

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
                EmailOrUsername = "testuser",
                Password = "password123"
            };

            var user = new UserEntity
            {
                Id = "user-id",
                UserName = "testuser",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            userManager.Setup(x => x.FindByNameAsync("testuser")).ReturnsAsync(user);
            userManager.Setup(x => x.CheckPasswordAsync(user, "password123")).ReturnsAsync(true);
            userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["User"]);
            jwtService.Setup(x => x.GenerateToken("user-id", "testuser", It.IsAny<List<string>>())).ReturnsAsync((GetAuthDto?)null);

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
                EmailOrUsername = "testuser",
                Password = "password123"
            };

            userManager.Setup(x => x.FindByNameAsync("testuser")).ThrowsAsync(new Exception("Database error"));

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
                Email = "newuser@test.com",
                Password = "password123"
            };

            userManager.Setup(x => x.FindByNameAsync("newuser")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), "password123"))
                .ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

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
                Email = "existing@test.com",
                Password = "password123"
            };

            var existingUser = new UserEntity
            {
                Id = "existing-id",
                UserName = "existinguser",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            userManager.Setup(x => x.FindByNameAsync("existinguser")).ReturnsAsync(existingUser);

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
                Email = "newuser@test.com",
                Password = "password123"
            };

            var identityError = new IdentityError { Description = "User creation failed" };
            var failureResult = IdentityResult.Failed(identityError);

            userManager.Setup(x => x.FindByNameAsync("newuser")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), "password123"))
                .ReturnsAsync(failureResult);

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User registration failed", result.Message);
        }

        [Fact]
        public async Task TestRegisterNewUser_WithWhitespaceUsername()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "  newuser  ",
                Email = "newuser@test.com",
                Password = "  password123  "
            };

            userManager.Setup(x => x.FindByNameAsync("newuser")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.FindByEmailAsync("newuser@test.com")).ReturnsAsync((UserEntity?)null);
            userManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), "password123"))
                .ReturnsAsync(IdentityResult.Success);
            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<UserEntity>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await authService.RegisterNewUser(registerDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);

            // Verify that username was trimmed
            userManager.Verify(x => x.FindByNameAsync("newuser"), Times.Once);
        }

        [Fact]
        public async Task TestRegisterNewUser_ServiceThrowsException()
        {
            // Arrange
            var registerDto = new RegisterNewUserDto
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "password123"
            };

            userManager.Setup(x => x.FindByNameAsync("newuser")).ThrowsAsync(new Exception("Database connection failed"));

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