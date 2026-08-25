# SAMACDX.MudBlazor.ThemeManager.Persistence — notas de migración

Extracción 1:1 del módulo de Theme/Branding de GDIP
(`C:\laragon\www\Work\GDIP\Components\Features\Theme` y sus contrapartes en
`GDIP.Domain`/`GDIP.Application`/`GDIP.Infrastructure`) hacia esta librería
reutilizable. **GDIP todavía NO consume esta librería** — esa integración
se hace en un paso posterior, deliberadamente fuera del alcance de esta
migración.

## Qué se migró

- **Entidades** (`Entities/`): `ThemeCatalog`, `ThemeFavicon`, `ThemeLogo`,
  `ThemePresent` (namespace `...Entities.ThemeCatalog`, tal como en GDIP) y
  `ThemeTerm` (namespace `...Entities.Theme` — la inconsistencia de
  namespaces respecto a las otras 4 entidades ya existía en el original y
  se conservó tal cual). `AuditableEntity` (`Entities/Abstracts/`) es una
  copia propia de la librería (ver "Decisiones" más abajo).
- **Interfaces y repositorios genéricos** (`Interfaces/Repositories/`,
  `Repositories/`): mismo patrón `IGenericRepository<T>` /
  `GenericRepository<T>` de GDIP, con un cambio: ahora son genéricos sobre
  `TContext : DbContext` en vez de asumir `ApplicationDbContext` de GDIP.
- **Servicios** (`Interfaces/Services/`, `Services/`): `ITermService`,
  `IThemeCatalogService`, `IThemeFaviconService`, `IThemeLogoService`,
  `IThemeManagerService`, `IThemePresentService`, `IThemeTermService` y sus
  implementaciones — lógica idéntica a GDIP. Se conservaron las
  inconsistencias de namespace originales (algunos servicios en
  `Services.Theme`, otros en `Services` a secas, y `ThemeManagerService`
  sin namespace/global, igual que en GDIP).
- **Seeders** (`Persistence/Seeders/Themes/`): mismos 5 seeders, mismo
  orden de siembra documentado abajo, mismos datos por defecto
  (incluyendo el tema visual y los ~35 términos en español).
- **Utilidades** (`Utilities/`): `SpanishArticleHelper` (artículos en
  español para la terminología) y `JsonHelper` (wrapper de
  `System.Text.Json`), ambos genéricos, sin dependencias de GDIP.
- **Componentes Razor** (`Components/Theme/`): `ThemeConfig` (con
  `@page "/ThemeCatalog"`), `ThemeFaviconAndLogoConfig`,
  `ThemePaletteSelector`, `ThemeTermConfig`, `ComponentsPreview` — UI
  idéntica, sólo se actualizaron los `@using` a los namespaces de la
  librería (y se quitaron dos `@using` que ya estaban sin uso en el
  original: `GDIP.Domain.Interfaces.Services` y
  `GDIP.Infrastructure.Interfaces.Services`).
- **MudBlazor.ThemeManager** (`External/MudBlazor.ThemeManager/`, git
  submodule): el mismo fork y el mismo commit que usa GDIP —
  `https://github.com/SAMACDX/ThemeManager.git`, rama
  `gdip-theme-manager`, commit `18ab1c397...` (agrega
  `ThemePresetsChanged`/`OnClickActiveThemePresent`/
  `IsSavingActiveThemeCatalog` a `MudThemeManager`, funcionalidad de la que
  depende `ThemePaletteSelector`). **No** es el paquete NuGet estándar de
  MudBlazor — es este fork específico.

## Orden de siembra (seeders)

Igual que `SeedManager.SeedThemes()` en GDIP:
`ThemeCatalogsSeeder` → `ThemesPresentSeeder` → `ThemeTermsSeeder` →
`ThemeFaviconsSeeder` → `ThemeLogosSeeder`.

## Decisiones de desacople (dependencias específicas de GDIP)

