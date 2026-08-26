namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services
{
    /// <summary>
    /// Contenido de un archivo a persistir (favicon/logo), desacoplado de
    /// IBrowserFile (Microsoft.AspNetCore.Components.Forms). El limite de la
    /// capa de aplicacion/persistencia de la libreria solo conoce este tipo;
    /// la traduccion desde IBrowserFile ocurre unicamente en el componente
    /// Razor que recibe el archivo del usuario. Esto permite que
    /// IThemeFaviconService/IThemeLogoService/IThemeFileStorageService puedan
    /// invocarse tambien desde un consumidor sin UI Blazor (un endpoint de
    /// API, un job en background), sin forzarlo a referenciar Blazor Forms.
    /// </summary>
    /// <param name="Content">Contenido del archivo. El llamador es responsable de
    /// posicionarlo al inicio (Position = 0) antes de pasarlo.</param>
    /// <param name="FileName">Nombre original del archivo (solo informativo/para
    /// derivar la extension; el almacenamiento genera su propio nombre seguro).</param>
    /// <param name="ContentType">Tipo MIME reportado por el origen del archivo.</param>
    /// <param name="Length">Tamaño en bytes del contenido.</param>
    public sealed record ThemeAssetFileContent(Stream Content, string FileName, string ContentType, long Length);
}
