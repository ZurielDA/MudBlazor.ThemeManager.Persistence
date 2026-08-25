using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Persistence.Seeders.Themes
{
    public static class ThemeFaviconsSeeder
    {
        public static async Task SeedAsync(DbContext context)
        {
            var defaultThemeCatalog = await context.Set<ThemeCatalog>().Include(t => t.ThemeFavicons).FirstOrDefaultAsync(t => t.IsActive);

            if (defaultThemeCatalog != null && !defaultThemeCatalog.ThemeFavicons.Any())
            {
                defaultThemeCatalog.ThemeFavicons.Add(new ThemeFavicon
                {
                    Name = "default.svg",
                    Path = "/Uploads/icons/adx1g43z.ka1.svg",
                    IsActive = true,
                    ThemeCatalogId = defaultThemeCatalog.Id
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
