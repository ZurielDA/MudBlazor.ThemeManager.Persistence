using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Theme;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Repositories.Theme
{
    public class ThemeTermRepository<TContext> : GenericRepository<ThemeTerm, TContext>, IThemeTermRepository
        where TContext : DbContext
    {
        public ThemeTermRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public ThemeTermRepository(TContext externalContext)
            : base(externalContext)
        {
        }
    }
}
