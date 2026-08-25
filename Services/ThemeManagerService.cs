using SAMACDX.MudBlazor.ThemeManager.Persistence.Interfaces.Services.Theme;

public class ThemeManagerService : IThemeManagerService
{
    public event Func<object, Task>? OnThemeChanged;

    public async Task ChangeTheme(object theme)
    {
        if (theme != null)
        {
            await OnThemeChanged.Invoke(theme);
        }
    }
}
