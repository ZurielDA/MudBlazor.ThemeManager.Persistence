using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Services
{
    public class ThemeLogoService : IThemeLogoService
    {
        private readonly IThemeFileStorageService _fileStorageService;
        private readonly IThemeLogoRepository _themeLogoRepository;

        public ThemeLogoService(IThemeFileStorageService fileStorageService, IThemeLogoRepository themeLogoRepository)
        {
            _fileStorageService = fileStorageService;
            _themeLogoRepository = themeLogoRepository;
        }

        public async Task<List<ThemeLogo>> GetAllByThemeCatalogIdAsync(int id)
        {
            var result = await _themeLogoRepository.FindAsync(t => t.ThemeCatalogId == id);

            return result.ToList();
        }

        public async Task<ThemeLogo> CreateAsync(ThemeLogo themeLogo, IBrowserFile file)
        {
            string path = await _fileStorageService.SaveFileAsync(file, "Uploads/logos");

            themeLogo.Path = path;

            return await _themeLogoRepository.AddAsync(themeLogo);
        }

        public async Task<List<ThemeLogo>> ActivateAsync(int themeCatalogId, int themeLogoId)
        {
            var logos = await _themeLogoRepository.FindAsync(t => t.ThemeCatalogId == themeCatalogId);

            logos.ToList().ForEach(t => t.IsActive = t.Id == themeLogoId);

            await _themeLogoRepository.UpdateRangeAsync(logos);

            return logos.ToList();
        }

        public async Task<string> GetCurrentLogoPathAsync()
        {
            var logos = await _themeLogoRepository.FindAsync(t => t.ThemeCatalogId == 1);

            var activeLogo = logos.FirstOrDefault(l => l.IsActive);

            return activeLogo?.Path ?? string.Empty;
        }
    }
}
