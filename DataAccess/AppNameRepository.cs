using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.DataAccess
{
    public class AppNameRepository<TContext> : GenericRepository<AppName, TContext>, IAppNameRepository
        where TContext : DbContext
    {
        public AppNameRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public AppNameRepository(TContext externalContext)
            : base(externalContext)
        {
        }
    }
}
