using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Common;
using System.Data;

namespace CodeLeap.Application.Services
{
    public class ProductService(
        IProductRepository productRepository,
        IKeycloakUserinfoService currentUserService,
        ILoggerService<ProductService> logger) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IKeycloakUserinfoService _currentUserService = currentUserService;
        private readonly ILoggerService<ProductService> _logger = logger;

        public async Task<BaseResponseModelPagination<IEnumerable<ProductDto>>> GetProductsByPaginationAsync(PaginationRequestDto paginationRequestDto)
        {
            try
            {
                var userId = _currentUserService.UserId!; // Guaranteed by UserAuthorizationFilter

                _logger.Info("Getting products by pagination By User : {userId}", userId);
                var products = await _productRepository.GetProductsByPaginationAsync(paginationRequestDto.PageNumber, paginationRequestDto.PageSize, paginationRequestDto.Search ?? string.Empty);
                var productDtos = products.Select(MapToProductDto).Where(dto => dto != null).Cast<ProductDto>();
                var totalItems = await _productRepository.GetTotalItemsAsync();
                var totalPages = Math.Ceiling((double)totalItems / paginationRequestDto.PageSize);
                var response = new BaseResponseModelPagination<IEnumerable<ProductDto>>
                {
                    Success = true,
                    StatusCode = HttpStatusCodes.Ok,
                    Data = productDtos,
                    Message = ResponseMessage.ProductMessage.GetAllSuccess,
                    Pagination = new PaginationDto
                    {
                        PageNumber = paginationRequestDto.PageNumber,
                        PageSize = paginationRequestDto.PageSize,
                        TotalItems = totalItems,
                        TotalPages = (int)totalPages
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting products by pagination", ex);
                return new BaseResponseModelPagination<IEnumerable<ProductDto>>
                {
                    Success = false,
                    StatusCode = HttpStatusCodes.InternalServerError,
                    Message = ResponseMessage.ProductMessage.CreatedFail,
                    Error = ex.Message
                };
            }
        }

        public async Task<BaseResponseModel<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                var userId = _currentUserService.UserId!; // Guaranteed by UserAuthorizationFilter

                _logger.Info("Getting all products By User : {userId}", userId);
                var products = await _productRepository.GetAllProductAsync();
                var productDtos = products.Select(MapToProductDto).Where(dto => dto != null).Cast<ProductDto>();

                return BaseResponseModel<IEnumerable<ProductDto>>.SuccessResponse(
                    productDtos,
                    ResponseMessage.ProductMessage.GetAllSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting all products", ex);
                return BaseResponseModel<IEnumerable<ProductDto>>.ServerErrorResponse(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> GetProductByIdAsync(string id)
        {
            try
            {
                var userId = _currentUserService.UserId!; // Guaranteed by UserAuthorizationFilter

                _logger.Info("Getting product by id : {id} By User : {userId}", id, userId);
                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                {
                    _logger.Error("Product not found : {id} By User : {userId}", id, userId);
                    return BaseResponseModel<ProductDto>.NotFoundResponse(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(product)!,
                    ResponseMessage.ProductMessage.GetSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting product by id", ex);
                return BaseResponseModel<ProductDto>.ServerErrorResponse(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                var userId = _currentUserService.UserId!; // Guaranteed by UserAuthorizationFilter

                _logger.Info("Creating product By User : {userId}", userId.ToString());

                if (await _productRepository.IsExistingProductAsync(createProductDto.Name))
                {
                    _logger.Error("Product already exists : {name} By User : {userId}", createProductDto.Name, userId);
                    return BaseResponseModel<ProductDto>.ConflictResponse(
                        ResponseMessage.ProductMessage.AlreadyExists
                    );
                }
                ProductEntity newProduct = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    ImageUrl = createProductDto.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                };

                var createdProduct = await _productRepository.CreateProductAsync(newProduct);

                if (createdProduct == null)
                {
                    _logger.Error("Product creation failed By User : {userId}", userId);
                    return BaseResponseModel<ProductDto>.ServerErrorResponse(
                        ResponseMessage.ProductMessage.CreatedFail
                    );
                }

                _logger.Info("Product created successfully By User : {userId}", userId);

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(createdProduct)!,
                    HttpStatusCodes.Created,
                    ResponseMessage.ProductMessage.CreatedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating product", ex);
                return BaseResponseModel<ProductDto>.ServerErrorResponse(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> UpdateProductAsync(string productId, CreateProductDto dto)
        {
            var userId = _currentUserService.UserId!;
            var product = await _productRepository.GetProductByIdAsync(productId);

            if (product == null)
                return BaseResponseModel<ProductDto>.NotFoundResponse(
                    ResponseMessage.ProductMessage.NotFound
                );

            // Check for conflict
            var conflict = await _productRepository.IsExistingProductAsync(dto.Name, productId);
            if (conflict)
                return BaseResponseModel<ProductDto>.ConflictResponse(
                    ResponseMessage.ProductMessage.AlreadyExists
                );

            // Only update changed fields (EF tracks automatically)
            if (product.Name != dto.Name) product.Name = dto.Name;
            if (product.Description != dto.Description) product.Description = dto.Description;
            if (product.Price != dto.Price) product.Price = dto.Price;
            if (product.Stock != dto.Stock) product.Stock = dto.Stock;
            if (product.ImageUrl != dto.ImageUrl) product.ImageUrl = dto.ImageUrl;

            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateProductAsync(product, product);

            return BaseResponseModel<ProductDto>.SuccessResponse(
                MapToProductDto(product)!,
                ResponseMessage.ProductMessage.UpdatedSuccess
            );
        }


        public async Task<BaseResponseModel<bool>> DeleteProductAsync(string id)
        {
            try
            {
                var userId = _currentUserService.UserId!; // Guaranteed by UserAuthorizationFilter

                _logger.Info("Deleting product By User : {userId}", userId);
                var existingProduct = await _productRepository.GetProductByIdAsync(id);

                if (existingProduct == null)
                {
                    _logger.Error("Product not found : {id} By User : {userId}", id, userId);
                    return BaseResponseModel<bool>.NotFoundResponse(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                await _productRepository.DeleteProductAsync(id);

                _logger.Info("Product deleted successfully By User : {userId}", userId);

                return BaseResponseModel<bool>.SuccessResponse(
                    true,
                    ResponseMessage.ProductMessage.DeletedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting product", ex);
                return BaseResponseModel<bool>.ServerErrorResponse(
                    ResponseMessage.ProductMessage.DeletedFail,
                    ex.Message
                );
            }
        }

        private static ProductDto? MapToProductDto(ProductEntity? productEntity)
        {
            if (productEntity == null)
                return null;

            return new ProductDto
            {
                Id = productEntity.Id,
                Name = productEntity.Name,
                Description = productEntity.Description,
                Price = productEntity.Price,
                Stock = productEntity.Stock,
                ImageUrl = productEntity.ImageUrl,
                CreatedBy = productEntity.CreatedBy,
            };
        }
    }
}
