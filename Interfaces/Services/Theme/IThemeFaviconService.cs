using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeFaviconService
    {
        Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id);

        Task<ThemeAsset> CreateAsync(ThemeAsset themeFavicon, IBrowserFile browserFile);

        Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeFaviconId);
    }
}
