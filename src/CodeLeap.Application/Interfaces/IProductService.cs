using CodeLeap.Application.Common;
using CodeLeap.Application.DTOs.Product;

namespace CodeLeap.Application.Interfaces
{
    public interface IProductService
    {
        Task<BaseResponseModelPagination<IEnumerable<ProductDto>>> GetProductsByPaginationAsync(PaginationRequestDto paginationRequestDto);
        Task<BaseResponseModel<IEnumerable<ProductDto>>> GetAllProductsAsync();
        Task<BaseResponseModel<ProductDto>> GetProductByIdAsync(string id);
        Task<BaseResponseModel<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
        Task<BaseResponseModel<ProductDto>> UpdateProductAsync(string ProductId, CreateProductDto updateProductDto);
        Task<BaseResponseModel<bool>> DeleteProductAsync(string id);
    }
}
