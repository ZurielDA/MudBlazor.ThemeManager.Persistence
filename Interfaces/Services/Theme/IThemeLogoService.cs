using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeLogoService
    {
        Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id);

        Task<ThemeAsset> CreateAsync(ThemeAsset themeLogo, ThemeAssetFileContent file);

        Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeLogoId);

        /// <summary>
        /// Ruta del logo activo del catalogo de tema ACTUALMENTE ACTIVO
        /// (ThemeCatalog.IsActive == true), o cadena vacia si no hay ninguno.
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
