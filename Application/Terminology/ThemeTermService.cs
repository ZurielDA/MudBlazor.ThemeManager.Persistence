using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;

namespace SAMACDX.ThemeManager.Persistence.Application.Terminology
{
    public class ThemeTermService : IThemeTermService
    {        
        private readonly IThemeTermRepository _themeTermRepository;

        public ThemeTermService(IThemeTermRepository themeTermRepository)
        {
            _themeTermRepository = themeTermRepository;
        }

        public async Task<List<ThemeTerm>> GetAllTermsAsync()
        {
            var result = await _themeTermRepository.GetAllAsync();

            return result.ToList();
        }

        public async Task<ThemeTerm> CreateTermsAsync(ThemeTerm themeTerm)
        {
            return await _themeTermRepository.AddAsync(themeTerm);
        }        

        public async Task<ThemeTerm> UpdateTermsAsync(ThemeTerm themeTerm)
        {            
            await _themeTermRepository.UpdateAsync(themeTerm);

            return themeTerm;
        }
    }
}
