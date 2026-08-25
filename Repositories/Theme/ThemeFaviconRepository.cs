using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Repositories.Theme
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
