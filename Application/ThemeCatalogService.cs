using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Application
{
    public class ThemeCatalogService : IThemeCatalogService
    {
        private readonly IThemeCatalogRepository _themeCatalogRepository;
        private readonly IThemePresentService _themePresentService;

        public ThemeCatalogService(IThemeCatalogRepository themeCatalogRepository, IThemePresentService themePresentService)
        {
            _themeCatalogRepository = themeCatalogRepository;
            _themePresentService = themePresentService;
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
            var catalog = await _themeCatalogRepository.Query(
                t => t.ThemePresent,
                t => t.ThemeAssets
            ).FirstOrDefaultAsync(t => t.IsActive);

            if (catalog is not null)
            {
                catalog.ThemeAssets = catalog.ThemeAssets?.Where(a => a.IsActive).ToList() ?? new();
            }

            return catalog;
        }

        public async Task<List<ThemeCatalog>> ActivateAsync(int id)
        {
            var allThemeCatalogs = await _themeCatalogRepository.GetAllAsync();

            allThemeCatalogs.ToList().ForEach(t => t.IsActive = t.Id == id);

            await _themeCatalogRepository.UpdateRangeAsync(allThemeCatalogs);

            return allThemeCatalogs.ToList();
        }

        public async Task<ThemeCatalog> CreateWithThemePresentAsync(ThemeCatalog themeCatalog, ThemePresent themePresent)
        {
            var themeCatalogTemp = await _themeCatalogRepository.AddAsync(themeCatalog);

            themePresent.ThemeCatalogId = themeCatalogTemp.Id;

            var themePresentTemp = await _themePresentService.CreateAsync(themePresent);

            themeCatalogTemp.ThemePresent = themePresentTemp;

            return themeCatalogTemp;
        }
    }
}
