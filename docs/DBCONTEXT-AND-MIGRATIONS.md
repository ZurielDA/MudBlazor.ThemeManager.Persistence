# Integración con DbContext y migraciones

## Qué debe proveer la aplicación consumidora

Un `DbContext` propio (por ejemplo `AppDbContext`) que:

1. Aplique el modelo de esta librería en `OnModelCreating`:

    ```csharp
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyThemeManagerPersistenceModel();
    }
    ```

2. Opcionalmente, exponga `DbSet<T>` para las 4 entidades (recomendado, no obligatorio — ver más abajo):

    ```csharp
    public DbSet<ThemeCatalog> ThemeCatalogs => Set<ThemeCatalog>();
    public DbSet<ThemeAsset> ThemeAssets => Set<ThemeAsset>();
    public DbSet<ThemePresent> ThemesPresent => Set<ThemePresent>();
    public DbSet<ThemeTerm> ThemeTerms => Set<ThemeTerm>();
    ```

Este es exactamente el patrón que usa `samples/TestHost/Data/TestDbContext.cs`, el único consumidor real de esta librería probado hasta ahora.

## Cómo registra la librería sus servicios

Ver [INSTALLATION-AND-CONFIGURATION.md](INSTALLATION-AND-CONFIGURATION.md) — en resumen, `services.AddThemeManagerPersistence<TContext>()` registra los repositorios de la librería resolviendo automáticamente si debe usar `IDbContextFactory<TContext>` (recomendado) o un `TContext` scoped externo, sin que el consumidor tenga que elegir explícitamente cuál — simplemente registrando uno de los dos, la librería lo detecta.

## Cómo se agregan las entidades/configuraciones

La librería NO expone sus entidades como parte de un `DbContext` propio: son clases POCO normales (`ThemeCatalog`, `ThemeAsset`, `ThemePresent`, `ThemeTerm`, en los namespaces `Entities.ThemeCatalog`/`Entities.Theme`) más 4 clases `IEntityTypeConfiguration<T>` (`DataAccess/Configurations/*`, namespace `DataAccess.Configurations`). El único punto de entrada soportado es la extensión `ModelBuilder.ApplyThemeManagerPersistenceModel()` (namespace `Extensions`), que aplica las 4 configuraciones de una sola vez.

**Declarar `DbSet<T>` es opcional.** `modelBuilder.ApplyConfiguration(new ThemeCatalogConfiguration())` (y las otras 3) ya registra cada tipo en el modelo de EF Core, con o sin una propiedad `DbSet<T>` en el `DbContext`. Los repositorios internos de la librería acceden a las entidades vía `context.Set<TEntity>()`, no por el nombre de una propiedad `DbSet`. Declarar los `DbSet<T>` (con el nombre que se prefiera — `samples/TestHost` usa `ThemeCatalogs`, `ThemeAssets`, `ThemesPresent`, `ThemeTerms`) sigue siendo recomendable porque:

- Las herramientas de migraciones de EF Core (`dotnet ef migrations add`) y de tooling en general suelen ser más predecibles cuando el `DbSet<T>` está declarado explícitamente.
- Permite a la aplicación consumidora consultar estas entidades directamente si lo necesita, sin pasar por los servicios de la librería.

## Qué resuelve la librería internamente

- Las relaciones entre las 4 entidades (`ThemeCatalog` 1—1 `ThemePresent`, `ThemeCatalog` 1—N `ThemeAsset`), configuradas una sola vez desde `ThemeCatalogConfiguration` (el lado "uno" de ambas relaciones) para no declararlas por duplicado.
- El índice único sobre `ThemeCatalog.Name` (declarado con el atributo `[Index(nameof(Name), IsUnique = true)]` directamente en la entidad, no en la configuración fluida).
- Las validaciones de "requerido" (`IsRequired()`) sobre las columnas de texto de cada entidad.

Las 4 configuraciones **reproducen exactamente lo que las convenciones de EF Core ya generaban** a partir de las entidades — no fijan nombres de tabla (`.ToTable(...)` no se usa en ninguna) ni cambian ningún comportamiento respecto a dejar que EF Core infiera todo por convención. Su propósito es dejar el modelo documentado y aplicable de forma explícita, no cambiar el esquema resultante.

## Cómo se generan y aplican las migraciones

La librería no trae ningún `DbContext` ni migraciones propias — las migraciones se generan sobre el `DbContext` de la aplicación consumidora, una vez que ese `DbContext` tiene `ApplyThemeManagerPersistenceModel()` en su `OnModelCreating`:

```
dotnet ef migrations add AddThemeManagerPersistence -c AppDbContext
dotnet ef database update -c AppDbContext
```

Si la base de datos ya existía (por ejemplo, creada antes con `EnsureCreatedAsync()` a partir de las convenciones por defecto de EF Core), aplicar el modelo explícito de esta librería no debería introducir cambios de esquema, porque las 4 configuraciones son deliberadamente un no-op respecto a las convenciones. Aun así, siempre es responsabilidad de la app consumidora revisar la migración generada antes de aplicarla contra una base de datos con datos reales.

## Qué responsabilidades permanecen en el proyecto consumidor

- Elegir y configurar el proveedor de EF Core (SQLite, SQL Server, etc.) y la cadena de conexión.
- Decidir si usa `AddDbContextFactory<TContext>()` (recomendado) o `AddDbContext<TContext>()` (ver la diferencia de comportamiento de `SaveChangesAsync` en [INSTALLATION-AND-CONFIGURATION.md](INSTALLATION-AND-CONFIGURATION.md)).
- Generar, revisar y aplicar sus propias migraciones (la librería no las genera ni las aplica automáticamente).
- Cualquier entidad propia de la aplicación que no sea parte de este módulo — la librería no toca ni conoce el resto del modelo del `DbContext` consumidor.
