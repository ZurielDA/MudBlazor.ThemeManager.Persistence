using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.ThemeManager.Persistence.Services
{
    public class ThemeLogoService : IThemeLogoService
    {
        private readonly IThemeFileStorageService _fileStorageService;
        private readonly IThemeAssetRepository _themeAssetRepository;

        public ThemeLogoService(IThemeFileStorageService fileStorageService, IThemeAssetRepository themeAssetRepository)
        {
            _fileStorageService = fileStorageService;
            _themeAssetRepository = themeAssetRepository;
        }

        public async Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id)
        {
            var result = await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == id && t.Type == ThemeAssetType.Logo);

            return result.ToList();
        }

        public async Task<ThemeAsset> CreateAsync(ThemeAsset themeLogo, IBrowserFile file)
        {
            string path = await _fileStorageService.SaveFileAsync(file, "Uploads/logos");

            themeLogo.Path = path;
            themeLogo.Type = ThemeAssetType.Logo;

            return await _themeAssetRepository.AddAsync(themeLogo);
        }

        public async Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeLogoId)
        {
            var logos = await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == themeCatalogId && t.Type == ThemeAssetType.Logo);

            logos.ToList().ForEach(t => t.IsActive = t.Id == themeLogoId);

            await _themeAssetRepository.UpdateRangeAsync(logos);

            return logos.ToList();
        }

        public async Task<string> GetCurrentLogoPathAsync()
        {
            var logos = await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == 1 && t.Type == ThemeAssetType.Logo);

            var activeLogo = logos.FirstOrDefault(l => l.IsActive);

            return activeLogo?.Path ?? string.Empty;
        }
    }
}
