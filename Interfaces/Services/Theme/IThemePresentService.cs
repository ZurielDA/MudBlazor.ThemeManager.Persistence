using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemePresentService
    {
        Task<ThemePresent?> GetByThemeIdAsync(int id);

        Task<ThemePresent> CreateAsync(ThemePresent themePresent);
    }
}
