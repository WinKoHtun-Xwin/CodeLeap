using CodeLeap.Core.Entities;

namespace CodeLeap.Core.IRepositories
{
    public interface IProductRepository
    {
        Task<ProductEntity?> GetProductByNameAsync(string name);
        Task<int> GetTotalItemsAsync();
        Task<IEnumerable<ProductEntity>> GetProductsByPaginationAsync(int pageNumber, int pageSize, string search);
        Task<IEnumerable<ProductEntity>> GetAllProductAsync();

        Task<ProductEntity?> GetProductByIdAsync(string id);

        Task<ProductEntity?> CreateProductAsync(ProductEntity Product);

        Task<ProductEntity?> UpdateProductAsync(string ProductId, ProductEntity Product);

        Task<Boolean> DeleteProductAsync(string id);
    }
}
