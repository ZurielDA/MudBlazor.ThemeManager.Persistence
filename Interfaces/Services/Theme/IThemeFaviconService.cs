using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeFaviconService
    {
        Task<List<ThemeFavicon>> GetAllByThemeCatalogIdAsync(int id);

        Task<ThemeFavicon> CreateAsync(ThemeFavicon themeFavicon, IBrowserFile browserFile);

        Task<List<ThemeFavicon>> ActivateAsync(int themeCatalogId, int themeFaviconId);
    }
}
