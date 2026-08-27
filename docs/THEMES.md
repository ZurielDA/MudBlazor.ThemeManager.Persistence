# Themes

## `MudBlazor.ThemeManager` vs. esta librería

`MudBlazor.ThemeManager` (el componente `MudThemeManager`) es el **editor visual**: mantiene un `ThemeManagerTheme` en memoria y permite modificarlo interactivamente (paleta, tipografía, elevaciones, radio de borde, modo claro/oscuro). No sabe guardar nada — cuando el usuario cierra la pestaña, cualquier cambio no guardado se pierde.

`SAMACDX.ThemeManager.Persistence` es la **persistencia**: envuelve cada `ThemeManagerTheme` guardado dentro de un `ThemePresent` (con nombre, e indicadores de si es el tema base y si está activo), y ofrece las operaciones para listarlos, activarlos y recuperarlos — incluso después de reiniciar la aplicación, porque viven en la base de datos, no en memoria.

## Obtener los temas disponibles

```csharp
List<ThemePresent> temas = await ThemePresentService.GetAllAsync();
```

`IThemePresentService.GetAllAsync()` devuelve todos los temas existentes (sin filtrar), cada uno con `Id`, `Name`, `IsBase`, `IsActive` y `JsonData` ya incluidos.

## Obtener el tema activo

```csharp
ThemePresent? activo = await ThemePresentService.GetActiveAsync();
```

`IThemePresentService.GetActiveAsync()` devuelve el tema con `IsActive == true`. El resultado se cachea en memoria (`ThemeManagerPersistenceOptions.ActivePresentCacheDuration`, 5 minutos por defecto) — la caché se invalida automáticamente al llamar a `ActivateAsync`.

Para reconstruir el objeto de tema que entiende `MudBlazor.ThemeManager`:

```csharp
ThemeManagerTheme? tema = activo?.JsonData is { } json
    ? JsonHelper.Deserialize<ThemeManagerTheme>(json)
    : null;
```

## Obtener el tema base

```csharp
ThemePresent? baseTheme = await ThemePresentService.GetBaseAsync();
```

`IThemePresentService.GetBaseAsync()` devuelve el tema con `IsBase == true`. No hay un método de la librería que **establezca** cuál tema es el base — `IsBase` se asigna directamente sobre la entidad `ThemePresent` al crearla, si la aplicación consumidora necesita ese concepto.

## Obtener un tema específico

```csharp
ThemePresent? especifico = await ThemePresentService.GetByIdAsync(id);
```

## Seleccionar un tema (previsualizar) vs. activar un tema (persistir la elección)

Estos son dos pasos distintos y no deben confundirse:

- **Seleccionar/previsualizar**: elegir un tema existente del desplegable de `MudThemeManager` (`ThemePresets`) dispara `ThemePresetsChanged`, que en `ThemePaletteSelector.razor` se traduce en cargar el `ThemePresent` correspondiente (`IThemePresentService.GetByIdAsync`) y aplicarlo **en memoria** vía `IThemeManagerService.ChangeTheme(...)` — esto actualiza cualquier `MudThemeProvider` suscrito, pero **no cambia qué tema está activo en la base de datos**.
- **Activar**: confirma que ese tema (el que se está previsualizando) pasa a ser el vigente para toda la aplicación, de forma persistente:

    ```csharp
    List<ThemePresent> temas = await ThemePresentService.ActivateAsync(id);
    ```

    `ActivateAsync` desactiva todos los demás temas, activa el indicado, invalida la caché de `GetActiveAsync()`, y dispara el evento `ThemePresentActivated`. Devuelve la lista completa de temas con su `IsActive` actualizado.

## Persistir cambios

```csharp
ThemePresent nuevo = await ThemePresentService.CreateAsync(
    new ThemePresent { Name = "Mi tema", JsonData = JsonHelper.Serialize(temaEditado) }
);
```

`CreateAsync` valida que `Name` no esté vacío ni duplicado (lanza `ThemeValidationException` si lo está) y crea el `ThemePresent` en una única escritura.

**Importante: esto siempre crea un tema nuevo.** No existe un método para sobrescribir el `JsonData` de un tema **ya existente** — `IThemePresentService` solo tiene `CreateAsync`, no `UpdateAsync`. En la práctica, "guardar cambios" sobre un tema que ya se había guardado antes da de alta un tema adicional con otro nombre; no hay una operación de "guardar sobre el mismo tema". Esto es el comportamiento actual de `ThemePaletteSelector.razor` tal cual está implementado hoy.

## Eliminar un tema

```csharp
await ThemePresentService.DeleteAsync(id);
```

`DeleteAsync` lanza `ThemeValidationException` si el tema es el base (`IsBase`) o el actualmente activo (`IsActive`). No hace nada (no lanza) si el `id` no existe.

## Recuperar el tema después de reiniciar la aplicación

No requiere ninguna acción especial: como el tema activo vive en la base de datos (`ThemePresent.IsActive` + `ThemePresent.JsonData`), simplemente volver a llamar a `GetActiveAsync()` al iniciar la aplicación (por ejemplo, en el `OnInitializedAsync`/`OnAfterRenderAsync` del `Layout` raíz) reconstruye el mismo tema que estaba activo antes del reinicio. Es exactamente lo que hace `ThemePaletteSelector.razor` en su `OnAfterRenderAsync`, y lo que documenta el ejemplo de [GETTING-STARTED.md](GETTING-STARTED.md).
