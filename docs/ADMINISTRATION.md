# Administración

Esta página distingue las operaciones de **administración** (crean, modifican o eliminan datos — típicamente detrás de una pantalla de configuración restringida) de las operaciones de **lectura/runtime** (consultadas por cualquier parte de la app en el camino normal de renderizado). Las de lectura/runtime están detalladas en [THEMES.md](THEMES.md) y [BRANDING.md](BRANDING.md); esta página se concentra en las de escritura.

## Themes / catálogo de tema — `IThemeCatalogService`

| Operación | Método | Notas |
|---|---|---|
| Crear | `CreateWithThemePresentAsync(ThemeCatalog, ThemePresent)` | Valida nombre no vacío y no duplicado (`ThemeValidationException`). Siempre crea un catálogo **nuevo** (no hay "editar en el lugar" — ver [THEMES.md](THEMES.md)). |
| Activar | `ActivateAsync(int id)` | Exclusivo: desactiva todos los demás. Invalida la caché de `GetActiveAsync()`. Dispara `ThemeCatalogActivated`. |
| Eliminar | `DeleteAsync(int id)` | Lanza `ThemeValidationException` si el catálogo es el base (`IsBase`) o el actualmente activo (`IsActive`). No hace nada (no lanza) si el `id` no existe. |

No hay una operación de "editar el nombre" o "actualizar" un `ThemeCatalog` existente expuesta por el servicio — solo crear, activar y eliminar.

## Configuración visual del tema — `IThemePresentService`

| Operación | Método | Notas |
|---|---|---|
| Crear | `CreateAsync(ThemePresent)` | Normalmente no se llama sola: `ThemeCatalogService.CreateWithThemePresentAsync` la invoca internamente como parte de la creación de un catálogo. |

No hay `UpdateAsync` — ver la observación correspondiente en [THEMES.md](THEMES.md) y en el README principal.

## Branding — `IThemeFaviconService` / `IThemeLogoService`

Misma forma para ambos (favicon y logo), solo cambia la interfaz:

| Operación | Método | Notas |
|---|---|---|
| Listar por catálogo | `GetAllByThemeCatalogIdAsync(int themeCatalogId)` | Todos los assets de ese tipo para ese catálogo (activos e inactivos). |
| Crear (subir) | `CreateAsync(ThemeAsset, ThemeAssetFileContent)` | Valida el tipo de contenido contra `AllowedAssetContentTypes` (`ThemeValidationException` si no está permitido). Guarda el archivo vía `IThemeFileStorageService.SaveFileAsync` y persiste el `ThemeAsset` con la ruta resultante. |
| Activar | `ActivateAsync(int themeCatalogId, int themeAssetId)` | Exclusivo dentro de `(themeCatalogId, Type)`: desactiva los demás assets de ese tipo en ese catálogo, activa el indicado. |
| Eliminar | `DeleteAsync(int themeAssetId)` | Elimina la fila y llama a `IThemeFileStorageService.DeleteFileAsync` sobre su archivo físico. No hace nada si el `id` no existe (o no corresponde a ese tipo). |

## Terminología — `IThemeTermService` / `ITermService`

| Operación | Método | Notas |
|---|---|---|
| Listar | `IThemeTermService.GetAllTermsAsync()` | Todos los `ThemeTerm`, sin caché (a diferencia de la lectura vía `ITermService`). |
| Crear | `IThemeTermService.CreateTermsAsync(ThemeTerm)` | Valida `Gender` (`"Masculine"`/`"Feminine"`, sin distinguir mayúsculas/minúsculas) — `ThemeValidationException` si no es reconocido. |
| Actualizar | `IThemeTermService.UpdateTermsAsync(ThemeTerm)` | Misma validación de `Gender`. |
| Eliminar | `IThemeTermService.DeleteTermsAsync(int id)` | No hace nada si el `id` no existe. |
| Invalidar caché de lectura | `ITermService.InvalidateCache()` | **Debe llamarse manualmente** después de `CreateTermsAsync`/`UpdateTermsAsync`/`DeleteTermsAsync` si se quiere que `ITermService` (la interfaz de lectura, cacheada) refleje el cambio de inmediato — ninguno de los tres métodos de `IThemeTermService` la invoca por sí mismo. El componente `ThemeTermConfig.razor` que trae la librería ya lo hace correctamente después de cada edición; una UI de administración de términos escrita a medida (sin reusar ese componente) debe recordar hacerlo también. |

## Componentes de administración listos para usar

La librería trae los componentes Razor que implementan toda esta administración (`Components/Theme/*.razor` — ver inventario en [ARCHITECTURE.md](ARCHITECTURE.md#componentes-razor-reutilizables-componentstheme-razor)). Estos ya manejan errores (try/catch con mensajes vía `ISnackbar`, distinguiendo `ThemeValidationException` de errores inesperados) y las invalidaciones de caché correspondientes. Montarlos directamente (dentro de la propia página/ruta del consumidor, ya que ninguno declara su propio `@page`) es la forma más rápida de tener una pantalla de administración funcional, sin tener que reimplementar las llamadas a los servicios de arriba.
