using System.Linq.Expressions;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories
{
        public interface IGenericRepository<TEntity> where TEntity : class
        {
            Task<TEntity?> GetByIdAsync(Guid id);

            Task<TEntity?> GetByIdAsync(int id);

            Task<IEnumerable<TEntity>> GetAllAsync();

            Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

            Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

            Task<TEntity> AddAsync(TEntity entity);

            Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

            Task UpdateAsync(TEntity entity);

            Task<List<TEntity>> UpdateWhereAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<Expression<Func<TEntity, object>>, object> updates);

            Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);

            Task RemoveAsync(TEntity entity);

            Task RemoveRangeAsync(IEnumerable<TEntity> entities);

            Task<bool> ExistsAsync(Guid id);

            Task<bool> ExistsAsync(int id);

            Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

            IQueryable<TEntity> Query(params Expression<Func<TEntity, object>>[] includes);

            IQueryable<TEntity> Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null);

    }
}
