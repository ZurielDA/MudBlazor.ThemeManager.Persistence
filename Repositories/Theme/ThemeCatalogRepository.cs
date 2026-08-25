using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Repositories.Theme
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
