using SAMACDX.ThemeManager.Persistence.TestHost.Components;
using SAMACDX.ThemeManager.Persistence.TestHost.Data;
using SAMACDX.ThemeManager.Persistence.TestHost.Services;
using SAMACDX.ThemeManager.Persistence.Extensions;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContextFactory<TestDbContext>(options =>
    options.UseSqlite("Data Source=themetesthost.db"));

// Esto es lo único que un consumidor real necesita para obtener todo el
// módulo Theme/Branding: repositorios, servicios y componentes.
builder.Services.AddThemeManagerPersistence<TestDbContext>();

// La app consumidora provee su propia implementación de almacenamiento de
// archivos (aquí, una mínima para el test host).
builder.Services.AddScoped<IThemeFileStorageService, LocalFileStorageService>();

var app = builder.Build();

// Crear la base SQLite (si no existe). El tema activo por defecto lo
// determina la implementación actual sin necesitar datos sembrados.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>();
    await using var db = await factory.CreateDbContextAsync();

    await db.Database.EnsureCreatedAsync();

    await db.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS ""ThemeAssets"" (
        ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ThemeAssets"" PRIMARY KEY AUTOINCREMENT,
        ""Name"" TEXT NOT NULL,
        ""Path"" TEXT NOT NULL,
        ""Type"" INTEGER NOT NULL,
        ""IsActive"" INTEGER NOT NULL,
        ""ThemeCatalogId"" INTEGER NOT NULL,
        CONSTRAINT ""FK_ThemeAssets_ThemeCatalogs_ThemeCatalogId"" FOREIGN KEY (""ThemeCatalogId"") REFERENCES ""ThemeCatalogs"" (""Id"") ON DELETE CASCADE
    );");
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SAMACDX.ThemeManager.Persistence.Extensions.ServiceCollectionExtensions).Assembly);

app.Run();
