# Instalación y configuración

## Instalación

### Dependencias obligatorias

| Dependencia | Versión | Motivo |
|---|---|---|
| .NET | `net9.0` | `TargetFramework` de la librería (`Microsoft.NET.Sdk.Razor`). |
| MudBlazor | `[8.0.0, 9.0.0)` | Toda la UI de la librería y del editor (`MudBlazor.ThemeManager`) depende de sus componentes. |
| Microsoft.EntityFrameworkCore | `[9.0.0, 10.0.0)` | Entidades y `IEntityTypeConfiguration<T>` de la librería. |
| Microsoft.AspNetCore.Components.Web | `[9.0.1, 10.0.0)` | Requerido transitivamente por MudBlazor 8.0.0. |
| Microsoft.Extensions.Caching.Memory | `[9.0.0, 10.0.0)` | `IThemeCatalogService`/`ITermService` cachean en `IMemoryCache`. |
| Microsoft.Extensions.DependencyInjection.Abstractions | `[9.0.1, 10.0.0)` | Requerido transitivamente por MudBlazor 8.0.0 (vía `Microsoft.Extensions.Localization`). |
| **Fork `MudBlazor.ThemeManager`** (`External/MudBlazor.ThemeManager`, submódulo git) | commit fijado del fork `SAMACDX/ThemeManager.git`, rama `gdip-theme-manager` | Ver detalle abajo — **no existe como paquete NuGet independiente**. |
| Un proveedor de base de datos de EF Core (SQLite, SQL Server, PostgreSQL, etc.) | el que use la app | La librería no depende de ninguno en particular; solo usa `DbContext`/`ModelBuilder` de EF Core. |
| Host Blazor Server (`Microsoft.NET.Sdk.Web`, con render interactivo de servidor) en el proyecto consumidor | — | Los componentes de la librería usan `ISnackbar` (MudBlazor) e inyectan servicios scoped-per-circuito; no está pensada para Blazor WebAssembly. |

### Dependencias opcionales

| Dependencia | Cuándo se necesita |
|---|---|
| `IWebHostEnvironment` (host `Sdk.Web`) | Solo si se usa `AddThemeManagerPersistenceLocalFileStorage()` (implementación de almacenamiento en disco que trae la librería). Si el consumidor provee su propia implementación de `IThemeFileStorageService`, no hace falta. |

### El submódulo `MudBlazor.ThemeManager`

Esta librería referencia el fork `MudBlazor.ThemeManager` como **`ProjectReference`** (no como paquete NuGet):

```xml
<ItemGroup>
  <ProjectReference Include="External\MudBlazor.ThemeManager\src\MudBlazor.ThemeManager\MudBlazor.ThemeManager.csproj" />
</ItemGroup>
```

Ese código vive bajo `External/MudBlazor.ThemeManager` como **submódulo git** (`.gitmodules`, apuntando a `https://github.com/SAMACDX/ThemeManager.git`, rama `gdip-theme-manager`). Esto significa que:

- Un proyecto consumidor que solo referencia el `.csproj`/paquete de esta librería (sin clonar el repositorio completo con submódulos) **no tendrá disponible el ensamblado `MudBlazor.ThemeManager`** — es una dependencia de compilación real, no opcional, mientras la librería se distribuya de esta forma.
- Para clonar el repositorio con el submódulo incluido: `git clone --recurse-submodules <url>`, o si ya está clonado: `git submodule update --init --recursive`.
- Esto es un acoplamiento **permanente e intencional** de esta etapa del proyecto, no un descuido — ver la nota correspondiente en `MIGRATION-NOTES.md` ("Etapa: implementación de correcciones y mejoras", punto R11). No se documenta aquí como si fuera a cambiar, porque no hay ninguna decisión tomada de publicar el fork como paquete NuGet independiente.

### Referencias del proyecto consumidor

En el `.csproj` de la aplicación consumidora:

```xml
<ItemGroup>
  <ProjectReference Include="..\ruta\a\SAMACDX.MudBlazor.ThemeManager.Persistence.csproj" />
</ItemGroup>
```

(La librería aún no está publicada como paquete NuGet — el `.csproj` tiene los metadatos de empaquetado listos, `Version`/`Authors`/`Description`/`PackageId`, pero no `RepositoryUrl` ni `PackageLicenseExpression`; ver observación en el README principal.)

## Configuración

### 1. Registrar servicios (Dependency Injection)

En `Program.cs`:

```csharp
using SAMACDX.ThemeManager.Persistence.Extensions;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddThemeManagerPersistence<AppDbContext>();

// Opción A: usar la implementación de almacenamiento en disco de la librería
builder.Services.AddThemeManagerPersistenceLocalFileStorage();

// Opción B: proveer la propia
builder.Services.AddScoped<IThemeFileStorageService, MiPropioAlmacenamiento>();
```

