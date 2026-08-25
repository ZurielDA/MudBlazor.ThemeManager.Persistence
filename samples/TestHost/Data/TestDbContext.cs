using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.TestHost.Data
{
    /// <summary>
    /// DbContext mínimo sólo para poder probar la librería de forma aislada.
    /// Cualquier app consumidora real usaría su propio DbContext (como
    /// ApplicationDbContext en GDIP) exponiendo estos mismos DbSet.
    /// </summary>
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<ThemeCatalog> ThemeCatalogs => Set<ThemeCatalog>();

        public DbSet<ThemeFavicon> ThemeFavicons => Set<ThemeFavicon>();

        public DbSet<ThemeLogo> ThemeLogos => Set<ThemeLogo>();

        public DbSet<ThemePresent> ThemesPresent => Set<ThemePresent>();

        public DbSet<ThemeTerm> ThemeTerms => Set<ThemeTerm>();
    }
}
