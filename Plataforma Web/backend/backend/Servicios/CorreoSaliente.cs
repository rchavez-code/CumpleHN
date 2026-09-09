using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace backend.Servicios
{
    /// <summary>
    /// Envío de correo del backend.
    ///
    /// Es el único punto que abre una conexión SMTP, por la misma razón por la
    /// que <see cref="AsistenteDatos"/> es el único que abre la conexión del
    /// login de la IA: si cada método que necesita mandar un mensaje armara su
    /// propio cliente, bastaría con que uno nuevo se olvidara de alguna opción
    /// para perder la garantía, y el síntoma sería que todo parece funcionar.
    ///
    /// Nunca lanza. Un fallo de envío devuelve <c>false</c> con su motivo, para
    /// que quien lo llama pueda registrarlo y seguir: la cuenta ya está creada
    /// y perderla porque el servidor de correo no respondió sería el peor de
    /// los dos males.
    ///
    /// La configuración vive en el Web.config, salvo el remitente y su
    /// contraseña, que van en secrets.config junto a la clave del asistente. Si
    /// falta, el envío falla con un motivo claro y el backend arranca igual —
    /// una máquina recién clonada tiene que poder levantar el sitio.
    /// </summary>
    public static class CorreoSaliente
    {
        /// <summary>
        /// Manda un mensaje en HTML. Devuelve false y deja el motivo en
        /// <paramref name="error"/> si no se pudo.
        /// </summary>
        public static bool Enviar(string destino, string asunto, string cuerpoHtml, out string error)
        {
            error = null;

            string remitente = ConfigurationManager.AppSettings["CorreoRemitente"];
            string clave = ConfigurationManager.AppSettings["CorreoClaveApp"];

            if (string.IsNullOrEmpty(remitente) || string.IsNullOrEmpty(clave))
            {
                error = "Falta la configuración del correo saliente en secrets.config "
                      + "(CorreoRemitente y CorreoClaveApp).";
                return false;
            }

            if (string.IsNullOrEmpty(destino))
            {
                error = "No se indicó a quién enviar el mensaje.";
                return false;
            }

            try
            {
                using (MailMessage mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(remitente, Ajuste("CorreoNombre", "CumpleHN"));
                    mensaje.To.Add(destino);
                    mensaje.Subject = asunto;
                    mensaje.Body = cuerpoHtml;
                    mensaje.IsBodyHtml = true;
                    mensaje.BodyEncoding = Encoding.UTF8;
                    mensaje.SubjectEncoding = Encoding.UTF8;

                    using (SmtpClient cliente = new SmtpClient())
                    {
                        cliente.Host = Ajuste("CorreoHost", "smtp.gmail.com");
                        cliente.Port = Numero("CorreoPuerto", 587);
                        cliente.EnableSsl = true;
                        cliente.DeliveryMethod = SmtpDeliveryMethod.Network;

                        // Sin esto el cliente manda antes las credenciales de la
                        // cuenta de Windows que corre el proceso, y Gmail lo
                        // rechaza sin decir por qué.
                        cliente.UseDefaultCredentials = false;
                        cliente.Credentials = new NetworkCredential(remitente, clave);

                        cliente.Timeout = Numero("CorreoTimeoutMs", 15000);

                        cliente.Send(mensaje);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // El motivo se devuelve tal cual porque lo guarda la bitácora,
                // no lo lee la persona que se registró.
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Cuerpo del mensaje de confirmación.
        ///
        /// Se arma acá y no en la base porque es presentación. El enlace lo
        /// forma <c>ConfirmacionUrlBase</c>, que apunta al frontend: el backend
        /// no conoce por su cuenta la dirección del sitio público.
        /// </summary>
        public static string CuerpoConfirmacion(string nombre, string enlace, int horas)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div style=\"font-family: Segoe UI, Arial, sans-serif; font-size: 15px; ");
            sb.Append("color: #1c1c1c; line-height: 1.55;\">");

            sb.Append("<p>Hola ").Append(Escapar(nombre)).Append(",</p>");

            sb.Append("<p>Creaste una cuenta en <strong>CumpleHN</strong>. Para poder apoyar ");
            sb.Append("publicaciones, comentar y responder las encuestas de percepción, ");
            sb.Append("confirmá que este correo es tuyo:</p>");

            sb.Append("<p style=\"margin: 26px 0;\">");
            sb.Append("<a href=\"").Append(Escapar(enlace)).Append("\" ");
            sb.Append("style=\"background: #c8102e; color: #ffffff; text-decoration: none; ");
            sb.Append("padding: 12px 22px; border-radius: 6px; font-weight: 600; ");
            sb.Append("display: inline-block;\">Confirmar mi correo</a>");
            sb.Append("</p>");

            sb.Append("<p style=\"font-size: 13px; color: #5a5a5a;\">");
            sb.Append("El enlace vence en ").Append(horas).Append(" horas. ");
            sb.Append("Si no te abre, copiá esta dirección en el navegador:<br />");
            sb.Append("<span style=\"word-break: break-all;\">").Append(Escapar(enlace));
            sb.Append("</span></p>");

            sb.Append("<p style=\"font-size: 13px; color: #5a5a5a;\">");
            sb.Append("Si no fuiste vos, ignorá este mensaje: sin confirmar, la cuenta no ");
            sb.Append("puede participar en nada.</p>");

            sb.Append("<hr style=\"border: none; border-top: 1px solid #e3e3e3; margin: 26px 0 14px;\" />");

            sb.Append("<p style=\"font-size: 12px; color: #767676;\">");
            sb.Append("CumpleHN — seguimiento ciudadano de compromisos políticos en Honduras. ");
            sb.Append("La plataforma organiza la evidencia disponible. El juicio es de cada persona.");
            sb.Append("</p>");

            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        /// Escapa el texto que se inserta en el HTML del mensaje. El nombre lo
        /// escribió quien se registró, así que entra en la misma categoría que
        /// cualquier otro dato de usuario en un marcado.
        /// </summary>
        private static string Escapar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            return texto.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;");
        }

        private static string Ajuste(string clave, string porOmision)
        {
            string v = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrEmpty(v) ? porOmision : v;
        }

        private static int Numero(string clave, int porOmision)
        {
            int n;
            return int.TryParse(ConfigurationManager.AppSettings[clave], out n) && n > 0
                 ? n : porOmision;
        }
    }
}
