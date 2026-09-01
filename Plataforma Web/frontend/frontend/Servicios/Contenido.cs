namespace frontend.Servicios
{
    /// <summary>
    /// Punto único de acceso al contenido para todas las páginas.
    ///
    /// Cambiar de origen de datos es cambiar la línea de abajo. Ninguna página
    /// necesita enterarse:
    ///
    ///   ContenidoServicio  -> Web Service ASMX del backend, contra BDCUMPLEHN.
    ///   ContenidoDemo      -> datos en memoria, útil para trabajar en el diseño
    ///                         sin levantar el backend ni la base de datos.
    /// </summary>
    public static class Contenido
    {
        private static readonly IContenidoServicio _datos = new ContenidoServicio();

        public static IContenidoServicio Datos
        {
            get { return _datos; }
        }
    }
}
