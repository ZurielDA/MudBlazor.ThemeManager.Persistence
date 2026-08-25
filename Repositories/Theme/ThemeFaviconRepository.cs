using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Repositories.Theme
{
    public class ThemeFaviconRepository<TContext> : GenericRepository<ThemeFavicon, TContext>, IThemeFaviconRepository
        where TContext : DbContext
    {
        public ThemeFaviconRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public ThemeFaviconRepository(TContext externalContext)
            : base(externalContext)
        {
        }
    }
}
