using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    /// <summary>
    /// El indice unico sobre Name se deja declarado con el atributo [Index]
    /// directamente en la entidad (Entities/ThemeCatalog/AppName.cs), para no
    /// declararlo dos veces.
    /// </summary>
    public class AppNameConfiguration : IEntityTypeConfiguration<AppName>
    {
        public void Configure(EntityTypeBuilder<AppName> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired();
        }
    }
}
