using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace SAMACDX.ThemeManager.Persistence.DataAccess
{
    /// <summary>
    /// Repositorio generico, adaptado del original de GDIP para trabajar con el
    /// DbContext (TContext) provisto por la aplicacion consumidora en lugar de
    /// asumir un DbContext especifico.
    /// </summary>
    public class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity>, IDisposable
        where TEntity : class
        where TContext : DbContext
    {
        // Cuantos DbContext creados por UseContext() (la variante SINCRONA,
        // usada por Query()) puede acumular sin liberar una misma instancia
        // de repositorio antes de empezar a liberar los mas antiguos. Ver
        // comentario en UseContext(). No es una configuracion publica: es un
        // limite de seguridad interno, no algo que un consumidor deba ajustar.
        private const int MaxTrackedContexts = 8;

        private readonly IDbContextFactory<TContext>? _contextFactory;
        private readonly TContext? _externalContext;

        // Contextos creados internamente por UseContext() que quedan "vivos"
        // mientras el caller enumera el IQueryable devuelto por Query(). A
        // diferencia de UseContextAsync (que ya hace "await using" y libera
        // el contexto de inmediato), UseContext no puede liberarlo de
        // inmediato porque el IQueryable todavia no fue ejecutado. Se
        // rastrean aqui y se liberan cuando el propio repositorio se libera
        // (el contenedor de DI llama a Dispose() al final del scope). En un
        // host tipico (Razor Pages/MVC/API) un scope dura un solo request,
        // asi que esto ocurre casi de inmediato. En Blazor Server, en
        // cambio, "Scoped" dura todo el circuito del usuario (potencialmente
        // horas) -- por eso, ademas, se acota cuantos contextos sin liberar
        // puede acumular esta lista (ver MaxTrackedContexts): superado el
        // umbral, se liberan los mas antiguos, que en todos los usos
        // actuales de Query() dentro de esta libreria ya fueron abandonados
        // por su caller original (que termina la consulta en la misma
        // sentencia que la inicia).
        private readonly List<TContext> _trackedContexts = new();
        private bool _disposed;

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
        /// Ejecuta una funcion usando el DbContext adecuado (externo o creado desde la factory)
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
        /// Ejecuta una accion usando el DbContext adecuado
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
            _trackedContexts.Add(ctx);

            while (_trackedContexts.Count > MaxTrackedContexts)
            {
                var oldest = _trackedContexts[0];
                _trackedContexts.RemoveAt(0);
                oldest.Dispose();
            }

            action?.Invoke(ctx);
            return ctx;
        }


        // -------------------------------
        // Operaciones normales/UnitOfWork
        // -------------------------------
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
                ctx.Set<TEntity>().Remove(entity);

                if (!UseExternalContext)
                    await ctx.SaveChangesAsync();
            });

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

        public IQueryable<TEntity> Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> shape)
        {
            var ctx = UseContext();

            return shape(ctx.Set<TEntity>());
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var ctx in _trackedContexts)
            {
                ctx.Dispose();
            }

            _trackedContexts.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
