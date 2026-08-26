# Branding, ThemeAsset y recursos activos

## El modelo `ThemeAsset`

Todos los recursos visuales de branding (hoy: **logo** y **favicon**) se modelan con una única entidad, `ThemeAsset`, distinguida por el campo `Type` (`ThemeAssetType`, enum con exactamente dos valores: `Logo` y `Favicon`). No existe hoy ningún tercer tipo de asset — si en el futuro se necesitara uno (por ejemplo, una imagen de fondo de login), habría que agregar un nuevo valor al enum; documentar eso sería "funcionalidad futura" y queda fuera del alcance de esta documentación.

```csharp
public class ThemeAsset
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public ThemeAssetType Type { get; set; }   // Logo | Favicon
    public bool IsActive { get; set; }
    public int ThemeCatalogId { get; set; }
    public ThemeCatalog ThemeCatalog { get; set; }
}
```

Cada `ThemeAsset` pertenece a un `ThemeCatalog` (`ThemeCatalogId`). Dentro de un mismo catálogo, puede haber varios assets del mismo tipo (por ejemplo, varios logos subidos), pero solo **uno** puede tener `IsActive == true` por combinación `(ThemeCatalogId, Type)`.

Aunque `IThemeFaviconService`/`IThemeLogoService` son interfaces separadas (para que el consumidor no tenga que conocer `ThemeAssetType` si no lo necesita), ambas operan sobre la misma tabla `ThemeAsset`, filtrando internamente por `Type`.

## "Branding" no es una API propia

No existe un `IBrandingService` ni un método "obtener branding activo" único: **"Branding" es el nombre conceptual** que usa esta documentación (y la descripción del paquete NuGet) para referirse en conjunto a logo + favicon. Para consumir "el branding activo" hay que llamar a los dos métodos por separado (`GetCurrentLogoPathAsync()` y `GetCurrentFaviconPathAsync()`), o leerlos juntos desde `ThemeCatalog.ThemeAssets` (ver más abajo).

De la misma forma, **no existe un "Icono activo" separado del favicon**: en este modelo, el favicon *es* el ícono de la aplicación — no hay un `ThemeAssetType.Icon` adicional.

## Obtener los recursos activos

### Logo activo

```csharp
string logoPath = await ThemeLogoService.GetCurrentLogoPathAsync();
```

Resuelve el `ThemeAsset` de tipo `Logo`, `IsActive == true`, del catálogo **actualmente activo** (`ThemeCatalog.IsActive == true`) — no de un catálogo fijo. Devuelve cadena vacía si no hay ninguno.

### Favicon activo

```csharp
string faviconPath = await ThemeFaviconService.GetCurrentFaviconPathAsync();
```

Misma lógica que el logo, filtrando por `Type == Favicon`.

### Ambos a la vez, desde el tema activo

Como `IThemeCatalogService.GetActiveAsync()` ya trae `ThemeAssets` cargados (filtrados a los `IsActive == true`), también se puede leer directamente:

```csharp
var activo = await ThemeCatalogService.GetActiveAsync();
var logo = activo?.ThemeAssets?.FirstOrDefault(a => a.Type == ThemeAssetType.Logo)?.Path;
var favicon = activo?.ThemeAssets?.FirstOrDefault(a => a.Type == ThemeAssetType.Favicon)?.Path;
```

### Fallback cuando no hay ningún asset activo aún

```csharp
string logoFallback = ThemeDefaultAssets.DefaultLogoPath;
string faviconFallback = ThemeDefaultAssets.DefaultFaviconPath;
```

`StaticAssets.ThemeDefaultAssets` publica dos SVG genéricos como *Static Web Assets* de la propia librería (`_content/{ensamblado}/default-assets/{favicon,logo}.svg`), para que una aplicación nueva sin ningún `ThemeAsset` activo tenga algo válido que mostrar en vez de una imagen rota. Un `ThemeAsset` real y activo siempre tiene prioridad — estos son solo el valor inicial.

## Dónde usar cada recurso

| Recurso | Dónde | Cómo |
|---|---|---|
| **Tema activo** | `Layout` raíz, enlazado al `MudThemeProvider` | Ver [THEMES.md](THEMES.md) y el ejemplo en [GETTING-STARTED.md](GETTING-STARTED.md). Se propaga en cascada a **todos** los componentes MudBlazor de la app — AppBar, Login, Dashboard, cualquier página — sin que cada uno tenga que leerlo por su cuenta. |
| **Logo activo** | AppBar, pantalla de Login, Dashboard, o cualquier componente que necesite mostrar el logotipo de la organización | `<MudImage Src="@logoPath" />` con el valor de `GetCurrentLogoPathAsync()` (o el fallback si viene vacío). |
| **Favicon activo** | `<head>` HTML del documento | El shell HTML estático (`App.razor` en Blazor Server) se renderiza una sola vez al inicio del circuito; para que el favicon refleje el catálogo activo, un componente que resuelva `GetCurrentFaviconPathAsync()` debe escribir el `<link rel="icon">` correspondiente usando el mecanismo de cabecera de Blazor (`HeadContent`/`PageTitle` de `Microsoft.AspNetCore.Components.Web`, capturado por el `<HeadOutlet>` en `App.razor`). La librería no hace esto automáticamente — expone la ruta, no un componente que la inyecte en el `<head>` (ver ejemplo en [GETTING-STARTED.md](GETTING-STARTED.md)). |
| **Componentes que necesiten branding en general** | Cualquier página de la app | Inyectar `IThemeLogoService`/`IThemeFaviconService` (o leer `IThemeCatalogService.GetActiveAsync().ThemeAssets`) directamente donde se necesite; no hay un componente "wrapper" de branding en la librería más allá de `ThemeFaviconAndLogoConfig.razor`, que es de **administración**, no de solo-lectura (ver [ADMINISTRATION.md](ADMINISTRATION.md)). |

## Almacenamiento de archivos

La librería no guarda los archivos por sí misma: delega en `IThemeFileStorageService`, que la aplicación consumidora debe implementar (o usar la opcional que trae la librería):

```csharp
public interface IThemeFileStorageService
{
    Task<string> SaveFileAsync(ThemeAssetFileContent file, string folder);
    Task DeleteFileAsync(string path);
}
```

- `SaveFileAsync` recibe el contenido ya leído (`ThemeAssetFileContent`, desacoplado de `IBrowserFile`) y la carpeta configurada (`ThemeManagerPersistenceOptions.FaviconUploadFolder`/`LogoUploadFolder`); debe devolver la ruta pública bajo la que quedó guardado el archivo (la misma que después se persiste en `ThemeAsset.Path`).
- `DeleteFileAsync` recibe esa misma ruta y debe ser tolerante a que el archivo ya no exista (no debe lanzar en ese caso).

**Implementación opcional incluida**: `Application.Assets.LocalDiskThemeFileStorageService`, que guarda bajo `IWebHostEnvironment.WebRootPath` (`wwwroot`). Se activa con `services.AddThemeManagerPersistenceLocalFileStorage()` — no se registra automáticamente al llamar a `AddThemeManagerPersistence<TContext>()`, para no imponerla a un consumidor que prefiera otro backend (blob storage, S3, etc.).

El tipo de contenido se valida contra `ThemeManagerPersistenceOptions.AllowedAssetContentTypes` antes de guardar (lanza `ThemeValidationException` si no está permitido); el tamaño máximo (`MaxUploadSizeBytes`) se valida en los componentes Razor de la librería antes de leer el archivo, no dentro de `IThemeFaviconService`/`IThemeLogoService`.
