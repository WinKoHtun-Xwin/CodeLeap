using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;
using CodeLeap.Core.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace CodeLeap.Infrastructure.Repositories
{
    public class ProductRepository(PostgresSqlDbContext dbContext) : IProductRepository
    {

        public async Task<ProductEntity?> GetProductByNameAsync(string name)
        {
            return await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "default") == EF.Functions.Collate(name, "default") && !x.IsDeleted);
        }

        public async Task<bool> IsExistingProductAsync(string name)
        {
            return await dbContext.Set<ProductEntity>().AnyAsync(x => EF.Functions.Collate(x.Name, "default") == EF.Functions.Collate(name, "default") && !x.IsDeleted);
        }

        public async Task<int> GetTotalItemsAsync()
        {
            return await dbContext.Set<ProductEntity>().Where(x => !x.IsDeleted).CountAsync();
        }

        public async Task<IEnumerable<ProductEntity>> GetProductsByPaginationAsync(int pageNumber, int pageSize, string search)
        {
            var baseQuery = dbContext.Set<ProductEntity>().AsQueryable();
            if (!string.IsNullOrEmpty(search.Trim()))
            {
                baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Name, $"%{search.Trim()}%"));
            }
            return await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductEntity>> GetAllProductAsync()
        {
            return await dbContext.Set<ProductEntity>().Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<ProductEntity?> GetProductByIdAsync(string id)
        {
            return await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<ProductEntity> CreateProductAsync(ProductEntity Product)
        {
            Product.Id = Guid.NewGuid().ToString();
            await dbContext.Set<ProductEntity>().AddAsync(Product);
            await dbContext.SaveChangesAsync();
            return Product;
        }

        public async Task<ProductEntity> UpdateProductAsync(string ProductId, ProductEntity Product)
        {
            var existProduct = await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == ProductId && !x.IsDeleted) ?? throw new KeyNotFoundException("Product not found");

            existProduct.Name = Product.Name;
            existProduct.Price = Product.Price;
            existProduct.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return existProduct;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var existProduct = await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted) ?? throw new KeyNotFoundException("Product not found");

            existProduct.IsDeleted = true;
            existProduct.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return true;
        }
    }
}
