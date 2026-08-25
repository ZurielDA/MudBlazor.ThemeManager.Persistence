using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost.Services
{
    /// <summary>
    /// Implementación mínima de IThemeFileStorageService para el test host,
    /// calcada de GDIP.Infrastructure.Services.FileStorageService.SaveFileAsync
    /// (la única pieza que el módulo Theme realmente necesita de esa clase).
    /// </summary>
    public class LocalFileStorageService : IThemeFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IBrowserFile file, string folder)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5 MB

            if (file.Size > maxFileSize)
            {
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido ({maxFileSize / 1024 / 1024} MB).");
            }

            using MemoryStream memoryStream = new MemoryStream();

            await file.OpenReadStream(maxFileSize).CopyToAsync(memoryStream);

            byte[] fileContent = memoryStream.ToArray();

            var path = Path.Combine(_environment.WebRootPath, folder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var safeFileName = Path.GetRandomFileName() + Path.GetExtension(file.Name);
            var filePath = Path.Combine(path, safeFileName);

            await File.WriteAllBytesAsync(filePath, fileContent);

            return $"/{folder}/{safeFileName}";
        }
    }
}
