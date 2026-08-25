using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Repositories
{
    /// <summary>
    /// Repositorio genérico, adaptado del original de GDIP para trabajar con el
    /// DbContext (TContext) provisto por la aplicación consumidora en lugar de
    /// asumir un DbContext específico.
    /// </summary>
    public class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext>? _contextFactory;
        private readonly TContext? _externalContext;

        public GenericRepository(IDbContextFactory<TContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public GenericRepository(TContext externalContext)
        {
            _externalContext = externalContext;
        }

        protected bool UseExternalContext => _externalContext != null;

        /// <summary>
        /// Ejecuta una función usando el DbContext adecuado (externo o creado desde la factory)
        /// </summary>
        protected async Task<TResult> UseContextAsync<TResult>(Func<TContext, Task<TResult>> func)
        {
            if (UseExternalContext)
            {
                return await func(_externalContext!);
            }

            if (_contextFactory == null)
            {
                throw new InvalidOperationException("No hay DbContext disponible.");
            }

            await using var context = _contextFactory.CreateDbContext();
            return await func(context);
        }

        /// <summary>
        /// Ejecuta una acción usando el DbContext adecuado
        /// </summary>
        protected async Task UseContextAsync(Func<TContext, Task> func)
        {
            if (UseExternalContext)
            {
                await func(_externalContext!);
                return;
            }

            if (_contextFactory == null)
            {
                throw new InvalidOperationException("No hay DbContext disponible.");
            }

            await using var context = _contextFactory.CreateDbContext();
            await func(context);
        }

        protected TContext UseContext(Action<TContext>? action = null)
        {
            if (UseExternalContext)
            {
                action?.Invoke(_externalContext!);
                return _externalContext!;
            }

            if (_contextFactory == null)
            {
                throw new InvalidOperationException("No hay DbContext disponible.");
            }

            var ctx = _contextFactory.CreateDbContext();
            action?.Invoke(ctx);
            return ctx;
        }


        // -------------------------------
        // Operaciones normales/UnitOfWork
        // -------------------------------
        public Task<TEntity?> GetByIdAsync(Guid id) =>
            UseContextAsync(ctx => ctx.Set<TEntity>().FindAsync(id).AsTask());


        public Task<TEntity?> GetByIdAsync(int id) =>
            UseContextAsync(ctx => ctx.Set<TEntity>().FindAsync(id).AsTask());

        public Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return UseContextAsync(async ctx =>
            {
                var list = await ctx.Set<TEntity>().AsNoTracking().ToListAsync();
                return list.AsEnumerable();
            });
        }

        public Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return UseContextAsync(async ctx =>
            {
                var list = await ctx.Set<TEntity>().Where(predicate).ToListAsync();
                return list.AsEnumerable();
            });
        }

        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) =>
            UseContextAsync(ctx => ctx.Set<TEntity>().FirstOrDefaultAsync(predicate));

        public Task<TEntity> AddAsync(TEntity entity) =>
            UseContextAsync(async ctx =>
            {
                await ctx.Set<TEntity>().AddAsync(entity);
                if (!UseExternalContext) await ctx.SaveChangesAsync();
                return entity;
            });

        public Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities) =>
            UseContextAsync(async ctx =>
            {
                await ctx.Set<TEntity>().AddRangeAsync(entities);
                if (!UseExternalContext) await ctx.SaveChangesAsync();
                return entities;
            });

        public Task UpdateAsync(TEntity entity) =>
            UseContextAsync(async ctx =>
            {
                var persistedEntity = await FindPersistedEntityAsync(ctx, entity);

                if (persistedEntity is null)
                {
                    ctx.Set<TEntity>().Update(entity);
                }
                else
                {
                    ctx.Entry(persistedEntity).CurrentValues.SetValues(entity);
                }

                if (!UseExternalContext) await ctx.SaveChangesAsync();
            });

        public Task<List<TEntity>> UpdateWhereAsync(Expression<Func<TEntity, bool>> predicate, Dictionary<Expression<Func<TEntity, object>>, object> updates)
        {
            return UseContextAsync(async ctx =>
            {
                var entities = await ctx.Set<TEntity>().Where(predicate).ToListAsync();

                foreach (var entity in entities)
                {
                    foreach (var update in updates)
                    {
                        var propertyName = GetPropertyName(update.Key);
                        var propertyInfo = typeof(TEntity).GetProperty(propertyName);
                        if (propertyInfo != null)
                            propertyInfo.SetValue(entity, update.Value);
                    }

                    ctx.Set<TEntity>().Update(entity);
                }

                if (!UseExternalContext)
                    await ctx.SaveChangesAsync();

                return entities;
            });
        }

        public Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities) =>
        UseContextAsync(async ctx =>
        {
            ctx.Set<TEntity>().UpdateRange(entities);

            if (!UseExternalContext)
                await ctx.SaveChangesAsync();

            return entities;
        });

        public Task RemoveAsync(TEntity entity) =>
            UseContextAsync(async ctx =>
            {
                var persistedEntity = await FindPersistedEntityAsync(ctx, entity);

                if (persistedEntity is null)
                {
                    ctx.Set<TEntity>().Remove(entity);
                }
                else
                {
                    ctx.Set<TEntity>().Remove(persistedEntity);
                }

                if (!UseExternalContext) await ctx.SaveChangesAsync();
            });

        public Task RemoveRangeAsync(IEnumerable<TEntity> entities) =>
            UseContextAsync(async ctx =>
            {
                ctx.Set<TEntity>().RemoveRange(entities);

                if (!UseExternalContext)
                    await ctx.SaveChangesAsync();
            });

        public Task<bool> ExistsAsync(Guid id) =>
            UseContextAsync(async ctx =>
            {
                var entity = await ctx.Set<TEntity>().FindAsync(id);
                return entity != null;
            });

        public Task<bool> ExistsAsync(int id) =>
            UseContextAsync(async ctx =>
            {
                var entity = await ctx.Set<TEntity>().FindAsync(id);
                return entity != null;
            });

        public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
            UseContextAsync(async ctx =>
                predicate == null
                    ? await ctx.Set<TEntity>().CountAsync()
                    : await ctx.Set<TEntity>().CountAsync(predicate));

        public IQueryable<TEntity> Query(params Expression<Func<TEntity, object>>[] includes)
        {
            var ctx = UseContext(); // obtiene el contexto (sin async)

            IQueryable<TEntity> query = ctx.Set<TEntity>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public IQueryable<TEntity> Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null)
        {
            var ctx = UseContext();

            IQueryable<TEntity> query = ctx.Set<TEntity>();

            if (include != null)
                query = include(query);

            return query;
        }

        private string GetPropertyName<TPropertySource>(Expression<Func<TPropertySource, object>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }

            if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberOperand)
            {
                return memberOperand.Member.Name;
            }

            throw new InvalidOperationException("No se pudo obtener el nombre de la propiedad de la expresión.");
        }

        private static async Task<TEntity?> FindPersistedEntityAsync(TContext ctx, TEntity entity)
        {
            var key = GetPrimaryKey(ctx);
            if (key is null)
            {
                return null;
            }

            var keyValues = new object?[key.Properties.Count];

            for (var index = 0; index < key.Properties.Count; index++)
            {
                var keyProperty = key.Properties[index];
                var keyValue = keyProperty.PropertyInfo?.GetValue(entity) ?? typeof(TEntity).GetProperty(keyProperty.Name)?.GetValue(entity);

                if (IsMissingKeyValue(keyValue))
                {
                    return null;
                }

                keyValues[index] = keyValue;
            }

            return await ctx.Set<TEntity>().FindAsync(keyValues);
        }

        private static IKey? GetPrimaryKey(TContext ctx)
        {
            return ctx.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();
        }

        private static bool IsMissingKeyValue(object? keyValue)
        {
            return keyValue switch
            {
                null => true,
                int intValue when intValue == 0 => true,
                long longValue when longValue == 0 => true,
                Guid guidValue when guidValue == Guid.Empty => true,
                string stringValue when string.IsNullOrWhiteSpace(stringValue) => true,
                _ => false
            };
        }

    }
}
