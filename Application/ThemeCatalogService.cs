using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SAMACDX.ThemeManager.Persistence.Application
{
    public class ThemeCatalogService : IThemeCatalogService
    {
        private const string ActiveCatalogCacheKey = "ThemeCatalogService_ActiveCatalog";

        private readonly IThemeCatalogRepository _themeCatalogRepository;
        private readonly IThemePresentService _themePresentService;
        private readonly IMemoryCache _cache;
        private readonly ThemeManagerPersistenceOptions _options;

        public event Func<ThemeCatalog, Task>? ThemeCatalogActivated;

        public ThemeCatalogService(
            IThemeCatalogRepository themeCatalogRepository,
            IThemePresentService themePresentService,
            IMemoryCache cache,
            ThemeManagerPersistenceOptions options)
        {
            _themeCatalogRepository = themeCatalogRepository;
            _themePresentService = themePresentService;
            _cache = cache;
            _options = options;
        }

        public async Task<List<ThemeCatalog>> GetAllAsync()
        {
            var result = await _themeCatalogRepository.GetAllAsync();

            return result.ToList();
        }

        public async Task<ThemeCatalog?> GetBaseAsync()
        {
            return await _themeCatalogRepository.Query(t => t.ThemePresent).FirstOrDefaultAsync(t => t.IsBase);
        }

        public async Task<ThemeCatalog?> GetActiveAsync()
        {
            if (_cache.TryGetValue(ActiveCatalogCacheKey, out ThemeCatalog? cached) && cached is not null)
            {
                return cached;
            }

            var catalog = await _themeCatalogRepository.Query(
                t => t.ThemePresent,
                t => t.ThemeAssets
            ).FirstOrDefaultAsync(t => t.IsActive);

            if (catalog is not null)
            {
                catalog.ThemeAssets = catalog.ThemeAssets?.Where(a => a.IsActive).ToList() ?? new();

                _cache.Set(ActiveCatalogCacheKey, catalog, new MemoryCacheEntryOptions().SetSlidingExpiration(_options.ActiveCatalogCacheDuration));
            }

            return catalog;
        }

        public async Task<List<ThemeCatalog>> ActivateAsync(int id)
        {
            var allThemeCatalogs = (await _themeCatalogRepository.GetAllAsync()).ToList();

            ExclusiveActivationHelper.ActivateOnly(allThemeCatalogs, id, t => t.Id, (t, active) => t.IsActive = active);

            await _themeCatalogRepository.UpdateRangeAsync(allThemeCatalogs);

            InvalidateActiveCache();

            var activated = allThemeCatalogs.FirstOrDefault(t => t.Id == id);

            if (activated is not null && ThemeCatalogActivated is not null)
            {
                await ThemeCatalogActivated.Invoke(activated);
            }

            return allThemeCatalogs;
        }

        public async Task<ThemeCatalog> CreateWithThemePresentAsync(ThemeCatalog themeCatalog, ThemePresent themePresent)
        {
            if (string.IsNullOrWhiteSpace(themeCatalog.Name))
            {
                throw new ThemeValidationException("El nombre del tema no puede estar vacio.");
            }

            var existing = await _themeCatalogRepository.FindAsync(t => t.Name == themeCatalog.Name);

            if (existing.Any())
            {
                throw new ThemeValidationException($"Ya existe un tema llamado \"{themeCatalog.Name}\".");
            }

            var themeCatalogTemp = await _themeCatalogRepository.AddAsync(themeCatalog);

            try
            {
                themePresent.ThemeCatalogId = themeCatalogTemp.Id;

                var themePresentTemp = await _themePresentService.CreateAsync(themePresent);

                themeCatalogTemp.ThemePresent = themePresentTemp;

                return themeCatalogTemp;
            }
            catch
            {
                // No es una transaccion real de base de datos (el diseño
                // generico de repositorios no comparte DbContext entre capas:
                // este servicio no conoce TContext), pero evita dejar un
                // ThemeCatalog huerfano sin su ThemePresent si la segunda
                // escritura falla.
                await _themeCatalogRepository.RemoveAsync(themeCatalogTemp);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            var catalog = (await _themeCatalogRepository.FindAsync(t => t.Id == id)).FirstOrDefault();

            if (catalog is null)
            {
                return;
            }

            if (catalog.IsBase)
            {
                throw new ThemeValidationException("No se puede eliminar el tema base.");
            }

            if (catalog.IsActive)
            {
                throw new ThemeValidationException("No se puede eliminar el tema actualmente activo.");
            }

            await _themeCatalogRepository.RemoveAsync(catalog);
        }

        private void InvalidateActiveCache()
        {
            _cache.Remove(ActiveCatalogCacheKey);
        }
    }
}
