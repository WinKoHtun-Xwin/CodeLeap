using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.DTOs.User;

namespace CodeLeap.Application.Interfaces
{
    public interface IProductService
    {
        Task<BaseResponseModel<IEnumerable<ProductDto>>> GetAllProductsAsync();
        Task<BaseResponseModel<ProductDto>> GetProductByIdAsync(string id);
        Task<BaseResponseModel<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
        Task<BaseResponseModel<ProductDto>> UpdateProductAsync(string userId, CreateProductDto updateProductDto);
        Task<BaseResponseModel<bool>> DeleteProductAsync(string id);
    }
}
