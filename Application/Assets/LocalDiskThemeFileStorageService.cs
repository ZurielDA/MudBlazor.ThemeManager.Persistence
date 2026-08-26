using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace SAMACDX.ThemeManager.Persistence.Application.Assets
{
    /// <summary>
    /// Implementacion de IThemeFileStorageService que guarda archivos en
    /// disco, bajo IWebHostEnvironment.WebRootPath (wwwroot) de la app
    /// consumidora. Opcional: se registra explicitamente via
    /// services.AddThemeManagerPersistenceLocalFileStorage() -- no se activa
    /// por defecto al llamar a AddThemeManagerPersistence&lt;TContext&gt;(), para
    /// no imponerla a un consumidor que prefiera su propio backend de
    /// almacenamiento (blob storage, S3, etc.). Equivalente, casi al detalle,
    /// a la implementacion que cada consumidor (incluido samples/TestHost)
    /// tenia que escribir por su cuenta antes de esta etapa.
    /// </summary>
    public class LocalDiskThemeFileStorageService : IThemeFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalDiskThemeFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(ThemeAssetFileContent file, string folder)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            var path = Path.Combine(_environment.WebRootPath, folder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var safeFileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(path, safeFileName);

            if (file.Content.CanSeek)
            {
                file.Content.Position = 0;
            }

            await using (var output = File.Create(filePath))
            {
                await file.Content.CopyToAsync(output);
            }

            return $"/{folder}/{safeFileName}";
        }

        public Task DeleteFileAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.CompletedTask;
            }

            var relative = path.TrimStart('/', '\\');
            var fullPath = Path.Combine(_environment.WebRootPath, relative);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
