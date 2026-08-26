# Arquitectura de consumo

## Frontera de responsabilidades

### `MudBlazor.ThemeManager` (fork/submódulo, `External/MudBlazor.ThemeManager`)

No sabe nada de bases de datos, catálogos, ni de esta librería. Su responsabilidad es exclusivamente la **configuración visual y el editor**:

- **`MudThemeManager`** (componente): drawer/editor visual para modificar en vivo la paleta, tipografía, elevaciones, radio de borde, modo claro/oscuro, etc. de un `MudTheme`. Parámetros relevantes: `Theme` (`ThemeManagerTheme`), `ThemeChanged` (`EventCallback<ThemeManagerTheme>`), `IsDarkMode`, `Open`/`OpenChanged`, `ColorPickerView`.
- **`MudThemeManager.ThemePreset`** (record anidado: `Id`, `Name`, `IsActive`): representación mínima de un tema seleccionable en el desplegable del editor. `MudThemeManager` no sabe qué es un "catálogo de tema" — solo trabaja con esta forma genérica. **Esta librería es quien traduce** cada `ThemeCatalog` persistido a un `ThemePreset` para alimentar al editor (`ThemePaletteSelector.razor`).
  - Parámetros relacionados: `ThemePresets` (`Dictionary<int, ThemePreset>`), `ThemePresetsChanged` (`EventCallback<ThemePreset>`, se dispara al elegir un preset del desplegable — esto es "seleccionar para previsualizar", no "activar"), `OnClickActiveThemePresent` (`EventCallback`, botón para confirmar/activar el preset actualmente elegido), `IsSavingActiveThemeCatalog` (`bool`, solo controla el estado visual de "guardando" del botón).
- **`ThemeManagerTheme`** (modelo): envoltorio en memoria de un `MudTheme` más un puñado de propiedades adicionales (`RTL`, `FontFamily`, `DefaultBorderRadius`, `DefaultElevation`, `AppBarElevation`, `DrawerElevation`, `DrawerClipMode`). **Este es el objeto que esta librería serializa/deserializa** como JSON dentro de `ThemePresent.JsonData`.
- **`MudThemeManagerButton`**: botón simple para abrir/cerrar el drawer del editor (`OnClick`).

### `SAMACDX.ThemeManager.Persistence` (esta librería)

Responsable de todo lo que `MudBlazor.ThemeManager` no sabe hacer:

- **Persistencia**: guardar un `ThemeManagerTheme` (serializado a JSON) asociado a un `ThemeCatalog` con nombre.
- **Recuperación**: listar catálogos, obtener el catálogo activo (con caché), obtener el catálogo base.
- **Activación**: marcar un catálogo como el vigente para toda la aplicación (exclusivo — desactiva los demás).
- **Branding**: administrar `ThemeAsset` (logo/favicon) por catálogo, con almacenamiento de archivos desacoplado (`IThemeFileStorageService`).
- **Integración con `DbContext`**: entidades + `IEntityTypeConfiguration<T>` listas para aplicar sobre el `DbContext` de la app consumidora.
- **Servicios de aplicación**: la capa que un componente Razor o un endpoint de la app consumidora realmente inyecta y usa.
- **Componentes Razor reutilizables**: UI de administración ya construida (`Components/Theme/*.razor`), montable dentro de cualquier página del consumidor.

### Proyecto consumidor

- Su propio `DbContext` (con el modelo de esta librería aplicado vía `ApplyThemeManagerPersistenceModel()`).
- Su propia configuración específica (cadena de conexión, proveedor de EF Core, dónde guardar archivos).
- Integración con su `Layout` (`MudThemeProvider`, `AppBar`, HTML `<head>` para el favicon).
- Uso de los recursos activos (tema/logo/favicon) donde su UI lo necesite (Login, Dashboard, cualquier página).
- Opcionalmente, su propia implementación de `IThemeFileStorageService` (o usar la que trae la librería).

**Ninguna responsabilidad está duplicada entre los tres**: `MudBlazor.ThemeManager` no persiste nada; esta librería no dibuja ningún editor de colores (solo compone los componentes de administración alrededor de `MudThemeManager`); el consumidor no necesita copiar entidades, configuraciones de EF Core, ni lógica de activación/branding — solo proveer su `DbContext` y su almacenamiento de archivos.

