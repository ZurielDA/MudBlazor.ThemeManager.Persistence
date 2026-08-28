using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Extensions;
using SAMACDX.ThemeManager.Persistence.StaticAssets;

namespace SAMACDX.ThemeManager.Persistence.Application.Assets
{
    public class ThemeFaviconService : IThemeFaviconService
    {
        private readonly ThemeAssetOperations _operations;

        public ThemeFaviconService(IThemeFileStorageService fileStorageService, IThemeAssetRepository themeAssetRepository, ThemeManagerPersistenceOptions options)
        {
            _operations = new ThemeAssetOperations(
                themeAssetRepository,
                fileStorageService,
                ThemeAssetType.Favicon,
                options.FaviconUploadFolder,
                options.AllowedAssetContentTypes,
                ThemeDefaultAssets.DefaultFaviconPath);
        }

        public Task<List<ThemeAsset>> GetAllAsync() => _operations.GetAllAsync();

        public Task<ThemeAsset> CreateAsync(ThemeAsset themeFavicon, ThemeAssetFileContent file) => _operations.CreateAsync(themeFavicon, file);

        public Task<List<ThemeAsset>> ActivateAsync(int themeFaviconId) => _operations.ActivateAsync(themeFaviconId);

        public Task<string> GetCurrentFaviconPathAsync() => _operations.GetCurrentPathAsync();

        public Task DeleteAsync(int themeFaviconId) => _operations.DeleteAsync(themeFaviconId);
    }
}
