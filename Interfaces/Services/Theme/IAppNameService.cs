using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    /// <summary>
    /// Administra el nombre de la aplicacion con historial: cada nombre
    /// creado queda disponible para reactivarse mas adelante, sin perder los
    /// nombres anteriores.
    /// </summary>
    public interface IAppNameService
    {
        /// <summary>
        /// Historial completo de nombres (activos e inactivos).
        /// </summary>
        Task<List<AppName>> GetAllAsync();

        /// <summary>
        /// Agrega un nombre nuevo al historial. Lanza ThemeValidationException
        /// si el nombre esta vacio o ya existe en el historial. No lo activa
        /// automaticamente -- llamar a ActivateAsync con el Id devuelto para
        /// que pase a ser el nombre vigente.
        /// </summary>
        Task<AppName> CreateAsync(AppName appName);

        /// <summary>
        /// Activa el nombre indicado (del historial o recien creado):
        /// desactiva los demas y activa este. Devuelve el historial completo
        /// actualizado.
        /// </summary>
        Task<List<AppName>> ActivateAsync(int id);

        /// <summary>
        /// Nombre actualmente activo, o cadena vacia si no hay ninguno.
        /// </summary>
        Task<string> GetCurrentNameAsync();
    }
}
