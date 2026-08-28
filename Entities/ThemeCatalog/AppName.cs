using Microsoft.EntityFrameworkCore;

namespace SAMACDX.ThemeManager.Persistence.Entities.ThemeCatalog
{
    /// <summary>
    /// Nombre de la aplicacion, con historial: cada fila es un nombre que
    /// estuvo (o esta) en uso. Solo una fila puede tener IsActive == true a
    /// la vez -- el mismo patron de activacion exclusiva que ThemePresent y
    /// ThemeAsset. Sin relacion con ninguna otra entidad de este modulo.
    /// </summary>
    [Index(nameof(Name), IsUnique = true)]
    public class AppName
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
