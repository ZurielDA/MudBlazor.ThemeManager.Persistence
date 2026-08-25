using SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost.Components;
using SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost.Data;
using SAMACDX.MudBlazor.ThemeManager.Persistence.TestHost.Services;
using SAMACDX.ThemeManager.Persistence.Extensions;
using SAMACDX.ThemeManager.Persistence.Interfaces.Services;
using SAMACDX.ThemeManager.Persistence.Persistence.Seeders.Themes;
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

// Crear la base SQLite (si no existe) y sembrar los datos por defecto del
// módulo Theme, en el mismo orden que usa GDIP.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>();
    await using var db = await factory.CreateDbContextAsync();

    await db.Database.EnsureCreatedAsync();

    await ThemeCatalogsSeeder.SeedAsync(db);
    await ThemesPresentSeeder.SeedAsync(db);
    await ThemeTermsSeeder.SeedAsync(db);
    await ThemeFaviconsSeeder.SeedAsync(db);
    await ThemeLogosSeeder.SeedAsync(db);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ServiceCollectionExtensions).Assembly);

app.Run();
