using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Extensions;

namespace SAMACDX.ThemeManager.Persistence.Application.Assets
{
    public class ThemeLogoService : IThemeLogoService
    {
        private readonly ThemeAssetOperations _operations;

        public ThemeLogoService(IThemeFileStorageService fileStorageService, IThemeAssetRepository themeAssetRepository, ThemeManagerPersistenceOptions options)
        {
            _operations = new ThemeAssetOperations(
                themeAssetRepository,
                fileStorageService,
                ThemeAssetType.Logo,
                options.LogoUploadFolder,
                options.AllowedAssetContentTypes);
        }

        public Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id) => _operations.GetAllByThemeCatalogIdAsync(id);

        public Task<ThemeAsset> CreateAsync(ThemeAsset themeLogo, ThemeAssetFileContent file) => _operations.CreateAsync(themeLogo, file);

        public Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeLogoId) => _operations.ActivateAsync(themeCatalogId, themeLogoId);

        public Task<string> GetCurrentLogoPathAsync() => _operations.GetCurrentPathAsync();

        public Task DeleteAsync(int themeLogoId) => _operations.DeleteAsync(themeLogoId);
    }
}