1. **`IUnitOfWork` de GDIP** (un god-object que agrega repositorios de
   TODA la aplicación — Audit, Conversation, Documents, etc.) no se migró:
   los servicios de este módulo dependían de él sólo para llegar a los 5
   repositorios de Theme. Ahora los servicios inyectan directamente
   `IThemeCatalogRepository`, `IThemeFaviconRepository`,
   `IThemeLogoRepository`, `IThemePresentRepository`,
   `IThemeTermRepository`. Es el cambio estructural más grande respecto al
   original, pero es mecánico (mismo cuerpo de métodos, sólo cambia de
   dónde viene el repositorio).
2. **`IFileStorageService` de GDIP** es una interfaz grande acoplada a
   DTOs de otra feature (`Communication.Conversation`). Sólo se usaba UN
   método desde Theme: `SaveFileAsync(IBrowserFile, string folder)`
   (verificado por grep — `SaveFaviconAsync`/`GetFileAsBase64Async` nunca
   se llaman desde el módulo Theme). Se creó
   `IThemeFileStorageService` con ese único método. La app consumidora
   debe implementarlo (lo más simple: que su `FileStorageService`
   existente implemente también esta interfaz, ya que la firma coincide
   exactamente — cero cambio de comportamiento).
3. **`GenericRepository<TEntity>`** pasó a ser
   `GenericRepository<TEntity, TContext>` (`TContext : DbContext`) para no
   asumir el `ApplicationDbContext` de GDIP. La app consumidora indica su
   propio DbContext vía el genérico al llamar a
   `AddThemeManagerPersistence<TContext>()`.
4. **Seeders**: firma cambiada de `SeedAsync(ApplicationDbContext context)`
   a `SeedAsync(DbContext context)`, usando `context.Set<T>()` en vez de
   propiedades `DbSet` con nombre — funciona igual pasando cualquier
   DbContext derivado (incluido el de GDIP, que es un `ApplicationDbContext
   : DbContext`).
5. **`AuditableEntity`**: la librería tiene su propia copia (idéntica:
   `CreatedAt`, `UpdatedAt`, `CreatedByUserId`, `UpdatedByUserId`) en vez
   de depender de `GDIP.Domain.Entities.Abstracts.AuditableEntity`. **Esto
   es importante para cuando se integre con GDIP**: el
   `ApplyAuditInfo()` de `ApplicationDbContext` en GDIP sólo escanea
   `ChangeTracker.Entries<GDIP...AuditableEntity>()`; para que las
   entidades de Theme sigan recibiendo `CreatedAt`/`UpdatedAt`
   automáticamente, ese método deberá también escanear
   `ChangeTracker.Entries<SAMACDX...AuditableEntity>()`. Ninguna entidad
   ni tabla cambia de nombre, así que esto no debería requerir una
   migración de EF Core nueva — pero **debe verificarse con
   `dotnet ef migrations add` al integrar**, ya que no fue posible
   ejecutar el compilador/CLI de .NET en esta sesión (no había `dotnet`
   disponible ni en este equipo ni en el contenedor de la tarea).
6. **Enum `EntityType`, permisos (`AppPermissions`/`RolePermissions`),
   `TermDisplay`/`TermArticle`**: se quedan en GDIP. Son parte del sistema
   de autorización y de un componente de UI específicos de GDIP, no del
   módulo Theme/Branding en sí; consumen la librería a través de sus
   interfaces públicas (`ITermService`, `IThemeLogoService`, etc.), no
   necesitan vivir dentro de ella.

## Qué falta (fuera de alcance de esta tarea, por instrucción explícita del usuario)

- Modificar GDIP para referenciar y consumir esta librería.
- Eliminar la implementación antigua dentro de GDIP.
- Verificación de compilación / build real (no hay `dotnet` disponible en
  esta sesión — ni en este equipo ni en el contenedor de la tarea — así
  que no se pudo correr `dotnet build` ni `dotnet ef migrations add`).

