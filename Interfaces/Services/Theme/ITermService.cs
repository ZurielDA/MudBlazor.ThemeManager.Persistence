using SAMACDX.ThemeManager.Persistence.Entities.Theme;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    /// <summary>
    /// Servicio centralizado de terminología del dominio.
    /// Provee acceso a los términos configurables con soporte de artículos en español
    /// y estrategia de caché en memoria para alto rendimiento.
    /// </summary>
    public interface ITermService
    {
        /// <summary>
        /// Retorna la forma singular del término asociado a la clave dada.
        /// Ejemplo: GetAsync("Audit") → "Auditoría"
        /// </summary>
        Task<string> GetAsync(string key);

        /// <summary>
        /// Retorna la forma plural del término asociado a la clave dada.
        /// Ejemplo: GetPluralAsync("Audit") → "Auditorías"
        /// </summary>
        Task<string> GetPluralAsync(string key);

        /// <summary>
        /// Retorna el artículo definido + singular del término.
        /// Ejemplo: GetWithDefiniteArticleAsync("Audit") → "la auditoría"
        /// Aplica la regla de acentuación femenina: "el área", "el agua".
        /// </summary>
        Task<string> GetWithDefiniteArticleAsync(string key);

        /// <summary>
        /// Retorna el artículo definido + plural del término.
        /// Ejemplo: GetPluralWithDefiniteArticleAsync("Audit") → "las auditorías"
        /// </summary>
        Task<string> GetPluralWithDefiniteArticleAsync(string key);

        /// <summary>
        /// Retorna el artículo indefinido + singular del término.
        /// Ejemplo: GetWithIndefiniteArticleAsync("FiscalEntity") → "una entidad fiscalizada"
        /// </summary>
        Task<string> GetWithIndefiniteArticleAsync(string key);

        /// <summary>
        /// Retorna el artículo indefinido + plural del término.
        /// Ejemplo: GetPluralWithIndefiniteArticleAsync("Document") → "unos documentos"
        /// </summary>
        Task<string> GetPluralWithIndefiniteArticleAsync(string key);

        /// <summary>
        /// Retorna el ThemeTerm completo asociado a la clave, o null si no existe.
        /// </summary>
        Task<ThemeTerm?> GetByKeyAsync(string key);

        /// <summary>
        /// Invalida el caché de terminología.
        /// Debe llamarse inmediatamente después de actualizar cualquier término desde la UI.
        /// </summary>
        void InvalidateCache();
    }
}
