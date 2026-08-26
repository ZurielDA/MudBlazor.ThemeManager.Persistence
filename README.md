# SAMACDX.ThemeManager.Persistence

Módulo reutilizable de gestión de **Theme/Branding** (catálogos de tema, paleta visual, favicon/logo, terminología) para aplicaciones Blazor Server que usan MudBlazor y el fork `MudBlazor.ThemeManager`.

Esta librería se encarga de **persistir, recuperar y activar** configuraciones de tema (creadas visualmente con `MudBlazor.ThemeManager`) y de administrar el **branding** (logo y favicon) asociado a cada configuración, contra el `DbContext` de la aplicación consumidora.

> Esta documentación describe exclusivamente la implementación actual de la librería. No documenta funcionalidad planeada ni comportamiento futuro. Cualquier observación sobre una API confusa o poco conveniente se señala explícitamente donde corresponde, en vez de "corregirse" silenciosamente en el código.

## Índice

- [Introducción](#introducción)
- [Arquitectura](#arquitectura)
- [Instalación](#instalación)
- [Configuración](#configuración)
- [Integración con DbContext](#integración-con-dbcontext)
- [Dependency Injection](#dependency-injection)
- [Migraciones](#migraciones)
- [Documentación adicional](#documentación-adicional)
- [Ejemplo mínimo](#ejemplo-mínimo)
- [Observaciones](#observaciones)

## Introducción

`SAMACDX.ThemeManager.Persistence` (paquete/ensamblado `SAMACDX.MudBlazor.ThemeManager.Persistence`) resuelve tres problemas para una aplicación Blazor Server con MudBlazor:

1. **Persistir** configuraciones de tema visual (paleta, tipografía, elevaciones, radios de borde) editadas con el componente `MudThemeManager` del fork `MudBlazor.ThemeManager`, como filas de base de datos versionables ("catálogos de tema").
2. **Activar** un catálogo de tema como el vigente para toda la aplicación, y recuperarlo de forma consistente (incluso después de reiniciar la aplicación).
3. **Administrar branding** (logo y favicon) asociado a cada catálogo de tema, con almacenamiento de archivos desacoplado (la librería no impone dónde ni cómo se guardan los archivos).

La librería **no reemplaza** a `MudBlazor.ThemeManager`: lo complementa. `MudBlazor.ThemeManager` sigue siendo responsable exclusivo del editor visual y de la estructura del objeto de tema (`ThemeManagerTheme`); esta librería solo sabe guardarlo, recuperarlo y activarlo.

## Arquitectura

Tres piezas con responsabilidades disjuntas:

| Proyecto | Responsabilidad |
|---|---|
| `MudBlazor.ThemeManager` (fork, submódulo git) | Editor visual de tema (`MudThemeManager`), estructura en memoria del tema (`ThemeManagerTheme`), integración con la paleta de MudBlazor. Sin ningún concepto de base de datos ni de "catálogo". |
| `SAMACDX.ThemeManager.Persistence` (esta librería) | Persistencia, recuperación y activación de catálogos de tema; administración de branding (`ThemeAsset`: logo/favicon); integración con Entity Framework Core; servicios de aplicación; componentes Razor reutilizables para administrar todo lo anterior. |
| Proyecto consumidor | Su propio `DbContext` (exponiendo el modelo de esta librería); su propia implementación de almacenamiento de archivos (`IThemeFileStorageService`, o la opcional que trae la librería); integración con su `Layout`/`MudThemeProvider`/`AppBar`; consumo de los recursos activos (tema, logo, favicon) donde su UI lo necesite. |

El detalle completo de responsabilidades, el inventario de la API pública (servicios, interfaces, DTOs, entidades, extensiones de DI, componentes, eventos, recursos estáticos) y el mapa `MudBlazor.ThemeManager` ↔ `SAMACDX.ThemeManager.Persistence` están en **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)**.

## Instalación

Requisitos obligatorios:

- **.NET 9** (`net9.0`). La librería es un *Razor Class Library* (`Microsoft.NET.Sdk.Razor`).
- **MudBlazor** `[8.0.0, 9.0.0)`.
- **Entity Framework Core** `[9.0.0, 10.0.0)` + el proveedor de base de datos que use la aplicación consumidora (SQLite, SQL Server, etc. — la librería no depende de ninguno en particular).
- El **submódulo/fork `MudBlazor.ThemeManager`** (`External/MudBlazor.ThemeManager` en este repositorio, referenciado vía `ProjectReference`) — no está publicado como paquete NuGet independiente, así que la aplicación consumidora necesita ese código fuente disponible (ver detalle en ARCHITECTURE.md).
- Una app Blazor Server (`Microsoft.NET.Sdk.Web`, con render interactivo de servidor) — la librería no fue probada como Blazor WebAssembly.

Opcional:

- `IWebHostEnvironment` (host `Sdk.Web`) — solo si se usa la implementación de almacenamiento de archivos en disco que trae la librería (`AddThemeManagerPersistenceLocalFileStorage()`). Si la app consumidora provee su propia implementación de `IThemeFileStorageService`, esto no es necesario.

Guía completa de instalación (dependencias, referencias de proyecto/submódulo, compatibilidad exacta) en **[docs/INSTALLATION-AND-CONFIGURATION.md](docs/INSTALLATION-AND-CONFIGURATION.md)**.

## Configuración

En `Program.cs` de la aplicación consumidora:

```csharp
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db")); // o el proveedor que use la app

builder.Services.AddThemeManagerPersistence<AppDbContext>();

// La app consumidora provee su propia implementación de almacenamiento de
// archivos, o usa la opcional que trae la librería (requiere Sdk.Web):
builder.Services.AddThemeManagerPersistenceLocalFileStorage();
// -- o bien --
builder.Services.AddScoped<IThemeFileStorageService, MiPropioAlmacenamiento>();
```

`AddThemeManagerPersistence<TContext>` registra todos los repositorios y servicios de la librería con `TryAddScoped`/`TryAddSingleton` (reemplazables por el consumidor, sin depender de orden de registro), y acepta un delegado opcional de configuración (`ThemeManagerPersistenceOptions`: duración de cachés, carpetas de subida, tamaño máximo de archivo, tipos de contenido permitidos). Paso a paso completo, incluida la integración con `MudThemeManager`, en **[docs/INSTALLATION-AND-CONFIGURATION.md](docs/INSTALLATION-AND-CONFIGURATION.md)**.

## Integración con DbContext

La aplicación consumidora provee su propio `DbContext`. La librería no impone un `DbContext` propio ni entidades separadas que haya que sincronizar a mano: solo hay que exponer su modelo en el `OnModelCreating` del `DbContext` consumidor:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyThemeManagerPersistenceModel();
}
```

Esto aplica las 4 configuraciones de EF Core (`IEntityTypeConfiguration<T>`) que trae la librería para `ThemeCatalog`, `ThemeAsset`, `ThemePresent` y `ThemeTerm`. Declarar `DbSet<T>` para estas 4 entidades en el `DbContext` consumidor es opcional (EF Core las incluye en el modelo igual, por `ApplyConfiguration`), pero se recomienda para que las herramientas de migraciones (`dotnet ef migrations add`) las detecten de forma explícita y para poder consultarlas directamente si la app lo necesita.

Detalle completo (qué debe proveer la app, qué resuelve la librería internamente, y una diferencia de comportamiento importante entre registrar `AddDbContextFactory<TContext>()` vs `AddDbContext<TContext>()`) en **[docs/INSTALLATION-AND-CONFIGURATION.md](docs/INSTALLATION-AND-CONFIGURATION.md)**.

## Dependency Injection

Un único punto de entrada, `services.AddThemeManagerPersistence<TContext>(configureOptions)`, registra:

- Los 4 repositorios (`IThemeCatalogRepository`, `IThemeAssetRepository`, `IThemePresentRepository`, `IThemeTermRepository`) — capa de persistencia interna; el consumidor normalmente no los usa directamente (ver ARCHITECTURE.md).
- Los servicios de aplicación públicos: `IThemeCatalogService`, `IThemePresentService`, `IThemeFaviconService`, `IThemeLogoService`, `IThemeTermService`, `ITermService`, `IThemeManagerService`.
- `IMemoryCache` (si no estaba ya registrada).
- `ThemeManagerPersistenceOptions` como singleton.

Todos los registros usan `TryAddScoped`/`TryAddSingleton`: el consumidor puede reemplazar cualquier pieza (por ejemplo, su propio `ITermService`) registrándola antes o después de llamar a `AddThemeManagerPersistence`, sin depender de un orden implícito. `AddThemeManagerPersistenceLocalFileStorage()` es un método aparte y opcional para `IThemeFileStorageService` (no se activa automáticamente).

## Migraciones

La librería no incluye migraciones de EF Core propias (no es un `DbContext` independiente). Las migraciones se generan y aplican con las herramientas normales de EF Core, sobre el `DbContext` de la app consumidora, una vez que ese `DbContext` incluye el modelo de la librería (`ApplyThemeManagerPersistenceModel()`):

```
dotnet ef migrations add AddThemeManagerPersistence -c AppDbContext
dotnet ef database update -c AppDbContext
```

Como el modelo de EF Core que trae la librería (`DataAccess/Configurations/*`) reproduce exactamente lo que las convenciones de EF Core ya generaban a partir de las entidades (mismos nombres de tabla implícitos, misma clave única en `ThemeCatalog.Name`), si la app consumidora ya tenía estas tablas creadas por convención (por ejemplo, vía `EnsureCreatedAsync()`), aplicar el modelo explícito no debería requerir cambios de esquema.

## Documentación adicional

| Documento | Contenido |
|---|---|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Arquitectura de consumo detallada + inventario completo de la API pública (servicios, interfaces, DTOs, entidades, extensiones de DI, opciones, componentes, eventos, recursos estáticos). |
| [docs/INSTALLATION-AND-CONFIGURATION.md](docs/INSTALLATION-AND-CONFIGURATION.md) | Instalación paso a paso y configuración detallada (DI, DbContext, EF Core, almacenamiento de assets). |
| [docs/THEMES.md](docs/THEMES.md) | Obtener temas disponibles/activo/específico, seleccionar, activar, persistir cambios, recuperar tras reiniciar la app; diferencia entre `MudBlazor.ThemeManager` (editor) y esta librería (persistencia). |
| [docs/BRANDING.md](docs/BRANDING.md) | Modelo `ThemeAsset`/`ThemeAssetType` (logo, favicon); cómo obtener y dónde usar los recursos activos (tema, logo, favicon) — Layout, `MudThemeProvider`, AppBar, Login, Dashboard, `<head>` HTML. |
| [docs/ADMINISTRATION.md](docs/ADMINISTRATION.md) | Operaciones de administración (crear/activar/eliminar temas, assets, términos) vs. operaciones de lectura/runtime. |
| [docs/RUNTIME.md](docs/RUNTIME.md) | Qué se notifica en vivo hoy (eventos existentes) y qué no tiene mecanismo de notificación actualmente. |
| [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md) | Guía paso a paso de integración desde cero, con el ejemplo completo (basado en `samples/TestHost`, el único consumidor real y probado de esta librería hoy). |

## Ejemplo mínimo

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(o => o.UseSqlite("Data Source=app.db"));
builder.Services.AddThemeManagerPersistence<AppDbContext>();
builder.Services.AddThemeManagerPersistenceLocalFileStorage();
```

```csharp
// AppDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyThemeManagerPersistenceModel();
}
```

```razor
@* MainLayout.razor *@
@inject IThemeManagerService ThemeManagerService
@inject IThemeCatalogService ThemeCatalogService

<MudThemeProvider Theme="_theme.Theme" />

@code {
    private ThemeManagerTheme _theme = new();

    protected override async Task OnInitializedAsync()
    {
        var active = await ThemeCatalogService.GetActiveAsync();
        if (active?.ThemePresent?.JsonData is { } json)
        {
            _theme = JsonHelper.Deserialize<ThemeManagerTheme>(json) ?? new();
        }

        ThemeManagerService.OnThemeChanged += t => InvokeAsync(() =>
        {
            if (t is ThemeManagerTheme tmt) { _theme = tmt; StateHasChanged(); }
            return Task.CompletedTask;
        });
    }
}
```

El ejemplo completo, con favicon/logo dinámicos, administración y activación de temas, está en **[docs/GETTING-STARTED.md](docs/GETTING-STARTED.md)**.

## Observaciones

Estas observaciones describen comportamiento *actual* de la API que puede resultar poco intuitivo. No son bugs corregidos en esta etapa (esta etapa es solo documentación) — quedan señaladas aquí para que quien continúe el proyecto las tenga en cuenta:

- **Guardar un tema editado siempre crea un catálogo nuevo.** `IThemePresentService` solo tiene `CreateAsync`, no un `UpdateAsync`/`Save` que sobrescriba el `ThemePresent` de un catálogo existente. En la práctica, "guardar cambios" sobre un tema ya existente (vía `ThemePaletteSelector.razor` / `IThemeCatalogService.CreateWithThemePresentAsync`) siempre da de alta un catálogo **nuevo**, con nombre distinto — no hay forma soportada de sobrescribir en el lugar la configuración visual de un catálogo ya guardado. Ver [docs/THEMES.md](docs/THEMES.md).
- **No existe una consulta directa "obtener un tema por Id".** `IThemeCatalogService` no tiene `GetByIdAsync(int id)`; para obtener un catálogo específico hay que filtrar el resultado de `GetAllAsync()`, o (si solo se necesita su configuración visual) usar `IThemePresentService.GetByThemeIdAsync(id)`.
- **La invalidación del caché de terminología es responsabilidad manual del llamador.** `ITermService.InvalidateCache()` no se llama automáticamente desde `ThemeTermService.CreateTermsAsync`/`UpdateTermsAsync`/`DeleteTermsAsync`. Si se construye una UI de administración de términos distinta al componente `ThemeTermConfig.razor` que trae la librería (que sí la invoca), hay que recordar llamarla; de lo contrario los cambios tardan hasta `ThemeManagerPersistenceOptions.TermCacheDuration` (30 min por defecto) en reflejarse en `ITermService`.
- **"Branding" y "recurso activo tipo Icono" no son APIs propias.** No existe un `IBrandingService` ni un `ThemeAssetType.Icon`: "Branding" es la agrupación conceptual de Logo + Favicon (los únicos dos valores de `ThemeAssetType` hoy), y "Favicon" cubre también el caso de uso de "ícono" de la aplicación — no hay un tercer tipo de asset separado.
- **No hay eventos de cambio para logo/favicon/términos**, solo para el tema (`IThemeManagerService.OnThemeChanged`, en memoria/antes de guardar) y para la activación de un catálogo (`IThemeCatalogService.ThemeCatalogActivated`, después de guardar). Ver [docs/RUNTIME.md](docs/RUNTIME.md).
- **El comportamiento de `SaveChangesAsync` de los repositorios depende de cómo se registró el `DbContext`.** Si la app usa `AddDbContextFactory<TContext>()` (recomendado, el patrón que usa `samples/TestHost`), cada operación de escritura de la librería guarda inmediatamente. Si en cambio la app registra su `TContext` como scoped externo (sin factory), la librería usa ese contexto pero **no** llama a `SaveChangesAsync` por sí misma — la app es responsable de persistir. Ver [docs/INSTALLATION-AND-CONFIGURATION.md](docs/INSTALLATION-AND-CONFIGURATION.md).