## Diagrama de flujo (edición → persistencia → activación → consumo)

```
Usuario edita colores en <MudThemeManager>
        │  (ThemeChanged: ThemeManagerTheme)
        ▼
ThemePaletteSelector.razor
        │  await ThemeManagerService.ChangeTheme(theme)   ← EN MEMORIA, aún no persistido
        ▼
IThemeManagerService.OnThemeChanged (evento)
        │  (cualquier suscriptor, p. ej. MainLayout, actualiza su <MudThemeProvider> EN VIVO)
        │
        │  usuario hace clic en "Guardar Configuración"
        ▼
IThemeCatalogService.CreateWithThemePresentAsync(catalog, present)
        │  (crea un ThemeCatalog NUEVO + su ThemePresent con el JSON del tema)
        │
        │  usuario hace clic en "Activar"
        ▼
IThemeCatalogService.ActivateAsync(id)
        │  (desactiva los demás catálogos, activa este, invalida caché)
        ▼
IThemeCatalogService.ThemeCatalogActivated (evento)
        │  (cualquier suscriptor reacciona: p. ej. refrescar favicon del <head>)
        │
        ▼
Cualquier página/Layout, en cualquier momento:
IThemeCatalogService.GetActiveAsync() → ThemeCatalog activo (cacheado)
IThemeLogoService.GetCurrentLogoPathAsync() → ruta del logo activo
IThemeFaviconService.GetCurrentFaviconPathAsync() → ruta del favicon activo
```

## Inventario de la API pública

Solo se listan aquí tipos y miembros públicos pensados para ser usados por el proyecto consumidor. Los tipos marcados **(interno)** existen en el código pero no forman parte del contrato de consumo — se listan solo para que quede claro por qué no se documentan en detalle en el resto de esta documentación.

### Servicios de aplicación (namespace `SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme` salvo donde se indique)

| Interfaz | Implementación | Para qué sirve |
|---|---|---|
| `IThemeCatalogService` | `Application.ThemeCatalogService` | Listar/obtener/crear/activar/eliminar catálogos de tema (ver [THEMES.md](THEMES.md)). |
| `IThemePresentService` | `Application.ThemePresentService` | Obtener/crear la configuración visual (`ThemePresent`, el JSON del `ThemeManagerTheme`) de un catálogo. |
| `IThemeFaviconService` | `Application.Assets.ThemeFaviconService` | Administrar y consultar el favicon activo (ver [BRANDING.md](BRANDING.md)). |
| `IThemeLogoService` | `Application.Assets.ThemeLogoService` | Administrar y consultar el logo activo (ver [BRANDING.md](BRANDING.md)). |
| `IThemeTermService` | `Application.Terminology.ThemeTermService` | CRUD de términos de terminología (`ThemeTerm`). |
| `ITermService` | `Application.Terminology.TermService` **(implementación interna, solo la interfaz es pública)** | Lectura de términos con soporte de artículos en español y caché (ver tabla siguiente). |
| `IThemeManagerService` (namespace `Interfaces.Services.Theme`) | `ThemeManagerIntegration.ThemeManagerService` | Puente en memoria entre el editor (`MudThemeManager`) y cualquier suscriptor que necesite reaccionar a cambios de tema en vivo, antes de guardar (ver [RUNTIME.md](RUNTIME.md)). |
| `IThemeFileStorageService` (namespace `Interfaces.Services`) | Debe proveerla el consumidor, o usar `Application.Assets.LocalDiskThemeFileStorageService` (opcional) | Contrato mínimo de almacenamiento de archivos que necesita la librería para favicons/logos. |

`ITermService` — métodos: `GetAsync(key)`, `GetPluralAsync(key)`, `GetWithDefiniteArticleAsync(key)`, `GetPluralWithDefiniteArticleAsync(key)`, `GetWithIndefiniteArticleAsync(key)`, `GetPluralWithIndefiniteArticleAsync(key)`, `GetByKeyAsync(key)` (devuelve el `ThemeTerm` completo o `null`), `InvalidateCache()`. Todos, salvo `InvalidateCache`, devuelven la clave tal cual si el término no existe (nunca lanzan por término no encontrado).

### DTOs / modelos a consumir

