using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Repositories.Theme
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
