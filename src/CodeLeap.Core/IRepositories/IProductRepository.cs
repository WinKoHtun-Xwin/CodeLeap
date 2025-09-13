using CodeLeap.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Core.IRepositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductEntity>> GetAllProductAsync();

        Task<ProductEntity?> GetProductByIdAsync(string id);

        Task<ProductEntity> CreateProductAsync(ProductEntity Product);

        Task<ProductEntity> UpdateProductAsync(string ProductId, ProductEntity Product);

        Task<Boolean> DeleteProductAsync(string id);
    }
}