| Tipo | Namespace | Descripción |
|---|---|---|
| `ThemeAssetFileContent` | `Interfaces.Services` | `record(Stream Content, string FileName, string ContentType, long Length)`. Reemplaza a `IBrowserFile` en la frontera de la capa de aplicación/persistencia — se arma en el componente Razor (u otro origen) leyendo el archivo, y se pasa a `IThemeFaviconService.CreateAsync`/`IThemeLogoService.CreateAsync`/`IThemeFileStorageService.SaveFileAsync`. |
| `ThemeValidationException` | `Application` | Excepción tipada para errores de validación de negocio (nombre de catálogo vacío/duplicado, tipo de archivo no permitido, género de término inválido, intento de borrar el catálogo base/activo). Los componentes de la librería la distinguen de cualquier otra excepción para mostrar su mensaje tal cual. |

### Entidades (persistidas vía EF Core)

| Entidad | Namespace | Campos | Notas |
|---|---|---|---|
| `ThemeCatalog` | `Entities.ThemeCatalog` | `Id`, `Name` (único), `IsBase`, `IsActive`, `ThemePresent` (nav 1:1), `ThemeAssets` (nav 1:N) | Un catálogo = un tema con nombre. Solo uno puede tener `IsActive == true` a la vez. |
| `ThemePresent` | `Entities.ThemeCatalog` | `Id`, `JsonData` (el `ThemeManagerTheme` serializado), `ThemeCatalogId`, `ThemeCatalog` (nav) | Relación 1:1 con `ThemeCatalog`. |
| `ThemeAsset` | `Entities.ThemeCatalog` | `Id`, `Name`, `Path`, `Type` (`ThemeAssetType`), `IsActive`, `ThemeCatalogId`, `ThemeCatalog` (nav) | Un logo o favicon subido, asociado a un catálogo. Solo uno por `(ThemeCatalogId, Type)` puede tener `IsActive == true`. |
| `ThemeAssetType` | `Entities.ThemeCatalog` | enum: `Logo`, `Favicon` | **Únicos dos valores existentes hoy.** No hay un tercer tipo de asset. |
| `ThemeTerm` | `Entities.Theme` | `Id`, `Key`, `Singular`, `Plural`, `Gender` (string, validado como `"Masculine"`/`"Feminine"`), `Special` | **No tiene relación con `ThemeCatalog`** — la terminología es global a la aplicación, independiente del tema visual activo. |

### Extensiones de Dependency Injection (namespace `Extensions`)

| Método | Para qué sirve |
|---|---|
| `IServiceCollection.AddThemeManagerPersistence<TContext>(Action<ThemeManagerPersistenceOptions>? configureOptions = null)` | Registra repositorios + servicios de la librería. Único método obligatorio. |
| `IServiceCollection.AddThemeManagerPersistenceLocalFileStorage()` | Registra `LocalDiskThemeFileStorageService` como `IThemeFileStorageService`. Opcional, requiere `IWebHostEnvironment` (host `Sdk.Web`). |
| `ModelBuilder.ApplyThemeManagerPersistenceModel()` | Aplica las 4 `IEntityTypeConfiguration<T>` de la librería sobre el `DbContext` consumidor. Se llama desde `OnModelCreating`. |

### Configuración (`Extensions.ThemeManagerPersistenceOptions`)

| Propiedad | Default | Para qué sirve |
|---|---|---|
| `TermCacheDuration` | 30 minutos | Duración (sliding) del caché en memoria de `ITermService`. |
| `ActiveCatalogCacheDuration` | 5 minutos | Duración (sliding) del caché en memoria del catálogo activo (`IThemeCatalogService.GetActiveAsync()`). |
| `FaviconUploadFolder` | `"Uploads/icons"` | Carpeta pasada a `IThemeFileStorageService.SaveFileAsync` al crear un favicon. |
| `LogoUploadFolder` | `"Uploads/logos"` | Ídem para logos. |
| `MaxUploadSizeBytes` | 10 MB | Tamaño máximo que los componentes de la librería permiten seleccionar antes de enviarlo a `IThemeFileStorageService`. |
| `AllowedAssetContentTypes` | `image/svg+xml`, `image/png`, `image/jpeg`, `image/x-icon`, `image/vnd.microsoft.icon`, `image/webp` | Tipos MIME permitidos para favicon/logo. Vacío o `null` desactiva la validación. |

### Componentes Razor reutilizables (`Components/Theme/*.razor`)

