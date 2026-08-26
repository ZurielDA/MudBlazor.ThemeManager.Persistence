namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services
{
    /// <summary>
    /// Contrato minimo de almacenamiento de archivos que necesita este modulo
    /// (favicons y logos). La aplicacion consumidora provee la implementacion,
    /// que tipicamente delega en su propio servicio de almacenamiento de archivos
    /// (por ejemplo, implementando esta interfaz adicionalmente en ese servicio).
    /// </summary>
    public interface IThemeFileStorageService
    {
        Task<string> SaveFileAsync(ThemeAssetFileContent file, string folder);

        /// <summary>
        /// Elimina el archivo previamente guardado en la ruta dada (la misma
        /// ruta devuelta por SaveFileAsync). Debe ser tolerante a que el
        /// archivo ya no exista (no lanzar en ese caso).
        /// </summary>
        Task DeleteFileAsync(string path);
    }
}
