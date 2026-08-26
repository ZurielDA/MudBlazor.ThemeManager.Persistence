using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.ThemeManager.Persistence.Services.Theme
{
    public class ThemeFaviconService : IThemeFaviconService
    {
        private readonly IThemeFileStorageService _fileStorageService;
        private readonly IThemeAssetRepository _themeAssetRepository;

        public ThemeFaviconService(IThemeFileStorageService fileStorageService, IThemeAssetRepository themeAssetRepository)
        { 
            _fileStorageService = fileStorageService;
            _themeAssetRepository = themeAssetRepository;
        }

        public async Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id)
        {
            var favicons = await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == id && t.Type == ThemeAssetType.Favicon);

            return favicons.ToList();
        }

        public async Task<ThemeAsset> CreateAsync(ThemeAsset themeFavicon, IBrowserFile file)
        {
            string path = await _fileStorageService.SaveFileAsync(file, "Uploads/icons");

            themeFavicon.Path = path;
            themeFavicon.Type = ThemeAssetType.Favicon;

            return  await _themeAssetRepository.AddAsync(themeFavicon);
        }

        public async Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeFaviconId)
        {
            var favicons = await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == themeCatalogId && t.Type == ThemeAssetType.Favicon);

            favicons.ToList().ForEach(t => t.IsActive = t.Id == themeFaviconId);

            await _themeAssetRepository.UpdateRangeAsync(favicons);

            return favicons.ToList();
        }
    }
}
