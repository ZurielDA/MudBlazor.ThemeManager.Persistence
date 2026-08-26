using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeFaviconService
    {
        Task<List<ThemeAsset>> GetAllByThemeCatalogIdAsync(int id);

        Task<ThemeAsset> CreateAsync(ThemeAsset themeFavicon, ThemeAssetFileContent file);

        Task<List<ThemeAsset>> ActivateAsync(int themeCatalogId, int themeFaviconId);

        /// <summary>
        /// Ruta del favicon activo del catalogo de tema ACTUALMENTE ACTIVO
        /// (ThemeCatalog.IsActive == true), o cadena vacia si no hay ninguno.
        /// </summary>
        Task<string> GetCurrentFaviconPathAsync();

        /// <summary>
        /// Elimina el ThemeAsset (favicon) dado, incluyendo su archivo fisico
        /// (via IThemeFileStorageService.DeleteFileAsync). No hace nada si el
        /// id no existe.
        /// </summary>
        Task DeleteAsync(int themeFaviconId);
    }
}
