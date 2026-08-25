using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;

namespace SAMACDX.ThemeManager.Persistence.Services
{
    public class ThemePresentService : IThemePresentService
    {        
        private readonly IThemePresentRepository _themePresentRepository;

        public ThemePresentService(IThemePresentRepository themePresentRepository)
        {
            _themePresentRepository = themePresentRepository;
        }

        public async Task<ThemePresent?> GetByThemeIdAsync(int id)
        {            
            return await _themePresentRepository.FirstOrDefaultAsync(t => t.ThemeCatalogId == id);
        }

        public async Task<ThemePresent> CreateAsync(ThemePresent themePresent)
        {
            return await _themePresentRepository.AddAsync(themePresent);
        }
    }
}
