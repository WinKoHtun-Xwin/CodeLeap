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
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<BaseResponseModel<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllProductAsync();
                var productDtos = products.Select(MapToProductDto);

                return BaseResponseModel<IEnumerable<ProductDto>>.SuccessResponse(
                    productDtos,
                    ResponseMessage.ProductMessage.GetAllSuccess
                );
            }
            catch (Exception ex)
            {
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
                var product = await _productRepository.GetProductByIdAsync(id);

                if (product == null)
                {
                    return BaseResponseModel<ProductDto>.Failure(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(product),
                    ResponseMessage.ProductMessage.GetSuccess
                );
            }
            catch (Exception ex)
            {
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
                ProductEntity newProduct= new ProductEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    ImageUrl = createProductDto.ImageUrl,
                    CreatedBy = createProductDto.CreatedBy,
                };

                var createdProduct = await _productRepository.CreateProductAsync(newProduct);

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(createdProduct),
                    ResponseMessage.ProductMessage.CreatedSuccess
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<ProductDto>.Failure(
                    ResponseMessage.ProductMessage.CreatedFail,
                    ex.Message
                );
            }
        }

        public async Task<BaseResponseModel<ProductDto>> UpdateProductAsync(string productId, CreateProductDto updateProductDto)
        {
            try
            {
                var existingProduct = await _productRepository.GetProductByIdAsync(productId);

                if (existingProduct == null)
                {
                    return BaseResponseModel<ProductDto>.Failure(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                existingProduct.Name = updateProductDto.Name;
                existingProduct.Description = updateProductDto.Description;
                existingProduct.Price = updateProductDto.Price;
                existingProduct.Stock = updateProductDto.Stock;
                existingProduct.ImageUrl = updateProductDto.ImageUrl;

                var updatedProduct = await _productRepository.UpdateProductAsync(productId, existingProduct);

                return BaseResponseModel<ProductDto>.SuccessResponse(
                    MapToProductDto(updatedProduct),
                    ResponseMessage.ProductMessage.UpdatedSuccess
                );
            }
            catch (Exception ex)
            {
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
                var existingProduct = await _productRepository.GetProductByIdAsync(id);

                if (existingProduct == null)
                {
                    return BaseResponseModel<bool>.Failure(
                        ResponseMessage.ProductMessage.NotFound
                    );
                }

                await _productRepository.DeleteProductAsync(id);

                return BaseResponseModel<bool>.SuccessResponse(
                    true,
                    ResponseMessage.ProductMessage.DeletedSuccess
                );
            }
            catch (Exception ex)
            {
                return BaseResponseModel<bool>.Failure(
                    ResponseMessage.ProductMessage.DeletedFail,
                    ex.Message
                );
            }
        }

        private static ProductDto MapToProductDto(ProductEntity productEntity)
        {
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
