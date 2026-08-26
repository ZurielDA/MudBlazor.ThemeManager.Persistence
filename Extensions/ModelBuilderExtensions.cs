using Microsoft.EntityFrameworkCore;
using SAMACDX.ThemeManager.Persistence.DataAccess.Configurations;

namespace SAMACDX.ThemeManager.Persistence.Extensions
{
    /// <summary>
    /// Punto de entrada para aplicar el modelo EF Core de este modulo al
    /// DbContext de la aplicacion consumidora. Antes de esta etapa, un
    /// consumidor nuevo no tenia forma soportada de saber que forma exacta
    /// debia tener su esquema (columnas, claves foraneas) mas alla de leer el
    /// codigo fuente de las entidades.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Aplica las IEntityTypeConfiguration&lt;T&gt; de las 4 entidades de este
        /// modulo (ThemeCatalog, ThemeAsset, ThemePresent, ThemeTerm). Llamar
        /// desde el OnModelCreating del DbContext consumidor:
        /// <code>
        /// protected override void OnModelCreating(ModelBuilder modelBuilder)
        /// {
        ///     base.OnModelCreating(modelBuilder);
        ///     modelBuilder.ApplyThemeManagerPersistenceModel();
        /// }
        /// </code>
        /// Las configuraciones aplicadas aqui restablecen exactamente lo que
        /// las convenciones de EF Core ya producian antes de esta etapa (no
        /// fijan nombres de tabla ni cambian ningun comportamiento); su
        /// proposito es dejar el modelo explicito y reutilizable para
        /// cualquier consumidor nuevo, en vez de depender implicitamente de
        /// que las convenciones por defecto de EF Core coincidan con lo
        /// esperado.
        /// </summary>
        public static ModelBuilder ApplyThemeManagerPersistenceModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ThemeCatalogConfiguration());
            modelBuilder.ApplyConfiguration(new ThemeAssetConfiguration());
            modelBuilder.ApplyConfiguration(new ThemePresentConfiguration());
            modelBuilder.ApplyConfiguration(new ThemeTermConfiguration());

            return modelBuilder;
        }
    }
}
