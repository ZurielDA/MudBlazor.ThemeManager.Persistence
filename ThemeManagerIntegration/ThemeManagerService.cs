using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;

namespace SAMACDX.ThemeManager.Persistence.ThemeManagerIntegration
{
    public class ThemeManagerService : IThemeManagerService
    {
        public event Func<object, Task>? OnThemeChanged;

        public async Task ChangeTheme(object theme)
        {
            if (theme is null)
            {
                return;
            }

            // Copiar el delegate a una variable local antes de comparar/invocar
            // evita una condicion de carrera si un suscriptor se desuscribe
            // justo entre el chequeo y la invocacion. Si nadie esta suscrito
            // (caso muy real: un consumidor nuevo que aun no conecto su
            // MudThemeProvider a este evento) esto ya no lanza
            // NullReferenceException, simplemente no hace nada.
            var handler = OnThemeChanged;

            if (handler is not null)
            {
                await handler.Invoke(theme);
            }
        }
    }
}
