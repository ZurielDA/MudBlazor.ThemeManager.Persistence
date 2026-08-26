# Themes

## `MudBlazor.ThemeManager` vs. esta librería

`MudBlazor.ThemeManager` (el componente `MudThemeManager`) es el **editor visual**: mantiene un `ThemeManagerTheme` en memoria y permite modificarlo interactivamente (paleta, tipografía, elevaciones, radio de borde, modo claro/oscuro). No sabe guardar nada ni tiene ningún concepto de "catálogo" — cuando el usuario cierra la pestaña, cualquier cambio no guardado se pierde.

`SAMACDX.ThemeManager.Persistence` es la **persistencia**: envuelve cada `ThemeManagerTheme` guardado dentro de un `ThemeCatalog` (con nombre) y su `ThemePresent` (el JSON serializado del tema), y ofrece las operaciones para listarlos, activarlos y recuperarlos — incluso después de reiniciar la aplicación, porque viven en la base de datos, no en memoria.

## Obtener los temas disponibles

```csharp
List<ThemeCatalog> temas = await ThemeCatalogService.GetAllAsync();
```

`IThemeCatalogService.GetAllAsync()` devuelve todos los catálogos existentes (sin filtrar). Cada `ThemeCatalog` trae `Id`, `Name`, `IsBase`, `IsActive` — pero **no** trae `ThemePresent`/`ThemeAssets` cargados (a diferencia de `GetActiveAsync()`, ver abajo); si se necesita el JSON de un catálogo puntual de esta lista, hay que pedirlo aparte con `IThemePresentService.GetByThemeIdAsync(id)`.

## Obtener el tema activo

```csharp
ThemeCatalog? activo = await ThemeCatalogService.GetActiveAsync();
```

`IThemeCatalogService.GetActiveAsync()` devuelve el catálogo con `IsActive == true`, con `ThemePresent` y `ThemeAssets` ya incluidos (los `ThemeAssets` vienen filtrados a solo los que están `IsActive == true`). El resultado se cachea en memoria (`ThemeManagerPersistenceOptions.ActiveCatalogCacheDuration`, 5 minutos por defecto) — la caché se invalida automáticamente al llamar a `ActivateAsync`.

Para reconstruir el objeto de tema que entiende `MudBlazor.ThemeManager`:

```csharp
ThemeManagerTheme? tema = activo?.ThemePresent?.JsonData is { } json
    ? JsonHelper.Deserialize<ThemeManagerTheme>(json)
    : null;
```

## Obtener el tema base

```csharp
ThemeCatalog? baseTheme = await ThemeCatalogService.GetBaseAsync();
```

`IThemeCatalogService.GetBaseAsync()` devuelve el catálogo con `IsBase == true` (con `ThemePresent` incluido). No hay un método de la librería que **establezca** cuál catálogo es el base — `IsBase` se asigna directamente sobre la entidad `ThemeCatalog` al crearla, si la aplicación consumidora necesita ese concepto.

## Obtener un tema específico

**No existe un `GetByIdAsync(int id)` en `IThemeCatalogService`.** Para obtener un catálogo puntual por su Id, hoy la única forma soportada es filtrar el resultado de `GetAllAsync()`:

```csharp
var todos = await ThemeCatalogService.GetAllAsync();
var especifico = todos.FirstOrDefault(t => t.Id == id);
```

Si solo se necesita la configuración visual (el JSON) de un catálogo puntual, sin el resto de sus campos, es más directo usar:

```csharp
ThemePresent? present = await ThemePresentService.GetByThemeIdAsync(id);
```

## Seleccionar un tema (previsualizar) vs. activar un tema (persistir la elección)

Estos son dos pasos distintos y no deben confundirse:

- **Seleccionar/previsualizar**: elegir un catálogo existente del desplegable de `MudThemeManager` (`ThemePresets`) dispara `ThemePresetsChanged`, que en `ThemePaletteSelector.razor` se traduce en cargar el `ThemePresent` de ese catálogo (`IThemePresentService.GetByThemeIdAsync`) y aplicarlo **en memoria** vía `IThemeManagerService.ChangeTheme(...)` — esto actualiza cualquier `MudThemeProvider` suscrito, pero **no cambia qué catálogo está activo en la base de datos**.
- **Activar**: confirma que ese catálogo (el que se está previsualizando) pasa a ser el vigente para toda la aplicación, de forma persistente:

    ```csharp
    List<ThemeCatalog> catalogos = await ThemeCatalogService.ActivateAsync(id);
    ```

    `ActivateAsync` desactiva todos los demás catálogos, activa el indicado, invalida la caché de `GetActiveAsync()`, y dispara el evento `ThemeCatalogActivated`. Devuelve la lista completa de catálogos con su `IsActive` actualizado.

## Persistir cambios

```csharp
ThemeCatalog nuevo = await ThemeCatalogService.CreateWithThemePresentAsync(
    new ThemeCatalog { Name = "Mi tema" },
    new ThemePresent { JsonData = JsonHelper.Serialize(temaEditado) }
);
```

`CreateWithThemePresentAsync` valida que `Name` no esté vacío ni duplicado (lanza `ThemeValidationException` si lo está), crea el `ThemeCatalog`, y luego su `ThemePresent` asociado; si la segunda escritura falla, revierte (elimina) el catálogo recién creado para no dejarlo huérfano.

**Importante: esto siempre crea un catálogo nuevo.** No existe un método para sobrescribir el `ThemePresent` de un catálogo **ya existente** — `IThemePresentService` solo tiene `CreateAsync`, no `UpdateAsync`. En la práctica, "guardar cambios" sobre un tema que ya se había guardado antes da de alta un catálogo adicional con otro nombre; no hay una operación de "guardar sobre el mismo tema". Esto es el comportamiento actual de `ThemePaletteSelector.razor` tal cual está implementado hoy.

## Recuperar el tema después de reiniciar la aplicación

No requiere ninguna acción especial: como el tema activo vive en la base de datos (`ThemeCatalog.IsActive` + `ThemePresent.JsonData`), simplemente volver a llamar a `GetActiveAsync()` al iniciar la aplicación (por ejemplo, en el `OnInitializedAsync`/`OnAfterRenderAsync` del `Layout` raíz) reconstruye el mismo tema que estaba activo antes del reinicio. Es exactamente lo que hace `ThemePaletteSelector.razor` en su `OnAfterRenderAsync`, y lo que documenta el ejemplo de [GETTING-STARTED.md](GETTING-STARTED.md).
