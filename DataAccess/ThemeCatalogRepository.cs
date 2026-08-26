using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.DataAccess
{
    public class ThemeCatalogRepository<TContext> : GenericRepository<ThemeCatalog, TContext>, IThemeCatalogRepository
        where TContext : DbContext
    {
        public ThemeCatalogRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public ThemeCatalogRepository(TContext externalContext)
            : base(externalContext)
        {
        }
    }
}
