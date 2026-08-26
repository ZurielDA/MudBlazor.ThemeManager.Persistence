using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.DataAccess
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
