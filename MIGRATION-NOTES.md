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

## Corrección: 404 en `/Uploads/favicons/default.svg` y `/Uploads/logos/default.svg` (2026-08-26)

### Investigación en GDIP

En GDIP, `Components/Features/Theme/ThemeFaviconAndLogoConfig.razor` tiene
exactamente los mismos dos valores hardcodeados como estado inicial de
`_faviconPreviewPath`/`_logoPreviewPath` (`"Uploads/favicons/default.svg"`
y `"Uploads/logos/default.svg"`). Se buscó en todo el repo de GDIP
(excluyendo `bin`/`obj`/`node_modules`/`.git`) un archivo físico llamado
`default.svg` bajo cualquier `Uploads/` — **no existe ninguno**. Lo que sí
existen son archivos subidos por usuarios con nombres aleatorios
(`wwwroot/Uploads/icons/adx1g43z.ka1.svg`,
`wwwroot/Uploads/logos/LogoCentrado.png`, etc.), y los seeders
`ThemeFaviconsSeeder`/`ThemeLogosSeeder` (`GDIP.Infrastructure/Persistences/
Seeders/Themes/`) insertan una fila `ThemeFavicon`/`ThemeLogo` activa cuyo
campo `Name` es literalmente `"default.svg"` pero cuyo `Path` apunta a esos
archivos reales (`/Uploads/icons/adx1g43z.ka1.svg`,
`/Uploads/logos/LogoCentrado.png`) — nunca a un archivo llamado
`default.svg` en disco.

Es decir: en GDIP, el string `"Uploads/favicons/default.svg"` nunca fue un
recurso estático real — es sólo el valor inicial del campo en el
`@code`, reemplazado por el `Path` real en cuanto `LoadFaviconAsync()`/
`LoadLogoAsync()` completan (porque GDIP siempre tiene una fila activa
sembrada). El 404 existía también en GDIP de forma latente (esa ruta
jamás existió como archivo), sólo que era una petición fallida transitoria
durante el primer render, invisible en la práctica porque la imagen se
reemplazaba enseguida por la real. En esta librería el problema se volvió
permanente porque la etapa de limpieza de seeders (ver más arriba) quitó
la siembra de datos: ahora, sin ningún `ThemeAsset` activo, el valor
inicial hardcodeado nunca se reemplaza y el 404 queda expuesto todo el
tiempo.

### Solución

En vez de replicar ese comportamiento (un nombre de archivo que nunca
existió), se agregó un recurso predeterminado real, propio de la
librería, distribuido como Static Web Asset:

- Nuevos `wwwroot/default-assets/favicon.svg` y
  `wwwroot/default-assets/logo.svg` dentro de
  `SAMACDX.MudBlazor.ThemeManager.Persistence` (SVGs simples y genéricos,
  sin pretender ser el arte original de GDIP, que nunca existió como tal).
  Al ser un proyecto `Microsoft.NET.Sdk.Razor`, cualquier archivo bajo
  `wwwroot/` se publica automáticamente como Static Web Asset con el
  prefijo convencional `_content/{AssemblyName}/...` — sin tocar el
  `.csproj` — exactamente el mismo mecanismo por el que ya funciona
  `_content/MudBlazor.ThemeManager/MudBlazorThemeManager.css` desde
  `External/MudBlazor.ThemeManager`.
- Nuevo `Utilities/ThemeDefaultAssets.cs`: dos constantes,
  `DefaultFaviconPath` y `DefaultLogoPath`, con la ruta completa
  `_content/SAMACDX.MudBlazor.ThemeManager.Persistence/default-assets/
  favicon.svg` (y análoga para logo). Centraliza el string para no
  duplicar el `AssemblyName` en varios lugares.
- `Components/Theme/ThemeFaviconAndLogoConfig.razor`: los dos valores
  iniciales de `_faviconPreviewPath`/`_logoPreviewPath` ahora usan
  `ThemeDefaultAssets.DefaultFaviconPath`/`DefaultLogoPath` en vez del
  string `"Uploads/.../default.svg"`. El resto del componente no cambió:
  en cuanto exista un `ThemeAsset` activo real (subido por el
  consumidor), `LoadFaviconAsync()`/`LoadLogoAsync()` lo sobreescriben
  igual que antes — el recurso por defecto de la librería es sólo el
  fallback inicial, nunca tiene prioridad sobre un asset real.

No se modificó `IThemeFileStorageService`/`LocalFileStorageService` ni la
forma en que se guardan los archivos subidos por el usuario (eso ya
estaba correctamente desacoplado de GDIP — escribe al `wwwroot` del
proyecto consumidor vía `IWebHostEnvironment`, sin ninguna ruta física de
GDIP involucrada). Tampoco se copiaron archivos manualmente al `wwwroot`
de `samples/TestHost` — el mecanismo es 100% Static Web Assets.

### Otros recursos estáticos revisados (punto 9)

Se revisaron los 5 componentes de la feature Theme en GDIP
(`ThemeConfig`, `ThemeFaviconAndLogoConfig`, `ThemePaletteSelector`,
`ThemeTermConfig`, `ComponentsPreview`) buscando cualquier otra
referencia a `.css`/`.js`/`.svg`/`.png`/`.ico`/`wwwroot`/`_content`: las
únicas encontradas fueron estas dos rutas de favicon/logo ya corregidas.
No quedó ningún otro recurso estático pendiente de migrar para que el
módulo funcione.

Aparte, en `Components/App.razor` de GDIP el favicon del *sitio* (no el
de esta pantalla de administración) se establece dinámicamente:
`<link rel="icon" ... href="@(themeFaviconPath ?? "favicon.ico")" />`,
leyendo el `ThemeCatalog` activo. Eso vive en el documento host de la
aplicación consumidora (equivalente a `samples/TestHost/Components/
App.razor`), no en la librería — es responsabilidad de cada consumidor
replicarlo si lo desea; no se tocó `samples/TestHost/App.razor` porque
está fuera del alcance de esta corrección (que es sólo sobre los recursos
predeterminados de la librería).

### Estado del build

No fue posible compilar en esta sesión (sin `dotnet` CLI disponible). Tras
agregar archivos nuevos a `wwwroot/`, puede ser necesario un **Rebuild**
completo (no incremental) en `samples/TestHost` para que el manifiesto de
Static Web Assets (`staticwebassets.json`/`*.staticwebassets.endpoints.json`
generados en `obj/`) detecte los dos archivos nuevos — igual que en la
corrección post-migración #6. Pendiente de que el usuario compile
localmente y confirme que `/_content/SAMACDX.MudBlazor.ThemeManager.
Persistence/default-assets/favicon.svg` y `.../logo.svg` cargan sin 404.

