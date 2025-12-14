using CodeLeap.Core.Entities;
using System.Threading.Tasks;

namespace CodeLeap.Core.IRepositories
{
    public interface IProductRepository : IGenericRepository<ProductEntity>
    {
        // Name conflict check with optional excludeId for update
        Task<bool> IsExistingProductAsync(string name, string? excludeId = null);

        // Specific methods can be added here if they are not covered by Generic Repository
        Task<ProductEntity?> GetProductByNameAsync(string name);
    }
}
