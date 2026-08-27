using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using SAMACDX.ThemeManager.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.TestHost.Data
{
    /// <summary>
    /// DbContext minimo solo para poder probar la libreria de forma aislada.
    /// Cualquier app consumidora real usaria su propio DbContext (como
    /// ApplicationDbContext en GDIP) exponiendo estos mismos DbSet.
    /// </summary>
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<ThemeAsset> ThemeAssets => Set<ThemeAsset>();

        public DbSet<ThemePresent> ThemesPresent => Set<ThemePresent>();

        public DbSet<ThemeTerm> ThemeTerms => Set<ThemeTerm>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nuevo en esta etapa: aplica las IEntityTypeConfiguration<T> que
            // ahora trae la libreria (DataAccess/Configurations/*), en vez de
            // depender implicitamente de que las convenciones de EF Core
            // coincidan con lo que el modulo espera. No cambia el esquema ya
            // creado por EnsureCreatedAsync (las configuraciones no fijan
            // nombres de tabla), solo lo deja explicito.
            modelBuilder.ApplyThemeManagerPersistenceModel();
        }
    }
}
