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
    ///      para ThemeCatalog, ThemeAsset, ThemePresent y ThemeTerm
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

            // Cada repositorio (Theme*Repository<TContext>) tiene dos constructores:
            // uno que recibe IDbContextFactory<TContext> y otro que recibe un TContext
            // externo, para soportar ambos estilos de host (ver comentario arriba). Si se
            // registraran los tipos directamente (services.AddScoped<TService, TImpl>()),
            // el contenedor de DI de ASP.NET Core intentaría elegir el constructor por
            // reflexión y lanzaría "constructors are ambiguous" en cuanto AMBOS parámetros
            // resulten resolvibles (p. ej. AddDbContextFactory<TContext>() en EF Core 8+
            // también deja TContext resolvible como scoped). Por eso aquí se resuelve
            // explícitamente: se prefiere la factory cuando está registrada, y si no, se
            // cae al TContext externo.
            services.AddScoped<IThemeCatalogRepository>(sp =>
                CreateRepository<ThemeCatalogRepository<TContext>, TContext>(sp,
                    f => new ThemeCatalogRepository<TContext>(f),
                    c => new ThemeCatalogRepository<TContext>(c)));
            services.AddScoped<IThemeAssetRepository>(sp =>
                CreateRepository<ThemeAssetRepository<TContext>, TContext>(sp,
                    f => new ThemeAssetRepository<TContext>(f),
                    c => new ThemeAssetRepository<TContext>(c)));
            services.AddScoped<IThemePresentRepository>(sp =>
                CreateRepository<ThemePresentRepository<TContext>, TContext>(sp,
                    f => new ThemePresentRepository<TContext>(f),
                    c => new ThemePresentRepository<TContext>(c)));
            services.AddScoped<IThemeTermRepository>(sp =>
                CreateRepository<ThemeTermRepository<TContext>, TContext>(sp,
                    f => new ThemeTermRepository<TContext>(f),
                    c => new ThemeTermRepository<TContext>(c)));

            services.AddScoped<IThemeManagerService, global::ThemeManagerService>();
            services.AddScoped<IThemeCatalogService, ThemeCatalogService>();
            services.AddScoped<IThemePresentService, ThemePresentService>();
            services.AddScoped<IThemeTermService, ThemeTermService>();
            services.AddScoped<IThemeFaviconService, ThemeFaviconService>();
            services.AddScoped<IThemeLogoService, ThemeLogoService>();
            services.AddScoped<ITermService, TermService>();

            return services;
        }

        private static TRepo CreateRepository<TRepo, TContext>(
            IServiceProvider sp,
            Func<IDbContextFactory<TContext>, TRepo> viaFactory,
            Func<TContext, TRepo> viaExternalContext)
            where TContext : DbContext
        {
            var factory = sp.GetService<IDbContextFactory<TContext>>();
            return factory != null
                ? viaFactory(factory)
                : viaExternalContext(sp.GetRequiredService<TContext>());
        }
    }
}
