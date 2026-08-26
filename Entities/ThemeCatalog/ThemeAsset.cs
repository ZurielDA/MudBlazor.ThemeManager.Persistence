namespace SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog
{
    public class ThemeAsset
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public ThemeAssetType Type { get; set; }
        public bool IsActive { get; set; } = false;
        public int ThemeCatalogId { get; set; }
        public ThemeCatalog ThemeCatalog { get; set; }
    }
}
