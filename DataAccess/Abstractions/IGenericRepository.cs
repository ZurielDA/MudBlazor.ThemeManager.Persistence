using System.Linq.Expressions;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Elimina la entidad dada. La entidad no necesita estar rastreada por
        /// ningun DbContext: basta con que tenga la clave primaria establecida.
        /// </summary>
        Task RemoveAsync(TEntity entity);

        IQueryable<TEntity> Query(params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Variante de <see cref="Query(Expression{Func{TEntity, object}}[])"/> que
        /// permite componer la consulta libremente (por ejemplo, para usar
        /// .ThenInclude en grafos anidados, algo que la sobrecarga de
        /// Expression[] no soporta). El shaper recibe el IQueryable base
        /// (ctx.Set&lt;TEntity&gt;()) y debe devolver el IQueryable final.
        /// </summary>
        IQueryable<TEntity> Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> shape);
    }
}
