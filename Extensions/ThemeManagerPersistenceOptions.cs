namespace SAMACDX.ThemeManager.Persistence.Extensions
{
    /// <summary>
    /// Opciones de configuracion del modulo Theme/Branding. Se configuran al
    /// registrar el modulo:
    /// <code>
    /// services.AddThemeManagerPersistence&lt;TContext&gt;(options =>
    /// {
    ///     options.TermCacheDuration = TimeSpan.FromMinutes(15);
    ///     options.FaviconUploadFolder = "Branding/favicons";
    /// });
    /// </code>
    /// </summary>
    public sealed class ThemeManagerPersistenceOptions
    {
        /// <summary>
        /// Tiempo de vida (sliding) del cache en memoria de terminologia
        /// (ITermService).
        /// </summary>
        public TimeSpan TermCacheDuration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Tiempo de vida (sliding) del cache en memoria del tema activo
        /// (IThemePresentService.GetActiveAsync). Se mantiene deliberadamente
        /// corto (a diferencia del cache de terminologia) porque el tema
        /// activo es el dato que un usuario espera ver reflejado de inmediato
        /// tras activarlo.
        /// </summary>
        public TimeSpan ActivePresentCacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Carpeta (relativa a donde el IThemeFileStorageService del consumidor
        /// decida almacenar archivos) usada al crear un favicon.
        /// </summary>
        public string FaviconUploadFolder { get; set; } = "Uploads/icons";

        /// <summary>
        /// Carpeta usada al crear un logo.
        /// </summary>
        public string LogoUploadFolder { get; set; } = "Uploads/logos";

        /// <summary>
        /// Tamaño maximo (en bytes) que los componentes de la libreria permiten
        /// seleccionar para favicon/logo antes de enviarlo a IThemeFileStorageService.
        /// No reemplaza ninguna validacion adicional que el propio
        /// IThemeFileStorageService del consumidor decida aplicar (por ejemplo,
        /// un limite mas estricto de almacenamiento).
        /// </summary>
        public long MaxUploadSizeBytes { get; set; } = 10 * 1024 * 1024;

        /// <summary>
        /// Tipos de contenido (MIME) permitidos para favicon/logo. Vacio o null
        /// desactiva la validacion (permite cualquier tipo).
        /// </summary>
        public string[] AllowedAssetContentTypes { get; set; } = new[]
        {
            "image/svg+xml", "image/png", "image/jpeg", "image/x-icon", "image/vnd.microsoft.icon", "image/webp"
        };
    }
}
