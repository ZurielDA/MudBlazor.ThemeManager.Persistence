namespace SAMACDX.ThemeManager.Persistence.StaticAssets
{
    /// <summary>
    /// Rutas de los recursos estáticos predeterminados (favicon/logo) que esta
    /// librería distribuye como Static Web Assets propios, para que un proyecto
    /// consumidor sin ningún ThemeAsset activo aún tenga algo válido que mostrar
    /// (en vez de un 404), sin depender de archivos físicos de GDIP ni de ningún
    /// otro consumidor.
    ///
    /// Los archivos viven en wwwroot/default-assets/ dentro de esta librería y
    /// se publican por convención de Razor Class Library bajo
    /// "_content/{AssemblyName}/default-assets/...". AssemblyName se mantiene
    /// como constante aquí (en vez de leerse por reflexión) porque es un dato
    /// fijo del proyecto (ver SAMACDX.MudBlazor.ThemeManager.Persistence.csproj);
    /// si el AssemblyName del csproj cambia alguna vez, estas dos rutas son las
    /// únicas que hay que actualizar.
    ///
    /// Un ThemeAsset real y activo (subido por el usuario) siempre tiene
    /// prioridad sobre estos valores: son sólo el fallback inicial.
    /// </summary>
    public static class ThemeDefaultAssets
    {
        private const string BasePath = "_content/SAMACDX.MudBlazor.ThemeManager.Persistence/default-assets";

        public const string DefaultFaviconPath = $"{BasePath}/favicon.svg";

        public const string DefaultLogoPath = $"{BasePath}/logo.svg";
    }
}
