using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Utilities;
using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Theme;
using Microsoft.Extensions.Caching.Memory;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Services.Theme
{
    internal class TermService : ITermService
    {
        private const string CacheKey = "TermService_AllTerms";
        private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        private readonly IThemeTermService _themeTermService;
        private readonly IMemoryCache _cache;

        public TermService(IThemeTermService themeTermService, IMemoryCache cache)
        {
            _themeTermService = themeTermService;
            _cache = cache;
        }

        public async Task<string> GetAsync(string key)
        {
            var term = await GetByKeyAsync(key);
            return term?.Singular ?? key;
        }

        public async Task<string> GetPluralAsync(string key)
        {
            var term = await GetByKeyAsync(key);
            return term?.Plural ?? key;
        }

        public async Task<string> GetWithDefiniteArticleAsync(string key)
        {
            var term = await GetByKeyAsync(key);
            if (term is null)
            {
                return key;                
            }

            return SpanishArticleHelper.WithDefiniteArticle(term.Gender, term.Singular);
        }

        public async Task<string> GetPluralWithDefiniteArticleAsync(string key)
        {
            var term = await GetByKeyAsync(key);

            if (term is null)
            {
                return key;                
            }

            return SpanishArticleHelper.WithDefiniteArticlePlural(term.Gender, term.Plural);
        }

        public async Task<string> GetWithIndefiniteArticleAsync(string key)
        {
            var term = await GetByKeyAsync(key);
            if (term is null)
            {
                return key;                
            }

            return SpanishArticleHelper.WithIndefiniteArticle(term.Gender, term.Singular);
        }

        public async Task<string> GetPluralWithIndefiniteArticleAsync(string key)
        {
            var term = await GetByKeyAsync(key);
            if (term is null)
            {                
                return key;
            }

            return SpanishArticleHelper.WithIndefiniteArticlePlural(term.Gender, term.Plural);
        }

        public async Task<ThemeTerm?> GetByKeyAsync(string key)
        {
            var dict = await GetOrLoadDictionaryAsync();
            return dict.TryGetValue(key, out var term) ? term : null;
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKey);
        }

        private async Task<Dictionary<string, ThemeTerm>> GetOrLoadDictionaryAsync()
        {
            if (_cache.TryGetValue(CacheKey, out Dictionary<string, ThemeTerm>? cached) && cached is not null)
            {
                return cached;                
            }

            var allTerms = await _themeTermService.GetAllTermsAsync();

            var dict = allTerms.GroupBy(t => t.Key).ToDictionary(g => g.Key, g => g.First());

            _cache.Set(CacheKey, dict, CacheOptions);

            return dict;
        }
    }
}
