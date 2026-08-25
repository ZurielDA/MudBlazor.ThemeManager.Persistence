using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Persistence.Seeders.Themes
{
    public static class ThemeLogosSeeder
    {
        public static async Task SeedAsync(DbContext context)
        {
            var defaultThemeCatalog = await context.Set<ThemeCatalog>().Include(t => t.ThemeLogos).FirstOrDefaultAsync(t => t.IsActive);

            if (defaultThemeCatalog != null && !defaultThemeCatalog.ThemeLogos.Any())
            {
                defaultThemeCatalog.ThemeLogos.Add(new ThemeLogo
                {
                    Name = "default.svg",
                    Path = "/Uploads/logos/LogoCentrado.png",
                    IsActive = true,
                    ThemeCatalogId = defaultThemeCatalog.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
