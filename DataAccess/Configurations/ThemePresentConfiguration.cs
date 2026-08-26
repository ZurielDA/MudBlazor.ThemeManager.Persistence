using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    /// <summary>
    /// La relacion con ThemeCatalog se configura desde ThemeCatalogConfiguration
    /// (lado "uno" de la relacion uno-a-uno) para no declararla dos veces.
    /// </summary>
    public class ThemePresentConfiguration : IEntityTypeConfiguration<ThemePresent>
    {
        public void Configure(EntityTypeBuilder<ThemePresent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.JsonData).IsRequired();
        }
    }
}
