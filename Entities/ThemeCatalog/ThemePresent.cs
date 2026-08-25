using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Abstracts;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog
{
    public class ThemePresent : AuditableEntity
    {
        public int Id { get; set; }

        public string JsonData { get; set; }

        public int ThemeCatalogId { get; set; }

        public ThemeCatalog ThemeCatalog { get; set; }
    }
}
