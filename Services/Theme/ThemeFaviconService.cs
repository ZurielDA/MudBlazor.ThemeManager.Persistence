using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Services.Theme
{
    public class ThemeFaviconService : IThemeFaviconService
    {
        private readonly IThemeFileStorageService _fileStorageService;
        private readonly IThemeFaviconRepository _themeFaviconRepository;

        public ThemeFaviconService(IThemeFileStorageService fileStorageService, IThemeFaviconRepository themeFaviconRepository)
        { 
            _fileStorageService = fileStorageService;
            _themeFaviconRepository = themeFaviconRepository;
        }

        public async Task<List<ThemeFavicon>> GetAllByThemeCatalogIdAsync(int id)
        {
            var favicons = await _themeFaviconRepository.FindAsync(t => t.ThemeCatalogId == id);

            return favicons.ToList();
        }

        public async Task<ThemeFavicon> CreateAsync(ThemeFavicon themeFavicon, IBrowserFile file)
        {
            string path = await _fileStorageService.SaveFileAsync(file, "Uploads/icons");

            themeFavicon.Path = path;

            return  await _themeFaviconRepository.AddAsync(themeFavicon);
        }

        public async Task<List<ThemeFavicon>> ActivateAsync(int themeCatalogId, int themeFaviconId)
        {
            var favicons = await _themeFaviconRepository.FindAsync(t => t.ThemeCatalogId == themeCatalogId);

            favicons.ToList().ForEach(t => t.IsActive = t.Id == themeFaviconId);

            await _themeFaviconRepository.UpdateRangeAsync(favicons);

            return favicons.ToList();
        }
    }
}
