using SAMACDX.ThemeManager.Persistence.Entities.Theme;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeTermService
    {
        Task<List<ThemeTerm>> GetAllTermsAsync();

        Task<ThemeTerm> CreateTermsAsync(ThemeTerm themeTerm);

        Task<ThemeTerm> UpdateTermsAsync(ThemeTerm themeTerm);

        /// <summary>
        /// Elimina el termino dado. No hace nada si el id no existe. El
        /// llamador es responsable de invalidar el cache de ITermService
        /// despues de llamar a este metodo (mismo patron ya usado tras
        /// CreateTermsAsync/UpdateTermsAsync).
        /// </summary>
        Task DeleteTermsAsync(int id);
    }
}
