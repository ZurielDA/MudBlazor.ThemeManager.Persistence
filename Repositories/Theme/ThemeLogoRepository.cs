using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Repositories.Theme
{
    public class ThemeLogoRepository<TContext> : GenericRepository<ThemeLogo, TContext>, IThemeLogoRepository
        where TContext : DbContext
    {
        public ThemeLogoRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public ThemeLogoRepository(TContext externalContext)
            : base(externalContext)
        {
        }
    }
}
