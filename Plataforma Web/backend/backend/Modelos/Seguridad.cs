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

        /* Solo con rol Candidato. Enlaza la cuenta con su ficha pública. */
        public int codigoCandidato { get; set; }
        public string candidatoSlug { get; set; }
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
