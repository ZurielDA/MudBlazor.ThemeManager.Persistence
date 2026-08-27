namespace SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog
{
    public class ThemeAsset
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public ThemeAssetType Type { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
