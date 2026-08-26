# Guía de integración desde cero

Esta guía integra `SAMACDX.ThemeManager.Persistence` en una aplicación Blazor Server nueva, paso a paso, basada en `samples/TestHost` (el único consumidor real de esta librería probado hasta ahora) — los fragmentos siguen el mismo patrón que usa ese proyecto (mismas APIs, mismo orden de registro), con nombres genéricos y, en algún punto puntual, una recomendación levemente distinta (por ejemplo, versiones de paquete fijadas en vez de comodín flotante, siguiendo la práctica ya adoptada en la propia librería). El paso 7 señala explícitamente el único caso donde agrega una pieza que `samples/TestHost` hoy no implementa (la carga inicial del tema activo), compuesta a partir de APIs públicas reales de la librería (`GetActiveAsync()` + `JsonHelper`).

## 1. Instalar las dependencias

En el `.csproj` de la aplicación (Blazor Server, `Microsoft.NET.Sdk.Web`):

```xml
<ItemGroup>
  <PackageReference Include="MudBlazor" Version="[8.0.0,9.0.0)" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="[9.0.0,10.0.0)" />
  <!-- o el proveedor de EF Core que corresponda -->
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\SAMACDX.MudBlazor.ThemeManager.Persistence.csproj" />
</ItemGroup>
```

`MudBlazor` no necesita registrarse dos veces: si ya se referencia desde `SAMACDX.MudBlazor.ThemeManager.Persistence.csproj`, agregarlo también aquí duplica sus *Static Web Assets* y puede romper `app.MapStaticAssets()` sirviendo `MudBlazor.min.css`/`.js` — referenciarlo solo en la librería y dejar que llegue de forma transitoria.

