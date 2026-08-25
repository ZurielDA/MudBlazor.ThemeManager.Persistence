namespace SAMACDX.ThemeManager.Persistence.Utilities
{
    /// <summary>
    /// Utilidad estática para generar artículos definidos e indefinidos en español
    /// a partir del género gramatical del término (Masculine / Feminine).
    ///
    /// Implementa la excepción de acentuación femenina:
    /// palabras femeninas que empiezan con 'a' o 'ha' tónicas usan
    /// el artículo "el" en singular (el área, el agua, el hacha).
    /// En plural siempre se usa "las".
    /// </summary>
    public static class SpanishArticleHelper
    {
        // ── Prefijos que desencadenan la excepción de acentuación femenina ──────────
        // Sílaba 'a' tónica inicial: palabras que comienzan con estas secuencias
        private static readonly string[] FeminineExceptionPrefixes =
        [
            "ha", "á", "a"
        ];

        // ── Artículos definidos ───────────────────────────────────────────────────────

        /// <summary>
        /// Retorna el artículo definido singular correcto para el género y término dados.
        /// Aplica la excepción de 'a' tónica: "el área", "el agua" (femenino → "el").
        /// </summary>
        public static string DefiniteArticle(string gender, string singular)
        {
            if (IsFeminine(gender))
            {
                return StartsWithAtonicA(singular) ? "el" : "la";
            }

            return "el";
        }

        /// <summary>
        /// Retorna el artículo definido plural correcto para el género dado.
        /// La excepción de 'a' tónica NO aplica en plural.
        /// </summary>
        public static string DefiniteArticlePlural(string gender)
        {
            return IsFeminine(gender) ? "las" : "los";
        }

        // ── Artículos indefinidos ─────────────────────────────────────────────────────

        /// <summary>
        /// Retorna el artículo indefinido singular correcto para el género y término dados.
        /// Aplica la excepción de 'a' tónica: "un área", "un agua" (femenino → "un").
        /// </summary>
        public static string IndefiniteArticle(string gender, string singular)
        {
            if (IsFeminine(gender))
            {
                return StartsWithAtonicA(singular) ? "un" : "una";
            }

            return "un";
        }

        /// <summary>
        /// Retorna el artículo indefinido plural correcto para el género dado.
        /// </summary>
        public static string IndefiniteArticlePlural(string gender)
        {
            return IsFeminine(gender) ? "unas" : "unos";
        }

        // ── Helpers de composición ────────────────────────────────────────────────────

        /// <summary>
        /// Compone: artículo definido singular + término en minúsculas.
        /// Ejemplo: "Feminine", "Auditoría" → "la auditoría"
        /// </summary>
        public static string WithDefiniteArticle(string gender, string singular)
        {
            return $"{DefiniteArticle(gender, singular)} {singular.ToLowerInvariant()}";
        }

        /// <summary>
        /// Compone: artículo definido plural + plural en minúsculas.
        /// Ejemplo: "Feminine", "Auditorías" → "las auditorías"
        /// </summary>
        public static string WithDefiniteArticlePlural(string gender, string plural)
        {
            return $"{DefiniteArticlePlural(gender)} {plural.ToLowerInvariant()}";
        }

        /// <summary>
        /// Compone: artículo indefinido singular + término en minúsculas.
        /// Ejemplo: "Feminine", "Entidad Fiscalizada" → "una entidad fiscalizada"
        /// </summary>
        public static string WithIndefiniteArticle(string gender, string singular)
        {
            return $"{IndefiniteArticle(gender, singular)} {singular.ToLowerInvariant()}";
        }

        /// <summary>
        /// Compone: artículo indefinido plural + plural en minúsculas.
        /// Ejemplo: "Masculine", "Documentos" → "unos documentos"
        /// </summary>
        public static string WithIndefiniteArticlePlural(string gender, string plural)
        {
            return $"{IndefiniteArticlePlural(gender)} {plural.ToLowerInvariant()}";
        }

        // ── Lógica interna ────────────────────────────────────────────────────────────

        private static bool IsFeminine(string gender)
        {
            return string.Equals(gender, "Feminine", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si una palabra femenina empieza con 'a' tónica,
        /// activando la excepción de artículo "el/un" en lugar de "la/una".
        /// Detecta: palabras que comienzan con 'á', 'a' (si la primera sílaba es tónica),
        /// o 'ha' seguido de vocal tónica (hacha, hada → excepciones conocidas).
        ///
        /// NOTA: Para máxima precisión, los casos especiales deben registrarse
        /// en el campo ThemeTerm.Special (ej: Special = "el área").
        /// Este algoritmo cubre los casos más comunes y sirve como fallback.
        /// </summary>
        private static bool StartsWithAtonicA(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            var lower = word.ToLowerInvariant().TrimStart();

            // Empieza con á (siempre tónica)
            if (lower.StartsWith('á'))
                return true;

            // Empieza con "ha" + vocal tónica (hacha, hada, hacha...)
            if (lower.StartsWith("ha") && lower.Length > 2 && IsVowel(lower[2]))
                return true;

            // Empieza con 'a' + consonante (agua, alma, arma, área...)
            // Excluimos palabras donde la 'a' es átona (por ej. "amiga" normalmente es tónica
            // pero estos casos son difíciles sin diccionario completo)
            // Heurística: si empieza con 'a' seguido de consonante, aplicar excepción
            if (lower.StartsWith('a') && lower.Length > 1 && !IsVowel(lower[1]))
                return true;

            return false;
        }

        private static bool IsVowel(char c)
        {
            return "aeiouáéíóú".Contains(c);
        }
    }
}
