using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.DataAccess
{
    public class ThemePresentRepository<TContext> : GenericRepository<ThemePresent, TContext>, IThemePresentRepository
        where TContext : DbContext
    {
        public ThemePresentRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public ThemePresentRepository(TContext externalContext)
            : base(externalContext)
        {
        }       
    }
}
