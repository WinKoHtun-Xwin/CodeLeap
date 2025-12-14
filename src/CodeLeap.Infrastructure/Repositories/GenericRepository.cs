using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CodeLeap.Core.Attributes;
using CodeLeap.Core.Entities;
using CodeLeap.Core.IRepositories;
using CodeLeap.Infrastructure.PostgresSQL;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly PostgresSqlDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(PostgresSqlDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalCountAsync(Expression<Func<T, bool>>? filter = null, string? searchTerm = null)
        {
            var query = _dbSet.Where(x => !x.IsDeleted);
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchExpression = BuildSearchExpression(searchTerm);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<T>> GetPagedReponseAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            string? searchTerm = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            var query = _dbSet.Where(x => !x.IsDeleted);

            // Apply external filters
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply dynamic search based on [Searchable] attribute
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchExpression = BuildSearchExpression(searchTerm);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            // Apply Ordering
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else
            {
                // Default ordering by CreatedAt if not specified
                query = query.OrderByDescending(x => x.CreatedAt);
            }

            // Apply Pagination
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        private Expression<Func<T, bool>>? BuildSearchExpression(string searchTerm)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<SearchableAttribute>() != null && p.PropertyType == typeof(string));

            if (!properties.Any())
                return null;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? orExpression = null;

            // We want case-insensitive search: x.Prop.ToLower().Contains(searchTerm.ToLower())
            // Or simpler: EF.Functions.Like(x.Prop, $"%{searchTerm}%") if we want to rely on EF

            // For Postgres, ILIKE is best but requires EF Core specific extensions directly or standardizing on ToLower()
            var searchTermLower = searchTerm.ToLower();
            var searchConstant = Expression.Constant(searchTermLower);
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            if (toLowerMethod == null || containsMethod == null) return null;

            foreach (var prop in properties)
            {
                var propExpression = Expression.Property(parameter, prop);

                // x.Prop (check for null if needed, but assuming required usually, let's add null check safety essentially: x.Prop != null && x.Prop.ToLower().Contains(...))

                var propToLower = Expression.Call(propExpression, toLowerMethod);
                var containsExpression = Expression.Call(propToLower, containsMethod, searchConstant);

                // If property is nullable, we should check for not null
                // but if we are confident they are strings...
                // To be safe: (x.Prop != null && x.Prop.ToLower().Contains(...))

                Expression condition = containsExpression;

                // Combine with OR
                orExpression = orExpression == null ? condition : Expression.OrElse(orExpression, condition);
            }

            return orExpression == null ? null : Expression.Lambda<Func<T, bool>>(orExpression, parameter);
        }
    }
}
