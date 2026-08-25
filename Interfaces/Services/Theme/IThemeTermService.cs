using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Theme;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeTermService
    {
        Task<List<ThemeTerm>> GetAllTermsAsync();

        Task<ThemeTerm> CreateTermsAsync(ThemeTerm themeTerm);

        Task<ThemeTerm> UpdateTermsAsync(ThemeTerm themeTerm);
    }
}