`AddThemeManagerPersistence<TContext>` acepta un delegado opcional para configurar `ThemeManagerPersistenceOptions`:

```csharp
builder.Services.AddThemeManagerPersistence<AppDbContext>(options =>
{
    options.TermCacheDuration = TimeSpan.FromMinutes(15);
    options.FaviconUploadFolder = "Branding/favicons";
    options.LogoUploadFolder = "Branding/logos";
    options.MaxUploadSizeBytes = 5 * 1024 * 1024;
    options.AllowedAssetContentTypes = new[] { "image/svg+xml", "image/png" };
});
```

No configurar nada preserva los valores por defecto documentados en [ARCHITECTURE.md](ARCHITECTURE.md#configuración-extensionsthememanagerpersistenceoptions).

**Qué debe proveer el consumidor**: su propio `DbContext` (o `IDbContextFactory<TContext>`), y una implementación de `IThemeFileStorageService` (propia, o la opcional de la librería).

**Qué resuelve la librería internamente**: la resolución de qué constructor de repositorio usar (factory vs. contexto externo — ver más abajo), el registro de todos los servicios de aplicación con `TryAddScoped` (reemplazables), el caché en memoria, y las opciones con sus valores por defecto.

### 2. `AddDbContextFactory` vs. `AddDbContext`: diferencia de comportamiento importante

`AddThemeManagerPersistence<TContext>` soporta **dos estilos** de registro del `DbContext`, y el comportamiento de las escrituras de la librería cambia según cuál se use:

**Opción recomendada — `IDbContextFactory<TContext>`** (la que usa `samples/TestHost`, la única probada de punta a punta):

```csharp
builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite("Data Source=app.db"));
```

Cada repositorio de la librería crea su propio `DbContext` de corta vida por operación (vía la factory) y **llama a `SaveChangesAsync()` automáticamente** al final de cada método de escritura (`AddAsync`, `UpdateAsync`, `UpdateRangeAsync`, `RemoveAsync`). Es el patrón recomendado para Blazor Server, donde un mismo circuito de usuario puede vivir horas y varios componentes pueden necesitar acceso concurrente a la base de datos.

**Opción alternativa — `AddDbContext<TContext>()` (contexto scoped externo, sin factory)**:

```csharp
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=app.db"));
```

Si **no** hay ningún `IDbContextFactory<TContext>` registrado, los repositorios usan el `TContext` scoped que sí esté registrado. En este modo, los métodos de escritura de la librería **no llaman a `SaveChangesAsync()`** — la aplicación consumidora es responsable de persistir los cambios (por ejemplo, si ya tiene su propio patrón de Unit of Work que llama a `SaveChangesAsync()` al final de cada request). Si la app no llama a `SaveChangesAsync()` en algún punto, los cambios hechos a través de esta librería **no se guardan**, sin ningún error explícito.

Si ambos están registrados, la librería **prefiere la factory**.

### 3. Configuración del `DbContext` consumidor

Ver [docs/DBCONTEXT-AND-MIGRATIONS.md](DBCONTEXT-AND-MIGRATIONS.md) para el detalle completo de cómo integrar el modelo de esta librería en el `OnModelCreating` del `DbContext` consumidor y cómo generar/aplicar migraciones.

### 4. Configuración requerida por `MudBlazor.ThemeManager`

El fork requiere, en el `<head>` del host (`App.razor` o equivalente), su hoja de estilos propia además de la de MudBlazor:

```html
<link href="@Assets["_content/MudBlazor/MudBlazor.min.css"]" rel="stylesheet" />
<link href="@Assets["_content/MudBlazor.ThemeManager/MudBlazorThemeManager.css"]" rel="stylesheet" />
```

Y los proveedores estándar de MudBlazor en el layout raíz:

```razor
<MudThemeProvider Theme="..." />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

`ISnackbar` (`MudSnackbarProvider`) es obligatorio para el consumidor: todos los componentes Razor de esta librería (`ThemeFaviconAndLogoConfig`, `ThemePaletteSelector`, `ThemeTermConfig`) inyectan `ISnackbar` para mostrar mensajes de error/éxito.

Esta librería no requiere ninguna configuración adicional propia de `MudBlazor.ThemeManager` más allá de la hoja de estilos — el resto (`MudThemeManager`, `ThemePreset`, etc.) se consume ya integrado a través de `ThemePaletteSelector.razor`.

### 5. Configuración de almacenamiento de assets

Ver [BRANDING.md](BRANDING.md#almacenamiento-de-archivos) para el contrato `IThemeFileStorageService` y cómo implementarlo (o usar `LocalDiskThemeFileStorageService`).
