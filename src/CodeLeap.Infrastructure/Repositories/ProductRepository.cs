using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;
using CodeLeap.Core.Entities;

namespace CodeLeap.Infrastructure.Repositories
{
    public class ProductRepository(PostgresSQLDbContext dbContext) : IProductRepository
    {
        public async Task<IEnumerable<ProductEntity>> GetAllProductAsync()
        {
            return await dbContext.Set<ProductEntity>().ToListAsync();
        }

        public async Task<ProductEntity?> GetProductByIdAsync(string id)
        {
            return await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ProductEntity> CreateProductAsync(ProductEntity product)
        {
            product.Id = Guid.NewGuid().ToString();
            var entityEntry = await dbContext.Set<ProductEntity>().AddAsync(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<ProductEntity> UpdateProductAsync(string productId, ProductEntity product)
        {
            var existProduct = await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == productId);
            if (existProduct == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            existProduct.Name = product.Name;
            existProduct.Price = product.Price;
            existProduct.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            return existProduct;
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var existProduct = await dbContext.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == id);
            if (existProduct == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            dbContext.Set<ProductEntity>().Remove(existProduct);
            return await dbContext.SaveChangesAsync() > 0;
        }
    }
}
