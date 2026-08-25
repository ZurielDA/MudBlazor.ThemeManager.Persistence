using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Repositories.Theme
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
