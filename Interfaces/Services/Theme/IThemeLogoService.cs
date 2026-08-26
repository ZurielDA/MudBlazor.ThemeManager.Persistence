using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeLogoService
    {
        Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id);

        Task<ThemeAsset> CreateAsync(ThemeAsset themeLogo, IBrowserFile browserFile);

        Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeLogoId);
        
        Task<string> GetCurrentLogoPathAsync();
    }
}
