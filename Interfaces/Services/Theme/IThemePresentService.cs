using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemePresentService
    {
        Task<ThemePresent?> GetByThemeIdAsync(int id);

        Task<ThemePresent> CreateAsync(ThemePresent themePresent);
    }
}
