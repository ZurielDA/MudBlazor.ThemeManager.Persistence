namespace SAMACDX.ThemeManager.Persistence.Application
{
    /// <summary>
    /// Excepcion tipada para fallos de validacion de negocio dentro del modulo
    /// Theme/Branding (por ejemplo: nombre de tema duplicado, archivo de asset
    /// con un tipo no permitido, termino con un genero no reconocido).
    ///
    /// Los componentes Razor de la libreria la distinguen explicitamente de
    /// cualquier otra excepcion para mostrar el mensaje de validacion tal cual
    /// (en vez de un mensaje generico de error inesperado).
    /// </summary>
    public sealed class ThemeValidationException : Exception
    {
        public ThemeValidationException(string message) : base(message)
        {
        }

        public ThemeValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
