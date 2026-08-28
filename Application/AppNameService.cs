using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;

namespace SAMACDX.ThemeManager.Persistence.Application
{
    public class AppNameService : IAppNameService
    {
        private readonly IAppNameRepository _appNameRepository;

        public AppNameService(IAppNameRepository appNameRepository)
        {
            _appNameRepository = appNameRepository;
        }

        public async Task<List<AppName>> GetAllAsync()
        {
            var result = await _appNameRepository.GetAllAsync();

            return result.ToList();
        }

        public async Task<AppName> CreateAsync(AppName appName)
        {
            if (string.IsNullOrWhiteSpace(appName.Name))
            {
                throw new ThemeValidationException("El nombre de la aplicacion no puede estar vacio.");
            }

            var existing = await _appNameRepository.FindAsync(a => a.Name == appName.Name);

            if (existing.Any())
            {
                throw new ThemeValidationException($"Ya existe un nombre de aplicacion \"{appName.Name}\" en el historial.");
            }

            return await _appNameRepository.AddAsync(appName);
        }

        public async Task<List<AppName>> ActivateAsync(int id)
        {
            var all = (await _appNameRepository.GetAllAsync()).ToList();

            ExclusiveActivationHelper.ActivateOnly(all, id, a => a.Id, (a, active) => a.IsActive = active);

            await _appNameRepository.UpdateRangeAsync(all);

            return all;
        }

        public async Task<string> GetCurrentNameAsync()
        {
            var active = await _appNameRepository.FirstOrDefaultAsync(a => a.IsActive);

            return active?.Name ?? string.Empty;
        }
    }
}
