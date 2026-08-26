using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeCatalogService
    {
        Task<List<ThemeCatalog>> GetAllAsync();

        Task<ThemeCatalog?> GetBaseAsync();

        Task<ThemeCatalog?> GetActiveAsync();

        Task<List<ThemeCatalog>> ActivateAsync(int id);

        Task<ThemeCatalog> CreateWithThemePresentAsync(ThemeCatalog themeCatalog, ThemePresent themePresent);

        /// <summary>
        /// Elimina el catalogo de tema dado. Lanza ThemeValidationException si
        /// es el catalogo base o el actualmente activo. No hace nada si el id
        /// no existe.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Se dispara cada vez que ActivateAsync completa exitosamente, con el
        /// catalogo recien activado. Permite a un consumidor (por ejemplo, el
        /// favicon dinamico del &lt;head&gt; de una app) reaccionar sin tener que
        /// re-consultar el catalogo activo por su cuenta.
        /// </summary>
        event Func<ThemeCatalog, Task>? ThemeCatalogActivated;
    }
}