## Corrección: el Preview no reflejaba en vivo los cambios de paleta (2026-08-26)

### Flujo revisado

Barra de personalización → estado del tema → Preview:

1. `MudThemeManagerColorItem` (dentro de `External/MudBlazor.ThemeManager`)
   dispara un cambio de color, que `MudThemeManager.UpdatePalette()` aplica
   sobre `Theme.Theme.PaletteLight`/`PaletteDark` (mutando el mismo objeto
   `ThemeManagerTheme` recibido por parámetro) y luego llama a
   `ThemeChanged.InvokeAsync(Theme)`.
2. `ThemePaletteSelector.UpdateTheme(ThemeManagerTheme)` recibe ese evento,
   actualiza su campo local `themeManagerTheme` y llama a
   `ThemeManagerService.ChangeTheme(...)`, que dispara el evento
   `IThemeManagerService.OnThemeChanged` — este es el "estado/configuración
   del tema" compartido (servicio `Scoped`, mismo circuito de Blazor Server).
3. `Components/Theme/ComponentsPreview.razor` **no tiene ningún estado de
   tema propio, ni inyecta `IThemeManagerService`**: como cualquier
   componente MudBlazor normal, sus colores (`Color.Primary`,
   `Color.Secondary`, etc.) dependen enteramente del `MudThemeProvider`
   ambiental más cercano (vía `CascadingValue`), que es quien realmente
   genera las variables CSS del tema. Este archivo es 1:1 idéntico al de
   GDIP — no es la causa del problema.

### Causa real

`samples/TestHost/Components/Layout/MainLayout.razor` (el único lugar del
sample donde vive `<MudThemeProvider />`) **no tenía su parámetro `Theme`
enlazado a nada** y **no se suscribía a `IThemeManagerService.OnThemeChanged`**.
En GDIP (`Components/Shared/Layout/MainLayout.razor`), el layout sí hace
ambas cosas: `<MudThemeProvider Theme="themeManager!.Theme" />` y una
suscripción a `OnThemeChanged` que reasigna `themeManager` y llama
`StateHasChanged()`. Esa suscripción nunca se replicó al construir
`samples/TestHost` (es un host de pruebas armado desde cero para esta
librería, no una extracción literal de GDIP como sí lo son los
componentes de `Components/Theme/`). Resultado: el evento se disparaba
correctamente en cada paso 1-2, pero no había ningún oyente que
actualizara el `MudThemeProvider`, así que sus variables CSS quedaban
congeladas en el tema por defecto de MudBlazor y el Preview nunca
cambiaba visualmente — sin importar cuántos colores se editaran.

### Corrección aplicada (mínima)

Se replicó en `samples/TestHost/Components/Layout/MainLayout.razor` el
mismo enlace que ya existe y funciona en GDIP: se inyecta
`IThemeManagerService`, se agrega un campo `_themeManagerTheme` (inicia en
`new ThemeManagerTheme()`, el mismo valor por defecto que ya usaba
`ThemePaletteSelector`), se enlaza `<MudThemeProvider Theme="
_themeManagerTheme.Theme" />`, y en `OnInitialized()` se suscribe a
`OnThemeChanged`; el handler reasigna `_themeManagerTheme` y llama
`StateHasChanged()` dentro de `InvokeAsync(...)` (el evento puede
dispararse fuera del contexto de renderizado de este componente). No se
tocó nada de `Components/Theme/*` (ya estaban correctos), ni la
persistencia (`ThemeCatalogService`, `ThemePresentService`, etc. sin
cambios), ni se agregó ninguna funcionalidad nueva más allá de restaurar
la reactividad que GDIP ya tenía. Deliberadamente NO se replicó la carga
inicial del tema activo que sí tiene GDIP en su `MainLayout`
(`GetThemeCatalogActive()` vía `IThemeCatalogService`): eso resuelve un
problema distinto (qué tema se ve al entrar a la página, antes de editar
nada) que no fue reportado ni pedido — el pedido era específicamente que
las ediciones en vivo se reflejen de inmediato, lo cual esta corrección
ya resuelve por completo, sin guardar ni recargar ni reseleccionar tema
(seleccionar un preset también pasa por el mismo `UpdateTheme()` →
`ChangeTheme()`, así que también se refleja igual de inmediato).

### Estado del build

No fue posible compilar en esta sesión (sin `dotnet` CLI disponible). Se
verificó por revisión de código que: `IThemeManagerService` y
`ThemeManagerTheme` ya estaban disponibles sin `@using` adicionales
(`_Imports.razor` de `samples/TestHost` ya importa
`SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme` y
`MudBlazor.ThemeManager`); `IThemeManagerService` está registrado como
`Scoped` (mismo circuito que `ThemePaletteSelector`, que inyecta la misma
interfaz); no hay otro `MudThemeProvider` en toda la solución que
necesite el mismo ajuste. Pendiente de que el usuario compile localmente,
abra `/ThemeCatalog` y confirme que mover cualquier color del panel
actualiza el Preview de inmediato.

## Etapa: reorganización de carpetas y namespaces por responsabilidad (2026-08-26)

Reorganización estructural pedida explícitamente por el usuario: agrupar
clases, carpetas y namespaces de la librería por responsabilidad (no sólo
por tipo técnico), eliminando estructuras heredadas de la extracción desde
GDIP, **sin** tocar `samples/TestHost`, GDIP, el comportamiento funcional
ni los contratos públicos salvo lo estrictamente necesario para la
organización. No se agregó funcionalidad nueva ni se rediseñó la API
pública — eso queda para una etapa posterior.

### Restricción que gobernó todo el reorden: namespaces "congelados"

Antes de mover nada se determinó, por `grep` exhaustivo de todos los
`using`/`@using SAMACDX...` dentro de `samples/TestHost`, qué namespaces
de la librería son importados por nombre directamente desde el sample.
Esos namespaces **no podían cambiar de string** (aunque el sample no deba
tocarse, cambiarles el nombre le habría roto la compilación):

1. `...Entities.ThemeCatalog` (`ThemeCatalog`, `ThemeAsset`,
   `ThemeAssetType`, `ThemePresent`)
2. `...Entities.Theme` (`ThemeTerm`)
3. `...Interfaces.Services.Theme` (las 7 interfaces de servicio)
4. `...Interfaces.Services` (`IThemeFileStorageService`, namespace plano)
5. `...Extensions` (`ServiceCollectionExtensions`)

