using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeCatalogService
    {
        Task<List<ThemeCatalog>> GetAllAsync();

        Task<ThemeCatalog?> GetBaseAsync();

        Task<ThemeCatalog?> GetActiveAsync();

        Task<List<ThemeCatalog>> ActivateAsync(int id);

        Task<ThemeCatalog> CreateWithThemePresentAsync(ThemeCatalog themeCatalog, ThemePresent themePresent);
    }
}
