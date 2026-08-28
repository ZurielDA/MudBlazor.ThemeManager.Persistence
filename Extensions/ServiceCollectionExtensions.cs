using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.ThemeManager.Persistence.DataAccess;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;
using SAMACDX.ThemeManager.Persistence.Application;
using SAMACDX.ThemeManager.Persistence.Application.Assets;
using SAMACDX.ThemeManager.Persistence.Application.Terminology;
using SAMACDX.ThemeManager.Persistence.ThemeManagerIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SAMACDX.ThemeManager.Persistence.Extensions
{
    /// <summary>
    /// Punto de entrada para registrar este modulo (Theme/Branding) en la aplicacion
    /// consumidora.
    ///
    /// La aplicacion consumidora debe:
    ///   1. Llamar a services.AddThemeManagerPersistence&lt;TContext&gt;() indicando su
    ///      propio DbContext (TContext), que debe exponer DbSet&lt;T&gt; (vía Set&lt;T&gt;())
    ///      para ThemePresent, ThemeAsset, AppName y ThemeTerm
    ///      (Entities/ThemeCatalog/* y Entities/Theme/ThemeTerm). Ver tambien
    ///      ModelBuilderExtensions.ApplyThemeManagerPersistenceModel() para
    ///      aplicar el modelo EF Core de estas 4 entidades explicitamente
    ///      desde el OnModelCreating de ese DbContext. ThemePresent,
    ///      ThemeAsset y AppName son catalogos independientes, sin ninguna
    ///      relacion entre si.
    ///   2. Registrar su propia implementacion de IThemeFileStorageService (tipicamente
    ///      agregando esa interfaz a su servicio de almacenamiento de archivos existente),
    ///      o usar la implementacion opcional que trae la libreria via
    ///      AddThemeManagerPersistenceLocalFileStorage() (requiere IWebHostEnvironment,
    ///      es decir, un host Microsoft.NET.Sdk.Web).
    ///   3. Registrar IDbContextFactory&lt;TContext&gt; (o un TContext externo) como ya
    ///      hace normalmente para EF Core.
    ///
    /// Todos los registros usan TryAdd*, asi que un consumidor puede
    /// reemplazar cualquier pieza (por ejemplo, su propio ITermService)
    /// registrandola ANTES o DESPUES de llamar a este metodo, sin depender de
    /// un orden implicito.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddThemeManagerPersistence<TContext>(
            this IServiceCollection services,
            Action<ThemeManagerPersistenceOptions>? configureOptions = null)
            where TContext : DbContext
        {
            var options = new ThemeManagerPersistenceOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton(options);

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
            services.TryAddScoped<IThemeAssetRepository>(sp =>
                CreateRepository<ThemeAssetRepository<TContext>, TContext>(sp,
                    f => new ThemeAssetRepository<TContext>(f),
                    c => new ThemeAssetRepository<TContext>(c)));
            services.TryAddScoped<IThemePresentRepository>(sp =>
                CreateRepository<ThemePresentRepository<TContext>, TContext>(sp,
                    f => new ThemePresentRepository<TContext>(f),
                    c => new ThemePresentRepository<TContext>(c)));
            services.TryAddScoped<IThemeTermRepository>(sp =>
                CreateRepository<ThemeTermRepository<TContext>, TContext>(sp,
                    f => new ThemeTermRepository<TContext>(f),
                    c => new ThemeTermRepository<TContext>(c)));
            services.TryAddScoped<IAppNameRepository>(sp =>
                CreateRepository<AppNameRepository<TContext>, TContext>(sp,
                    f => new AppNameRepository<TContext>(f),
                    c => new AppNameRepository<TContext>(c)));

            services.TryAddScoped<IThemeManagerService, ThemeManagerService>();
            services.TryAddScoped<IThemePresentService, ThemePresentService>();
            services.TryAddScoped<IThemeTermService, ThemeTermService>();
            services.TryAddScoped<IThemeFaviconService, ThemeFaviconService>();
            services.TryAddScoped<IThemeLogoService, ThemeLogoService>();
            services.TryAddScoped<IAppNameService, AppNameService>();
            services.TryAddScoped<ITermService, TermService>();

            return services;
        }

        /// <summary>
        /// Registra una implementacion opcional de IThemeFileStorageService
        /// que guarda archivos en disco bajo IWebHostEnvironment.WebRootPath.
        /// No se activa automaticamente al llamar a
        /// AddThemeManagerPersistence&lt;TContext&gt;() -- es un metodo aparte para
        /// que el consumidor la elija explicitamente si le sirve, en vez de
        /// forzarla sobre quien prefiera su propio backend de almacenamiento.
        /// Requiere un host que exponga IWebHostEnvironment (Microsoft.NET.Sdk.Web).
        /// </summary>
        public static IServiceCollection AddThemeManagerPersistenceLocalFileStorage(this IServiceCollection services)
        {
            services.TryAddScoped<IThemeFileStorageService, LocalDiskThemeFileStorageService>();

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
