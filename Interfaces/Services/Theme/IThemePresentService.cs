using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemePresentService
    {
        Task<List<ThemePresent>> GetAllAsync();

        Task<ThemePresent?> GetByIdAsync(int id);

        Task<ThemePresent?> GetBaseAsync();

        Task<ThemePresent?> GetActiveAsync();

        Task<List<ThemePresent>> ActivateAsync(int id);

        Task<ThemePresent> CreateAsync(ThemePresent themePresent);

        /// <summary>
        /// Elimina el tema dado. Lanza ThemeValidationException si es el tema
        /// base o el actualmente activo. No hace nada si el id no existe.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Se dispara cada vez que ActivateAsync completa exitosamente, con el
        /// tema recien activado. Permite a un consumidor (por ejemplo, el
        /// favicon dinamico del &lt;head&gt; de una app) reaccionar sin tener que
        /// re-consultar el tema activo por su cuenta.
        /// </summary>
        event Func<ThemePresent, Task>? ThemePresentActivated;
    }
}
