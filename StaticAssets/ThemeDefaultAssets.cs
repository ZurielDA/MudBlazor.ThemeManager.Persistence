namespace SAMACDX.ThemeManager.Persistence.StaticAssets
{
    /// <summary>
    /// Rutas de los recursos estaticos predeterminados (favicon/logo) que esta
    /// libreria distribuye como Static Web Assets propios, para que un proyecto
    /// consumidor sin ningun ThemeAsset activo aun tenga algo valido que mostrar
    /// (en vez de un 404), sin depender de archivos fisicos de GDIP ni de ningun
    /// otro consumidor.
    ///
    /// Los archivos viven en wwwroot/default-assets/ dentro de esta libreria y
    /// se publican por convencion de Razor Class Library bajo
    /// "_content/{AssemblyName}/default-assets/...". El nombre del ensamblado se
    /// resuelve en tiempo de ejecucion via reflexion (en vez de un literal
    /// hardcodeado) para que, si el AssemblyName del csproj cambia alguna vez,
    /// estas dos rutas sigan siendo correctas automaticamente.
    ///
    /// Un ThemeAsset real y activo (subido por el usuario) siempre tiene
    /// prioridad sobre estos valores: son solo el fallback inicial.
    /// </summary>
    public static class ThemeDefaultAssets
    {
        private static readonly string BasePath =
            $"_content/{typeof(ThemeDefaultAssets).Assembly.GetName().Name}/default-assets";

        public static string DefaultFaviconPath => $"{BasePath}/favicon.svg";

        public static string DefaultLogoPath => $"{BasePath}/logo.svg";
    }
}
