namespace SAMACDX.ThemeManager.Persistence.Application.Terminology
{
    /// <summary>
    /// Representacion fuertemente tipada de los valores validos para
    /// ThemeTerm.Gender. La entidad ThemeTerm conserva Gender como string (sin
    /// cambio de esquema/tipo de columna, para no arriesgar datos ya
    /// persistidos ni forzar una migracion); este enum y su parser solo se
    /// usan como ayuda de validacion en el punto de escritura
    /// (ThemeTermService.CreateTermsAsync/UpdateTermsAsync), para detectar un
    /// valor invalido en el momento en que se guarda en vez de que caiga en
    /// silencio al comportamiento masculino por defecto en SpanishArticleHelper.
    /// </summary>
    public enum ThemeTermGender
    {
        Masculine,
        Feminine
    }

    public static class ThemeTermGenderParser
    {
        public static bool TryParse(string? gender, out ThemeTermGender result)
        {
            if (string.Equals(gender, nameof(ThemeTermGender.Feminine), StringComparison.OrdinalIgnoreCase))
            {
                result = ThemeTermGender.Feminine;
                return true;
            }

            if (string.Equals(gender, nameof(ThemeTermGender.Masculine), StringComparison.OrdinalIgnoreCase))
            {
                result = ThemeTermGender.Masculine;
                return true;
            }

            result = default;
            return false;
        }
    }
}
