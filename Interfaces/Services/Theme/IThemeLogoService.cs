using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeLogoService
    {
        Task<List<ThemeAsset>> GetAllAsync();

        Task<ThemeAsset> CreateAsync(ThemeAsset themeLogo, ThemeAssetFileContent file);

        Task<List<ThemeAsset>> ActivateAsync(int themeLogoId);

        /// <summary>
        /// Ruta del logo actualmente activo. Si no hay ninguno (o ningun
        /// ThemeAsset de tipo Logo en absoluto), devuelve el logo por
        /// defecto de la libreria (StaticAssets.ThemeDefaultAssets.
        /// DefaultLogoPath) en vez de una cadena vacia -- el llamador no
        /// necesita conocer ni resolver ese fallback por su cuenta.
        /// </summary>
        Task<string> GetCurrentLogoPathAsync();

        /// <summary>
        /// Elimina el ThemeAsset (logo) dado, incluyendo su archivo fisico
        /// (via IThemeFileStorageService.DeleteFileAsync). No hace nada si el
        /// id no existe.
        /// </summary>
        Task DeleteAsync(int themeLogoId);
    }
}
