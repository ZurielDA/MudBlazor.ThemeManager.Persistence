using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace SAMACDX.ThemeManager.Persistence.Application
{
    public class ThemePresentService : IThemePresentService
    {
        private const string ActivePresentCacheKey = "ThemePresentService_ActivePresent";

        private readonly IThemePresentRepository _themePresentRepository;
        private readonly IMemoryCache _cache;
        private readonly ThemeManagerPersistenceOptions _options;

        public event Func<ThemePresent, Task>? ThemePresentActivated;

        public ThemePresentService(
            IThemePresentRepository themePresentRepository,
            IMemoryCache cache,
            ThemeManagerPersistenceOptions options)
        {
            _themePresentRepository = themePresentRepository;
            _cache = cache;
            _options = options;
        }

        public async Task<List<ThemePresent>> GetAllAsync()
        {
            var result = await _themePresentRepository.GetAllAsync();

            return result.ToList();
        }

        public async Task<ThemePresent?> GetByIdAsync(int id)
        {
            return await _themePresentRepository.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<ThemePresent?> GetBaseAsync()
        {
            return await _themePresentRepository.FirstOrDefaultAsync(t => t.IsBase);
        }

        public async Task<ThemePresent?> GetActiveAsync()
        {
            if (_cache.TryGetValue(ActivePresentCacheKey, out ThemePresent? cached) && cached is not null)
            {
                return cached;
            }

            var present = await _themePresentRepository.FirstOrDefaultAsync(t => t.IsActive);

            if (present is not null)
            {
                _cache.Set(ActivePresentCacheKey, present, new MemoryCacheEntryOptions().SetSlidingExpiration(_options.ActivePresentCacheDuration));
            }

            return present;
        }

        public async Task<List<ThemePresent>> ActivateAsync(int id)
        {
            var all = (await _themePresentRepository.GetAllAsync()).ToList();

            ExclusiveActivationHelper.ActivateOnly(all, id, t => t.Id, (t, active) => t.IsActive = active);

            await _themePresentRepository.UpdateRangeAsync(all);

            InvalidateActiveCache();

            var activated = all.FirstOrDefault(t => t.Id == id);

            if (activated is not null && ThemePresentActivated is not null)
            {
                await ThemePresentActivated.Invoke(activated);
            }

            return all;
        }

        public async Task<ThemePresent> CreateAsync(ThemePresent themePresent)
        {
            if (string.IsNullOrWhiteSpace(themePresent.Name))
            {
                throw new ThemeValidationException("El nombre del tema no puede estar vacio.");
            }

            var existing = await _themePresentRepository.FindAsync(t => t.Name == themePresent.Name);

            if (existing.Any())
            {
                throw new ThemeValidationException($"Ya existe un tema llamado \"{themePresent.Name}\".");
            }

            return await _themePresentRepository.AddAsync(themePresent);
        }

        public async Task<ThemePresent> UpdateAsync(ThemePresent themePresent)
        {
            if (themePresent.Id <= 0)
            {
                throw new ThemeValidationException("El tema a actualizar no es valido.");
            }

            if (string.IsNullOrWhiteSpace(themePresent.Name))
            {
                throw new ThemeValidationException("El nombre del tema no puede estar vacio.");
            }

            var duplicateName = await _themePresentRepository.FirstOrDefaultAsync(t => t.Name == themePresent.Name && t.Id != themePresent.Id);

            if (duplicateName is not null)
            {
                throw new ThemeValidationException($"Ya existe un tema llamado \"{themePresent.Name}\".");
            }

            await _themePresentRepository.UpdateAsync(themePresent);
            InvalidateActiveCache();

            return themePresent;
        }

        public async Task DeleteAsync(int id)
        {
            var present = (await _themePresentRepository.FindAsync(t => t.Id == id)).FirstOrDefault();

            if (present is null)
            {
                return;
            }

            if (present.IsBase)
            {
                throw new ThemeValidationException("No se puede eliminar el tema base.");
            }

            if (present.IsActive)
            {
                throw new ThemeValidationException("No se puede eliminar el tema actualmente activo.");
            }

            await _themePresentRepository.RemoveAsync(present);
        }

        private void InvalidateActiveCache()
        {
            _cache.Remove(ActivePresentCacheKey);
        }
    }
}
