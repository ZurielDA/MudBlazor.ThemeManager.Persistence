using System.Linq.Expressions;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Repositories
{
        public interface IGenericRepository<TEntity> where TEntity : class
        {
            Task<IEnumerable<TEntity>> GetAllAsync();

            Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

            Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

            Task<TEntity> AddAsync(TEntity entity);

            Task UpdateAsync(TEntity entity);

            Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);

            IQueryable<TEntity> Query(params Expression<Func<TEntity, object>>[] includes);

    }
}
