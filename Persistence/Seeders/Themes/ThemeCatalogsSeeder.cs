using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Persistence.Seeders.Themes
{
    public static class ThemeCatalogsSeeder
    {
        public static async Task SeedAsync(DbContext context)
        {
            if (!context.Set<ThemeCatalog>().Any())
            {
                await context.Set<ThemeCatalog>().AddAsync(new ThemeCatalog
                {
                    Name = "default",
                    IsActive = true
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