Confirmar que `MudBlazor.ThemeManager` (el submódulo/fork del que depende la librería) está disponible — ver [INSTALLATION-AND-CONFIGURATION.md](INSTALLATION-AND-CONFIGURATION.md#el-submódulo-mudblazorthememanager).

## 2. Registrar la librería

En `Program.cs`:

```csharp
using SAMACDX.ThemeManager.Persistence.Extensions;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddThemeManagerPersistence<AppDbContext>();

builder.Services.AddThemeManagerPersistenceLocalFileStorage();
// -- o bien, una implementación propia --
// builder.Services.AddScoped<IThemeFileStorageService, MiAlmacenamiento>();
```

## 3. Integrar su DbContext

```csharp
using SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog;
using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using SAMACDX.ThemeManager.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Recomendado (no obligatorio, ver DBCONTEXT-AND-MIGRATIONS.md):
    public DbSet<ThemeCatalog> ThemeCatalogs => Set<ThemeCatalog>();
    public DbSet<ThemeAsset> ThemeAssets => Set<ThemeAsset>();
    public DbSet<ThemePresent> ThemesPresent => Set<ThemePresent>();
    public DbSet<ThemeTerm> ThemeTerms => Set<ThemeTerm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyThemeManagerPersistenceModel();
    }
}
```

## 4. Configurar Entity Framework

Ya quedó configurado en el paso 2 (`AddDbContextFactory<AppDbContext>`) y en el paso 3 (`ApplyThemeManagerPersistenceModel()`). No hace falta ninguna configuración adicional de EF Core específica de esta librería — el resto es la configuración normal de EF Core de la aplicación (proveedor, cadena de conexión).

## 5. Ejecutar las migraciones

```
dotnet ef migrations add AddThemeManagerPersistence -c AppDbContext
dotnet ef database update -c AppDbContext
```

> Nota: `samples/TestHost` (el proyecto de prueba interno de esta librería) no usa migraciones — llama a `EnsureCreatedAsync()` sobre una base SQLite vacía al arrancar, por simplicidad de un entorno de prueba descartable. Para una aplicación real, con una base de datos que evoluciona con el tiempo, el camino soportado y recomendado es el de arriba (`dotnet ef migrations`), no `EnsureCreatedAsync()`.

## 6. Obtener el Theme activo

```csharp
@inject IThemeCatalogService ThemeCatalogService

var activo = await ThemeCatalogService.GetActiveAsync();
ThemeManagerTheme? tema = activo?.ThemePresent?.JsonData is { } json
    ? JsonHelper.Deserialize<ThemeManagerTheme>(json)
    : null;
```

## 7. Aplicarlo a MudBlazor

En el `Layout` raíz (`MainLayout.razor`):

```razor
@inherits LayoutComponentBase
@inject IThemeManagerService ThemeManagerService
@inject IThemeCatalogService ThemeCatalogService

<MudThemeProvider Theme="_themeManagerTheme.Theme" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudText Typo="Typo.h5">Mi aplicación</MudText>
    </MudAppBar>
    <MudMainContent Class="pa-4">
        @Body
    </MudMainContent>
</MudLayout>

@code {
    private ThemeManagerTheme _themeManagerTheme = new();

    protected override async Task OnInitializedAsync()
    {
        var activo = await ThemeCatalogService.GetActiveAsync();
        if (activo?.ThemePresent?.JsonData is { } json)
        {
            _themeManagerTheme = JsonHelper.Deserialize<ThemeManagerTheme>(json) ?? new();
        }
    }

    protected override void OnInitialized()
    {
        ThemeManagerService.OnThemeChanged -= ThemeChangedHandler;
        ThemeManagerService.OnThemeChanged += ThemeChangedHandler;
    }

    private Task ThemeChangedHandler(object newTheme) => InvokeAsync(() =>
    {
        if (newTheme is ThemeManagerTheme theme)
        {
            _themeManagerTheme = theme;
            StateHasChanged();
        }
    });
}
```

Con esto, cualquier página de la aplicación (AppBar, Login, Dashboard, lo que sea) hereda automáticamente la paleta activa a través del `MudThemeProvider` ambiental, sin tener que leer el tema por su cuenta — y se actualiza en vivo mientras un administrador edita colores desde `ThemePaletteSelector`.

> **Nota sobre fidelidad con `samples/TestHost`**: el bloque `OnInitializedAsync` de arriba (cargar `GetActiveAsync()` en `_themeManagerTheme` al iniciar) **no existe hoy en el `MainLayout.razor` real de `samples/TestHost`** — ese proyecto de prueba solo suscribe `OnThemeChanged` (la parte de `OnInitialized`), y arranca siempre con un `ThemeManagerTheme` por defecto hasta que alguien edita la paleta en vivo. Es una omisión conocida de ese sample (documentada como tal en su historial), no una limitación de la librería: `GetActiveAsync()` sí devuelve el tema guardado correctamente. Se incluye aquí porque sin ella el paso "recuperar el tema después de reiniciar la aplicación" (ver [THEMES.md](THEMES.md)) no se refleja en el `MudThemeProvider` del layout.

## 8. Obtener el Logo activo

```csharp
@inject IThemeLogoService ThemeLogoService

string logoPath = await ThemeLogoService.GetCurrentLogoPathAsync();
```

```razor
<MudImage Src="@(string.IsNullOrEmpty(logoPath) ? ThemeDefaultAssets.DefaultLogoPath : logoPath)" Height="40" />
```

## 9. Obtener el Favicon activo

```csharp
@inject IThemeFaviconService ThemeFaviconService

string faviconPath = await ThemeFaviconService.GetCurrentFaviconPathAsync();
```

Para reflejarlo en el `<head>` del documento, usar el mecanismo de cabecera de Blazor desde un componente (por ejemplo, dentro del propio `MainLayout` o `Routes.razor`), ya que el `App.razor` estático no puede resolver datos de forma asíncrona:

```razor
<HeadContent>
    <link rel="icon" type="image/x-icon" href="@(string.IsNullOrEmpty(faviconPath) ? ThemeDefaultAssets.DefaultFaviconPath : faviconPath)" />
</HeadContent>
```

(Requiere que `App.razor` tenga `<HeadOutlet @rendermode="InteractiveServer" />` en su `<head>`, que es la configuración estándar de una plantilla Blazor Server con render interactivo.)

## 10. Obtener el Branding activo

No hay un único método "branding" — se obtienen logo y favicon por separado (pasos 8 y 9) y, si se necesita, también los `ThemeAssets` completos del tema activo:

```csharp
var activo = await ThemeCatalogService.GetActiveAsync();
var assetsDeBranding = activo?.ThemeAssets; // logo(s) y favicon(s) activos de ese catálogo
```

## 11. Obtener los temas disponibles

```csharp
@inject IThemeCatalogService ThemeCatalogService

List<ThemeCatalog> temas = await ThemeCatalogService.GetAllAsync();
```

## 12. Activar un tema

```csharp
await ThemeCatalogService.ActivateAsync(idDelTemaElegido);
```

## 13. Persistir cambios

```csharp
var nuevo = await ThemeCatalogService.CreateWithThemePresentAsync(
    new ThemeCatalog { Name = "Tema institucional" },
    new ThemePresent { JsonData = JsonHelper.Serialize(temaEditado) }
);
```

Recordar (ver [THEMES.md](THEMES.md)): esto siempre crea un catálogo **nuevo**; no hay una operación de "actualizar" el `ThemePresent` de un catálogo ya existente.

## Montar la pantalla de administración

En vez de reconstruir la UI de administración a mano, se puede montar directamente el componente compuesto que trae la librería en una página propia:

```razor
@* Pages/AdministrarTema.razor *@
@page "/admin/tema"
@using SAMACDX.ThemeManager.Persistence.Components.Theme

<ThemeConfig />
```

`ThemeConfig` no declara su propia ruta — el consumidor decide en qué página montarlo (con la restricción de acceso que su aplicación necesite, por ejemplo autorización de administrador, que la librería no impone).

## Resumen: qué queda en cada proyecto

Con los 13 pasos anteriores completos, la frontera queda así (ver también [ARCHITECTURE.md](ARCHITECTURE.md)):

- `MudBlazor.ThemeManager`: el editor visual, usado internamente por `ThemePaletteSelector.razor` — la aplicación no lo referencia directamente salvo por su hoja de estilos en `App.razor`.
- `SAMACDX.ThemeManager.Persistence`: toda la persistencia/activación/branding — la aplicación solo inyecta sus interfaces (`IThemeCatalogService`, `IThemeLogoService`, `IThemeFaviconService`, etc.) y, opcionalmente, monta sus componentes de administración.
- La aplicación consumidora: su `DbContext` con el modelo aplicado, su almacenamiento de archivos, y el cableado del `Layout`/`AppBar`/`<head>` a los recursos activos.
