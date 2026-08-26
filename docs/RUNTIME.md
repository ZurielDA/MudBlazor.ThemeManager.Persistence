# Actualización en runtime

## Lo que sí tiene un mecanismo de notificación hoy

### Cambios de tema (edición en vivo, antes de guardar)

```csharp
public interface IThemeManagerService
{
    event Func<object, Task>? OnThemeChanged;
    Task ChangeTheme(object theme);
}
```

Cada vez que el usuario modifica algo en el editor `MudThemeManager` (color, tipografía, elevación, etc.), `ThemePaletteSelector.razor` llama a `IThemeManagerService.ChangeTheme(nuevoTema)`, que dispara el evento `OnThemeChanged` a todos los suscriptores. Este evento representa el tema **en memoria**, todavía no persistido — es lo que le permite, por ejemplo, al `Layout` raíz de la aplicación actualizar su `MudThemeProvider` y mostrar el cambio de inmediato en toda la interfaz, sin recargar la página ni esperar a que el usuario guarde.

Patrón de suscripción (usado por `samples/TestHost/Components/Layout/MainLayout.razor`):

```csharp
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
```

Si nadie está suscrito, `ChangeTheme` simplemente no hace nada (no lanza excepción).

### Activación de un catálogo (después de guardar)

```csharp
public interface IThemeCatalogService
{
    // ...
    event Func<ThemeCatalog, Task>? ThemeCatalogActivated;
}
```

Se dispara **después** de que `ActivateAsync(id)` completa exitosamente, con el `ThemeCatalog` recién activado como argumento. Permite a un consumidor reaccionar sin tener que volver a consultar `GetActiveAsync()` por su cuenta — por ejemplo, para refrescar el favicon del `<head>` en el momento en que un administrador activa un tema distinto, sin que el resto de los usuarios conectados necesiten recargar la página (dentro de las limitaciones normales de Blazor Server: cada circuito de usuario tendría que tener su propio suscriptor).

## Lo que NO tiene un mecanismo de notificación hoy

No inventar uno aquí — esto es simplemente el estado actual del código:

- **No hay un evento para logo/favicon.** Crear, activar o eliminar un `ThemeAsset` (vía `IThemeFaviconService`/`IThemeLogoService`) no dispara ningún evento. Un consumidor que necesite reflejar el cambio en vivo (por ejemplo, refrescar el logo mostrado en el AppBar de todos los usuarios conectados apenas un administrador lo cambia) debe volver a llamar a `GetCurrentLogoPathAsync()`/`GetCurrentFaviconPathAsync()` por su cuenta — hoy no hay forma de enterarse del cambio sin re-consultar.
- **No hay un evento para terminología.** Crear, actualizar o eliminar un `ThemeTerm` no dispara ningún evento; solo existe la invalidación manual de caché (`ITermService.InvalidateCache()`, ver [ADMINISTRATION.md](ADMINISTRATION.md)) para que la *propia sesión* que hizo el cambio deje de leer el valor cacheado — eso no notifica a ningún otro circuito de usuario conectado.
- **No hay un evento para la creación o eliminación de catálogos** (`CreateWithThemePresentAsync`, `DeleteAsync`) — solo `ActivateAsync` dispara `ThemeCatalogActivated`.
- El caché de `GetActiveAsync()` (`IThemeCatalogService`) se auto-expira por tiempo (`ThemeManagerPersistenceOptions.ActiveCatalogCacheDuration`, 5 minutos por defecto) — esto significa que, incluso sin ningún evento, el catálogo activo eventualmente se refresca por sí solo para cualquier lector nuevo después de ese lapso, pero no es un mecanismo de notificación en tiempo real.

En resumen: si la aplicación consumidora necesita que un cambio de branding o terminología se refleje **de inmediato** en todos los circuitos de usuario activos (no solo en el que hizo el cambio), no existe hoy ninguna pieza de la librería que resuelva eso — habría que construirlo aparte (por ejemplo, con un mecanismo propio de notificación entre circuitos), lo cual queda fuera del alcance de esta librería tal como está implementada.
