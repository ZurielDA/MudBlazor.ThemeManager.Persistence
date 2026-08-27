namespace SAMACDX.ThemeManager.Persistence.Application
{
    /// <summary>
    /// Helper interno que centraliza el patron "dentro de un grupo, solo un
    /// elemento puede estar activo a la vez", usado por
    /// ThemePresentService.ActivateAsync y por ThemeAssetOperations.ActivateAsync
    /// (compartido entre ThemeFaviconService y ThemeLogoService).
    /// </summary>
    internal static class ExclusiveActivationHelper
    {
        public static void ActivateOnly<T>(IEnumerable<T> group, int targetId, Func<T, int> idSelector, Action<T, bool> setActive)
        {
            foreach (var item in group)
            {
                setActive(item, idSelector(item) == targetId);
            }
        }
    }
}
