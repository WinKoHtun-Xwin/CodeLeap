using Moq;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Services;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.User;
using CodeLeap.Core.Entities;

namespace CodeLeap.Test
{
    public class UserTest
    {
        private readonly Mock<IUserRepository> userRepo;
        private readonly Mock<ILoggerService<UserService>> logger;
        private readonly Mock<ICurrentUserService> currentUserService;
        private readonly UserService userService;
        
        public UserTest()
        {
            userRepo = new Mock<IUserRepository>();
            logger = new Mock<ILoggerService<UserService>>();
            currentUserService = new Mock<ICurrentUserService>();
            
            // UserService now only takes 3 parameters (removed IPasswordService)
            userService = new UserService(userRepo.Object, currentUserService.Object, logger.Object);
        }
        
        [Fact]
        public async Task TestGetAllUsersAsync()
        {
            var users = new List<UserEntity>
            {
                new UserEntity { Id = "1", UserName = "John Doe", CreatedAt = DateTime.UtcNow, CreatedBy = "1" },
                new UserEntity { Id = "2", UserName = "Jane Doe", CreatedAt = DateTime.UtcNow, CreatedBy = "1" }
            };
            userRepo.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);
            var result = await userService.GetAllUsersAsync();
            Assert.True(result.Success);
            Assert.Equal(users.Count, result.Data.Count());
        }

        [Fact]
        public async Task TestGetUserByIdAsync()
        {
            var user = new UserEntity { Id = "1", UserName = "John Doe", CreatedAt = DateTime.UtcNow, CreatedBy = "1" };
            userRepo.Setup(x => x.GetUserByIdAsync("1")).ReturnsAsync(user);
            var result = await userService.GetUserByIdAsync("1");
            Assert.True(result.Success);
            Assert.Equal(user.Id, result.Data.Id);
        }

        [Fact]
        public async Task TestCreateUserAsync()
        {
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            
            userRepo.Setup(x => x.CreateUserAsync(It.IsAny<UserEntity>()))
                   .ReturnsAsync((UserEntity input) => input);
            
            // Note: Password is removed from UserEntity, but DTO still has it for API input
            // UserService no longer hashes passwords - that's handled by Identity
            CreateUserDto createUserDto = new CreateUserDto { Username = "John Doe", Password = "123456" };
            var result = await userService.CreateUserAsync(createUserDto);
            
            Assert.True(result.Success);
            Assert.Equal("John Doe", result.Data.UserName); // Changed from Username to UserName
            Assert.Equal("current-user-id", result.Data.CreatedBy);
        }

        [Fact]
        public async Task TestUpdateUserAsync()
        {
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            
            userRepo.Setup(x => x.UpdateUserAsync("1", It.IsAny<UserEntity>()))
                   .ReturnsAsync((string id, UserEntity input) => input);
            
            CreateUserDto updateUserDto = new CreateUserDto { Username = "John Doe Updated", Password = "654321" };
            var result = await userService.UpdateUserAsync("1", updateUserDto);
            
            Assert.True(result.Success);
            Assert.Equal("1", result.Data.Id);
            Assert.Equal("John Doe Updated", result.Data.UserName); // Changed from Username to UserName
        }

        [Fact]
        public async Task TestDeleteUserAsync()
        {
            userRepo.Setup(x => x.DeleteUserAsync("1")).ReturnsAsync(true);
            var result = await userService.DeleteUserAsync("1");
            Assert.True(result.Success);
        }
    }
}