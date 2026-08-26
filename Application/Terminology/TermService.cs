using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using SAMACDX.ThemeManager.Persistence.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace SAMACDX.ThemeManager.Persistence.Application.Terminology
{
    internal class TermService : ITermService
    {
        private const string CacheKey = "TermService_AllTerms";

        private readonly IThemeTermService _themeTermService;
        private readonly IMemoryCache _cache;
        private readonly ThemeManagerPersistenceOptions _options;

        public TermService(IThemeTermService themeTermService, IMemoryCache cache, ThemeManagerPersistenceOptions options)
        {
            _themeTermService = themeTermService;
            _cache = cache;
            _options = options;
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

            _cache.Set(CacheKey, dict, new MemoryCacheEntryOptions().SetSlidingExpiration(_options.TermCacheDuration));

            return dict;
        }
    }
}
