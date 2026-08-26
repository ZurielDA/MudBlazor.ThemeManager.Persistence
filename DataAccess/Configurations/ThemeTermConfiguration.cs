using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAMACDX.ThemeManager.Persistence.DataAccess.Configurations
{
    public class ThemeTermConfiguration : IEntityTypeConfiguration<ThemeTerm>
    {
        public void Configure(EntityTypeBuilder<ThemeTerm> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key).IsRequired();
            builder.Property(x => x.Singular).IsRequired();
            builder.Property(x => x.Plural).IsRequired();
            builder.Property(x => x.Gender).IsRequired();
            builder.Property(x => x.Special).IsRequired();
        }
    }
}
