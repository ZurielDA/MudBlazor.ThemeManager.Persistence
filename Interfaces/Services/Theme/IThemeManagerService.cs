
namespace SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme
{
    public interface IThemeManagerService
    {
        event Func<object, Task>? OnThemeChanged;

        Task ChangeTheme(object theme);
    }
}
