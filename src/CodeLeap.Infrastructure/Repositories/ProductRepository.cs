using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;
using CodeLeap.Core.Entities;

namespace CodeLeap.Infrastructure.Repositories
{
    public class ProductRepository(PostgresSqlDbContext dbContext) : GenericRepository<ProductEntity>(dbContext), IProductRepository
    {
        public async Task<ProductEntity?> GetProductByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(
                x => EF.Functions.Collate(x.Name, "default") == EF.Functions.Collate(name, "default")
                && !x.IsDeleted
            );
        }

        public async Task<bool> IsExistingProductAsync(string name, string? excludeId = null)
        {
            return await _dbSet.AnyAsync(
                x =>
                    EF.Functions.Collate(x.Name, "default") == EF.Functions.Collate(name, "default")
                    && x.Id != excludeId
                    && !x.IsDeleted
            );
        }
    }
}
