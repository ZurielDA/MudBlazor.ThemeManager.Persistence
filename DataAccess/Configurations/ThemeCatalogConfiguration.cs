using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    /// <summary>
    /// Configuracion EF Core explicita para ThemeCatalog. Restablece
    /// exactamente lo que las convenciones de EF Core ya producian (no cambia
    /// nombres de tabla ni comportamiento); su unico proposito es dejar el
    /// modelo documentado y aplicable por un consumidor nuevo via
    /// ModelBuilderExtensions.ApplyThemeManagerPersistenceModel(). El indice
    /// unico sobre Name se deja como ya estaba, declarado por el atributo
    /// [Index] directamente en la entidad (Entities/ThemeCatalog/ThemeCatalog.cs)
    /// para no declarar el mismo indice dos veces.
    /// </summary>
    public class ThemeCatalogConfiguration : IEntityTypeConfiguration<ThemeCatalog>
    {
        public void Configure(EntityTypeBuilder<ThemeCatalog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();

            builder.HasOne(x => x.ThemePresent)
                .WithOne(x => x.ThemeCatalog)
                .HasForeignKey<ThemePresent>(x => x.ThemeCatalogId);

            builder.HasMany(x => x.ThemeAssets)
                .WithOne(x => x.ThemeCatalog)
                .HasForeignKey(x => x.ThemeCatalogId);
        }
    }
}
