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

## Corrección post-migración #4: CS0103 (Typo/Variant/Color) + CS0104 en samples/TestHost

Cuarto error de build reportado, esta vez ya con la librería compilando
bien y solo `samples/TestHost` fallando: `CS0103` para `Typo`, `Variant`,
`Color` (enums de MudBlazor) en `Routes.razor`, `MainLayout.razor` y
`Home.razor`, y `CS0104: 'ServiceCollectionExtensions' is an ambiguous
reference` en `Program.cs`.

Causa 1 (Typo/Variant/Color): el mismo problema que la corrección #1, pero
en el `RootNamespace` de **TestHost**, que seguía siendo
`SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost` — con "MudBlazor"
como segmento literal anidado, lo que sombreaba el namespace global real
`MudBlazor` (de donde vienen `Typo`/`Variant`/`Color`) pese al
`@using MudBlazor` en `_Imports.razor`.

Corrección: `RootNamespace` de TestHost → `SAMACDX.ThemeManager.Persistence.TestHost`
(análogo al de la librería), actualizando todas las referencias
`using SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost...` en
`Routes.razor`, `_Imports.razor`, `TestDbContext.cs`, `Program.cs` y
`LocalFileStorageService.cs`.

Causa 2 (ambigüedad): `Program.cs` tiene tanto
`using SAMACDX.ThemeManager.Persistence.Extensions;` (con la clase
`ServiceCollectionExtensions` de la librería) como `using MudBlazor.Services;`
(que también define una clase `ServiceCollectionExtensions`), por lo que
`typeof(ServiceCollectionExtensions)` sin calificar era ambiguo.

Corrección: se calificó completamente como
`typeof(SAMACDX.ThemeManager.Persistence.Extensions.ServiceCollectionExtensions)`.

