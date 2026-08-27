using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    /// <summary>
    /// ThemeAsset es un catalogo independiente: no tiene ninguna relacion con
    /// ThemePresent ni con ninguna otra entidad de este modulo.
    /// </summary>
    public class ThemeAssetConfiguration : IEntityTypeConfiguration<ThemeAsset>
    {
        public void Configure(EntityTypeBuilder<ThemeAsset> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Path).IsRequired();
        }
    }
}
