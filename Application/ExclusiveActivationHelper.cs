namespace SAMACDX.ThemeManager.Persistence.Application
{
    /// <summary>
    /// Helper interno que centraliza el patron "dentro de un grupo, solo un
    /// elemento puede estar activo a la vez", repetido de forma casi identica
    /// en ThemeCatalogService.ActivateAsync, ThemeFaviconService.ActivateAsync
    /// y ThemeLogoService.ActivateAsync antes de esta etapa. No cambia el
    /// comportamiento de ninguno de los tres: solo evita que la logica de
    /// "apagar todos, encender el elegido" viva por triplicado.
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
