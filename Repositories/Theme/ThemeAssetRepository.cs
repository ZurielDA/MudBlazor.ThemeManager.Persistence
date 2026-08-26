using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Repositories.Theme
{
    public class ThemeAssetRepository<TContext> : GenericRepository<ThemeAsset, TContext>, IThemeAssetRepository
        where TContext : DbContext
    {
        public ThemeAssetRepository(IDbContextFactory<TContext> contextFactory)
            : base(contextFactory)
        {
        }

        public ThemeAssetRepository(TContext externalContext)
            : base(externalContext)
        {
        }
    }
}
