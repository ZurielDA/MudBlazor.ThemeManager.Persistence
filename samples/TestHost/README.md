# Test Host — cómo probar la librería

App Blazor Server mínima, dentro de este mismo repo, que consume
`SAMACDX.MudBlazor.ThemeManager.Persistence` exactamente como lo haría
cualquier app real (GDIP incluido, cuando se integre): sólo referencia el
proyecto, llama a `AddThemeManagerPersistence<TContext>()` e implementa
`IThemeFileStorageService`. No es parte de la librería en sí — es sólo un
arnés de pruebas para verla funcionando sin depender de GDIP.

Usa SQLite (un archivo local `themetesthost.db`, creado automáticamente)
para no requerir SQL Server/LocalDB. Al iniciar, crea la base (si no
existe) y siembra los datos por defecto del módulo Theme (mismo orden que
usa GDIP: catálogo → present → términos → favicons → logos).

## Cómo correrlo

Desde la raíz del repo:

```
dotnet run --project samples/TestHost
```

O ábrelo en Visual Studio: `samples/TestHost/SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost.csproj`
como proyecto de inicio (F5).

## Qué probar

- **`/`** (Inicio): muestra el tema activo, el favicon/logo activos y el
  resultado de `IThemeLogoService.GetCurrentLogoPathAsync()` — confirma que
  la recuperación de configuración activa funciona.
- **`/ThemeCatalog`** (Administrar Theme): la UI completa migrada de GDIP.
  Ahí puedes:
  - Ver los temas existentes y crear uno nuevo (nombre + configuración
    visual con `MudThemeManager`).
  - Activar un tema (comprueba persistencia + selección).
  - Subir y activar un favicon y un logo (comprueba `IThemeFileStorageService`
    — los archivos quedan en `wwwroot/Uploads/icons` y
    `wwwroot/Uploads/logos`).
  - Editar la terminología (tabla editable de `ThemeTerm`) y ver que se
    guarda al hacer clic fuera de la celda.
- **Reiniciar la app** (`Ctrl+C` y volver a `dotnet run`): el tema/branding
  activo debe seguir siendo el mismo que dejaste — confirma persistencia a
  través de reinicios usando el archivo SQLite.

Si algo falla aquí, es un problema de la librería en sí (no de GDIP ni de
esta app de prueba), así que es el lugar correcto para aislar y reportar
errores antes de integrarla a GDIP.
