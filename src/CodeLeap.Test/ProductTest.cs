using Moq;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Services;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.Product;
using CodeLeap.Core.Entities;

namespace CodeLeap.Test
{
    public class ProductTest
    {
        private readonly Mock<IProductRepository> productRepo;
        private readonly Mock<ILoggerService<ProductService>> logger;
        private readonly Mock<IKeycloakUserinfoService> currentUserService;
        private readonly ProductService productService;

        public ProductTest()
        {
            productRepo = new Mock<IProductRepository>();
            logger = new Mock<ILoggerService<ProductService>>();
            currentUserService = new Mock<IKeycloakUserinfoService>();
            productService = new ProductService(productRepo.Object, currentUserService.Object, logger.Object);
        }

        [Fact]
        public async Task TestGetAllProductsAsync()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new()
                {
                    Id = "1",
                    Name = "Product 1",
                    Description = "Description 1",
                    Price = 10.99m,
                    Stock = 100,
                    ImageUrl = "https://example.com/image1.jpg",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "user1"
                },
                new()
                {
                    Id = "2",
                    Name = "Product 2",
                    Description = "Description 2",
                    Price = 25.50m,
                    Stock = 50,
                    ImageUrl = "https://example.com/image2.jpg",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "user1"
                }
            };

            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetAllProductAsync()).ReturnsAsync(products);

            // Act
            var result = await productService.GetAllProductsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(products.Count, result.Data!.Count());
        }

        [Fact]
        public async Task TestGetProductByIdAsync_Success()
        {
            // Arrange
            var product = new ProductEntity
            {
                Id = "1",
                Name = "Test Product",
                Description = "Test Description",
                Price = 15.99m,
                Stock = 75,
                ImageUrl = "https://example.com/test.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user1"
            };

            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("1")).ReturnsAsync(product);

            // Act
            var result = await productService.GetProductByIdAsync("1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(product.Id, result.Data!.Id);
            Assert.Equal(product.Name, result.Data.Name);
            Assert.Equal(product.Price, result.Data.Price);
        }

        [Fact]
        public async Task TestGetProductByIdAsync_NotFound()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("999")).ReturnsAsync((ProductEntity?)null);

            // Act
            var result = await productService.GetProductByIdAsync("999");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product not found", result.Message);
        }

        [Fact]
        public async Task TestCreateProductAsync_Success()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");

            productRepo.Setup(x => x.CreateProductAsync(It.IsAny<ProductEntity>()))
                      .ReturnsAsync((ProductEntity input) => input);

            var createProductDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "New Description",
                Price = 29.99m,
                Stock = 200,
                ImageUrl = "https://example.com/new.jpg"
            };

            // Act
            var result = await productService.CreateProductAsync(createProductDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New Product", result.Data!.Name);
            Assert.Equal(29.99m, result.Data.Price);
            Assert.Equal("current-user-id", result.Data.CreatedBy);
        }

        [Fact]
        public async Task TestCreateProductAsync_RepositoryReturnsNull()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.CreateProductAsync(It.IsAny<ProductEntity>()))
                      .ReturnsAsync((ProductEntity?)null!);

            var createProductDto = new CreateProductDto
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 19.99m,
                Stock = 100,
                ImageUrl = "https://example.com/test.jpg"
            };

            // Act
            var result = await productService.CreateProductAsync(createProductDto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task TestUpdateProductAsync_Success()
        {
            // Arrange
            var existingProduct = new ProductEntity
            {
                Id = "1",
                Name = "Old Product",
                Description = "Old Description",
                Price = 15.99m,
                Stock = 50,
                ImageUrl = "https://example.com/old.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user1"
            };

            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("1")).ReturnsAsync(existingProduct);
            productRepo.Setup(x => x.UpdateProductAsync("1", It.IsAny<ProductEntity>()))
                      .ReturnsAsync((string id, ProductEntity input) => input);

            var updateProductDto = new CreateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 25.99m,
                Stock = 75,
                ImageUrl = "https://example.com/updated.jpg"
            };

            // Act
            var result = await productService.UpdateProductAsync("1", updateProductDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("1", result.Data!.Id);
            Assert.Equal("Updated Product", result.Data.Name);
            Assert.Equal(25.99m, result.Data.Price);
        }

        [Fact]
        public async Task TestUpdateProductAsync_ProductNotFound()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("999")).ReturnsAsync((ProductEntity?)null);

            var updateProductDto = new CreateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 25.99m,
                Stock = 75,
                ImageUrl = "https://example.com/updated.jpg"
            };

            // Act
            var result = await productService.UpdateProductAsync("999", updateProductDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product not found", result.Message);
        }

        [Fact]
        public async Task TestUpdateProductAsync_RepositoryReturnsNull()
        {
            // Arrange
            var existingProduct = new ProductEntity
            {
                Id = "1",
                Name = "Test Product",
                Description = "Test Description",
                Price = 15.99m,
                Stock = 50,
                ImageUrl = "https://example.com/test.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user1"
            };

            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("1")).ReturnsAsync(existingProduct);
            productRepo.Setup(x => x.UpdateProductAsync("1", It.IsAny<ProductEntity>()))
                      .ReturnsAsync((ProductEntity?)null!);

            var updateProductDto = new CreateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 25.99m,
                Stock = 75,
                ImageUrl = "https://example.com/updated.jpg"
            };

            // Act
            var result = await productService.UpdateProductAsync("1", updateProductDto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task TestDeleteProductAsync_Success()
        {
            // Arrange
            var existingProduct = new ProductEntity
            {
                Id = "1",
                Name = "Product to Delete",
                Description = "Description",
                Price = 15.99m,
                Stock = 10,
                ImageUrl = "https://example.com/delete.jpg",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user1"
            };

            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("1")).ReturnsAsync(existingProduct);
            productRepo.Setup(x => x.DeleteProductAsync("1")).ReturnsAsync(true);

            // Act
            var result = await productService.DeleteProductAsync("1");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task TestDeleteProductAsync_ProductNotFound()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns("current-user-id");
            productRepo.Setup(x => x.GetProductByIdAsync("999")).ReturnsAsync((ProductEntity?)null);

            // Act
            var result = await productService.DeleteProductAsync("999");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Product not found", result.Message);
        }

        [Fact]
        public async Task TestGetAllProductsAsync_Unauthorized()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns((string?)null);

            // Act
            var result = await productService.GetAllProductsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized access", result.Message);
        }

        [Fact]
        public async Task TestGetProductByIdAsync_Unauthorized()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns((string?)null);

            // Act
            var result = await productService.GetProductByIdAsync("1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized access", result.Message);
        }

        [Fact]
        public async Task TestCreateProductAsync_Unauthorized()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns((string?)null);
            var createProductDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "Description",
                Price = 10,
                Stock = 10,
                ImageUrl = "http://example.com/image.jpg"
            };

            // Act
            var result = await productService.CreateProductAsync(createProductDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized access", result.Message);
        }

        [Fact]
        public async Task TestUpdateProductAsync_Unauthorized()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns((string?)null);
            var updateProductDto = new CreateProductDto
            {
                Name = "Updated Product",
                Description = "Description",
                Price = 10,
                Stock = 10,
                ImageUrl = "http://example.com/image.jpg"
            };

            // Act
            var result = await productService.UpdateProductAsync("1", updateProductDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized access", result.Message);
        }

        [Fact]
        public async Task TestDeleteProductAsync_Unauthorized()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns((string?)null);

            // Act
            var result = await productService.DeleteProductAsync("1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized access", result.Message);
        }

        [Fact]
        public async Task TestGetProductsByPaginationAsync_Unauthorized()
        {
            // Arrange
            currentUserService.Setup(x => x.UserId).Returns((string?)null);
            var paginationRequest = new PaginationRequestDto { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await productService.GetProductsByPaginationAsync(paginationRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized access", result.Message);
        }
    }
}