De nuevo se borraron `obj\`/`bin\` de `samples/TestHost` tras el cambio de
`RootNamespace` para evitar artefactos incrementales corruptos.

## Corrección post-migración #5: excepción en runtime "constructors are ambiguous"

Con el build ya en verde (librería y TestHost compilando sin errores), el
usuario reportó una excepción al ejecutar la app: `InvalidOperationException:
Unable to activate type 'ThemeLogoRepository<TestDbContext>'. The following
constructors are ambiguous: .ctor(IDbContextFactory<TestDbContext>) y
.ctor(TestDbContext)`.

Causa: cada `Theme*Repository<TContext>` tiene, por diseño (para soportar
tanto un `IDbContextFactory<TContext>` como un `TContext` externo ya
gestionado por el host - ver comentario en `ServiceCollectionExtensions`),
dos constructores. `AddThemeManagerPersistence<TContext>()` registraba estos
repositorios directamente (`services.AddScoped<TService, TImpl>()`), dejando
que el contenedor de DI de ASP.NET Core eligiera el constructor por
reflexión. Ese contenedor lanza "ambiguous" en cuanto AMBOS constructores
resultan resolvibles - y eso ocurre con `AddDbContextFactory<TContext>()` en
EF Core 8+, que además de `IDbContextFactory<TContext>` deja también
`TContext` resolvible como scoped.

Corrección: en `Extensions/ServiceCollectionExtensions.cs`, cada repositorio
ahora se resuelve explícitamente mediante un `CreateRepository<TRepo,
TContext>(...)` que prefiere `IDbContextFactory<TContext>` cuando está
registrada, y si no, cae al `TContext` externo - sin dejar la elección de
constructor al contenedor. No se tocaron las clases de los repositorios ni
`GenericRepository`.

## Corrección post-migración #6: 500 al servir el CSS/JS de MudBlazor en TestHost

El usuario reportó que la app ya corría pero no se aplicaba el estilo de
MudBlazor. En el Network tab del navegador se vio que
`_content/MudBlazor/MudBlazor.min.css` y `MudBlazor.min.js` devolvían
**500 Internal Server Error** (no 404), lo que además rompía por completo
el interop de JS de MudBlazor y terminaba tumbando el circuito de Blazor
("No interop methods are registered for renderer 1").

Causa: `samples/TestHost.csproj` tenía su propio
`<PackageReference Include="MudBlazor" ... />` DIRECTO, además de recibir
MudBlazor de forma transitiva a través del `ProjectReference` a la
librería (que ya la referencia). Con MudBlazor declarado en dos proyectos
del mismo árbol, `app.MapStaticAssets()` (el nuevo pipeline de static web
assets de .NET 9) encontraba dos fuentes para el mismo asset lógico
(`_content/MudBlazor/...`) y fallaba con 500 al intentar resolverlo/
servirlo.

Corrección: se quitó el `PackageReference` directo a MudBlazor del
`.csproj` de TestHost - sigue compilando y funcionando igual porque lo
recibe transitivamente vía el `ProjectReference` a la librería. Solo debe
existir una fuente del paquete en todo el árbol.

Nota aparte (no corregida, no reportada como bug de código): también se
vieron 404 en `/Uploads/favicons/default.svg`, `/Uploads/logos/default.svg`,
`/Uploads/logos/LogoCentrado.png` y un ícono en `/Uploads/icons/*.svg` -
son datos de los seeders (`ThemeFaviconsSeeder`/`ThemeLogosSeeder`) que
referencian nombres de archivo que existían físicamente en el `wwwroot` de
GDIP pero nunca se migraron como archivos de ejemplo a
`samples/TestHost/wwwroot/Uploads/`. Es solo un ícono roto en el sample;
no afecta la funcionalidad. Si se quiere, se pueden agregar imágenes de
placeholder ahí.

## Etapa: limpieza de código no utilizado (2026-08-26)

Revisión completa del proyecto `SAMACDX.MudBlazor.ThemeManager.Persistence`
(excluyendo el submódulo `External/MudBlazor.ThemeManager` y el sample
`samples/TestHost`, que no son parte del código migrado desde GDIP) en
busca de código sin consumidores dejado por la extracción. Se verificó cada
clase, interfaz, método, entidad, servicio, componente, extensión de DI,
helper, configuración de EF Core, dependencia NuGet, using y namespace
contra el resto de la solución (incluyendo `samples/TestHost` como
consumidor real) antes de decidir qué eliminar.

### Eliminado

- **`_Imports.razor`**: se quitaron 3 directivas `@using` sin consumidores:
  - `System.Net.Http` - ningún `HttpClient`/tipo de ese namespace se usa en
    ninguno de los 5 componentes de `Components/Theme/`.
  - `Microsoft.JSInterop` - no hay `@inject IJSRuntime`, `IJSObjectReference`
    ni `[JSInvokable]` en ningún componente.
  - `Microsoft.AspNetCore.Components.Web` - ningún tipo de ese namespace
    (`ChangeEventArgs`, `KeyboardEventArgs`, etc.) se referencia, y ningún
    componente usa directivas nativas de DOM (`@onclick`, `@bind` sobre
    `<input>`, etc.) que dependan de él en su código generado: todos los
    eventos van a través de parámetros propios de MudBlazor (`OnClick`,
    `FilesChanged`, `ValueChanged`), no de atributos HTML nativos.

### Revisado y conservado (con motivo)

- **Métodos "Guid" de `IGenericRepository<TEntity>`** (`GetByIdAsync(Guid)`,
  `ExistsAsync(Guid)`) y otros miembros del repositorio genérico sin
  consumidores actuales dentro de este módulo (`RemoveAsync`,
  `RemoveRangeAsync`, `UpdateWhereAsync`, `CountAsync`, etc.): son parte de
  un contrato genérico reutilizable, extraído literalmente del
  `IGenericRepository` original de GDIP (que sí los usa con otras
  entidades de clave `Guid` en ese proyecto). No son residuos de la
  extracción sino capacidad general intencional de la abstracción; quitar
  métodos de una interfaz pública es además un cambio de contrato, fuera
  del alcance de esta etapa.
- **`IThemeManagerService.OnThemeChanged`**: nadie se suscribe a este
  evento dentro de la librería ni en `samples/TestHost`, pero es el punto
  de integración público que una app host real (como GDIP) usa para
  reaccionar a cambios de tema en vivo. No es código muerto, es una
  extensión pensada para el consumidor.
- **Todas las entidades, interfaces, repositorios, servicios y los 5
  componentes de `Components/Theme/`**: se verificó que cada uno tiene al
  menos un consumidor real (registro en DI, inyección en un componente, o
  uso en `samples/TestHost`).
- **Los 5 `PackageReference` del `.csproj`**: los 5 se usan activamente
  (`Microsoft.AspNetCore.Components.Web` provee el ensamblado donde vive
  `Microsoft.AspNetCore.Components.Forms.IBrowserFile`, que sí se usa,
  aunque el namespace `.Web` en sí ya no se importe).
- **Namespace duplicado `Persistence.Persistence.Seeders.Themes`** (en los
  5 seeders): es una rareza heredada de la extracción, pero corregirla
  implicaría un rename de namespace, fuera del alcance de esta etapa
  (no renombrar). Queda documentada aquí para una futura limpieza si se
  solicita explícitamente.
- No se encontraron clases, DTOs, entidades, servicios no registrados,
  configuraciones de EF Core huérfanas, archivos duplicados/de respaldo, ni
  referencias funcionales a GDIP (solo comentarios de documentación que
  explican el origen del código, que sí tienen razón válida para existir).

### Estado del build

No fue posible compilar la librería en esta sesión (sin `dotnet` CLI
disponible aquí, igual que en todas las correcciones anteriores) - el
cambio se limita a 3 líneas `@using` sin ningún tipo referenciado en el
código actual, verificado exhaustivamente por búsqueda de texto en toda la
solución. Pendiente de que el usuario compile localmente y confirme.

## Etapa: eliminación de funcionalidad sin propósito (2026-08-26)

Segunda etapa de limpieza, explícitamente solicitada por el usuario: a
diferencia de la etapa anterior (solo código sin consumidores), esta
elimina funcionalidad completa que dejó de tener propósito dentro de la
librería. **Nota**: las secciones "Qué se migró" y "Decisiones de
desacople" más arriba describen el estado ORIGINAL de la extracción (para
mantener el historial) - los puntos sobre `AuditableEntity` y los seeders
ya no reflejan el código actual; lo que sigue es lo vigente.

### 1. `AuditableEntity` eliminada

Se eliminó `Entities/Abstracts/AuditableEntity.cs` por completo (y la
carpeta `Entities/Abstracts/`, que quedó vacía). Los campos `CreatedAt`,
`UpdatedAt`, `CreatedByUserId`, `UpdatedByUserId` no eran usados por
ningún código de la librería (no hay lógica de auditoría tipo
`ApplyAuditInfo()` aquí; eso era una preocupación exclusivamente del lado
de GDIP, documentada como paso pendiente de integración). Se quitó
`: AuditableEntity` de las 5 entidades (`ThemeCatalog`, `ThemeFavicon`,
`ThemeLogo`, `ThemePresent`, `ThemeTerm`) y el `using` correspondiente. No
se reemplazó por ninguna otra abstracción de auditoría.

**Importante para una futura integración con GDIP**: el punto 5 de
"Decisiones de desacople" arriba ya no aplica - `ApplyAuditInfo()` de
GDIP no necesita (ni puede) escanear una `AuditableEntity` de esta
librería, porque ya no existe.

### 2. Seeders de Themes eliminados

Se eliminó `Persistence/Seeders/Themes/` completo (5 seeders) y la carpeta
`Persistence/`, que quedó vacía. La sección "Orden de siembra (seeders)"
arriba ya no aplica - la librería no siembra datos iniciales.

Como los seeders eran invocados desde `samples/TestHost/Program.cs` (el
único punto de "inicialización/startup" real en este repo, ya que la
librería en sí no tiene uno), se ajustó ese archivo para quitar el
`using` y las 5 llamadas `SeedAsync(db)`, dejando intacto el resto
(`EnsureCreatedAsync()` para crear el esquema SQLite sigue igual). No se
tocó la lógica que determina o provee el tema activo por defecto: cuando
no hay un `ThemeCatalog` con `IsActive = true` en la base de datos,
`ThemeCatalogService.GetActiveAsync()` sigue devolviendo `null` tal como
antes, y `ThemePaletteSelector` sigue usando su propio fallback
(`new ThemeManagerTheme()`, ya presente en el código desde la extracción
original) - ese es "el tema predeterminado definido por la implementación
actual" al que se refiere el pedido, no algo sembrado en la base de
datos.

### 3. `GenericRepository` reducido a lo que se usa

Se verificó, método por método, contra toda la solución (servicios de la
librería + `samples/TestHost` como consumidor real) qué operaciones de
`IGenericRepository<TEntity>` / `GenericRepository<TEntity, TContext>`
tenían al menos un consumidor. Se eliminaron los que no tenían ninguno,
tanto de la clase como de la interfaz (dejar el método solo en la
interfaz habría dejado un contrato roto, imposible de implementar sin
volver a agregar el método):

- `GetByIdAsync(Guid)` / `GetByIdAsync(int)`
- `AddRangeAsync(IEnumerable<TEntity>)`
- `UpdateWhereAsync(predicate, updates)` (y su único helper privado,
  `GetPropertyName<TPropertySource>`, que quedó sin propósito al
  eliminarse su único llamador)
- `RemoveAsync(TEntity)`
- `RemoveRangeAsync(IEnumerable<TEntity>)`
- `ExistsAsync(Guid)` / `ExistsAsync(int)`
- `CountAsync(predicate)`
- `Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null)`
  (la sobrecarga de composición por delegado; la otra sobrecarga,
  `Query(params Expression<Func<TEntity, object>>[] includes)`, sí se usa
  y se conservó)

Se conservaron sin cambios (todos con consumidores confirmados):
`GetAllAsync()`, `FindAsync(predicate)`, `FirstOrDefaultAsync(predicate)`,
`AddAsync(entity)`, `UpdateAsync(entity)`, `UpdateRangeAsync(entities)`,
`Query(params Expression<...>[] includes)`, y los helpers privados/
protegidos que estos sí usan (`UseContext`, `UseContextAsync` (ambas
sobrecargas), `UseExternalContext`, `FindPersistedEntityAsync`,
`GetPrimaryKey`, `IsMissingKeyValue`).

### Estado del build

No fue posible compilar en esta sesión (sin `dotnet` CLI disponible, como
en toda la migración) - se verificó exhaustivamente por búsqueda de texto
en toda la solución (librería + `samples/TestHost`) que no quedó ninguna
referencia rota a `AuditableEntity`, a los seeders eliminados, ni a los
métodos removidos de `GenericRepository`/`IGenericRepository`. Pendiente
de que el usuario compile localmente y confirme.

## Etapa: consolidación de `ThemeLogo`/`ThemeFavicon` en `ThemeAsset` (2026-08-26)

Refactor completo para unificar las entidades `ThemeLogo` y `ThemeFavicon`
(que eran clases casi idénticas: `Id`, `Name`, `Path`, `IsActive`,
`ThemeCatalogId`, `ThemeCatalog`) en una única entidad reutilizable
`ThemeAsset`, con un nuevo enum `ThemeAssetType` (`Logo`, `Favicon`) para
discriminar el tipo — sin usar strings libres.

### Entidades

- `Entities/ThemeCatalog/ThemeFavicon.cs` y `ThemeLogo.cs` **eliminadas**.
- `Entities/ThemeCatalog/ThemeAsset.cs` **nueva**: mismas propiedades que
  las entidades eliminadas más `public ThemeAssetType Type { get; set; }`.
- `Entities/ThemeCatalog/ThemeAssetType.cs` **nuevo** enum: `Logo`,
  `Favicon`.
- `ThemeCatalog.ThemeFavicons` + `ThemeCatalog.ThemeLogos` (dos
  `ICollection`) colapsados en una única `ICollection<ThemeAsset>
  ThemeAssets`.

### Repositorios

`IThemeFaviconRepository`/`ThemeFaviconRepository<TContext>` y
`IThemeLogoRepository`/`ThemeLogoRepository<TContext>` **eliminados**;
reemplazados por un único `IThemeAssetRepository`/
`ThemeAssetRepository<TContext>` (mismo patrón de doble constructor
`IDbContextFactory<TContext>` / `TContext` externo que ya usaban los
demás repositorios). `ServiceCollectionExtensions` registra ahora sólo
`IThemeAssetRepository` en vez de los dos anteriores.

### Servicios

Se conservaron `IThemeFaviconService`/`ThemeFaviconService` e
`IThemeLogoService`/`ThemeLogoService` como dos interfaces/clases
separadas (decisión explícita del pedido: los consumidores de alto nivel
no deben verse forzados a conocer `ThemeAssetType`), pero:

- Sus firmas ahora usan `ThemeAsset` en vez de `ThemeFavicon`/`ThemeLogo`.
- Ambas dependen internamente del mismo `IThemeAssetRepository` (antes
  cada una tenía su propio repositorio dedicado).
- Cada consulta (`GetAllByThemeCatalogIdAsync`, `ActivateAsync`,
  `GetCurrentLogoPathAsync`) agrega el filtro `t.Type ==
  ThemeAssetType.Favicon` / `.Logo` que antes era implícito (estaba dado
  por consultar una tabla separada).
- `CreateAsync` asigna `Type` internamente antes de persistir
  (`ThemeAssetType.Favicon` en `ThemeFaviconService`, `.Logo` en
  `ThemeLogoService`), de modo que ni los componentes Razor ni ningún
  otro consumidor necesitan tocar el enum.
- No se fusionaron ambas clases en una sola implementación genérica: eso
  habría introducido una arquitectura distinta a la existente, algo que
  el pedido excluyó explícitamente. La reutilización interna pedida
  ("evalúa si pueden reutilizar una única implementación basada en
  ThemeAssetType") se resolvió al nivel del repositorio, que es lo que
  antes estaba duplicado 1:1.
- `ThemeCatalogService.GetActiveAsync()` actualizado: ahora incluye
  `t.ThemeAssets` (antes `t.ThemeFavicons` + `t.ThemeLogos`) y filtra la
  colección resultante a `IsActive` de la misma forma que antes, sólo que
  sobre una única colección.
- La regla de activación no cambió de arquitectura (tal como se pidió):
  sigue siendo "activar el elegido, desactivar el resto del mismo grupo
  vía `ActivateAsync`", ahora con el grupo acotado por `ThemeCatalogId`
  + `Type` en vez de por tabla separada.

### UI / componentes

`Components/Theme/ThemeFaviconAndLogoConfig.razor`: se actualizaron sólo
las referencias de TIPO (`List<ThemeFavicon>` → `List<ThemeAsset>`,
`new ThemeFavicon { ... }` → `new ThemeAsset { ... }`, ídem para
`ThemeLogo`). Los nombres de campos/métodos/variables (`_themeFavicons`,
`_themeLogos`, `SaveFaviconAsync`, `_isActivatingThemeLogo`, etc.) se
dejaron intactos porque siguen describiendo correctamente su propósito y
renombrarlos no fue parte del pedido. No hubo cambios de markup/CSS/UX:
el usuario sigue pudiendo seleccionar/ver/activar logo y favicon por
separado, exactamente igual que antes.

`samples/TestHost/Data/TestDbContext.cs`: `DbSet<ThemeFavicon>
ThemeFavicons` + `DbSet<ThemeLogo> ThemeLogos` reemplazados por
`DbSet<ThemeAsset> ThemeAssets`.

`samples/TestHost/Components/Pages/Home.razor`: las dos líneas que leían
`_activeCatalog.ThemeFavicons`/`ThemeLogos` ahora filtran
`_activeCatalog.ThemeAssets` por `Type == ThemeAssetType.Favicon`/`.Logo`
(además de `IsActive`, igual que antes).

### Limpieza posterior

Se rebuscó toda la solución (librería + `samples/TestHost`, excluyendo
`bin`/`obj`/`.git`/`External`) por `ThemeFavicon`/`ThemeLogo`: las únicas
coincidencias restantes son el nombre del componente
`ThemeFaviconAndLogoConfig` (no forma parte del pedido, es el nombre del
componente UI, no de una entidad) y nombres de interfaces/servicios/
campos/métodos que se decidió explícitamente conservar
(`IThemeFaviconService`, `IThemeLogoService`, `_isSavingThemeFavicon`,
etc.). No quedó ninguna clase de compatibilidad, alias ni wrapper para
las entidades eliminadas. No existían `IEntityTypeConfiguration<T>` (el
modelado es 100% por convención, confirmado en la etapa de limpieza
anterior), DTOs separados para logo/favicon, ni migraciones EF Core
(el esquema SQLite de `samples/TestHost` se crea con
`EnsureCreatedAsync()`), así que no hubo nada que ajustar en esas capas.
Ningún seeder referenciaba estas entidades (los seeders de Theme ya
habían sido eliminados en la etapa de limpieza anterior).

### Estado del build

No fue posible compilar en esta sesión (sin `dotnet` CLI disponible, como
en toda la migración) — se verificó exhaustivamente por búsqueda de texto
en toda la solución que no quedó ninguna referencia rota a
`ThemeFavicon`/`ThemeLogo`/`IThemeFaviconRepository`/
`IThemeLogoRepository`/`ThemeFaviconRepository`/`ThemeLogoRepository`.
Pendiente de que el usuario compile localmente y confirme.
