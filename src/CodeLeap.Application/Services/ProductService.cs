using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Interfaces;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Application.Common;
using System.Data;

namespace CodeLeap.Application.Services
{
    public class ProductService : IProductService
    {
        private const string UnknownUser = "Unknown";
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILoggerService<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ICurrentUserService currentUserService, ILoggerService<ProductService> logger)
        {

            _productRepository = productRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }
        public async Task<BaseResponseModelPagination<IEnumerable<ProductDto>>> GetProductsByPaginationAsync(PaginationRequestDto paginationRequestDto)
        {
            try
            {
                _logger.Info("Getting products by pagination By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                var products = await _productRepository.GetProductsByPaginationAsync(paginationRequestDto.PageNumber, paginationRequestDto.PageSize, paginationRequestDto.Search ?? string.Empty);
                var productDtos = products.Select(MapToProductDto).Where(dto => dto != null).Cast<ProductDto>();
                var totalItems = await _productRepository.GetTotalItemsAsync();
                var totalPages = Math.Ceiling((double)totalItems / paginationRequestDto.PageSize);
                var response = new BaseResponseModelPagination<IEnumerable<ProductDto>>
                {
                    Success = true,
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
                _logger.Error("Error getting products by pagination By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser, ex);
                return new BaseResponseModelPagination<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = ResponseMessage.ProductMessage.CreatedFail,
                    Error = ex.Message
                };
            }
        }

        public async Task<BaseResponseModel<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                _logger.Info("Getting all products By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                var products = await _productRepository.GetAllProductAsync();
                var productDtos = products.Select(MapToProductDto).Where(dto => dto != null).Cast<ProductDto>();

                return BaseResponseModel<IEnumerable<ProductDto>>.SuccessResponse(
                    productDtos,
                    ResponseMessage.ProductMessage.GetAllSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting all products By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser, ex);
                return BaseResponseModel<IEnumerable<ProductDto>>.Failure(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> GetProductByIdAsync(string id)
        {
            try
            {
                _logger.Info("Getting product by id : {id} By User : {userId}", id, _currentUserService.GetUserId() ?? UnknownUser);
                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                {
                    _logger.Error("Product not found : {id} By User : {userId}", id, _currentUserService.GetUserId() ?? UnknownUser);
                    return BaseResponseModel<ProductDto>.Failure(
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
                _logger.Error("Error getting product by id By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser, ex);
                return BaseResponseModel<ProductDto>.Failure(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                _logger.Info("Creating product By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                var existingProduct = await _productRepository.GetProductByNameAsync(createProductDto.Name);
                if (existingProduct != null)
                {
                    _logger.Error("Product already exists : {name} By User : {userId}", createProductDto.Name, _currentUserService.GetUserId() ?? UnknownUser);
                    return BaseResponseModel<ProductDto>.Failure(
                        ResponseMessage.ProductMessage.AlreadyExists
                    );
                }
                ProductEntity newProduct= new ProductEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    ImageUrl = createProductDto.ImageUrl,
                    CreatedBy = _currentUserService.GetUserId() ?? UnknownUser,
                };

                var createdProduct = await _productRepository.CreateProductAsync(newProduct);

                if (createdProduct == null)
                {
                    _logger.Error("Product creation failed By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                    return BaseResponseModel<ProductDto>.Failure(
                        ResponseMessage.ProductMessage.CreatedFail
                    );
                }

                _logger.Info("Product created successfully By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(createdProduct)!,
                    ResponseMessage.ProductMessage.CreatedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating product By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser, ex);
                return BaseResponseModel<ProductDto>.Failure(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> UpdateProductAsync(string ProductId, CreateProductDto updateProductDto)
        {
            try
            {
                _logger.Info("Updating product By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                var existingProduct = await _productRepository.GetProductByIdAsync(ProductId);

                if (existingProduct == null)
                {
                    _logger.Error("Product not found : {ProductId} By User : {userId}", ProductId, _currentUserService.GetUserId() ?? UnknownUser);
                    return BaseResponseModel<ProductDto>.Failure(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                existingProduct.Name = updateProductDto.Name;
                existingProduct.Description = updateProductDto.Description;
                existingProduct.Price = updateProductDto.Price;
                existingProduct.Stock = updateProductDto.Stock;
                existingProduct.ImageUrl = updateProductDto.ImageUrl;
                existingProduct.UpdatedBy = _currentUserService.GetUserId() ?? UnknownUser;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                var updatedProduct = await _productRepository.UpdateProductAsync(ProductId, existingProduct);

                if (updatedProduct == null)
                {
                    _logger.Error("Product update failed By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                    return BaseResponseModel<ProductDto>.Failure(
                        ResponseMessage.ProductMessage.UpdatedFail
                    );
                }

                _logger.Info("Product updated successfully By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(updatedProduct)!,
                    ResponseMessage.ProductMessage.UpdatedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating product By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser, ex);
                return BaseResponseModel<ProductDto>.Failure(
                    ResponseMessage.ProductMessage.UpdatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<bool>> DeleteProductAsync(string id)
        {
            try
            {
                _logger.Info("Deleting product By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);
                var existingProduct = await _productRepository.GetProductByIdAsync(id);

                if (existingProduct == null)
                {
                    _logger.Error("Product not found : {id} By User : {userId}", id, _currentUserService.GetUserId() ?? UnknownUser);
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                await _productRepository.DeleteProductAsync(id);

                _logger.Info("Product deleted successfully By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser);

                return BaseResponseModel<bool>.SuccessResponse(
                    true,
                    ResponseMessage.ProductMessage.DeletedSuccess
                );
            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting product By User : {userId}", _currentUserService.GetUserId() ?? UnknownUser, ex);
                return BaseResponseModel<bool>.Failure(
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