Estas carpetas/namespaces se dejaron exactamente donde estaban. Esto deja
dos inconsistencias conocidas, documentadas aquí como pendientes explícitos
para la etapa de diseño de API pública (no se resolvieron ahora porque
resolverlas requeriría modificar `samples/TestHost`, fuera de alcance):

- `ThemeTerm` sigue en `Entities.Theme`, un namespace distinto al de las
  otras 4 entidades (`Entities.ThemeCatalog`) — inconsistencia heredada del
  original, no introducida ni corregida en esta etapa.
- `IThemeManagerService` sigue en `Interfaces/Services/Theme/` junto con
  las demás interfaces de servicio, aunque su implementación
  (`ThemeManagerService`) sí se movió a la nueva carpeta dedicada
  `ThemeManagerIntegration/` — la interfaz no pudo acompañarla sin romper
  el `using` congelado que ya tiene el sample.

### Nueva estructura por responsabilidad

- **`DataAccess/`** (antes `Repositories/` + `Interfaces/Repositories/`):
  lógica de acceso a datos. `DataAccess/Abstractions/` agrupa
  `IGenericRepository<T>` y las 4 interfaces `ITheme*Repository`
  (namespace único `...DataAccess.Abstractions`); `DataAccess/` (raíz)
  agrupa `GenericRepository` y los 4 repositorios concretos (namespace
  único `...DataAccess`). Antes estaban repartidos en 3 namespaces
  distintos (`Interfaces.Repositories`, `Interfaces.Repositories.Theme`,
  `Repositories`, `Repositories.Theme`); ahora son 2, reflejando
  exactamente la separación abstracción/implementación.
