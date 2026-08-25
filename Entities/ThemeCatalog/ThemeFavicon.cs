using SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.Abstracts;

namespace SAMACDX.MudBlazor.ThemeManager.Persistence.Entities.ThemeCatalog
{
    public class ThemeFavicon : AuditableEntity
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsActive { get; set; } = false;

        public int ThemeCatalogId { get; set; }

        public ThemeCatalog ThemeCatalog { get; set; }
    }
}
