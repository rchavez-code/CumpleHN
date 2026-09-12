namespace backend.Modelos
{
    /// <summary>
    /// Datos del usuario autenticado. No incluye la contraseña ni su hash:
    /// eso nunca sale del backend.
    /// </summary>
    public class InfoUsuario
    {
        public int codigoUsuario { get; set; }
        public string login { get; set; }
        public string nombre { get; set; }
        public string correo { get; set; }
        public string rol { get; set; }

        /* Con el correo sin confirmar la cuenta entra y consulta, pero no
           participa. Viaja hasta el frontend para que la plantilla pueda
           avisarlo, nunca para que la página decida: quien autoriza es el
           Web Service. */
        public bool correoConfirmado { get; set; }

        /* Solo con rol Candidato. Enlaza la cuenta con su ficha pública. */
        public int codigoCandidato { get; set; }
        public string candidatoSlug { get; set; }

        /* Solo con rol Administrador: el espacio que administra. Para la
           cuenta de la plataforma (Usuarios.codigoEspacio en NULL) viaja el
           espacio de la plataforma, y administraPlataforma en verdadero le
           permite cambiar de espacio desde la administración. Igual que
           correoConfirmado, viaja para que la interfaz sepa qué mostrar:
           quien autoriza es el Web Service contra la base. */
        public int codigoEspacio { get; set; }
        public string espacioNombre { get; set; }
        public bool administraPlataforma { get; set; }
    }

    /// <summary>
    /// Resultado de un intento de acceso.
    ///
    /// El frontend decide con <c>ok</c> y muestra <c>mensaje</c> cuando falla.
    /// El mensaje es deliberadamente genérico: decir si falló el usuario o la
    /// contraseña le confirma a un atacante qué cuentas existen.
    /// </summary>
    public class RespuestaLogin
    {
        public bool ok { get; set; }
        public string mensaje { get; set; }
        public InfoUsuario usuario { get; set; }
    }
}
