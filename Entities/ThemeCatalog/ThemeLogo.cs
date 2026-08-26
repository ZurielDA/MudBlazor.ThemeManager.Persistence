namespace SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog
{
    public class ThemeLogo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsActive { get; set; } = false;

        public int ThemeCatalogId { get; set; }

        public ThemeCatalog ThemeCatalog { get; set; }
    }
}
