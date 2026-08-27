using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    /// <summary>
    /// El indice unico sobre Name se deja declarado con el atributo [Index]
    /// directamente en la entidad (Entities/ThemeCatalog/ThemePresent.cs),
    /// para no declararlo dos veces.
    /// </summary>
    public class ThemePresentConfiguration : IEntityTypeConfiguration<ThemePresent>
    {
        public void Configure(EntityTypeBuilder<ThemePresent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.JsonData).IsRequired();
        }
    }
}
