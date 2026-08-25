using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services
{
    /// <summary>
    /// Contrato mínimo de almacenamiento de archivos que necesita este módulo
    /// (favicons y logos). La aplicación consumidora provee la implementación,
    /// que típicamente delega en su propio servicio de almacenamiento de archivos
    /// (por ejemplo, implementando esta interfaz adicionalmente en ese servicio).
    /// </summary>
    public interface IThemeFileStorageService
    {
        Task<string> SaveFileAsync(IBrowserFile file, string folder);
    }
}
