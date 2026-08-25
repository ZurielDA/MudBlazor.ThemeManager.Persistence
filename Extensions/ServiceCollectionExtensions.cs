using SAMACDX.ThemeManager.Persistence.Interfaces.Repositories.Theme;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Repositories.Theme;
using SAMACDX.ThemeManager.Persistence.Services;
using SAMACDX.ThemeManager.Persistence.Services.Theme;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SAMACDX.ThemeManager.Persistence.Extensions
{
    /// <summary>
    /// Punto de entrada para registrar este módulo (Theme/Branding) en la aplicación
    /// consumidora.
    ///
    /// La aplicación consumidora debe:
    ///   1. Llamar a services.AddThemeManagerPersistence&lt;TContext&gt;() indicando su
    ///      propio DbContext (TContext), que debe exponer DbSet&lt;T&gt; (vía Set&lt;T&gt;())
    ///      para ThemeCatalog, ThemeFavicon, ThemeLogo, ThemePresent y ThemeTerm
    ///      (Entities/ThemeCatalog/* y Entities/Theme/ThemeTerm).
    ///   2. Registrar su propia implementación de IThemeFileStorageService (típicamente
    ///      agregando esa interfaz a su servicio de almacenamiento de archivos existente).
    ///   3. Registrar IDbContextFactory&lt;TContext&gt; (o un TContext externo) como ya
    ///      hace normalmente para EF Core.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThemeManagerPersistence<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddMemoryCache();

            services.AddScoped<IThemeCatalogRepository, ThemeCatalogRepository<TContext>>();
            services.AddScoped<IThemeFaviconRepository, ThemeFaviconRepository<TContext>>();
            services.AddScoped<IThemeLogoRepository, ThemeLogoRepository<TContext>>();
            services.AddScoped<IThemePresentRepository, ThemePresentRepository<TContext>>();
            services.AddScoped<IThemeTermRepository, ThemeTermRepository<TContext>>();

            services.AddScoped<IThemeManagerService, global::ThemeManagerService>();
            services.AddScoped<IThemeCatalogService, ThemeCatalogService>();
            services.AddScoped<IThemePresentService, ThemePresentService>();
            services.AddScoped<IThemeTermService, ThemeTermService>();
            services.AddScoped<IThemeFaviconService, ThemeFaviconService>();
            services.AddScoped<IThemeLogoService, ThemeLogoService>();
            services.AddScoped<ITermService, TermService>();

            return services;
        }
    }
}
