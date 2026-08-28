using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;

namespace SAMACDX.ThemeManager.Persistence.Application.Assets
{
    /// <summary>
    /// Logica compartida entre ThemeFaviconService y ThemeLogoService: crear,
    /// listar y activar un ThemeAsset de un ThemeAssetType dado, y resolver el
    /// asset activo de ese tipo. ThemeAsset es un catalogo independiente (sin
    /// relacion con ThemePresent ni ninguna otra entidad), asi que "activo"
    /// es exclusivo dentro de cada ThemeAssetType, sin ningun otro alcance.
    /// Se usa por composicion (no por herencia) para mantener
    /// IThemeFaviconService e IThemeLogoService como interfaces publicas
    /// independientes.
    /// </summary>
    internal sealed class ThemeAssetOperations
    {
        private readonly IThemeAssetRepository _themeAssetRepository;
        private readonly IThemeFileStorageService _fileStorageService;
        private readonly ThemeAssetType _type;
        private readonly string _uploadFolder;
        private readonly string[] _allowedContentTypes;
        private readonly string _defaultPath;

        public ThemeAssetOperations(
            IThemeAssetRepository themeAssetRepository,
            IThemeFileStorageService fileStorageService,
            ThemeAssetType type,
            string uploadFolder,
            string[] allowedContentTypes,
            string defaultPath)
        {
            _themeAssetRepository = themeAssetRepository;
            _fileStorageService = fileStorageService;
            _type = type;
            _uploadFolder = uploadFolder;
            _allowedContentTypes = allowedContentTypes ?? Array.Empty<string>();
            _defaultPath = defaultPath ?? string.Empty;
        }

        public async Task<List<ThemeAsset>> GetAllAsync()
        {
            var assets = await _themeAssetRepository.FindAsync(t => t.Type == _type);

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

        public async Task<List<ThemeAsset>> ActivateAsync(int themeAssetId)
        {
            var assets = (await _themeAssetRepository.FindAsync(t => t.Type == _type)).ToList();

            ExclusiveActivationHelper.ActivateOnly(assets, themeAssetId, t => t.Id, (t, active) => t.IsActive = active);

            await _themeAssetRepository.UpdateRangeAsync(assets);

            return assets;
        }

        /// <summary>
        /// Resuelve el asset activo (de este tipo) -- ThemeAsset no tiene
        /// ningun alcance ademas de su propio Type, asi que "activo" es
        /// global para ese tipo. Si no hay ningun ThemeAsset activo (o
        /// ninguno en absoluto) devuelve el asset por defecto de la libreria
        /// (defaultPath, resuelto por el llamador via StaticAssets.
        /// ThemeDefaultAssets) en vez de una cadena vacia -- este es el unico
        /// punto donde se decide ese fallback, para que ningun consumidor
        /// (componentes incluidos) tenga que conocer ThemeDefaultAssets.
        /// </summary>
        public async Task<string> GetCurrentPathAsync()
        {
            var active = await _themeAssetRepository.FirstOrDefaultAsync(t => t.Type == _type && t.IsActive);

            return active?.Path ?? _defaultPath;
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