## Uso previsto por una app consumidora (referencia para la integración futura)

```csharp
// Program.cs / DI de la app consumidora
services.AddThemeManagerPersistence<ApplicationDbContext>();

// La app debe:
// 1) Exponer DbSet<T> (o Set<T>()) para ThemeCatalog, ThemeFavicon, ThemeLogo,
//    ThemePresent y ThemeTerm en su propio DbContext.
// 2) Implementar IThemeFileStorageService (o agregarla a su servicio de
//    almacenamiento de archivos existente).
// 3) Registrar IDbContextFactory<TContext> como ya lo hace para EF Core.
// 4) Referenciar el assembly de esta librería en el descubrimiento de
//    componentes Razor (p. ej. AddAdditionalAssemblies) para que la ruta
//    "/ThemeCatalog" y los demás componentes se resuelvan.
```

## Corrección post-migración: colisión de namespace con MudBlazor.ThemeManager

Al intentar compilar por primera vez (`dotnet build` desde Visual Studio del
usuario) aparecieron errores CS0246/CS0234 en los tres componentes que usan
tipos de `MudBlazor` / `MudBlazor.ThemeManager`
(`ISnackbar`, `ThemeManagerTheme`, `MudThemeManager`, `ThemePreset`).

Causa: el namespace raíz original del proyecto,
`SAMACDX.MudBlazor.ThemeManager.Persistence`, contiene literalmente
`MudBlazor.ThemeManager` como segmentos anidados. La resolución de
namespaces de C# busca primero en los namespaces envolventes antes que en
el global; como `SAMACDX.MudBlazor.ThemeManager` "existe" (es el propio
namespace de esta librería), cualquier referencia sin calificar completa a
`MudBlazor` o `MudBlazor.ThemeManager` dentro de la librería se resolvía
contra ESE namespace (vacío de esos tipos) en vez del namespace global real
del paquete MudBlazor y del submódulo MudBlazor.ThemeManager.

Corrección: el `RootNamespace` del `.csproj` (y por lo tanto el namespace
de todo el código fuente) se cambió a `SAMACDX.ThemeManager.Persistence`
(sin el segmento `MudBlazor`). El nombre del proyecto/ensamblado/paquete
(`AssemblyName`, nombre del `.csproj`, nombre del repo/carpeta) se mantiene
como `SAMACDX.MudBlazor.ThemeManager.Persistence` — sólo cambió el
namespace de C#, que es un detalle interno y no afecta el nombre público de
la librería.

## Corrección post-migración #2: RZ9985 "Multiple components use the tag..."

Segundo error de build reportado por el usuario, tras corregir el namespace:
`RZ9985: Multiple components use the tag 'MudThemeManager'` (y
`MudThemeManagerColorItem`), ambos listados dos veces con el mismo nombre
completo `MudBlazor.ThemeManager.MudThemeManager`.

