using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Abstracts;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Theme
{
    public class ThemeTerm  : AuditableEntity
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Singular { get; set; }

        public string Plural { get; set; }

        public string Gender { get; set; }

        public string Special { get; set; }
    }
}
