using SAMACDX.ThemeManager.Persistence.Entities.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog
{
    [Index(nameof(Name), IsUnique = true)]
    public class ThemeCatalog : AuditableEntity
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public bool IsBase { get; set; } = false;

        public bool IsActive { get; set; } = false;

        public ThemePresent ThemePresent { get; set; }

        public ICollection<ThemeFavicon> ThemeFavicons { get; set; }

        public ICollection<ThemeLogo> ThemeLogos { get; set; }
    }
}
