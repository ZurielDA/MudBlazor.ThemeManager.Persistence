using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    /// <summary>
    /// La relacion con ThemeCatalog se configura desde ThemeCatalogConfiguration
    /// (lado "uno" de la relacion uno-a-muchos) para no declararla dos veces.
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
