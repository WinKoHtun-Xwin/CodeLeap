using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;
using CodeLeap.Core.Entities;

namespace CodeLeap.Infrastructure.Repositories
{
    public class ProductRepository(PostgresSqlDbContext dbContext) : IProductRepository
    {
        private DbSet<ProductEntity> Table => dbContext.Set<ProductEntity>();

        // ------------------------ GET METHODS ------------------------

        public async Task<ProductEntity?> GetProductByIdAsync(string id)
        {
            return await Table.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<ProductEntity?> GetProductByNameAsync(string name)
        {
            return await Table.FirstOrDefaultAsync(
                x => EF.Functions.Collate(x.Name, "default") == EF.Functions.Collate(name, "default")
                && !x.IsDeleted
            );
        }

        public async Task<bool> IsExistingProductAsync(string name, string? excludeId = null)
        {
            return await Table.AnyAsync(
                x =>
                    EF.Functions.Collate(x.Name, "default") == EF.Functions.Collate(name, "default")
                    && x.Id != excludeId
                    && !x.IsDeleted
            );
        }

        public async Task<int> GetTotalItemsAsync()
        {
            return await Table.CountAsync(x => !x.IsDeleted);
        }

        public async Task<IEnumerable<ProductEntity>> GetProductsByPaginationAsync(
            int pageNumber, int pageSize, string search)
        {
            var baseQuery = Table.Where(x => !x.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                baseQuery = baseQuery.Where(x => EF.Functions.Like(x.Name, $"%{search}%"));
            }

            return await baseQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductEntity>> GetAllProductAsync()
        {
            return await Table.Where(x => !x.IsDeleted).ToListAsync();
        }

        // ------------------------ CREATE ------------------------

        public async Task<ProductEntity> CreateProductAsync(ProductEntity product)
        {
            product.Id = Guid.NewGuid().ToString();
            await Table.AddAsync(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        // ------------------------ UPDATE (EF Core Change Tracking) ------------------------

        public async Task<ProductEntity?> UpdateProductAsync(ProductEntity existingProduct, ProductEntity updateModel)
        {
            // Only update changed fields
            if (existingProduct.Name != updateModel.Name)
                existingProduct.Name = updateModel.Name;

            if (existingProduct.Description != updateModel.Description)
                existingProduct.Description = updateModel.Description;

            if (existingProduct.Price != updateModel.Price)
                existingProduct.Price = updateModel.Price;

            if (existingProduct.Stock != updateModel.Stock)
                existingProduct.Stock = updateModel.Stock;

            if (existingProduct.ImageUrl != updateModel.ImageUrl)
                existingProduct.ImageUrl = updateModel.ImageUrl;

            existingProduct.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return existingProduct;
        }

        // ------------------------ DELETE ------------------------

        public async Task<bool> DeleteProductAsync(string id)
        {
            var existProduct = await GetProductByIdAsync(id)
                ?? throw new KeyNotFoundException("Product not found");

            existProduct.IsDeleted = true;
            existProduct.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            return true;
        }

    }
}
