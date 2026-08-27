using Microsoft.EntityFrameworkCore;
using SAMACDX.ThemeManager.Persistence.DataAccess.Configurations;

namespace SAMACDX.ThemeManager.Persistence.Extensions
{
    /// <summary>
    /// Punto de entrada para aplicar el modelo EF Core de este modulo al
    /// DbContext de la aplicacion consumidora. Llamar desde el
    /// OnModelCreating del DbContext consumidor:
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     base.OnModelCreating(modelBuilder);
    ///     modelBuilder.ApplyThemeManagerPersistenceModel();
    /// }
    /// </code>
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Aplica las IEntityTypeConfiguration&lt;T&gt; de las 2 entidades de este
        /// modulo (ThemePresent, ThemeAsset). Independientes entre si: no hay
        /// ninguna relacion que declarar entre ellas.
        /// </summary>
        public static ModelBuilder ApplyThemeManagerPersistenceModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ThemePresentConfiguration());
            modelBuilder.ApplyConfiguration(new ThemeAssetConfiguration());

            return modelBuilder;
        }
    }
}
