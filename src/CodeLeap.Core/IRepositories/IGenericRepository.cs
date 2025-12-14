using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CodeLeap.Core.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(string id);

        Task<int> GetTotalCountAsync(Expression<Func<T, bool>>? filter = null, string? searchTerm = null);

        Task<IEnumerable<T>> GetPagedReponseAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            string? searchTerm = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
        );
    }
}