Causa: el `.csproj` de esta librería (SDK `Microsoft.NET.Sdk.Razor`) no
excluía la carpeta `External\` de su propio globbing por defecto. Como
`External\MudBlazor.ThemeManager` es un proyecto aparte (referenciado vía
`ProjectReference`), sus archivos `.razor`/`.cs` se estaban compilando DOS
veces: una vez como parte de su propio ensamblado, y otra vez directamente
dentro de esta librería (por el glob implícito `**/*.razor`), generando dos
tipos de componente con el mismo nombre.

Corrección: se agregó al `.csproj` el mismo `ItemGroup` de exclusión que ya
usa el propio `GDIP.csproj` para esta misma carpeta submódulo
(`Compile/Content/EmbeddedResource/None Remove="External\**"`).

## App de prueba: samples/TestHost

Se agregó `samples/TestHost/`, una app Blazor Server mínima (SDK
`Microsoft.NET.Sdk.Web`, SQLite vía `Microsoft.EntityFrameworkCore.Sqlite`)
que consume esta librería igual que lo haría una app host real, sin
depender de GDIP en absoluto. Sirve para que el usuario pueda probar el
módulo Theme/Branding directamente (crear/activar temas, subir
favicon/logo, editar terminología) y confirmar que la persistencia
sobrevive a un reinicio.

Piezas del sample:

- `TestDbContext` — un `DbContext` mínimo con `DbSet<T>` para las 5
  entidades de Theme, pasado como `TContext` a
  `AddThemeManagerPersistence<TContext>()`.
- `LocalFileStorageService` — implementación mínima de
  `IThemeFileStorageService` (equivalente a lo que hace
  `FileStorageService.SaveFileAsync` en GDIP), escribe a `wwwroot/{folder}`.
- `Program.cs` — registra Razor Components + modo interactivo server,
  `AddMudServices()`, el `DbContextFactory<TestDbContext>` con SQLite,
  `AddThemeManagerPersistence<TestDbContext>()`, siembra los datos por
  defecto (mismo orden que GDIP: Catalogs → Present → Terms → Favicons →
  Logos) contra `EnsureCreatedAsync()`, y expone las rutas de la librería
  vía `AddAdditionalAssemblies(typeof(ServiceCollectionExtensions).Assembly)`.
- Ver `samples/TestHost/README.md` para instrucciones de ejecución.

No requiere ni toca GDIP. No es un cambio a la librería en sí — es solo un
arnés de prueba adicional, tal como fue solicitado explícitamente por el
usuario.

## Corrección post-migración #3: CS0579 duplicados + CS0234 al agregar samples/TestHost

Tercer error de build reportado por el usuario, esta vez tras agregar
`samples/TestHost/`: 35 errores, entre ellos `CS0579: Duplicate ... attribute`
sobre los `AssemblyInfo.cs`/`AssemblyAttributes.cs` generados del propio
proyecto de la librería, `CS0234` para `Builder`/`Hosting`/`Http`/
`Authorization`/`Components` (namespaces de ASP.NET Core Web), `CS0246` para
`IWebHostEnvironment`, y de nuevo `CS0234`/`CS0246` para `MudThemeManager`/
`ISnackbar`/`ThemeManagerTheme` en `ThemePaletteSelector.razor` y
`ThemeFaviconAndLogoConfig.razor`.

Causa: exactamente la misma raíz que la corrección #2 (RZ9985), pero para
`samples\`: el `.csproj` de la librería excluía `External\**` de su propio
globbing por defecto, pero no `samples\**`. Como resultado, el SDK globbing
por defecto (`Microsoft.NET.Sdk.Razor`) compilaba TAMBIÉN, dentro del
ensamblado de la librería, todos los `.cs`/`.razor` de
`samples\TestHost\` -incluyendo archivos generados bajo su propio
`obj\Debug\net9.0\**` de una compilación previa (AssemblyInfo, GlobalUsings)-
mezclando dos proyectos con configuraciones incompatibles (la librería es
`Sdk.Razor` sin referencia al framework compartido de ASP.NET Core; TestHost
es `Sdk.Web` con esa referencia) dentro de un mismo ensamblado. Esto generaba
atributos de ensamblado duplicados, tipos ASP.NET Core Web no resueltos, y
-por la compilación cruzada resultante- confundía nuevamente la resolución
de namespaces de MudBlazor/MudBlazor.ThemeManager en archivos de la propia
librería que ya estaban correctos.

Corrección: se agregó al mismo `ItemGroup` de exclusión del `.csproj` de la
librería las entradas `Compile/Content/EmbeddedResource/None
Remove="samples\**"`, análogas a las ya existentes para `External\**`.
También se eliminaron las carpetas `obj\`/`bin\` (de la librería y de
`samples/TestHost`) que quedaron con artefactos generados antes de esta
corrección, para evitar builds incrementales corruptos: **si el error
persiste tras actualizar, borrar manualmente `obj\` y `bin\` en la raíz del
repo y en `samples\TestHost\` antes de reconstruir.**
