using CodeLeap.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeLeap.Core.IRepositories
{
    public interface IProductRepository
    {
        Task<ProductEntity?> GetProductByIdAsync(string id);
        Task<ProductEntity?> GetProductByNameAsync(string name);

        // Name conflict check with optional excludeId for update
        Task<bool> IsExistingProductAsync(string name, string? excludeId = null);

        Task<int> GetTotalItemsAsync();
        Task<IEnumerable<ProductEntity>> GetProductsByPaginationAsync(int pageNumber, int pageSize, string search);
        Task<IEnumerable<ProductEntity>> GetAllProductAsync();

        Task<ProductEntity> CreateProductAsync(ProductEntity product);

        // EF Core change tracking update
        Task<ProductEntity?> UpdateProductAsync(ProductEntity existingProduct, ProductEntity updateModel);

        Task<bool> DeleteProductAsync(string id);
    }
}
