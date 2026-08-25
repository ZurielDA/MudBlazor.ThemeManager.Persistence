using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeLogoService
    {
        Task<List<ThemeLogo>> GetAllByThemeCatalogIdAsync(int id);

        Task<ThemeLogo> CreateAsync(ThemeLogo themeLogo, IBrowserFile browserFile);

        Task<List<ThemeLogo>> ActivateAsync(int themeCatalogId, int themeLogoId);
        
        Task<string> GetCurrentLogoPathAsync();
    }
}
