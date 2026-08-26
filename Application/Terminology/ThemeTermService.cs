using SAMACDX.ThemeManager.Persistence.Interfaces.Services.Theme;
using SAMACDX.ThemeManager.Persistence.Entities.Theme;
using SAMACDX.ThemeManager.Persistence.DataAccess.Abstractions;

namespace SAMACDX.ThemeManager.Persistence.Application.Terminology
{
    public class ThemeTermService : IThemeTermService
    {        
        private readonly IThemeTermRepository _themeTermRepository;

        public ThemeTermService(IThemeTermRepository themeTermRepository)
        {
            _themeTermRepository = themeTermRepository;
        }

        public async Task<List<ThemeTerm>> GetAllTermsAsync()
        {
            var result = await _themeTermRepository.GetAllAsync();

            return result.ToList();
        }

        public async Task<ThemeTerm> CreateTermsAsync(ThemeTerm themeTerm)
        {
            ValidateGender(themeTerm);

            return await _themeTermRepository.AddAsync(themeTerm);
        }        

        public async Task<ThemeTerm> UpdateTermsAsync(ThemeTerm themeTerm)
        {            
            ValidateGender(themeTerm);

            await _themeTermRepository.UpdateAsync(themeTerm);

            return themeTerm;
        }

        public async Task DeleteTermsAsync(int id)
        {
            var term = (await _themeTermRepository.FindAsync(t => t.Id == id)).FirstOrDefault();

            if (term is null)
            {
                return;
            }

            await _themeTermRepository.RemoveAsync(term);
        }

        /// <summary>
        /// Valida que Gender sea uno de los valores reconocidos por
        /// SpanishArticleHelper ("Masculine"/"Feminine", sin distinguir
        /// mayusculas/minusculas) ANTES de guardar. Antes de esta validacion,
        /// un valor invalido o un typo no se rechazaba: simplemente caia en
        /// silencio al comportamiento masculino por defecto al leer el
        /// termino, sin ningun aviso. Gender se mantiene como string (sin
        /// cambio de tipo/esquema) para no afectar datos ya persistidos.
        /// </summary>
        private static void ValidateGender(ThemeTerm themeTerm)
        {
            if (!ThemeTermGenderParser.TryParse(themeTerm.Gender, out _))
            {
                throw new ThemeValidationException(
                    $"El genero \"{themeTerm.Gender}\" no es valido. Los valores permitidos son \"Masculine\" y \"Feminine\".");
            }
        }
    }
}