| Componente | Parámetros | Descripción |
|---|---|---|
| `ThemeConfig` | — | Compone los tres componentes siguientes en una sola pantalla de administración. **No declara ninguna ruta (`@page`) propia** — el consumidor lo monta dentro de su propia página. |
| `ThemeFaviconAndLogoConfig` | `SelectThemeCatalog` (`ThemeCatalog?`) | Administración de favicon/logo del catálogo indicado. |
| `ThemePaletteSelector` | `ThemeCatalogChanged` (`EventCallback<ThemeCatalog>`) | Editor de paleta (envuelve a `MudThemeManager`), selector de catálogos existentes, guardado y activación. |
| `ThemeTermConfig` | — | Grilla editable de terminología (`ThemeTerm`). |
| `ComponentsPreview` | — | Vitrina de componentes MudBlazor comunes (botones, alertas, chips, etc.) para previsualizar el tema en vivo. Sin estado propio: hereda el tema del `MudThemeProvider` ambiental, igual que cualquier página de la app. |

### Integración con Entity Framework Core

- 4 `IEntityTypeConfiguration<T>` en `DataAccess/Configurations/` (`ThemeCatalogConfiguration`, `ThemeAssetConfiguration`, `ThemePresentConfiguration`, `ThemeTermConfiguration`) — el punto de entrada soportado es `ModelBuilder.ApplyThemeManagerPersistenceModel()`, no aplicarlas una por una.
- Reproducen exactamente lo que las convenciones de EF Core ya generaban a partir de las entidades (mismas claves, mismas relaciones, mismo índice único en `ThemeCatalog.Name` — declarado con `[Index]` directamente en la entidad, no duplicado en la configuración fluida).

### Eventos / mecanismos de notificación

| Evento | Declarado en | Cuándo se dispara |
|---|---|---|
| `IThemeManagerService.OnThemeChanged` (`event Func<object, Task>?`) | `IThemeManagerService` | En memoria, en cada edición de paleta desde `MudThemeManager`/`ThemePaletteSelector`, **antes** de cualquier persistencia. |
| `IThemeCatalogService.ThemeCatalogActivated` (`event Func<ThemeCatalog, Task>?`) | `IThemeCatalogService` | Después de que `ActivateAsync` completa exitosamente. |

Ver [RUNTIME.md](RUNTIME.md) para el detalle de qué NO tiene un evento hoy (logo, favicon, términos).

### Recursos activos disponibles

Ver [BRANDING.md](BRANDING.md) para logo/favicon y [THEMES.md](THEMES.md) para el tema. Resumen de los métodos de lectura:

| Recurso | Método |
|---|---|
| Tema activo | `IThemeCatalogService.GetActiveAsync()` |
| Logo activo | `IThemeLogoService.GetCurrentLogoPathAsync()` |
| Favicon activo | `IThemeFaviconService.GetCurrentFaviconPathAsync()` |
| Fallback estático (sin ningún asset activo aún) | `StaticAssets.ThemeDefaultAssets.DefaultLogoPath` / `.DefaultFaviconPath` |

### Tipos públicos que existen pero **no** están pensados para consumo directo

| Tipo | Por qué no se documenta como API de consumo |
|---|---|
| `IGenericRepository<TEntity>`, `GenericRepository<TEntity, TContext>` | Capa de persistencia genérica interna de la librería. Es pública (necesaria para el registro de DI), pero el consumidor debe usar los servicios de aplicación (`IThemeCatalogService`, etc.), no los repositorios. |
| `IThemeCatalogRepository`, `IThemeAssetRepository`, `IThemePresentRepository`, `IThemeTermRepository` | Interfaces marcador sin miembros propios (`: IGenericRepository<T>`), registradas en DI para uso interno de los servicios de aplicación. |
| `DataAccess.Configurations.*Configuration` | Se usan a través de `ApplyThemeManagerPersistenceModel()`, no individualmente. |
| `Application.Terminology.TermService`, `Application.Assets.ThemeAssetOperations` (`internal`), `Application.ExclusiveActivationHelper` (`internal`) | Implementaciones internas; no son accesibles fuera del ensamblado (`ThemeAssetOperations`/`ExclusiveActivationHelper`) o no deben instanciarse directamente (`TermService`, resuélvase siempre vía `ITermService`). |
