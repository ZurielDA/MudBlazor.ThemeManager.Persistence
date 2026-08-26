using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog
{
    [Index(nameof(Name), IsUnique = true)]
    public class ThemeCatalog
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public bool IsBase { get; set; } = false;

        public bool IsActive { get; set; } = false;

        public ThemePresent ThemePresent { get; set; }

        public ICollection<ThemeAsset> ThemeAssets { get; set; }
    }
}