- **`Application/`** (antes `Services/` + `Services/Theme/`, namespaces
  mezclados `Services` y `Services.Theme` sin criterio claro): capa de
  servicios de aplicación, con dos subcarpetas por relación funcional
  clara, tal como pidió el usuario ("agrupa por responsabilidad cuando
  exista una relación funcional clara"):
  - `Application/Assets/`: `ThemeFaviconService` y `ThemeLogoService`
    (ambos operan sobre `ThemeAsset`).
  - `Application/Terminology/`: `TermService`, `ThemeTermService` y
    `SpanishArticleHelper` — este último se reubicó aquí desde
    `Utilities/` porque es un helper de gramática española con un único
    consumidor (`TermService`), no una utilidad genérica; agruparlo junto
    a su único consumidor refleja mejor su responsabilidad real que
    dejarlo en una carpeta de utilidades genéricas.
  - `ThemeCatalogService` y `ThemePresentService` quedan en la raíz de
    `Application/` (no encajan en ningún subgrupo temático con otro
    servicio).
- **`ThemeManagerIntegration/`** (antes `Services/ThemeManagerService.cs`,
  sin namespace — ver corrección debajo): punto de integración con
  `MudBlazor.ThemeManager` (el submódulo), separado del resto de
  `Application/` porque su responsabilidad es puentear con el paquete
  externo, no lógica de negocio sobre las entidades propias.
- **`StaticAssets/`** (antes `Utilities/ThemeDefaultAssets.cs`): las
  constantes de rutas de recursos públicos (`_content/...`) se separaron
  de `Utilities/` a su propia carpeta, ya que no son una utilidad de
  código sino la definición de recursos estáticos públicos de la
  librería — una de las categorías que el usuario pidió mantener separada
  explícitamente.
- **`Utilities/`**: ahora contiene únicamente `JsonHelper.cs`, la única
  utilidad genérica real que queda (sin dependencias de ningún dominio
  específico de Theme). `SpanishArticleHelper` y `ThemeDefaultAssets`
  salieron de aquí por las razones ya explicadas.
- **`Extensions/`, `Entities/`, `Interfaces/`, `Components/`,
  `wwwroot/`**: sin cambios de contenido más allá de los `using`
  necesarios (ver debajo); conservan su rol de configuración/DI,
  entidades, abstracciones congeladas, UI y recursos físicos
  respectivamente.

### Corrección incluida: `ThemeManagerService` sin namespace

Al mover `Services/ThemeManagerService.cs` a `ThemeManagerIntegration/` se
aprovechó para corregir un artefacto heredado de la extracción: el archivo
no declaraba **ningún namespace** (namespace global), lo que obligaba a
`ServiceCollectionExtensions` a referenciarlo como
`global::ThemeManagerService`. Ahora vive en
`SAMACDX.ThemeManager.Persistence.ThemeManagerIntegration` como el resto
de la librería, y el registro en `ServiceCollectionExtensions` se
simplificó a `services.AddScoped<IThemeManagerService, ThemeManagerService>()`
(mismo comportamiento, sin el prefijo `global::`). Es la única línea de
`ServiceCollectionExtensions.cs` cuyo *comportamiento* en tiempo de
ejecución es idéntico — el cambio es puramente de resolución de
namespace/using, no de lógica.

### `Entities` (configuraciones de EF Core) y DTOs/modelos: nada que mover

Se confirmó (ya documentado en la etapa de limpieza anterior, reverificado
ahora) que este proyecto no tiene ninguna clase `IEntityTypeConfiguration<T>`
— el modelado es 100% por convención más el atributo
`[Index(nameof(Name), IsUnique = true)]` directamente sobre `ThemeCatalog`
— ni DTOs/modelos separados de las entidades (las entidades cumplen ese
doble rol). No se creó ninguna carpeta `Configurations/`/`Dtos/` vacía ni
se fabricó contenido para esas categorías, ya que el pedido excluye
explícitamente agregar funcionalidad.

### Namespaces: mapeo completo (viejo → nuevo)

| Antes | Después |
|---|---|
| `Interfaces.Repositories` | `DataAccess.Abstractions` |
| `Interfaces.Repositories.Theme` | `DataAccess.Abstractions` |
| `Repositories` | `DataAccess` |
| `Repositories.Theme` | `DataAccess` |
| `Services` (Catalog/Present/Term/Logo) | `Application` / `Application.Assets` / `Application.Terminology` |
| `Services.Theme` (TermService, ThemeFaviconService) | `Application.Terminology` / `Application.Assets` |
| `Services` (ThemeManagerService, sin namespace) | `ThemeManagerIntegration` |
| `Utilities` (ThemeDefaultAssets) | `StaticAssets` |
| `Utilities` (SpanishArticleHelper) | `Application.Terminology` |
| `Utilities` (JsonHelper) | *(sin cambio)* |
| `Entities.ThemeCatalog`, `Entities.Theme`, `Interfaces.Services*`, `Extensions` | *(sin cambio — congelados por `samples/TestHost`)* |

### Verificación realizada

- Los 19 `git mv` se completaron sin conflictos; las carpetas vacías
  resultantes (`Interfaces/Repositories/`, `Repositories/`, `Services/` y
  sus subcarpetas `Theme/`) se eliminaron.
- Se re-grepeó todo el árbol de la librería (excluyendo
  `External/`/`samples/`/`bin/`/`obj/`) sin anclar el patrón al inicio de
  línea (para no perderse líneas con BOM) y no quedó ninguna referencia a
  los namespaces viejos (`Interfaces.Repositories`, `Repositories.Theme`,
  `Services.Theme`, `global::ThemeManagerService`,
  `Utilities.ThemeDefaultAssets`, `Utilities.SpanishArticleHelper`).
- Se contaron los archivos `.cs`/`.razor` antes y después del reorden: 40
  en ambos casos (incluyendo `_Imports.razor`) — ningún archivo se perdió.
- No se modificó `SAMACDX.MudBlazor.ThemeManager.Persistence.csproj`: el
  SDK `Microsoft.NET.Sdk.Razor` incluye por globbing implícito cualquier
  `.cs`/`.razor` bajo la raíz del proyecto (sólo excluye `External\**` y
  `samples\**`, ya configurado), así que las carpetas nuevas
  (`Application/`, `DataAccess/`, `ThemeManagerIntegration/`,
  `StaticAssets/`) se recogen automáticamente sin tocar el `.csproj`.
- No se tocó `samples/TestHost` en ningún archivo, ni GDIP.

### Estado del build

No fue posible compilar en esta sesión (sin `dotnet` CLI disponible, como
en toda la migración). Se verificó exhaustivamente por lectura completa de
cada archivo movido/editado y por búsqueda de texto en toda la solución
que no quedó ninguna referencia rota a los namespaces/rutas anteriores.
Pendiente de que el usuario compile localmente (librería + `samples/TestHost`)
y confirme.


---

## Etapa: implementación de correcciones y mejoras (a partir del diagnóstico)

Esta etapa parte del documento de diagnóstico técnico entregado previamente
(`DIAGNOSTICO-LIBRERIA.md`, 22 puntos agrupados en Críticas/Recomendadas/
Opcionales/Futuras). El pedido del usuario fue implementar **Críticas +
Recomendadas + Opcionales (27 puntos, C1-C8, R1-R11, O1-O8)** y omitir por
ahora el bloque de **Funcionalidades futuras (F1-F6)**.

A diferencia de la etapa anterior (reorden de namespaces, que deliberadamente
no tocó `samples/TestHost`), esta etapa **sí modifica `samples/TestHost`** en
3 archivos, porque varios de los cambios de la librería son *breaking
changes* de interfaz (ver R6 y C5 abajo) y el TestHost debe seguir
compilando contra ellas:

- `samples/TestHost/Services/LocalFileStorageService.cs`: actualizado a la
  nueva firma de `IThemeFileStorageService` (recibe `ThemeAssetFileContent`
  en vez de `IBrowserFile`; se agregó `DeleteFileAsync`).
- `samples/TestHost/Data/TestDbContext.cs`: se agregó un override de
  `OnModelCreating` que llama a la nueva
  `modelBuilder.ApplyThemeManagerPersistenceModel()` (C5).
- `samples/TestHost/Components/Pages/ThemeCatalogPage.razor` (nuevo): se
  recreó la ruta `/ThemeCatalog` que antes vivía directamente en el
  componente de librería `ThemeConfig.razor` (ver R8 abajo).

`samples/TestHost/Program.cs` **no** requiere cambios: el nuevo parámetro de
`AddThemeManagerPersistence<TContext>(configureOptions = null)` es opcional,
y el registro existente de `IThemeFileStorageService` sigue compilando
contra la implementación actualizada de `LocalFileStorageService`.

### Críticas (C1-C8)

| # | Resumen | Qué se hizo |
|---|---|---|
| C1 | Fuga de `DbContext` en `GenericRepository.Query()` (variante síncrona, sin `using`) | `GenericRepository<TEntity,TContext>` ahora implementa `IDisposable`; los contextos creados por `Query()` se rastrean en una lista acotada (`MaxTrackedContexts = 8`) — al superar el umbral se libera el más antiguo, y el resto se libera en `Dispose()` (llamado por el contenedor de DI al final del scope). El límite acotado existe porque en Blazor Server el scope "Scoped" dura todo el circuito del usuario (potencialmente horas), no un solo request. |
| C2 | `ThemeManagerService.ChangeTheme` no esperaba (`await`) el invocador de `OnThemeChanged`, y lanzaba si nadie estaba suscrito | Ahora usa `var handler = OnThemeChanged; if (handler is not null) await handler.Invoke(theme);`. El caller en `ThemePaletteSelector.razor` (`UpdateTheme`) ahora también hace `await` en vez de fire-and-forget. |
| C3 | `GetCurrentPathAsync()` de favicon/logo resolvía por el primer asset activo de cualquier catálogo, no del catálogo activo | `ThemeAssetOperations.GetCurrentPathAsync()` ahora filtra por `t.ThemeCatalog.IsActive`. |
| C4 | Sin manejo de errores en componentes Razor (excepciones no capturadas rompían la UI silenciosamente) | Se envolvieron todos los métodos de guardado/activación/carga en `ThemeFaviconAndLogoConfig.razor`, `ThemePaletteSelector.razor` y `ThemeTermConfig.razor` en try/catch/finally, con mensajes vía `ISnackbar` (inyectado nuevo en los 3 componentes). |
| C5 | Sin `IEntityTypeConfiguration<T>` explícito — dependencia total de convenciones EF Core, sin defensa ante cambios accidentales | Se agregaron 4 configuraciones (`ThemeCatalogConfiguration`, `ThemeAssetConfiguration`, `ThemePresentConfiguration`, `ThemeTermConfiguration`) bajo `DataAccess/Configurations/`, aplicadas vía `ModelBuilderExtensions.ApplyThemeManagerPersistenceModel()`. **Deliberadamente no llaman a `.ToTable(...)`** ni redeclaran el índice único existente, para ser un no-op respecto a lo que ya generaban las convenciones — no hay riesgo de drift de esquema contra la base SQLite existente del usuario. `TestDbContext.OnModelCreating` la invoca. |
| C6 | Metadatos de empaquetado NuGet ausentes en el `.csproj` (`Version`, `Authors`, `Description`, `PackageId`) | Agregados con valores seguros de inferir. **`RepositoryUrl` y `PackageLicenseExpression` se dejaron sin agregar deliberadamente** — son datos legalmente significativos que no corresponde inventar; pendientes de que el usuario los indique. |
| C7 | Versiones de `PackageReference` con comodín flotante (`8.*`) | Cambiadas a rangos fijos `[8.0.0,9.0.0)` para evitar romper el build con una versión mayor futura no probada. |
| C8 | `CreateWithThemePresentAsync` podía dejar un `ThemeCatalog` huérfano (sin su `ThemePresent`) si la segunda escritura fallaba | Se agregó una acción compensatoria: si `_themePresentService.CreateAsync` falla, se hace `RemoveAsync` del `ThemeCatalog` recién creado antes de relanzar la excepción. **No es una transacción real de base de datos** (el diseño genérico de repositorios no expone `TContext` a la capa de servicio) — se documentó explícitamente en el código por qué. |

### Recomendadas (R1-R11)

| # | Resumen | Qué se hizo |
|---|---|---|
| R1 | Lógica duplicada entre `ThemeFaviconService`/`ThemeLogoService` | Extraída a `Application/Assets/ThemeAssetOperations.cs` (clase interna parametrizada por `ThemeAssetType`); ambos servicios ahora son wrappers delgados. |
| R2 | Patrón de "activar exclusivamente uno del grupo" repetido 3 veces | Extraído a `Application/ExclusiveActivationHelper.cs` (`ActivateOnly<T>`), usado en `ThemeCatalogService.ActivateAsync` y `ThemeAssetOperations.ActivateAsync`. |
| R3 | Registros de DI con `AddScoped` (no reemplazables por el consumidor sin orden implícito) | Todos los registros de `ServiceCollectionExtensions.AddThemeManagerPersistence` cambiados a `TryAddScoped`/`TryAddSingleton`. |
| R4 | Sin forma de configurar duraciones de caché, carpetas de subida, tamaño máximo, tipos de contenido permitidos | Nuevo `ThemeManagerPersistenceOptions`, configurable vía el parámetro opcional `configureOptions` de `AddThemeManagerPersistence<TContext>`. |
| R5 | Efectos "fake loading" con `Task.Delay(2000)` en vez de estados reales de carga | Eliminados todos los `Task.Delay(2000)`; los estados `isSaving*`/`isActivating*` ahora reflejan el `await` real. |
| R6 | `IThemeFileStorageService`/`IThemeFaviconService`/`IThemeLogoService` acoplados a `IBrowserFile` (tipo de Blazor Server) en la capa de aplicación/persistencia | Nuevo tipo `ThemeAssetFileContent` (record con `Stream`, `FileName`, `ContentType`, `Length`) en `Interfaces/Services/`; las interfaces y sus implementaciones ya no referencian `IBrowserFile` — queda confinado a los componentes Razor, que leen el archivo a bytes/stream antes de llamar al servicio. **El diagnóstico original sugería posponer esto a un futuro rediseño de API por ser un cambio disruptivo; se implementó ahora igualmente porque el pedido explícito del usuario fue "todas las correcciones y mejoras"**, lo que obligó a actualizar también `samples/TestHost/Services/LocalFileStorageService.cs` (ver arriba). |
| R7 | Sin invalidación de caché al activar un catálogo (`GetActiveAsync` podía devolver el catálogo viejo) | `ThemeCatalogService.ActivateAsync` invalida `ActiveCatalogCacheKey` tras actualizar. |
| R8 | `@page "/ThemeCatalog"` declarado directamente en el componente de librería `ThemeConfig.razor` (fuerza esa ruta exacta en cualquier consumidor) | Se quitó la directiva `@page` de `ThemeConfig.razor` (ahora es un componente puro, embebible en cualquier ruta); se recreó la página en `samples/TestHost/Components/Pages/ThemeCatalogPage.razor` para no romper el TestHost. |
| R9 | Sin implementación por defecto de `IThemeFileStorageService` — todo consumidor debe escribir la suya desde cero | Nueva `Application/Assets/LocalDiskThemeFileStorageService.cs` (usa `IWebHostEnvironment.WebRootPath`), registrable opcionalmente vía `AddThemeManagerPersistenceLocalFileStorage()`. Requiere `FrameworkReference Include="Microsoft.AspNetCore.App"` (agregado al `.csproj`) porque `IWebHostEnvironment` no está disponible en un `Microsoft.NET.Sdk.Razor` sin él. |
| R10 | Sin operación de borrado en `IGenericRepository<T>` ni en los servicios de catálogo/términos | `IGenericRepository<TEntity>.RemoveAsync(TEntity)` (y su implementación); `IThemeCatalogService.DeleteAsync(int)` (con guardas: no permite borrar el catálogo base ni el activo); `IThemeTermService.DeleteTermsAsync(int)`; `IThemeFaviconService`/`IThemeLogoService.DeleteAsync(int)` (vía `ThemeAssetOperations`, borra la fila y el archivo físico). |
| R11 | Documentar el acoplamiento permanente al fork/submódulo `MudBlazor.ThemeManager` | Ver nota abajo. |

**Nota R11 — acoplamiento a `MudBlazor.ThemeManager`:** esta librería depende
de manera permanente e intencional del fork/submódulo ubicado en
`External/MudBlazor.ThemeManager` (referenciado vía `ProjectReference` en el
`.csproj`, excluido del globbing normal de `.cs`/`.razor` de este proyecto).
No es una dependencia opcional ni reemplazable sin modificar código: los
componentes de la librería (`ThemePaletteSelector.razor`, entre otros) usan
tipos de ese fork directamente (`MudThemeManager`, `ThemePreset`,
`ThemeManagerTheme`). Cualquier consumidor que clone/instale esta librería
debe traer consigo ese submódulo/carpeta (`git submodule update --init` si se
formaliza como submódulo git, o copiar la carpeta `External/` tal cual). Esto
queda pendiente de convertirse en una guía de instalación formal (fuera de
alcance de esta etapa, ya que las "funcionalidades futuras" del diagnóstico
quedaron explícitamente excluidas).

### Opcionales (O1-O8)

| # | Resumen | Qué se hizo |
|---|---|---|
| O1 | `GetActiveAsync()` sin caché (una consulta a la BD en cada render) | Cacheado vía `IMemoryCache`, duración configurable (`Options.ActiveCatalogCacheDuration`, default 5 min), invalidado en `ActivateAsync`. |
| O2 | `TermService` con caché de duración fija hardcodeada | Ahora usa `Options.TermCacheDuration` (default 30 min). |
| O3 | `ThemeTerm.Gender` es `string` libre, sin validación | **No se cambió el tipo de la columna** (el diagnóstico mismo señalaba el riesgo de migración/datos contra la BD SQLite existente del usuario). En su lugar: nuevo `ThemeTermGender` (enum) + `ThemeTermGenderParser.TryParse`, usado solo para *validar* en escritura (`ThemeTermService.Create/Update` lanzan `ThemeValidationException` si el valor no es reconocible) — el dato persistido sigue siendo `string`, sin riesgo de romper filas existentes. |
| O4 | Sin `GenerateDocumentationFile`/XML docs | Agregado `<GenerateDocumentationFile>true</GenerateDocumentationFile>` + `<NoWarn>$(NoWarn);CS1591</NoWarn>` (no se escribieron docs XML retroactivas en cada miembro público existente — eso sería una tarea aparte de gran volumen; el flag deja el mecanismo listo). |
| O5 | Rutas de assets por defecto (`ThemeDefaultAssets`) con el nombre del ensamblado hardcodeado como literal | `BasePath` ahora se calcula desde `typeof(ThemeDefaultAssets).Assembly.GetName().Name` en vez de un string literal. |
| O6 | Sin límite de tamaño de archivo subido en favicon/logo | Validado en `UploadFiles`/`UploadLogoFile` contra `Options.MaxUploadSizeBytes` (default 10 MB) antes de leer el stream. |
| O7 | Sin validación de nombre duplicado/vacío al crear un `ThemeCatalog` | `CreateWithThemePresentAsync` valida vacío y duplicado, lanzando `ThemeValidationException` (nueva clase, `Application/ThemeValidationException.cs`). |
| O8 | `Query()` de `IGenericRepository<T>` solo soporta `Include` de un nivel (no `.ThenInclude`) | Nueva sobrecarga `Query(Func<IQueryable<TEntity>, IQueryable<TEntity>> shape)` que permite componer la consulta libremente. Se verificó por resolución de sobrecarga que no genera ambigüedad con los call sites existentes de la sobrecarga original (`Query(params Expression<Func<TEntity,object>>[] includes)`). |

### Funcionalidades futuras (F1-F6): omitidas por pedido explícito del usuario

No se implementó nada del bloque "Funcionalidades futuras" del diagnóstico
(F1-F6) — el usuario pidió explícitamente omitirlas por el momento.

### Archivos nuevos de esta etapa

- `Application/ThemeValidationException.cs`
- `Application/ExclusiveActivationHelper.cs`
- `Application/Assets/ThemeAssetOperations.cs`
- `Application/Assets/LocalDiskThemeFileStorageService.cs`
- `Application/Terminology/ThemeTermGender.cs`
- `Extensions/ThemeManagerPersistenceOptions.cs`
- `Extensions/ModelBuilderExtensions.cs`
- `Interfaces/Services/ThemeAssetFileContent.cs`
- `DataAccess/Configurations/ThemeCatalogConfiguration.cs`
- `DataAccess/Configurations/ThemeAssetConfiguration.cs`
- `DataAccess/Configurations/ThemePresentConfiguration.cs`
- `DataAccess/Configurations/ThemeTermConfiguration.cs`
- `samples/TestHost/Components/Pages/ThemeCatalogPage.razor`

### Estado del build

Igual que en la etapa anterior: no fue posible compilar en esta sesión (sin
`dotnet` CLI disponible). Se verificó por lectura completa de cada archivo
nuevo/reescrito, razonamiento manual de tipos/resolución de sobrecargas, y
una batería de búsquedas de texto en todo el árbol (sin `IBrowserFile` fuera
de `Components/`/`samples/`, sin `Task.Delay(2000)` remanente, sin
`ThemeCatalogId == 1` hardcodeado, sin el typo `fivicon`, firma de
`SaveFileAsync` consistente en todos los call sites, etc.). **Pendiente de
que el usuario compile localmente (librería + `samples/TestHost`) y
confirme** — sigue sin haber forma de verificarlo por compilación real
dentro de esta sesión.


---

## Corrección post-implementación: NU1605 (downgrade de paquete) en `dotnet restore`

Al compilar localmente la etapa anterior ("implementación de correcciones y
mejoras"), `dotnet restore` falló con:

```
error NU1605: Warning As Error: Detected package downgrade:
  Microsoft.AspNetCore.Components.Web from 9.0.1 to 9.0.0
error NU1605: Warning As Error: Detected package downgrade:
  Microsoft.Extensions.DependencyInjection.Abstractions from 9.0.1 to 9.0.0
```

**Causa**: C7 (fijar rangos de versión en vez de comodines flotantes `8.*`/
`9.*`) fijó el piso de `Microsoft.AspNetCore.Components.Web` y de
`Microsoft.Extensions.DependencyInjection.Abstractions` en `9.0.0`. Pero
`MudBlazor 8.0.0` exige transitivamente `Components.Web >= 9.0.1`, y (vía
`Microsoft.Extensions.Localization 9.0.1`) `DependencyInjection.Abstractions
>= 9.0.1`. Al declarar la referencia DIRECTA con un piso de `9.0.0`, NuGet
resuelve esa referencia a `9.0.0` — más baja que lo que el propio grafo
transitivo ya exige (`9.0.1`) — y eso es exactamente lo que `NU1605`
detecta como "downgrade".

**Fix**: se subió el piso de esas dos referencias a `[9.0.1,10.0.0)` (antes
`[9.0.0,10.0.0)`). El valor `9.0.1` no es arbitrario: es el mínimo exacto
que el propio mensaje de error de NuGet señala como requerido. El resto de
paquetes (`MudBlazor`, `Microsoft.EntityFrameworkCore`,
`Microsoft.Extensions.Caching.Memory`) no se tocaron — el error no los
mencionó, así que sus pisos actuales ya son compatibles con el grafo
transitivo.

**Nota**: `samples/TestHost/*.csproj` sigue con
`Microsoft.EntityFrameworkCore.Sqlite` en comodín flotante (`9.*`, sin
tocar, fuera de alcance de C7 según lo documentado en esa etapa). Si al
compilar el TestHost aparece un NU1605 similar (por ejemplo si `9.*`
resuelve una versión de `EntityFrameworkCore.Sqlite` que exige una versión
de `Microsoft.EntityFrameworkCore` más alta que el piso `9.0.0` fijado en
la librería), avisar para revisarlo — no se modificó preventivamente sin
evidencia de un error real, siguiendo la práctica de no tocar
`samples/TestHost` salvo necesidad concreta.

## Etapa: eliminacion de `ThemeCatalog` y fusion en `ThemePresent` (2026-08-27)

Refactor de persistencia para simplificar el modulo Theme/Branding a
exactamente **dos entidades independientes**: `ThemePresent` (el tema,
con nombre y estado) y `ThemeAsset` (recursos visuales, sin relacion con
ningun tema). Se elimina por completo la entidad intermedia `ThemeCatalog`
que hasta ahora conectaba ambas.

### Modelo resultante

```csharp
public class ThemePresent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsBase { get; set; } = false;
    public bool IsActive { get; set; } = false;
    public string JsonData { get; set; }
}

public class ThemeAsset
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public ThemeAssetType Type { get; set; }
    public bool IsActive { get; set; } = false;
}
```

`Name`, `IsBase` e `IsActive` se movieron de `ThemeCatalog` a
`ThemePresent` (que ya tenia `JsonData`). `ThemeAsset` perdio
`ThemeCatalogId`/`ThemeCatalog`. Ninguna de las dos entidades tiene FK
hacia la otra ni hacia ninguna entidad mas. `ThemeAssetType` (`Logo`,
`Favicon`) no cambio.

### Archivos eliminados

- `Entities/ThemeCatalog/ThemeCatalog.cs`
- `DataAccess/Configurations/ThemeCatalogConfiguration.cs`
- `DataAccess/Abstractions/IThemeCatalogRepository.cs`
- `DataAccess/ThemeCatalogRepository.cs`
- `Interfaces/Services/Theme/IThemeCatalogService.cs`
- `Application/ThemeCatalogService.cs`

### Servicios de aplicacion

`IThemeCatalogService`/`ThemeCatalogService` se elimino por completo; su
responsabilidad (listar/obtener/activar/crear/eliminar/evento) se fusiono
dentro de `IThemePresentService`/`ThemePresentService`, ya que
`ThemePresent` ahora ES el tema (no hay mas una entidad "catalogo" por
separado). Cambios de firma respecto a los dos servicios anteriores:

- `IThemePresentService.GetByThemeIdAsync(int id)` (buscaba por el Id del
  `ThemeCatalog` padre) se renombro a `GetByIdAsync(int id)`: la
  indireccion desaparece porque el propio Id de `ThemePresent` cumple
  ahora el rol que antes cumplia `ThemeCatalog.Id`.
- `ThemeCatalogService.CreateWithThemePresentAsync(ThemeCatalog,
  ThemePresent)` (dos inserts + rollback compensatorio si el segundo
  fallaba) se reemplazo por `ThemePresentService.CreateAsync(ThemePresent)`,
  un unico insert. La logica de rollback compensatorio desaparece porque
  ya no hay una segunda escritura que pueda fallar de forma parcial — no
  es una omision, es la consecuencia directa de fusionar las dos
  entidades en una.
- `ThemeCatalogService.DeleteAsync(int id)` y el evento
  `ThemeCatalogActivated` pasan a `ThemePresentService.DeleteAsync(int
  id)` y `ThemePresentService.ThemePresentActivated`, con la misma logica
  (no se puede eliminar el tema base ni el activo).

`ThemeAssetOperations` (interno, usado por `ThemeFaviconService`/
`ThemeLogoService`): `GetAllByThemeCatalogIdAsync(int)` → `GetAllAsync()`;
`ActivateAsync(int themeCatalogId, int themeAssetId)` →
`ActivateAsync(int themeAssetId)`; `GetCurrentPathAsync()` ya no filtra
por catalogo activo, solo por `Type == _type && IsActive`. La
exclusividad de "activo" para un asset pasa de estar acotada a
`(ThemeCatalogId, Type)` a estar acotada solo a `Type`, de forma global
para toda la aplicacion — consecuencia directa de que `ThemeAsset` ya no
tiene ninguna relacion con un tema. `IThemeFaviconService`/
`IThemeLogoService` reflejan el mismo cambio de firmas.

### EF Core / DI

`ModelBuilderExtensions.ApplyThemeManagerPersistenceModel()` aplica ahora
solo `ThemePresentConfiguration` + `ThemeAssetConfiguration` (elimina
`ThemeCatalogConfiguration`). Ninguna de las dos declara relaciones. El
indice unico `[Index(nameof(Name), IsUnique = true)]` se movio de
`ThemeCatalog` a `ThemePresent`. `ServiceCollectionExtensions` elimina el
registro de `IThemeCatalogRepository`/`ThemeCatalogRepository<TContext>`
e `IThemeCatalogService`/`ThemeCatalogService`.

`ThemeManagerPersistenceOptions.ActiveCatalogCacheDuration` se renombro a
`ActivePresentCacheDuration` (cambio de nombre deliberado, dentro del
alcance: "Catalog" se elimina de toda la terminologia por pedido
explicito). La clave de cache interna paso de
`"ThemeCatalogService_ActiveCatalog"` a
`"ThemePresentService_ActivePresent"`.

### Componentes Razor

- `ThemeFaviconAndLogoConfig.razor`: se elimino el parametro
  `[Parameter] ThemeCatalog? SelectThemeCatalog` (favicon/logo ya no
  estan acotados "al tema que se esta editando" — son globales). Como
  consecuencia, el hook de ciclo de vida paso de `OnParametersSetAsync` a
  `OnInitializedAsync` (ya no depende de un parametro externo que pueda
  cambiar). `LoadFaviconAsync`/`LoadLogoAsync`/`SaveFaviconAsync`/
  `SaveLogoAsync`/`ActivateFaviconAsync`/`ActivateLogoAsync` perdieron sus
  guards de "seleccionar un tema primero" y el `ThemeCatalogId =
  SelectThemeCatalog.Id` al construir un `ThemeAsset` nuevo.
- `ThemePaletteSelector.razor`: se elimino el parametro
  `[Parameter] EventCallback<ThemeCatalog> ThemeCatalogChanged` (y todos
  sus sitios de invocacion) — era el unico consumidor de
  `SelectThemeCatalog` arriba, y al desaparecer ese parametro el callback
  queda sin ningun proposito. Renombres internos: `themesCatalog` →
  `themePresents`, `themeCatalogActive` → `activeThemePresent`,
  `isSavingThemeCatalog`/`isSavingActiveThemeCatalog` →
  `isSavingThemePresent`/`isSavingActiveThemePresent`,
  `GetThemesCatalog()` → `GetThemePresents()`, `AddThemesCatalog(...)` →
  `AddThemePresent(...)`, `saveThemeCatalog()` → `saveThemePresent()`.
- `ThemeConfig.razor`: al desaparecer el cableado
  `SelectThemeCatalog`/`ThemeCatalogChanged` entre los dos componentes
  anteriores, quedo como un wrapper puramente de composicion, sin
  `@code`.

### samples/TestHost

- `Data/TestDbContext.cs`: se elimino `DbSet<ThemeCatalog> ThemeCatalogs`.
- `Program.cs`: se elimino el bloque de SQL crudo
  (`ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS \"ThemeAssets\" ...
  ThemeCatalogId ... FOREIGN KEY ...")`) que quedaba de una etapa
  anterior — ya no aplica, `ThemeAsset` no tiene esa columna. El archivo
  `themetesthost.db` local (esquema viejo) se elimino en esta sesion para
  que `EnsureCreatedAsync()` genere el esquema nuevo correcto al proximo
  arranque (`EnsureCreatedAsync()` no corrige un archivo ya existente).
- `Components/Pages/Home.razor`: se reescribio para inyectar
  `IThemePresentService`/`IThemeFaviconService`/`IThemeLogoService` en
  vez de `IThemeCatalogService`/`IThemeLogoService`, leyendo
  `_activePresent.Name` y llamando a
  `GetCurrentFaviconPathAsync()`/`GetCurrentLogoPathAsync()`
  directamente en vez de indexar `ThemeCatalog.ThemeAssets`.
- **Ruta renombrada**: `ThemeCatalogPage.razor` (ruta `/ThemeCatalog`) se
  elimino y se reemplazo por `ThemeAdministrationPage.razor` (ruta
  `/administrar-tema`) — decision de alcance: el nombre/ruta viejos leian
  como terminologia obsoleta despues de eliminar `ThemeCatalog` en todo
  el resto del codigo. Se actualizo el link correspondiente en
  `Components/Layout/MainLayout.razor`, `Components/Pages/Home.razor` y
  `samples/TestHost/README.md`.

### Base de datos y migraciones

La libreria no trae migraciones propias (ver
[docs/DBCONTEXT-AND-MIGRATIONS.md](docs/DBCONTEXT-AND-MIGRATIONS.md)).
Para una app consumidora que ya tenia el esquema anterior (con
`ThemeCatalog`) y datos reales, `dotnet ef migrations add` sobre el
`DbContext` consumidor generara una migracion que debe, en este orden:

1. Agregar las columnas `Name`, `IsBase`, `IsActive` a `ThemePresent`
   (nullable o con default temporal, para poder poblarlas antes del
   paso 2).
2. Copiar `Name`, `IsBase`, `IsActive` desde `ThemeCatalog` hacia el
   `ThemePresent` correspondiente, vinculando por
   `ThemePresent.ThemeCatalogId == ThemeCatalog.Id` (la FK vieja, todavia
   presente en este punto) — con SQL crudo dentro de la migracion
   (`migrationBuilder.Sql(...)`) o un paso de datos equivalente.
3. Marcar `ThemePresent.Name`/`IsBase`/`IsActive` como NOT NULL /sin
   default una vez pobladas (si se usaron nullable/default en el paso 1).
4. Eliminar la FK `ThemeAsset.ThemeCatalogId` → `ThemeCatalog.Id` y la
   columna `ThemeAsset.ThemeCatalogId`.
5. Eliminar la FK `ThemePresent.ThemeCatalogId` → `ThemeCatalog.Id` y la
   columna `ThemePresent.ThemeCatalogId`.
6. Eliminar la tabla `ThemeCatalog`.

`ThemeAsset` y `ThemePresent` quedan como tablas independientes, sin
ninguna FK entre si ni hacia ninguna tabla eliminada. Es responsabilidad
de la app consumidora generar, revisar y aplicar esta migracion contra su
propio `DbContext` — la libreria no la genera ni la aplica.

### Decisiones de alcance (no son bugs, quedan documentadas)

- **El namespace/carpeta `Entities/ThemeCatalog/` no se renombro.**
  Sigue conteniendo `ThemePresent.cs`, `ThemeAsset.cs` y
  `ThemeAssetType.cs` bajo ese nombre heredado de la etapa anterior del
  proyecto. Renombrarlo habria significado tocar el `using` de practicamente
  todos los archivos de la libreria y de `samples/TestHost` sin ningun
  beneficio funcional — se considero fuera del alcance de este refactor
  (que es sobre el modelo de datos, no sobre organizacion de carpetas).
- **`IGenericRepository<T>.Query(...)` no se elimino**, aunque despues de
  este refactor ningun codigo interno lo sigue llamando (su unico llamador,
  `ThemeCatalogService.GetBaseAsync`, desaparecio junto con esa clase). Es
  una capacidad generica del repositorio, agregada en una etapa anterior
  para uso futuro de cualquier entidad — no es compatibilidad artificial
  con `ThemeCatalog`, asi que quitarla seria una decision de arquitectura
  aparte, fuera del alcance de este refactor.
- **El parametro `IsSavingActiveThemeCatalog` del componente
  `MudThemeManager`** (fork externo, submodulo git `External/
  MudBlazor.ThemeManager`, repositorio separado
  `SAMACDX/ThemeManager.git`) conserva ese nombre. No se modifico porque
  ese codigo vive en otro repositorio git — cualquier cambio ahi no
  formaria parte de este repositorio ni de este refactor.

### Estado del build

No fue posible compilar en esta sesion (sin `dotnet` CLI disponible, como
en toda la migracion). Se verifico exhaustivamente por lectura completa
de cada archivo reescrito y una bateria de busquedas de texto en toda la
solucion (excluyendo `External/`, `bin/`, `obj/`, `.git/`) confirmando
cero referencias remanentes a `ThemeCatalog`, `ThemeCatalogId`,
`ThemeCatalogService`, `IThemeCatalogService`, `ThemeCatalogRepository`,
`IThemeCatalogRepository`, `ThemeCatalogConfiguration`,
`ThemeCatalogActivated`, `GetAllByThemeCatalogIdAsync`,
`ActiveCatalogCacheDuration`, `SelectThemeCatalog`, `ThemeCatalogChanged`,
`CreateWithThemePresentAsync` y `GetByThemeIdAsync`, salvo las
excepciones documentadas arriba (namespace `Entities.ThemeCatalog` y el
parametro del fork externo). Pendiente de que el usuario compile
localmente (libreria + `samples/TestHost`) y confirme.
