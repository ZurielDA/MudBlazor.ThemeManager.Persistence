using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;

namespace SAMACDX.ThemeManager.Persistence.Application.Assets
{
    /// <summary>
    /// Logica compartida entre ThemeFaviconService y ThemeLogoService (antes
    /// duplicada casi identicamente en las dos clases): crear, listar y
    /// activar un ThemeAsset de un ThemeAssetType dado, y resolver el asset
    /// activo del catalogo ACTUALMENTE ACTIVO. Se usa por composicion (no por
    /// herencia) para mantener IThemeFaviconService e IThemeLogoService como
    /// interfaces publicas independientes, tal como se decidio explicitamente
    /// en la etapa de consolidacion de ThemeAsset (los consumidores de alto
    /// nivel no deben verse forzados a conocer ThemeAssetType).
    /// </summary>
    internal sealed class ThemeAssetOperations
    {
        private readonly IThemeAssetRepository _themeAssetRepository;
        private readonly IThemeFileStorageService _fileStorageService;
        private readonly ThemeAssetType _type;
        private readonly string _uploadFolder;
        private readonly string[] _allowedContentTypes;

        public ThemeAssetOperations(
            IThemeAssetRepository themeAssetRepository,
            IThemeFileStorageService fileStorageService,
            ThemeAssetType type,
            string uploadFolder,
            string[] allowedContentTypes)
        {
            _themeAssetRepository = themeAssetRepository;
            _fileStorageService = fileStorageService;
            _type = type;
            _uploadFolder = uploadFolder;
            _allowedContentTypes = allowedContentTypes ?? Array.Empty<string>();
        }

        public async Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id)
        {
            var assets = await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == id && t.Type == _type);

            return assets.ToList();
        }

        public async Task<ThemeAsset> CreateAsync(ThemeAsset themeAsset, ThemeAssetFileContent file)
        {
            if (_allowedContentTypes.Length > 0 && !_allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                throw new ThemeValidationException($"El tipo de archivo \"{file.ContentType}\" no esta permitido.");
            }

            string path = await _fileStorageService.SaveFileAsync(file, _uploadFolder);

            themeAsset.Path = path;
            themeAsset.Type = _type;

            return await _themeAssetRepository.AddAsync(themeAsset);
        }

        public async Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeAssetId)
        {
            var assets = (await _themeAssetRepository.FindAsync(t => t.ThemeCatalogId == themeCatalogId && t.Type == _type)).ToList();

            ExclusiveActivationHelper.ActivateOnly(assets, themeAssetId, t => t.Id, (t, active) => t.IsActive = active);

            await _themeAssetRepository.UpdateRangeAsync(assets);

            return assets;
        }

        /// <summary>
        /// Resuelve el asset activo (de este tipo) del catalogo ACTUALMENTE
        /// ACTIVO (ThemeCatalog.IsActive == true) -- no de un catalogo
        /// hardcodeado por id.
        /// </summary>
        public async Task<string> GetCurrentPathAsync()
        {
            var assets = await _themeAssetRepository.FindAsync(t => t.ThemeCatalog.IsActive && t.Type == _type);

            var active = assets.FirstOrDefault(a => a.IsActive);

            return active?.Path ?? string.Empty;
        }

        public async Task DeleteAsync(int themeAssetId)
        {
            var asset = (await _themeAssetRepository.FindAsync(t => t.Id == themeAssetId && t.Type == _type)).FirstOrDefault();

            if (asset is null)
            {
                return;
            }

            await _themeAssetRepository.RemoveAsync(asset);
            await _fileStorageService.DeleteFileAsync(asset.Path);
        }
    }
}
