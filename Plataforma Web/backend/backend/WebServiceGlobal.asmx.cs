using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Services;
using System.Web.Services;
using backend.Modelos;
using backend.Servicios;

namespace backend
{
    /// <summary>
    /// Web Service único de CumpleHN.
    ///
    /// Es el único punto del sistema que toca la base de datos. El frontend no
    /// tiene cadena de conexión: todo lo consulta a través de acá, por SOAP
    /// mediante un Service Reference, o por JSON si se llama desde script.
    ///
    /// Todas las consultas usan parámetros. No se concatena nunca la entrada
    /// del usuario dentro del SQL, que es el punto que revisa OWASP para
    /// inyección (anexo A.4 del proyecto).
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class WebServiceGlobal : System.Web.Services.WebService
    {
        /// <summary>Cadena de conexión, definida en el Web.config del backend.</summary>
        private static string cadenaConexion
        {
            get { return ConfigurationManager.ConnectionStrings["CnxCumpleHN"].ConnectionString; }
        }

        /// <summary>
        /// Nombre del rol de administración, tal como lo guarda el catálogo
        /// dbo.Roles. Cambiarlo en la base obliga a cambiarlo acá.
        /// </summary>
        private const string RolAdministrador = "Administrador";
        private const string RolCiudadano = "Ciudadano";

        // =============================================================
        //  Seguridad
        // =============================================================

        /// <summary>
        /// SHA-256 en hexadecimal minúscula. Debe producir exactamente el mismo
        /// resultado que el HASHBYTES del script de datos, o ningún login entra.
        /// </summary>
        public static string EncriptarSHA256(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Valida las credenciales de acceso. Acepta el login o el correo.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaLogin ValidarLogin(string usuario, string clave)
        {
            RespuestaLogin respuesta = new RespuestaLogin();
            respuesta.ok = false;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
            {
                respuesta.mensaje = "Ingresá tu usuario y tu contraseña.";
                return respuesta;
            }

            string claveCifrada = EncriptarSHA256(clave.Trim());

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    string query =
                        "SELECT u.codigoUsuario, u.login, u.nombre, u.correo, r.nombre AS rol, " +
                        "       u.correoConfirmado, " +
                        "       ISNULL(u.codigoCandidato, 0) AS codigoCandidato, " +
                        "       ISNULL(c.slug, '') AS candidatoSlug " +
                        "FROM dbo.Usuarios u " +
                        "INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol " +
                        "LEFT JOIN dbo.Candidatos c ON c.codigoCandidato = u.codigoCandidato " +
                        "WHERE (u.login = @usuario OR u.correo = @usuario) " +
                        "  AND u.clave = @clave AND u.activo = 1";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@usuario", usuario.Trim());
                    cmd.Parameters.AddWithValue("@clave", claveCifrada);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        respuesta.usuario = new InfoUsuario
                        {
                            codigoUsuario = Convert.ToInt32(reader["codigoUsuario"]),
                            login = Texto(reader, "login"),
                            nombre = Texto(reader, "nombre"),
                            correo = Texto(reader, "correo"),
                            rol = Texto(reader, "rol"),
                            correoConfirmado = Convert.ToBoolean(reader["correoConfirmado"]),
                            codigoCandidato = Convert.ToInt32(reader["codigoCandidato"]),
                            candidatoSlug = Texto(reader, "candidatoSlug")
                        };
                        respuesta.ok = true;
                    }
                }

                if (!respuesta.ok)
                {
                    // Mensaje único a propósito: no se revela si el usuario existe.
                    respuesta.mensaje = "El usuario o la contraseña no son correctos.";
                }
            }
            catch (Exception ex)
            {
                respuesta.ok = false;
                respuesta.mensaje = "No se pudo validar el acceso: " + ex.Message;
            }

            return respuesta;
        }

        /// <summary>
        /// Longitud mínima de una contraseña nueva. Se exige acá y no solo en
        /// el formulario porque una validación que vive únicamente en la página
        /// se salta llamando al servicio directamente.
        /// </summary>
        private const int ClaveMinima = 8;

        /// <summary>
        /// Alta de una cuenta ciudadana desde el registro público.
        ///
        /// Devuelve la misma <see cref="RespuestaLogin"/> que
        /// <see cref="ValidarLogin"/>: cuando la cuenta se crea, el frontend
        /// abre la sesión con esos datos en lugar de mandar a la persona a un
        /// formulario de acceso por algo que se acaba de comprobar.
        ///
        /// La contraseña se cifra acá, con el mismo <see cref="EncriptarSHA256"/>
        /// que usa el acceso, y al procedimiento le llega ya el hash. Cifrarla
        /// con HASHBYTES del lado de la base, como hace la creación de cuentas
        /// de candidatura, da un hash distinto en cuanto la contraseña lleva
        /// una tilde: HASHBYTES trabaja sobre la página de códigos de la base y
        /// EncriptarSHA256 sobre UTF-8. Una cuenta así se crea bien y el acceso
        /// la rechaza siempre.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaLogin RegistrarCiudadano(string nombres, string apellidos,
                                                 string correo, string clave)
        {
            RespuestaLogin respuesta = new RespuestaLogin();
            respuesta.ok = false;

            nombres = (nombres ?? string.Empty).Trim();
            apellidos = (apellidos ?? string.Empty).Trim();
            correo = (correo ?? string.Empty).Trim();
            clave = clave ?? string.Empty;

            // El resto de las comprobaciones las hace el procedimiento, que es
            // donde quedan documentadas. La longitud de la contraseña se queda
            // acá porque es lo único que la base no llega a ver: le mandamos el
            // hash, que mide igual para cualquier contraseña.
            if (clave.Length < ClaveMinima)
            {
                respuesta.mensaje = "La contraseña debe tener al menos "
                    + ClaveMinima + " caracteres.";
                return respuesta;
            }

            // El token viaja al buzón de la persona y a la base solo llega su
            // hash, igual que la contraseña y por la misma razón: quien lea la
            // tabla no puede confirmar cuentas ajenas con lo que ve.
            string token = NuevoToken();
            int confirmacion = 0;
            string destino = null;
            string nombreCompleto = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("dbo.spRegistrarCiudadano", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombres", nombres);
                    cmd.Parameters.AddWithValue("@apellidos", apellidos);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.AddWithValue("@claveHash", EncriptarSHA256(clave));
                    cmd.Parameters.AddWithValue("@tokenHash", EncriptarSHA256(token));
                    cmd.Parameters.AddWithValue("@horasVigencia", HorasConfirmacion);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            respuesta.ok = Convert.ToBoolean(reader["ok"]);
                            respuesta.mensaje = Texto(reader, "mensaje");

                            if (!respuesta.ok) continue;

                            confirmacion = Convert.ToInt32(reader["codigoConfirmacion"]);
                            destino = Texto(reader, "correo");
                            nombreCompleto = Texto(reader, "nombre");

                            respuesta.usuario = new InfoUsuario
                            {
                                codigoUsuario = Convert.ToInt32(reader["codigoUsuario"]),
                                login = Texto(reader, "login"),
                                nombre = nombreCompleto,
                                correo = destino,
                                rol = Texto(reader, "rol"),
                                correoConfirmado = false,
                                codigoCandidato = 0,
                                candidatoSlug = string.Empty
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.ok = false;
                respuesta.usuario = null;
                respuesta.mensaje = "No se pudo crear la cuenta: " + ex.Message;
                return respuesta;
            }

            if (!respuesta.ok) return respuesta;

            // El envío va después de que la cuenta ya existe. Si el servidor de
            // correo no responde, la cuenta no se pierde: la pantalla ofrece
            // reenviar y el motivo del fallo queda guardado.
            respuesta.mensaje = EnviarConfirmacion(confirmacion, destino, nombreCompleto, token)
                ? "Te enviamos un enlace a " + destino + " para confirmar la cuenta."
                : "La cuenta quedó creada, pero no pudimos enviarte el correo de confirmación. "
                  + "Pedí el enlace de nuevo desde tu cuenta.";

            return respuesta;
        }

        /// <summary>
        /// Confirma la cuenta a la que pertenece el token del enlace.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin ConfirmarCorreo(string token)
        {
            RespuestaAdmin r = new RespuestaAdmin();
            r.ok = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("dbo.spConfirmarCorreo", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tokenHash", EncriptarSHA256(token ?? string.Empty));

                    conn.Open();
                    r = LeerRespuesta(cmd);
                }
            }
            catch (Exception ex)
            {
                r.ok = false;
                r.mensaje = "No se pudo confirmar la cuenta: " + ex.Message;
            }

            return r;
        }

        /// <summary>
        /// Genera y manda un enlace nuevo.
        ///
        /// El intervalo mínimo y el tope diario los exige el procedimiento, no
        /// esta capa: el destinatario del mensaje lo eligió quien se registró,
        /// así que un reenvío sin límite es una manera de llenarle el buzón a
        /// un tercero.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin ReenviarConfirmacion(int codigoUsuario)
        {
            RespuestaAdmin r = new RespuestaAdmin();
            r.ok = false;

            string token = NuevoToken();
            int confirmacion = 0;
            string destino = null;
            string nombre = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("dbo.spSolicitarConfirmacion", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                    cmd.Parameters.AddWithValue("@tokenHash", EncriptarSHA256(token));
                    cmd.Parameters.AddWithValue("@horasVigencia", HorasConfirmacion);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            r.ok = Convert.ToBoolean(reader["ok"]);
                            r.mensaje = Texto(reader, "mensaje");

                            if (!r.ok) continue;

                            confirmacion = Convert.ToInt32(reader["codigoConfirmacion"]);
                            destino = Texto(reader, "correo");
                            nombre = Texto(reader, "nombre");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                r.ok = false;
                r.mensaje = "No se pudo generar el enlace: " + ex.Message;
                return r;
            }

            if (!r.ok) return r;

            if (EnviarConfirmacion(confirmacion, destino, nombre, token))
            {
                r.mensaje = "Te enviamos un enlace a " + destino + ".";
            }
            else
            {
                r.ok = false;
                r.mensaje = "No pudimos enviar el correo. Intentalo en unos minutos.";
            }

            return r;
        }

        // ------------------------------------------- Confirmación: apoyo

        /// <summary>Horas que dura el enlace de confirmación.</summary>
        private static int HorasConfirmacion
        {
            get
            {
                int n;
                return int.TryParse(ConfigurationManager.AppSettings["ConfirmacionHoras"], out n) && n > 0
                     ? n : 48;
            }
        }

        /// <summary>
        /// Token del enlace: 32 bytes del generador criptográfico, en
        /// hexadecimal. No se deriva del correo ni del código de la cuenta
        /// porque un token que se puede calcular no protege nada.
        /// </summary>
        private static string NuevoToken()
        {
            byte[] bytes = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            StringBuilder sb = new StringBuilder(64);
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Manda el mensaje y deja anotado cómo salió, incluso cuando falla.
        /// Una bitácora que solo guarda los envíos buenos no sirve para revisar
        /// los malos, que son los únicos que hay que revisar.
        /// </summary>
        private static bool EnviarConfirmacion(int codigoConfirmacion, string destino,
                                               string nombre, string token)
        {
            string baseUrl = ConfigurationManager.AppSettings["ConfirmacionUrlBase"];

            string error;
            bool ok;

            if (string.IsNullOrEmpty(baseUrl))
            {
                ok = false;
                error = "Falta ConfirmacionUrlBase en el Web.config del backend.";
            }
            else
            {
                string enlace = baseUrl.TrimEnd('/') + "/Confirmar?t=" + token;

                ok = CorreoSaliente.Enviar(
                    destino,
                    "Confirmá tu cuenta de CumpleHN",
                    CorreoSaliente.CuerpoConfirmacion(nombre, enlace, HorasConfirmacion),
                    out error);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    SqlCommand cmd = new SqlCommand("dbo.spConfirmacionEnvio", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codigoConfirmacion", codigoConfirmacion);
                    cmd.Parameters.AddWithValue("@enviado", ok);
                    cmd.Parameters.AddWithValue("@error", (object)error ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Que no se pueda anotar el resultado no cambia el resultado.
            }

            return ok;
        }

        // =============================================================
        //  Campañas
        // =============================================================

        private const string SelectCampana =
            "SELECT c.codigoCampana, c.slug, c.nombre, c.resumen, c.descripcion, c.alcance, " +
            "       c.fechaInicio, c.fechaEleccion, c.estado, c.esActual, " +
            "       (SELECT COUNT(*) FROM dbo.Candidatos    x WHERE x.codigoCampana = c.codigoCampana) AS totalCandidatos, " +
            "       (SELECT COUNT(*) FROM dbo.Propuestas    x WHERE x.codigoCampana = c.codigoCampana) AS totalPropuestas, " +
            "       (SELECT COUNT(*) FROM dbo.Publicaciones x WHERE x.codigoCampana = c.codigoCampana) AS totalPublicaciones " +
            "FROM dbo.Campanas c ";

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Campana> listarCampanas()
        {
            List<Campana> lista = new List<Campana>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                string query = SelectCampana +
                    "ORDER BY CASE c.estado WHEN 'Activa' THEN 0 WHEN 'Proxima' THEN 1 ELSE 2 END, " +
                    "         c.fechaEleccion DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(LeerCampana(reader));
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Campana obtenerCampanaActual()
        {
            Campana campana = null;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(SelectCampana + "WHERE c.esActual = 1", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    campana = LeerCampana(reader);
                }
            }

            return campana;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Campana obtenerCampana(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return obtenerCampanaActual();

            Campana campana = null;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(SelectCampana + "WHERE c.slug = @slug", conn);
                cmd.Parameters.AddWithValue("@slug", slug);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    campana = LeerCampana(reader);
                }
            }

            return campana;
        }

        // =============================================================
        //  Candidatos
        // =============================================================

        private const string SelectCandidato =
            "SELECT k.codigoCandidato, k.slug, k.codigoCampana, ca.slug AS campanaSlug, ca.nombre AS campanaNombre, " +
            "       k.nombres, k.apellidos, ISNULL(k.partido,'') AS partido, ISNULL(k.partidoSiglas,'') AS partidoSiglas, " +
            "       cg.nombre AS cargo, cg.nivelGobierno, ISNULL(d.nombre,'') AS departamento, ISNULL(k.municipio,'') AS municipio, " +
            "       ISNULL(k.fotoUrl,'') AS fotoUrl, ISNULL(k.titular,'') AS titular, ISNULL(k.biografia,'') AS biografia, " +
            "       ISNULL(k.informacionProfesional,'') AS informacionProfesional, " +
            "       ISNULL(k.descripcionCandidatura,'') AS descripcionCandidatura, " +
            "       ISNULL(k.correoPublico,'') AS correoPublico, ISNULL(k.telefono,'') AS telefono, " +
            "       ISNULL(k.sitioWeb,'') AS sitioWeb, ISNULL(k.facebook,'') AS facebook, " +
            "       ISNULL(k.x,'') AS x, ISNULL(k.instagram,'') AS instagram, " +
            "       nv.nombre AS verificacion, ISNULL(pa.slug,'') AS partidoSlug, " +
            "       (SELECT COUNT(*) FROM dbo.Propuestas    p WHERE p.codigoCandidato = k.codigoCandidato) AS totalPropuestas, " +
            "       (SELECT COUNT(*) FROM dbo.Publicaciones p WHERE p.codigoCandidato = k.codigoCandidato) AS totalPublicaciones, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Candidato' AND v.codigoObjeto = k.codigoCandidato AND v.valor = 1) AS meGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Candidato' AND v.codigoObjeto = k.codigoCandidato AND v.valor = -1) AS noMeGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Comentarios cm INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = cm.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Candidato' AND cm.codigoObjeto = k.codigoCandidato AND cm.aprobado = 1) AS comentarios " +
            "FROM dbo.Candidatos k " +
            "INNER JOIN dbo.Campanas ca ON ca.codigoCampana = k.codigoCampana " +
            "INNER JOIN dbo.Cargos cg ON cg.codigoCargo = k.codigoCargo " +
            "INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = k.codigoVerificacion " +
            "LEFT JOIN dbo.Departamentos d ON d.codigoDepartamento = k.codigoDepartamento " +
            "LEFT JOIN dbo.Partidos pa ON pa.codigoPartido = k.codigoPartido ";

        /// <summary>
        /// Candidatos de una campaña. Con <paramref name="campanaSlug"/> vacío
        /// devuelve todos los de la plataforma.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Candidato> listarCandidatos(string campanaSlug)
        {
            List<Candidato> lista = new List<Candidato>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                string filtro = string.IsNullOrEmpty(campanaSlug)
                    ? "WHERE k.activo = 1 "
                    : "WHERE k.activo = 1 AND ca.slug = @campana ";

                SqlCommand cmd = new SqlCommand(
                    SelectCandidato + filtro + "ORDER BY k.apellidos, k.nombres", conn);

                if (!string.IsNullOrEmpty(campanaSlug))
                    cmd.Parameters.AddWithValue("@campana", campanaSlug);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(LeerCandidato(reader));
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Candidato obtenerCandidato(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            Candidato candidato = null;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(SelectCandidato + "WHERE k.slug = @slug", conn);
                cmd.Parameters.AddWithValue("@slug", slug);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    candidato = LeerCandidato(reader);
                }
            }

            return candidato;
        }

        // =============================================================
        //  Propuestas
        // =============================================================

        private const string SelectPropuesta =
            "SELECT p.codigoPropuesta, p.codigoCandidato, k.slug AS candidatoSlug, " +
            "       (k.nombres + ' ' + k.apellidos) AS candidatoNombre, ca.slug AS campanaSlug, " +
            "       p.nombre, p.descripcion, ISNULL(p.problema,'') AS problema, ISNULL(p.objetivo,'') AS objetivo, " +
            "       ISNULL(p.beneficiarios,'') AS beneficiarios, cat.nombre AS categoria, " +
            "       ISNULL(p.ubicacion,'') AS ubicacion, ISNULL(p.periodoEjecucion,'') AS periodoEjecucion, " +
            "       ep.nombre AS estado, ISNULL(p.imagenUrl,'') AS imagenUrl, " +
            "       ISNULL(p.informacionAdicional,'') AS informacionAdicional, " +
            "       nv.nombre AS verificacion, p.fechaRegistro, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Propuesta' AND v.codigoObjeto = p.codigoPropuesta AND v.valor = 1) AS meGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Propuesta' AND v.codigoObjeto = p.codigoPropuesta AND v.valor = -1) AS noMeGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Comentarios cm INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = cm.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Propuesta' AND cm.codigoObjeto = p.codigoPropuesta AND cm.aprobado = 1) AS comentarios " +
            "FROM dbo.Propuestas p " +
            "INNER JOIN dbo.Candidatos k ON k.codigoCandidato = p.codigoCandidato " +
            "INNER JOIN dbo.Campanas ca ON ca.codigoCampana = p.codigoCampana " +
            "INNER JOIN dbo.Categorias cat ON cat.codigoCategoria = p.codigoCategoria " +
            "INNER JOIN dbo.EstadosPropuesta ep ON ep.codigoEstado = p.codigoEstado " +
            "INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = p.codigoVerificacion ";

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Propuesta> listarPropuestasDeCandidato(string candidatoSlug)
        {
            List<Propuesta> lista = new List<Propuesta>();
            if (string.IsNullOrEmpty(candidatoSlug)) return lista;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(
                    SelectPropuesta + "WHERE k.slug = @slug ORDER BY p.fechaRegistro DESC", conn);
                cmd.Parameters.AddWithValue("@slug", candidatoSlug);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(LeerPropuesta(reader));
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Propuesta> listarPropuestasDeCampana(string campanaSlug)
        {
            List<Propuesta> lista = new List<Propuesta>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                string filtro = string.IsNullOrEmpty(campanaSlug) ? "" : "AND ca.slug = @campana ";

                SqlCommand cmd = new SqlCommand(
                    SelectPropuesta + filtro + "ORDER BY p.fechaRegistro DESC", conn);

                if (!string.IsNullOrEmpty(campanaSlug))
                    cmd.Parameters.AddWithValue("@campana", campanaSlug);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(LeerPropuesta(reader));
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Propuesta obtenerPropuesta(int codigoPropuesta)
        {
            Propuesta propuesta = null;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(
                    SelectPropuesta + "WHERE p.codigoPropuesta = @codigo", conn);
                cmd.Parameters.AddWithValue("@codigo", codigoPropuesta);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    propuesta = LeerPropuesta(reader);
                }
            }

            return propuesta;
        }

        // =============================================================
        //  Publicaciones
        // =============================================================

        private const string SelectPublicacion =
            "SELECT b.codigoPublicacion, ca.slug AS campanaSlug, b.codigoCandidato, " +
            "       k.slug AS candidatoSlug, (k.nombres + ' ' + k.apellidos) AS candidatoNombre, " +
            "       cg.nombre AS candidatoCargo, ISNULL(k.fotoUrl,'') AS candidatoFotoUrl, " +
            "       b.fecha, b.texto, ISNULL(b.imagenUrl,'') AS imagenUrl, " +
            "       ISNULL(cat.nombre,'') AS categoria, ISNULL(b.codigoPropuesta,0) AS codigoPropuesta, " +
            "       ISNULL(p.nombre,'') AS propuestaNombre, nv.nombre AS verificacion, " +
            /* Los contadores se derivan de las tablas de interacción. No se
               guardan como columnas para que el número mostrado no pueda
               contradecir a las filas reales. */
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Publicacion' AND v.codigoObjeto = b.codigoPublicacion AND v.valor = 1) AS meGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Publicacion' AND v.codigoObjeto = b.codigoPublicacion AND v.valor = -1) AS noMeGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Comentarios cm INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = cm.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Publicacion' AND cm.codigoObjeto = b.codigoPublicacion AND cm.aprobado = 1) AS comentarios " +
            "FROM dbo.Publicaciones b " +
            "INNER JOIN dbo.Candidatos k ON k.codigoCandidato = b.codigoCandidato " +
            "INNER JOIN dbo.Campanas ca ON ca.codigoCampana = b.codigoCampana " +
            "INNER JOIN dbo.Cargos cg ON cg.codigoCargo = k.codigoCargo " +
            "INNER JOIN dbo.NivelesVerificacion nv ON nv.codigoVerificacion = b.codigoVerificacion " +
            "LEFT JOIN dbo.Categorias cat ON cat.codigoCategoria = b.codigoCategoria " +
            "LEFT JOIN dbo.Propuestas p ON p.codigoPropuesta = b.codigoPropuesta " +
            /* La consulta pública nunca ve una publicación retirada por
               moderación. El filtro va en el SELECT compartido y no en cada
               método, para que agregar una consulta nueva no pueda olvidarlo. */
            "WHERE b.activo = 1 ";

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Publicacion> listarFeed(string campanaSlug)
        {
            List<Publicacion> lista = new List<Publicacion>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                string filtro = string.IsNullOrEmpty(campanaSlug) ? "" : "AND ca.slug = @campana ";

                SqlCommand cmd = new SqlCommand(
                    SelectPublicacion + filtro + "ORDER BY b.fecha DESC", conn);

                if (!string.IsNullOrEmpty(campanaSlug))
                    cmd.Parameters.AddWithValue("@campana", campanaSlug);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(LeerPublicacion(reader));
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Publicacion> listarPublicacionesDeCandidato(string candidatoSlug)
        {
            List<Publicacion> lista = new List<Publicacion>();
            if (string.IsNullOrEmpty(candidatoSlug)) return lista;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(
                    SelectPublicacion + "AND k.slug = @slug ORDER BY b.fecha DESC", conn);
                cmd.Parameters.AddWithValue("@slug", candidatoSlug);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(LeerPublicacion(reader));
                }
            }

            return lista;
        }

        // =============================================================
        //  Catálogos
        // =============================================================

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Catalogo> listarCategorias()
        {
            return LeerCatalogo(
                "SELECT codigoCategoria AS codigo, nombre, ISNULL(descripcion,'') AS detalle " +
                "FROM dbo.Categorias ORDER BY orden");
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Catalogo> listarCargos()
        {
            return LeerCatalogo(
                "SELECT codigoCargo AS codigo, nombre, nivelGobierno AS detalle " +
                "FROM dbo.Cargos ORDER BY orden");
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Catalogo> listarDepartamentos()
        {
            return LeerCatalogo(
                "SELECT codigoDepartamento AS codigo, nombre, '' AS detalle " +
                "FROM dbo.Departamentos ORDER BY nombre");
        }

        // =============================================================
        //  Partidos políticos
        // =============================================================

        private const string SelectPartido =
            "SELECT pa.codigoPartido, pa.slug, pa.nombre, ISNULL(pa.siglas,'') AS siglas, " +
            "       ISNULL(pa.descripcion,'') AS descripcion, " +
            "       (SELECT COUNT(*) FROM dbo.Candidatos k WHERE k.codigoPartido = pa.codigoPartido) AS totalCandidatos, " +
            "       (SELECT COUNT(*) FROM dbo.Propuestas r INNER JOIN dbo.Candidatos k2 ON k2.codigoCandidato = r.codigoCandidato " +
            "         WHERE k2.codigoPartido = pa.codigoPartido) AS totalPropuestas, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Partido' AND v.codigoObjeto = pa.codigoPartido AND v.valor = 1) AS meGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Partido' AND v.codigoObjeto = pa.codigoPartido AND v.valor = -1) AS noMeGusta, " +
            "       (SELECT COUNT(*) FROM dbo.Comentarios cm INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = cm.codigoTipoObjeto " +
            "         WHERE t.nombre = 'Partido' AND cm.codigoObjeto = pa.codigoPartido AND cm.aprobado = 1) AS comentarios " +
            "FROM dbo.Partidos pa ";

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Partido> listarPartidos()
        {
            List<Partido> lista = new List<Partido>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(
                    SelectPartido + "WHERE pa.activo = 1 ORDER BY pa.nombre", conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) lista.Add(LeerPartido(reader));
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Partido obtenerPartido(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            Partido partido = null;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(SelectPartido + "WHERE pa.slug = @slug", conn);
                cmd.Parameters.AddWithValue("@slug", slug);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) partido = LeerPartido(reader);
            }

            return partido;
        }

        /// <summary>Candidaturas afiliadas a un partido.</summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Candidato> listarCandidatosDePartido(string partidoSlug)
        {
            List<Candidato> lista = new List<Candidato>();
            if (string.IsNullOrEmpty(partidoSlug)) return lista;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(
                    SelectCandidato + "WHERE k.activo = 1 AND pa.slug = @slug ORDER BY k.apellidos, k.nombres", conn);
                cmd.Parameters.AddWithValue("@slug", partidoSlug);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read()) lista.Add(LeerCandidato(reader));
            }

            return lista;
        }

        // =============================================================
        //  Interacción ciudadana
        // =============================================================

        /// <summary>
        /// Tipos de objeto aceptados. Se valida contra esta lista antes de
        /// tocar la base, de modo que un tipo inventado se rechaza acá y no
        /// llega a la consulta.
        /// </summary>
        private static bool TipoValido(string tipoObjeto)
        {
            if (string.IsNullOrEmpty(tipoObjeto)) return false;

            return tipoObjeto == "Publicacion"
                || tipoObjeto == "Candidato"
                || tipoObjeto == "Partido"
                || tipoObjeto == "Propuesta"
                || tipoObjeto == "Encuesta"
                || tipoObjeto == "Iniciativa";
        }

        /// <summary>
        /// Confirma que el objeto valorado o comentado existe. Hace falta porque
        /// la referencia al objeto no puede tener llave foránea: el destino
        /// cambia según el tipo. Cada tipo usa su propia consulta fija, sin SQL
        /// armado con texto.
        /// </summary>
        private static bool ExisteObjeto(SqlConnection conn, string tipoObjeto, int codigoObjeto)
        {
            string query;

            switch (tipoObjeto)
            {
                case "Publicacion":
                    query = "SELECT COUNT(*) FROM dbo.Publicaciones WHERE codigoPublicacion = @codigo";
                    break;
                case "Candidato":
                    query = "SELECT COUNT(*) FROM dbo.Candidatos WHERE codigoCandidato = @codigo";
                    break;
                case "Partido":
                    query = "SELECT COUNT(*) FROM dbo.Partidos WHERE codigoPartido = @codigo";
                    break;
                case "Propuesta":
                    query = "SELECT COUNT(*) FROM dbo.Propuestas WHERE codigoPropuesta = @codigo";
                    break;
                case "Encuesta":
                    // Filtra por activo, junto con la iniciativa: una encuesta
                    // retirada sale del sitio público, y comentar algo que ya
                    // no se ve dejaría el hilo colgando de la nada.
                    query = "SELECT COUNT(*) FROM dbo.Encuestas WHERE codigoEncuesta = @codigo AND activo = 1";
                    break;
                case "Iniciativa":
                    query = "SELECT COUNT(*) FROM dbo.Iniciativas WHERE codigoIniciativa = @codigo AND activo = 1";
                    break;
                default:
                    return false;
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@codigo", codigoObjeto);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Confirma que la cuenta existe, está activa y tiene el rol de
        /// administrador.
        ///
        /// Es el punto por el que tiene que pasar toda acción de administración
        /// antes de escribir. El frontend decide qué botones muestra a partir de
        /// su sesión, pero esa sesión no es una credencial: quien llame a este
        /// servicio directamente puede enviar el código de usuario que quiera.
        /// El rol se confirma acá, contra la base, o no se confirma.
        ///
        /// Comprobar el rol no cierra del todo la deuda conocida del anexo
        /// OWASP: mientras el código de usuario venga en el parámetro en lugar
        /// de un token de sesión firmado, alguien que conozca el código del
        /// administrador puede suplantarlo. Lo que esta comprobación garantiza
        /// es que ninguna cuenta sin el rol pueda administrar.
        /// </summary>
        private static bool EsAdministrador(SqlConnection conn, int codigoUsuario)
        {
            if (codigoUsuario <= 0) return false;

            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) " +
                "FROM dbo.Usuarios u " +
                "INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol " +
                "WHERE u.codigoUsuario = @u AND u.activo = 1 AND r.nombre = @rol", conn);
            cmd.Parameters.AddWithValue("@u", codigoUsuario);
            cmd.Parameters.AddWithValue("@rol", RolAdministrador);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Confirma que un módulo está visible en el sitio.
        ///
        /// Hace falta acá y no solo en el frontend: si la comprobación viviera
        /// únicamente en la página, cerrar la participación se saltaría
        /// llamando a este servicio directamente, que es la misma razón por la
        /// que el rol se confirma contra la base.
        /// </summary>
        private static bool ModuloVisible(SqlConnection conn, string clave)
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.vwModulosEfectivos WHERE clave = @c AND visible = 1", conn);
            cmd.Parameters.AddWithValue("@c", clave);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Devuelve por qué la cuenta no puede participar, o null si sí puede.
        ///
        /// Es la única puerta de la participación y la consultan las cuatro
        /// acciones que escriben en nombre de alguien: valorar, comentar,
        /// responder una encuesta y preguntarle al asistente. Está en un solo
        /// lugar por lo mismo que la comprobación de rol: si cada método la
        /// resolviera por su cuenta, bastaría con que uno nuevo se olvidara
        /// para dejar la puerta abierta, y nada en la pantalla lo delataría.
        ///
        /// Devuelve el motivo y no un booleano porque las dos causas piden
        /// cosas distintas. A quien tiene la cuenta dada de baja no hay nada
        /// que decirle, pero a quien solo le falta abrir un enlace, un «no
        /// tenés cuenta activa» lo deja sin saber qué hacer — que fue el
        /// problema de la versión anterior, con un texto único para las dos.
        ///
        /// El texto de la cuenta ausente lo pone quien llama, porque comentar,
        /// valorar y preguntarle al asistente no se nombran igual. El de la
        /// confirmación pendiente es uno solo: la explicación de qué hacer no
        /// cambia según lo que se intentaba.
        /// </summary>
        private static string MotivoSinParticipacion(SqlConnection conn, int codigoUsuario,
                                                     string sinCuenta)
        {
            if (codigoUsuario <= 0) return sinCuenta;

            SqlCommand cmd = new SqlCommand(
                "SELECT correoConfirmado FROM dbo.Usuarios " +
                "WHERE codigoUsuario = @u AND activo = 1", conn);
            cmd.Parameters.AddWithValue("@u", codigoUsuario);

            object v = cmd.ExecuteScalar();

            if (v == null || v == DBNull.Value) return sinCuenta;

            if (!Convert.ToBoolean(v))
                return "Confirmá tu correo para participar. "
                     + "Podés pedir el enlace de nuevo desde el aviso de tu cuenta.";

            return null;
        }

        /// <summary>
        /// Lee los contadores de un objeto y, si se envía un usuario, cuál fue
        /// su voto. Con <paramref name="codigoUsuario"/> en cero devuelve solo
        /// los totales, que es el caso de un visitante sin cuenta.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Interaccion obtenerInteraccion(string tipoObjeto, int codigoObjeto, int codigoUsuario)
        {
            Interaccion i = new Interaccion();
            i.tipoObjeto = tipoObjeto;
            i.codigoObjeto = codigoObjeto;

            if (!TipoValido(tipoObjeto)) return i;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                LeerInteraccion(conn, tipoObjeto, codigoObjeto, codigoUsuario, i);
            }

            return i;
        }

        private static void LeerInteraccion(SqlConnection conn, string tipoObjeto,
            int codigoObjeto, int codigoUsuario, Interaccion i)
        {
            string query =
                "SELECT " +
                "  (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
                "    WHERE t.nombre = @tipo AND v.codigoObjeto = @codigo AND v.valor = 1) AS meGusta, " +
                "  (SELECT COUNT(*) FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
                "    WHERE t.nombre = @tipo AND v.codigoObjeto = @codigo AND v.valor = -1) AS noMeGusta, " +
                "  (SELECT COUNT(*) FROM dbo.Comentarios cm INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = cm.codigoTipoObjeto " +
                "    WHERE t.nombre = @tipo AND cm.codigoObjeto = @codigo AND cm.aprobado = 1) AS comentarios, " +
                "  ISNULL((SELECT v.valor FROM dbo.Valoraciones v INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
                "    WHERE t.nombre = @tipo AND v.codigoObjeto = @codigo AND v.codigoUsuario = @usuario), 0) AS miValoracion";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tipo", tipoObjeto);
            cmd.Parameters.AddWithValue("@codigo", codigoObjeto);
            cmd.Parameters.AddWithValue("@usuario", codigoUsuario);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                i.meGusta = Convert.ToInt32(reader["meGusta"]);
                i.noMeGusta = Convert.ToInt32(reader["noMeGusta"]);
                i.comentarios = Convert.ToInt32(reader["comentarios"]);
                i.miValoracion = Convert.ToInt32(reader["miValoracion"]);
            }
            reader.Close();
        }

        /// <summary>
        /// Registra el me gusta o el no me gusta de una persona sobre un objeto.
        ///
        /// El comportamiento es el de una red social: votar lo mismo dos veces
        /// retira el voto, y votar lo contrario lo cambia. Una persona nunca
        /// acumula más de un voto por objeto, y eso lo garantiza la restricción
        /// única de la tabla, no solo este código.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaInteraccion valorar(string tipoObjeto, int codigoObjeto, int codigoUsuario, int valor)
        {
            RespuestaInteraccion r = new RespuestaInteraccion();
            r.ok = false;

            if (!TipoValido(tipoObjeto))
            {
                r.mensaje = "El tipo de contenido no es válido.";
                return r;
            }

            if (valor != 1 && valor != -1)
            {
                r.mensaje = "La valoración solo puede ser me gusta o no me gusta.";
                return r;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();

                    if (!ModuloVisible(conn, "interaccion"))
                    {
                        r.mensaje = "La participación está temporalmente cerrada.";
                        return r;
                    }

                    string motivo = MotivoSinParticipacion(conn, codigoUsuario,
                        "Necesitás una cuenta activa para participar.");
                    if (motivo != null)
                    {
                        r.mensaje = motivo;
                        return r;
                    }

                    if (!ExisteObjeto(conn, tipoObjeto, codigoObjeto))
                    {
                        r.mensaje = "El contenido que intentás valorar no existe.";
                        return r;
                    }

                    SqlCommand cmd = new SqlCommand(
                        "SELECT codigoValoracion, valor FROM dbo.Valoraciones v " +
                        "INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = v.codigoTipoObjeto " +
                        "WHERE t.nombre = @tipo AND v.codigoObjeto = @codigo AND v.codigoUsuario = @usuario", conn);
                    cmd.Parameters.AddWithValue("@tipo", tipoObjeto);
                    cmd.Parameters.AddWithValue("@codigo", codigoObjeto);
                    cmd.Parameters.AddWithValue("@usuario", codigoUsuario);

                    int codigoExistente = 0;
                    int valorExistente = 0;

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        codigoExistente = Convert.ToInt32(reader["codigoValoracion"]);
                        valorExistente = Convert.ToInt32(reader["valor"]);
                    }
                    reader.Close();

                    if (codigoExistente == 0)
                    {
                        SqlCommand ins = new SqlCommand(
                            "INSERT INTO dbo.Valoraciones (codigoTipoObjeto, codigoObjeto, codigoUsuario, valor) " +
                            "SELECT t.codigoTipoObjeto, @codigo, @usuario, @valor " +
                            "FROM dbo.TiposObjeto t WHERE t.nombre = @tipo", conn);
                        ins.Parameters.AddWithValue("@tipo", tipoObjeto);
                        ins.Parameters.AddWithValue("@codigo", codigoObjeto);
                        ins.Parameters.AddWithValue("@usuario", codigoUsuario);
                        ins.Parameters.AddWithValue("@valor", valor);
                        ins.ExecuteNonQuery();
                    }
                    else if (valorExistente == valor)
                    {
                        // Mismo voto otra vez: se retira.
                        SqlCommand del = new SqlCommand(
                            "DELETE FROM dbo.Valoraciones WHERE codigoValoracion = @id", conn);
                        del.Parameters.AddWithValue("@id", codigoExistente);
                        del.ExecuteNonQuery();
                    }
                    else
                    {
                        // Cambio de opinión: se actualiza en lugar de agregar otra fila.
                        SqlCommand upd = new SqlCommand(
                            "UPDATE dbo.Valoraciones SET valor = @valor, fecha = SYSDATETIME() " +
                            "WHERE codigoValoracion = @id", conn);
                        upd.Parameters.AddWithValue("@valor", valor);
                        upd.Parameters.AddWithValue("@id", codigoExistente);
                        upd.ExecuteNonQuery();
                    }

                    Interaccion i = new Interaccion();
                    i.tipoObjeto = tipoObjeto;
                    i.codigoObjeto = codigoObjeto;
                    LeerInteraccion(conn, tipoObjeto, codigoObjeto, codigoUsuario, i);

                    r.interaccion = i;
                    r.ok = true;
                }
            }
            catch (Exception ex)
            {
                r.ok = false;
                r.mensaje = "No se pudo registrar la valoración: " + ex.Message;
            }

            return r;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Comentario> listarComentarios(string tipoObjeto, int codigoObjeto)
        {
            List<Comentario> lista = new List<Comentario>();
            if (!TipoValido(tipoObjeto)) return lista;

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT c.codigoComentario, c.codigoUsuario, u.nombre AS autor, ro.nombre AS rol, " +
                    "       c.fecha, c.texto " +
                    "FROM dbo.Comentarios c " +
                    "INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = c.codigoTipoObjeto " +
                    "INNER JOIN dbo.Usuarios u ON u.codigoUsuario = c.codigoUsuario " +
                    "INNER JOIN dbo.Roles ro ON ro.codigoRol = u.codigoRol " +
                    "WHERE t.nombre = @tipo AND c.codigoObjeto = @codigo AND c.aprobado = 1 " +
                    "ORDER BY c.fecha DESC", conn);
                cmd.Parameters.AddWithValue("@tipo", tipoObjeto);
                cmd.Parameters.AddWithValue("@codigo", codigoObjeto);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Comentario
                    {
                        codigoComentario = Convert.ToInt32(reader["codigoComentario"]),
                        codigoUsuario = Convert.ToInt32(reader["codigoUsuario"]),
                        autor = Texto(reader, "autor"),
                        rol = Texto(reader, "rol"),
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        texto = Texto(reader, "texto")
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Publica un comentario. El texto se guarda tal como lo escribió la
        /// persona y el escape corresponde a quien lo muestra, de modo que nunca
        /// se pierde el contenido original.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaComentario agregarComentario(string tipoObjeto, int codigoObjeto,
            int codigoUsuario, string texto)
        {
            RespuestaComentario r = new RespuestaComentario();
            r.ok = false;

            if (!TipoValido(tipoObjeto))
            {
                r.mensaje = "El tipo de contenido no es válido.";
                return r;
            }

            string limpio = (texto ?? string.Empty).Trim();

            if (limpio.Length < 2)
            {
                r.mensaje = "Escribí tu comentario antes de publicarlo.";
                return r;
            }

            if (limpio.Length > 1200)
            {
                r.mensaje = "El comentario no puede pasar de 1200 caracteres.";
                return r;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();

                    if (!ModuloVisible(conn, "interaccion"))
                    {
                        r.mensaje = "La participación está temporalmente cerrada.";
                        return r;
                    }

                    string motivo = MotivoSinParticipacion(conn, codigoUsuario,
                        "Necesitás una cuenta activa para comentar.");
                    if (motivo != null)
                    {
                        r.mensaje = motivo;
                        return r;
                    }

                    if (!ExisteObjeto(conn, tipoObjeto, codigoObjeto))
                    {
                        r.mensaje = "El contenido que intentás comentar no existe.";
                        return r;
                    }

                    SqlCommand ins = new SqlCommand(
                        "INSERT INTO dbo.Comentarios (codigoTipoObjeto, codigoObjeto, codigoUsuario, texto) " +
                        "SELECT t.codigoTipoObjeto, @codigo, @usuario, @texto " +
                        "FROM dbo.TiposObjeto t WHERE t.nombre = @tipo; " +
                        "SELECT CAST(SCOPE_IDENTITY() AS int);", conn);
                    ins.Parameters.AddWithValue("@tipo", tipoObjeto);
                    ins.Parameters.AddWithValue("@codigo", codigoObjeto);
                    ins.Parameters.AddWithValue("@usuario", codigoUsuario);
                    ins.Parameters.AddWithValue("@texto", limpio);

                    int nuevo = Convert.ToInt32(ins.ExecuteScalar());

                    SqlCommand lee = new SqlCommand(
                        "SELECT c.codigoComentario, c.codigoUsuario, u.nombre AS autor, ro.nombre AS rol, " +
                        "       c.fecha, c.texto " +
                        "FROM dbo.Comentarios c " +
                        "INNER JOIN dbo.Usuarios u ON u.codigoUsuario = c.codigoUsuario " +
                        "INNER JOIN dbo.Roles ro ON ro.codigoRol = u.codigoRol " +
                        "WHERE c.codigoComentario = @id", conn);
                    lee.Parameters.AddWithValue("@id", nuevo);

                    SqlDataReader reader = lee.ExecuteReader();
                    while (reader.Read())
                    {
                        r.comentario = new Comentario
                        {
                            codigoComentario = Convert.ToInt32(reader["codigoComentario"]),
                            codigoUsuario = Convert.ToInt32(reader["codigoUsuario"]),
                            autor = Texto(reader, "autor"),
                            rol = Texto(reader, "rol"),
                            fecha = Convert.ToDateTime(reader["fecha"]),
                            texto = Texto(reader, "texto")
                        };
                    }
                    reader.Close();

                    SqlCommand total = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Comentarios c " +
                        "INNER JOIN dbo.TiposObjeto t ON t.codigoTipoObjeto = c.codigoTipoObjeto " +
                        "WHERE t.nombre = @tipo AND c.codigoObjeto = @codigo AND c.aprobado = 1", conn);
                    total.Parameters.AddWithValue("@tipo", tipoObjeto);
                    total.Parameters.AddWithValue("@codigo", codigoObjeto);
                    r.total = Convert.ToInt32(total.ExecuteScalar());

                    r.ok = true;
                }
            }
            catch (Exception ex)
            {
                r.ok = false;
                r.mensaje = "No se pudo publicar el comentario: " + ex.Message;
            }

            return r;
        }

        // =============================================================
        //  Analítica
        // =============================================================

        /// <summary>
        /// Tablero completo en una sola llamada.
        ///
        /// Nueve indicadores, nueve procedimientos almacenados, una sola
        /// conexión. Se devuelve todo junto por dos razones: con este volumen
        /// —decenas de filas por indicador— nueve viajes cuestan más que una
        /// respuesta agregada, y sobre todo garantiza que los nueve gráficos
        /// de la pantalla correspondan al mismo instante de los datos. Nueve
        /// llamadas sueltas pueden devolver cifras que no cuadran entre sí si
        /// alguien vota mientras se cargan.
        ///
        /// Este método no arma SQL. Solo pasa los filtros como parámetros
        /// tipados y transporta filas. Todo el cálculo está en el script 08 y
        /// se puede verificar ejecutando los procedimientos a mano.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Analitica obtenerAnalitica(FiltroAnalitica filtro)
        {
            if (filtro == null) filtro = new FiltroAnalitica();

            Analitica a = new Analitica();
            a.generado = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                // Sin campaña indicada se usa la destacada. El filtro se
                // completa acá para que todos los procedimientos reciban el
                // mismo valor y el tablero declare cuál está mostrando.
                if (string.IsNullOrEmpty(Limpio(filtro.campanaSlug)))
                {
                    SqlCommand cmdActual = new SqlCommand(
                        "SELECT TOP (1) slug FROM dbo.Campanas WHERE esActual = 1", conn);
                    object v = cmdActual.ExecuteScalar();
                    filtro.campanaSlug = v == null ? string.Empty : Convert.ToString(v);
                }

                a.campanaSlug = Limpio(filtro.campanaSlug);

                SqlCommand cmdNombre = new SqlCommand(
                    "SELECT nombre FROM dbo.Campanas WHERE slug = @slug", conn);
                cmdNombre.Parameters.AddWithValue("@slug", a.campanaSlug);
                object nombre = cmdNombre.ExecuteScalar();
                a.campanaNombre = nombre == null ? string.Empty : Convert.ToString(nombre);

                a.opciones      = LeerOpciones(conn);
                a.resumen       = LeerResumen(conn, filtro);
                a.categorias    = LeerCategorias(conn, filtro);
                a.estados       = LeerEstados(conn, filtro);
                a.verificacion  = LeerVerificacion(conn, filtro);
                a.participacion = LeerParticipacion(conn, filtro);
                a.candidatos    = LeerCandidatos(conn, filtro);
                a.partidos      = LeerPartidos(conn, filtro);
                a.territorio    = LeerTerritorio(conn, filtro);
                a.actividad     = LeerActividad(conn, filtro);

                // La selección puede ser válida y aun así no contener nada:
                // una campaña sin candidaturas, o un cruce de filtros que no
                // deja ninguna fila. La página lo trata como estado vacío, no
                // como error.
                a.sinDatos = a.resumen.candidaturas == 0
                          && a.resumen.propuestas == 0
                          && a.resumen.valoraciones == 0;
            }

            return a;
        }

        // ------------------------------------------------- Armado de comandos

        /// <summary>Cadena sin espacios sobrantes. Nulo se vuelve vacío.</summary>
        internal static string Limpio(string texto)
        {
            return texto == null ? string.Empty : texto.Trim();
        }

        /// <summary>
        /// Convierte a parámetro lo que el tablero entiende por «sin filtrar».
        /// Cero y cadena vacía viajan como NULL, que es lo que los
        /// procedimientos interpretan como «todas».
        /// </summary>
        private static object Opcional(string valor)
        {
            string v = Limpio(valor);
            return v.Length == 0 ? (object)DBNull.Value : v;
        }

        private static object Opcional(int valor)
        {
            return valor <= 0 ? (object)DBNull.Value : valor;
        }

        /// <summary>
        /// Fecha recibida como texto aaaa-MM-dd. Cualquier cosa que no sea una
        /// fecha válida se descarta como «sin acotar» en lugar de propagar un
        /// error: el filtro es una comodidad, no debe poder romper el tablero.
        /// </summary>
        private static object OpcionalFecha(string valor)
        {
            DateTime fecha;
            string v = Limpio(valor);

            if (v.Length == 0) return DBNull.Value;

            if (!DateTime.TryParseExact(v, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out fecha))
                return DBNull.Value;

            return fecha;
        }

        /// <summary>
        /// Prepara la llamada a un procedimiento con solo los parámetros que
        /// ese procedimiento declara. Pasar uno de más es un error en SQL
        /// Server, así que cada indicador dice explícitamente por qué
        /// dimensiones se puede filtrar.
        /// </summary>
        private static SqlCommand Procedimiento(SqlConnection conn, string nombre, FiltroAnalitica f,
                                                bool categoria, bool partido, bool departamento,
                                                bool nivel, bool fechas)
        {
            SqlCommand cmd = new SqlCommand(nombre, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@campanaSlug", Opcional(f.campanaSlug));

            if (categoria)    cmd.Parameters.AddWithValue("@codigoCategoria", Opcional(f.codigoCategoria));
            if (partido)      cmd.Parameters.AddWithValue("@codigoPartido", Opcional(f.codigoPartido));
            if (departamento) cmd.Parameters.AddWithValue("@codigoDepartamento", Opcional(f.codigoDepartamento));
            if (nivel)        cmd.Parameters.AddWithValue("@nivelGobierno", Opcional(f.nivelGobierno));

            if (fechas)
            {
                cmd.Parameters.Add("@desde", SqlDbType.Date).Value = OpcionalFecha(f.desde);
                cmd.Parameters.Add("@hasta", SqlDbType.Date).Value = OpcionalFecha(f.hasta);
            }

            return cmd;
        }

        // ------------------------------------------------------- Indicadores

        internal static OpcionFiltro[] LeerOpciones(SqlConnection conn)
        {
            List<OpcionFiltro> lista = new List<OpcionFiltro>();

            SqlCommand cmd = new SqlCommand("dbo.spAnaliticaCatalogos", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new OpcionFiltro
                    {
                        grupo = Texto(reader, "grupo"),
                        valor = Texto(reader, "valor"),
                        texto = Texto(reader, "texto")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static ResumenAnalitica LeerResumen(SqlConnection conn, FiltroAnalitica f)
        {
            ResumenAnalitica r = new ResumenAnalitica();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaResumen", f,
                categoria: true, partido: true, departamento: true, nivel: true, fechas: true);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    r.propuestas                 = Entero(reader, "propuestas");
                    r.candidaturas               = Entero(reader, "candidaturas");
                    r.partidos                   = Entero(reader, "partidos");
                    r.publicaciones              = Entero(reader, "publicaciones");
                    r.categoriasConOferta        = Entero(reader, "categoriasConOferta");
                    r.categoriasTotal            = Entero(reader, "categoriasTotal");
                    r.propuestasEvaluadas        = Entero(reader, "propuestasEvaluadas");
                    r.propuestasVerificadas      = Entero(reader, "propuestasVerificadas");
                    r.candidaturasVerificadas    = Entero(reader, "candidaturasVerificadas");
                    r.departamentosConCandidatura = Entero(reader, "departamentosConCandidatura");
                    r.departamentosTotal         = Entero(reader, "departamentosTotal");
                    r.valoraciones               = Entero(reader, "valoraciones");
                    r.meGusta                    = Entero(reader, "meGusta");
                    r.noMeGusta                  = Entero(reader, "noMeGusta");
                    r.comentarios                = Entero(reader, "comentarios");
                    r.personasParticipando       = Entero(reader, "personas");
                    r.ultimoRegistro             = Fecha(reader, "ultimoRegistro");
                }
            }

            return r;
        }

        internal static FilaCategoria[] LeerCategorias(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaCategoria> lista = new List<FilaCategoria>();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaCategorias", f,
                categoria: false, partido: true, departamento: true, nivel: true, fechas: false);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaCategoria
                    {
                        codigoCategoria = Entero(reader, "codigoCategoria"),
                        categoria       = Texto(reader, "categoria"),
                        interesEncuesta = Decimal(reader, "interesEncuesta"),
                        propuestas      = Entero(reader, "propuestas"),
                        candidaturas    = Entero(reader, "candidaturas")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaEstado[] LeerEstados(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaEstado> lista = new List<FilaEstado>();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaEstados", f,
                categoria: true, partido: true, departamento: true, nivel: true, fechas: false);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaEstado
                    {
                        estado      = Texto(reader, "estado"),
                        descripcion = Texto(reader, "descripcion"),
                        ponderacion = Decimal(reader, "ponderacion"),
                        propuestas  = Entero(reader, "propuestas")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaVerificacion[] LeerVerificacion(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaVerificacion> lista = new List<FilaVerificacion>();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaVerificacion", f,
                categoria: true, partido: true, departamento: true, nivel: true, fechas: false);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaVerificacion
                    {
                        entidad = Texto(reader, "entidad"),
                        nivel   = Texto(reader, "nivel"),
                        total   = Entero(reader, "total")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaParticipacion[] LeerParticipacion(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaParticipacion> lista = new List<FilaParticipacion>();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaParticipacion", f,
                categoria: true, partido: true, departamento: true, nivel: true, fechas: true);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaParticipacion
                    {
                        tipoObjeto  = Texto(reader, "tipoObjeto"),
                        meGusta     = Entero(reader, "meGusta"),
                        noMeGusta   = Entero(reader, "noMeGusta"),
                        comentarios = Entero(reader, "comentarios")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaCandidato[] LeerCandidatos(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaCandidato> lista = new List<FilaCandidato>();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaCandidatos", f,
                categoria: true, partido: true, departamento: true, nivel: true, fechas: true);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaCandidato
                    {
                        candidatoSlug = Texto(reader, "candidatoSlug"),
                        candidato     = Texto(reader, "candidato"),
                        partidoSiglas = Texto(reader, "partidoSiglas"),
                        departamento  = Texto(reader, "departamento"),
                        cargo         = Texto(reader, "cargo"),
                        nivelGobierno = Texto(reader, "nivelGobierno"),
                        verificacion  = Texto(reader, "verificacion"),
                        propuestas    = Entero(reader, "propuestas"),
                        meGusta       = Entero(reader, "meGusta"),
                        noMeGusta     = Entero(reader, "noMeGusta"),
                        saldo         = Entero(reader, "saldo"),
                        comentarios   = Entero(reader, "comentarios")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaPartido[] LeerPartidos(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaPartido> lista = new List<FilaPartido>();

            // Sin @codigoPartido: filtrar el ranking de partidos por un partido
            // dejaría un gráfico de una sola barra, que no compara nada.
            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaPartidos", f,
                categoria: true, partido: false, departamento: true, nivel: true, fechas: true);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaPartido
                    {
                        codigoPartido           = Entero(reader, "codigoPartido"),
                        partidoSiglas           = Texto(reader, "partidoSiglas"),
                        partido                 = Texto(reader, "partido"),
                        candidaturas            = Entero(reader, "candidaturas"),
                        propuestas              = Entero(reader, "propuestas"),
                        propuestasPorCandidatura = Decimal(reader, "propuestasPorCandidatura"),
                        meGusta                 = Entero(reader, "meGusta"),
                        noMeGusta               = Entero(reader, "noMeGusta")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaDepartamento[] LeerTerritorio(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaDepartamento> lista = new List<FilaDepartamento>();

            // Sin @codigoDepartamento: el indicador es justamente el mapa de
            // los 18, y acotarlo a uno lo vaciaría de sentido.
            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaTerritorio", f,
                categoria: true, partido: true, departamento: false, nivel: true, fechas: false);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaDepartamento
                    {
                        codigoDepartamento = Entero(reader, "codigoDepartamento"),
                        departamento       = Texto(reader, "departamento"),
                        candidaturas       = Entero(reader, "candidaturas"),
                        propuestas         = Entero(reader, "propuestas"),
                        valoraciones       = Entero(reader, "valoraciones")
                    });
                }
            }

            return lista.ToArray();
        }

        internal static FilaActividad[] LeerActividad(SqlConnection conn, FiltroAnalitica f)
        {
            List<FilaActividad> lista = new List<FilaActividad>();

            SqlCommand cmd = Procedimiento(conn, "dbo.spAnaliticaActividad", f,
                categoria: false, partido: true, departamento: true, nivel: true, fechas: true);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new FilaActividad
                    {
                        fecha       = Fecha(reader, "fecha"),
                        meGusta     = Entero(reader, "meGusta"),
                        noMeGusta   = Entero(reader, "noMeGusta"),
                        comentarios = Entero(reader, "comentarios")
                    });
                }
            }

            return lista.ToArray();
        }

        // --------------------------------------------------------- Lectores

        /// <summary>Entero tratando NULL como cero.</summary>
        private static int Entero(IDataRecord reader, string columna)
        {
            object valor = reader[columna];
            return valor == DBNull.Value ? 0 : Convert.ToInt32(valor);
        }

        /// <summary>Decimal tratando NULL como cero.</summary>
        private static decimal Decimal(IDataRecord reader, string columna)
        {
            object valor = reader[columna];
            return valor == DBNull.Value ? 0m : Convert.ToDecimal(valor);
        }

        /// <summary>
        /// Fecha en formato aaaa-MM-dd, invariante. Viaja como texto para que
        /// la cultura del servidor no altere lo que el frontend recibe.
        /// </summary>
        private static string Fecha(IDataRecord reader, string columna)
        {
            object valor = reader[columna];
            if (valor == DBNull.Value) return string.Empty;

            return Convert.ToDateTime(valor).ToString("yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture);
        }


        // =============================================================
        //  Lectores
        // =============================================================

        private static Partido LeerPartido(IDataRecord reader)
        {
            return new Partido
            {
                codigoPartido = Convert.ToInt32(reader["codigoPartido"]),
                slug = Texto(reader, "slug"),
                nombre = Texto(reader, "nombre"),
                siglas = Texto(reader, "siglas"),
                descripcion = Texto(reader, "descripcion"),
                totalCandidatos = Convert.ToInt32(reader["totalCandidatos"]),
                totalPropuestas = Convert.ToInt32(reader["totalPropuestas"]),
                meGusta = Convert.ToInt32(reader["meGusta"]),
                noMeGusta = Convert.ToInt32(reader["noMeGusta"]),
                comentarios = Convert.ToInt32(reader["comentarios"])
            };
        }

        private static List<Catalogo> LeerCatalogo(string query)
        {
            List<Catalogo> lista = new List<Catalogo>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Catalogo
                    {
                        codigo = Convert.ToInt32(reader["codigo"]),
                        nombre = Texto(reader, "nombre"),
                        detalle = Texto(reader, "detalle")
                    });
                }
            }

            return lista;
        }

        private static Campana LeerCampana(IDataRecord reader)
        {
            return new Campana
            {
                codigoCampana = Convert.ToInt32(reader["codigoCampana"]),
                slug = Texto(reader, "slug"),
                nombre = Texto(reader, "nombre"),
                resumen = Texto(reader, "resumen"),
                descripcion = Texto(reader, "descripcion"),
                alcance = Texto(reader, "alcance"),
                fechaInicio = Convert.ToDateTime(reader["fechaInicio"]),
                fechaEleccion = Convert.ToDateTime(reader["fechaEleccion"]),
                estado = Texto(reader, "estado"),
                esActual = Convert.ToBoolean(reader["esActual"]),
                totalCandidatos = Convert.ToInt32(reader["totalCandidatos"]),
                totalPropuestas = Convert.ToInt32(reader["totalPropuestas"]),
                totalPublicaciones = Convert.ToInt32(reader["totalPublicaciones"])
            };
        }

        private static Candidato LeerCandidato(IDataRecord reader)
        {
            return new Candidato
            {
                codigoCandidato = Convert.ToInt32(reader["codigoCandidato"]),
                slug = Texto(reader, "slug"),
                codigoCampana = Convert.ToInt32(reader["codigoCampana"]),
                campanaSlug = Texto(reader, "campanaSlug"),
                campanaNombre = Texto(reader, "campanaNombre"),
                nombres = Texto(reader, "nombres"),
                apellidos = Texto(reader, "apellidos"),
                partido = Texto(reader, "partido"),
                partidoSiglas = Texto(reader, "partidoSiglas"),
                cargo = Texto(reader, "cargo"),
                nivelGobierno = Texto(reader, "nivelGobierno"),
                departamento = Texto(reader, "departamento"),
                municipio = Texto(reader, "municipio"),
                fotoUrl = Texto(reader, "fotoUrl"),
                titular = Texto(reader, "titular"),
                biografia = Texto(reader, "biografia"),
                informacionProfesional = Texto(reader, "informacionProfesional"),
                descripcionCandidatura = Texto(reader, "descripcionCandidatura"),
                correoPublico = Texto(reader, "correoPublico"),
                telefono = Texto(reader, "telefono"),
                sitioWeb = Texto(reader, "sitioWeb"),
                facebook = Texto(reader, "facebook"),
                x = Texto(reader, "x"),
                instagram = Texto(reader, "instagram"),
                verificacion = Texto(reader, "verificacion"),
                partidoSlug = Texto(reader, "partidoSlug"),
                totalPropuestas = Convert.ToInt32(reader["totalPropuestas"]),
                totalPublicaciones = Convert.ToInt32(reader["totalPublicaciones"]),
                meGusta = Convert.ToInt32(reader["meGusta"]),
                noMeGusta = Convert.ToInt32(reader["noMeGusta"]),
                comentarios = Convert.ToInt32(reader["comentarios"])
            };
        }

        private static Propuesta LeerPropuesta(IDataRecord reader)
        {
            return new Propuesta
            {
                codigoPropuesta = Convert.ToInt32(reader["codigoPropuesta"]),
                codigoCandidato = Convert.ToInt32(reader["codigoCandidato"]),
                candidatoSlug = Texto(reader, "candidatoSlug"),
                candidatoNombre = Texto(reader, "candidatoNombre"),
                campanaSlug = Texto(reader, "campanaSlug"),
                nombre = Texto(reader, "nombre"),
                descripcion = Texto(reader, "descripcion"),
                problema = Texto(reader, "problema"),
                objetivo = Texto(reader, "objetivo"),
                beneficiarios = Texto(reader, "beneficiarios"),
                categoria = Texto(reader, "categoria"),
                ubicacion = Texto(reader, "ubicacion"),
                periodoEjecucion = Texto(reader, "periodoEjecucion"),
                estado = Texto(reader, "estado"),
                imagenUrl = Texto(reader, "imagenUrl"),
                informacionAdicional = Texto(reader, "informacionAdicional"),
                verificacion = Texto(reader, "verificacion"),
                fechaRegistro = Convert.ToDateTime(reader["fechaRegistro"]),
                meGusta = Convert.ToInt32(reader["meGusta"]),
                noMeGusta = Convert.ToInt32(reader["noMeGusta"]),
                comentarios = Convert.ToInt32(reader["comentarios"])
            };
        }

        private static Publicacion LeerPublicacion(IDataRecord reader)
        {
            return new Publicacion
            {
                codigoPublicacion = Convert.ToInt32(reader["codigoPublicacion"]),
                campanaSlug = Texto(reader, "campanaSlug"),
                codigoCandidato = Convert.ToInt32(reader["codigoCandidato"]),
                candidatoSlug = Texto(reader, "candidatoSlug"),
                candidatoNombre = Texto(reader, "candidatoNombre"),
                candidatoCargo = Texto(reader, "candidatoCargo"),
                candidatoFotoUrl = Texto(reader, "candidatoFotoUrl"),
                fecha = Convert.ToDateTime(reader["fecha"]),
                texto = Texto(reader, "texto"),
                imagenUrl = Texto(reader, "imagenUrl"),
                categoria = Texto(reader, "categoria"),
                codigoPropuesta = Convert.ToInt32(reader["codigoPropuesta"]),
                propuestaNombre = Texto(reader, "propuestaNombre"),
                verificacion = Texto(reader, "verificacion"),
                meGusta = Convert.ToInt32(reader["meGusta"]),
                noMeGusta = Convert.ToInt32(reader["noMeGusta"]),
                comentarios = Convert.ToInt32(reader["comentarios"])
            };
        }


        // =============================================================
        //  Administración de la plataforma
        //
        //  Todo lo de esta sección exige el rol Administrador, incluidas las
        //  consultas: la bandeja de verificación y la bitácora no son públicas.
        //
        //  La comprobación se hace dos veces, acá y dentro de cada
        //  procedimiento de escritura. No es redundancia por descuido: el
        //  control de acceso no debe depender de un solo punto, y la
        //  comprobación de la base queda documentada como parte del modelo.
        //
        //  Ninguno de estos métodos escribe SQL propio. Cada uno invoca su
        //  procedimiento del script 09, que es donde vive la regla y donde el
        //  Manual Técnico del capítulo IX la puede citar.
        // =============================================================

        /// <summary>
        /// Cola de trabajo de la verificación: candidaturas, propuestas y
        /// publicaciones con su nivel actual.
        ///
        /// Con <paramref name="soloPendientes"/> en verdadero deja fuera lo ya
        /// verificado, que es la vista con la que se trabaja a diario.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<ItemVerificacion> listarBandejaVerificacion(
            int codigoUsuario, string tipoObjeto, string campanaSlug, bool soloPendientes)
        {
            List<ItemVerificacion> lista = new List<ItemVerificacion>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminBandejaVerificacion", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tipoObjeto", (object)tipoObjeto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@campanaSlug", (object)campanaSlug ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@soloPendientes", soloPendientes);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new ItemVerificacion
                    {
                        tipoObjeto = Texto(reader, "tipoObjeto"),
                        codigoObjeto = Convert.ToInt32(reader["codigoObjeto"]),
                        titulo = Texto(reader, "titulo"),
                        resumen = Texto(reader, "resumen"),
                        slug = Texto(reader, "slug"),
                        candidato = Texto(reader, "candidato"),
                        candidatoSlug = Texto(reader, "candidatoSlug"),
                        campanaSlug = Texto(reader, "campanaSlug"),
                        codigoVerificacion = Convert.ToInt32(reader["codigoVerificacion"]),
                        verificacion = Texto(reader, "verificacion"),
                        verificacionOrden = Convert.ToInt32(reader["verificacionOrden"]),
                        fecha = Convert.ToDateTime(reader["fecha"])
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Asigna el nivel de verificación de una candidatura, una propuesta o
        /// una publicación.
        ///
        /// El motivo es donde queda anotada la fuente que respalda la decisión,
        /// y el procedimiento lo exige al marcar como verificado.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin cambiarVerificacion(
            int codigoUsuario, string tipoObjeto, int codigoObjeto,
            int codigoVerificacion, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para verificar contenido.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminCambiarVerificacion", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@tipoObjeto", (object)tipoObjeto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@codigoObjeto", codigoObjeto);
                cmd.Parameters.AddWithValue("@codigoVerificacion", codigoVerificacion);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Publicaciones para moderación, incluidas las retiradas.
        ///
        /// <paramref name="estado"/> acepta Activas, Retiradas o vacío para
        /// todas.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<PublicacionModerada> listarPublicacionesModeracion(
            int codigoUsuario, string campanaSlug, string estado)
        {
            List<PublicacionModerada> lista = new List<PublicacionModerada>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminPublicaciones", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@campanaSlug", (object)campanaSlug ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@estado", (object)estado ?? DBNull.Value);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new PublicacionModerada
                    {
                        codigoPublicacion = Convert.ToInt32(reader["codigoPublicacion"]),
                        texto = Texto(reader, "texto"),
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        activo = Convert.ToBoolean(reader["activo"]),
                        motivoBaja = Texto(reader, "motivoBaja"),
                        retiradaPor = Texto(reader, "retiradaPor"),
                        candidato = Texto(reader, "candidato"),
                        candidatoSlug = Texto(reader, "candidatoSlug"),
                        campanaSlug = Texto(reader, "campanaSlug"),
                        categoria = Texto(reader, "categoria"),
                        verificacion = Texto(reader, "verificacion"),
                        meGusta = Convert.ToInt32(reader["meGusta"]),
                        noMeGusta = Convert.ToInt32(reader["noMeGusta"]),
                        comentarios = Convert.ToInt32(reader["comentarios"])
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Retira una publicación de la consulta pública, o la restaura.
        ///
        /// Retirar es una baja lógica: la fila se conserva con el motivo y el
        /// responsable. El motivo es obligatorio en las dos direcciones.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin moderarPublicacion(
            int codigoUsuario, int codigoPublicacion, bool activo, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para moderar publicaciones.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminModerarPublicacion", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoPublicacion", codigoPublicacion);
                cmd.Parameters.AddWithValue("@activo", activo);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Bitácora de administración, de lo más reciente a lo más antiguo.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<RegistroAuditoria> listarAuditoria(int codigoUsuario, string accion, int limite)
        {
            List<RegistroAuditoria> lista = new List<RegistroAuditoria>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminAuditoria", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@accion", (object)accion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@limite", limite);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new RegistroAuditoria
                    {
                        codigoAuditoria = Convert.ToInt32(reader["codigoAuditoria"]),
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        usuario = Texto(reader, "usuario"),
                        accion = Texto(reader, "accion"),
                        tipoObjeto = Texto(reader, "tipoObjeto"),
                        codigoObjeto = Convert.ToInt32(reader["codigoObjeto"]),
                        detalle = Texto(reader, "detalle"),
                        motivo = Texto(reader, "motivo")
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Niveles de verificación disponibles, para el desplegable del área de
        /// administración. Es público: los mismos niveles se muestran como
        /// etiqueta en todo el sitio.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Catalogo> listarNivelesVerificacion()
        {
            return LeerCatalogo(
                "SELECT codigoVerificacion AS codigo, nombre, ISNULL(descripcion,'') AS detalle " +
                "FROM dbo.NivelesVerificacion ORDER BY orden");
        }

        /// <summary>
        /// Lee la fila de ok y mensaje que devuelven los procedimientos de
        /// escritura del script 09.
        /// </summary>
        /// <summary>
        /// Lee la fila de ok y mensaje que devuelven los procedimientos de
        /// escritura.
        ///
        /// El lector se cierra antes de devolver. Mientras cada método hacía
        /// una sola llamada y soltaba la conexión enseguida, dejarlo abierto no
        /// se notaba, pero el primer método que quiso ejecutar algo más sobre
        /// la misma conexión falló con «ya hay un DataReader abierto». Se cierra
        /// acá y no en cada llamador por la misma razón por la que la
        /// comprobación de rol vive en un solo lugar.
        /// </summary>
        private static RespuestaAdmin LeerRespuesta(SqlCommand cmd)
        {
            RespuestaAdmin respuesta = new RespuestaAdmin();
            respuesta.ok = false;
            respuesta.mensaje = "No se pudo completar la acción.";

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    respuesta.ok = Convert.ToBoolean(reader["ok"]);
                    respuesta.mensaje = Convert.ToString(reader["mensaje"]);
                }
            }

            return respuesta;
        }

        private static RespuestaAdmin Rechazo(string mensaje)
        {
            return new RespuestaAdmin { ok = false, mensaje = mensaje };
        }


        // =============================================================
        //  Administración de catálogos
        //
        //  Partidos, campañas y candidaturas. Mismas reglas que la sección
        //  anterior: el rol se confirma acá y otra vez dentro de cada
        //  procedimiento, y ninguna consulta se escribe a mano.
        // =============================================================

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<PartidoAdmin> listarPartidosAdmin(int codigoUsuario, bool soloActivos)
        {
            List<PartidoAdmin> lista = new List<PartidoAdmin>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminPartidos", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@soloActivos", soloActivos);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new PartidoAdmin
                    {
                        codigoPartido = Convert.ToInt32(reader["codigoPartido"]),
                        slug = Texto(reader, "slug"),
                        nombre = Texto(reader, "nombre"),
                        siglas = Texto(reader, "siglas"),
                        descripcion = Texto(reader, "descripcion"),
                        activo = Convert.ToBoolean(reader["activo"]),
                        candidaturas = Convert.ToInt32(reader["candidaturas"])
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Alta o edición de un partido. Con <paramref name="codigoPartido"/>
        /// en cero es alta.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaGuardado guardarPartido(
            int codigoUsuario, int codigoPartido, string nombre, string siglas, string descripcion)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return RechazoGuardado("La cuenta no tiene permiso para administrar partidos.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminGuardarPartido", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoPartido", codigoPartido);
                cmd.Parameters.AddWithValue("@nombre", (object)nombre ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@siglas", (object)siglas ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);

                return LeerGuardado(cmd);
            }
        }

        /// <summary>
        /// Activa o desactiva un partido. No hay borrado: eliminar un partido
        /// dejaría candidaturas huérfanas y borraría de la historia a quién se
        /// presentó por él.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin cambiarEstadoPartido(
            int codigoUsuario, int codigoPartido, bool activo, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para administrar partidos.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminEstadoPartido", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoPartido", codigoPartido);
                cmd.Parameters.AddWithValue("@activo", activo);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<CampanaAdmin> listarCampanasAdmin(int codigoUsuario)
        {
            List<CampanaAdmin> lista = new List<CampanaAdmin>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminCampanas", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new CampanaAdmin
                    {
                        codigoCampana = Convert.ToInt32(reader["codigoCampana"]),
                        slug = Texto(reader, "slug"),
                        nombre = Texto(reader, "nombre"),
                        resumen = Texto(reader, "resumen"),
                        descripcion = Texto(reader, "descripcion"),
                        alcance = Texto(reader, "alcance"),
                        fechaInicio = Convert.ToDateTime(reader["fechaInicio"]),
                        fechaEleccion = Convert.ToDateTime(reader["fechaEleccion"]),
                        estado = Texto(reader, "estado"),
                        esActual = Convert.ToBoolean(reader["esActual"]),
                        candidaturas = Convert.ToInt32(reader["candidaturas"]),
                        propuestas = Convert.ToInt32(reader["propuestas"])
                    });
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaGuardado guardarCampana(
            int codigoUsuario, int codigoCampana, string nombre, string resumen,
            string descripcion, string alcance, DateTime fechaInicio, DateTime fechaEleccion,
            string estado, bool esActual)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return RechazoGuardado("La cuenta no tiene permiso para administrar campañas.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminGuardarCampana", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoCampana", codigoCampana);
                cmd.Parameters.AddWithValue("@nombre", (object)nombre ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@resumen", (object)resumen ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@alcance", (object)alcance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaEleccion", fechaEleccion);
                cmd.Parameters.AddWithValue("@estado", (object)estado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@esActual", esActual);

                return LeerGuardado(cmd);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<CandidatoAdmin> listarCandidatosAdmin(
            int codigoUsuario, string campanaSlug, bool soloActivos)
        {
            List<CandidatoAdmin> lista = new List<CandidatoAdmin>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminCandidatos", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@campanaSlug", (object)campanaSlug ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@soloActivos", soloActivos);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new CandidatoAdmin
                    {
                        codigoCandidato = Convert.ToInt32(reader["codigoCandidato"]),
                        slug = Texto(reader, "slug"),
                        nombres = Texto(reader, "nombres"),
                        apellidos = Texto(reader, "apellidos"),
                        nombreCompleto = Texto(reader, "nombreCompleto"),
                        codigoCampana = Convert.ToInt32(reader["codigoCampana"]),
                        campanaSlug = Texto(reader, "campanaSlug"),
                        campana = Texto(reader, "campana"),
                        codigoPartido = Convert.ToInt32(reader["codigoPartido"]),
                        partido = Texto(reader, "partido"),
                        codigoCargo = Convert.ToInt32(reader["codigoCargo"]),
                        cargo = Texto(reader, "cargo"),
                        codigoDepartamento = Convert.ToInt32(reader["codigoDepartamento"]),
                        departamento = Texto(reader, "departamento"),
                        municipio = Texto(reader, "municipio"),
                        titular = Texto(reader, "titular"),
                        verificacion = Texto(reader, "verificacion"),
                        activo = Convert.ToBoolean(reader["activo"]),
                        fechaRegistro = Convert.ToDateTime(reader["fechaRegistro"]),
                        login = Texto(reader, "login"),
                        propuestas = Convert.ToInt32(reader["propuestas"])
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Registra los datos de identificación de una candidatura. El resto
        /// del perfil lo llena la propia candidatura desde su panel: la
        /// plataforma la registra, no la redacta.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaGuardado guardarCandidato(
            int codigoUsuario, int codigoCandidato, string nombres, string apellidos,
            int codigoCampana, int codigoCargo, int codigoPartido, int codigoDepartamento,
            string municipio, string titular)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return RechazoGuardado("La cuenta no tiene permiso para administrar candidaturas.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminGuardarCandidato", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoCandidato", codigoCandidato);
                cmd.Parameters.AddWithValue("@nombres", (object)nombres ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@apellidos", (object)apellidos ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@codigoCampana", codigoCampana);
                cmd.Parameters.AddWithValue("@codigoCargo", codigoCargo);
                cmd.Parameters.AddWithValue("@codigoPartido", codigoPartido);
                cmd.Parameters.AddWithValue("@codigoDepartamento", codigoDepartamento);
                cmd.Parameters.AddWithValue("@municipio", (object)municipio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@titular", (object)titular ?? DBNull.Value);

                return LeerGuardado(cmd);
            }
        }

        /// <summary>
        /// Retira una candidatura de la consulta pública, o la reincorpora. La
        /// cuenta de acceso sigue el mismo estado: una candidatura retirada que
        /// aún puede publicar sería una puerta abierta sin ficha detrás.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin cambiarEstadoCandidato(
            int codigoUsuario, int codigoCandidato, bool activo, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para administrar candidaturas.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminEstadoCandidato", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoCandidato", codigoCandidato);
                cmd.Parameters.AddWithValue("@activo", activo);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Crea la cuenta de acceso de una candidatura.
        ///
        /// La contraseña llega en claro por el canal del servicio y se convierte
        /// a hash dentro del procedimiento. Ni la respuesta ni la bitácora la
        /// devuelven: quien la crea es quien la entrega.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin crearCuentaCandidato(
            int codigoUsuario, int codigoCandidato, string login, string correo, string clave)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para crear cuentas de acceso.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminCrearCuentaCandidato", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoCandidato", codigoCandidato);
                cmd.Parameters.AddWithValue("@login", (object)login ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@correo", (object)correo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@clave", (object)clave ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Lee la fila de ok, mensaje y codigo de los procedimientos de
        /// guardado.
        /// </summary>
        private static RespuestaGuardado LeerGuardado(SqlCommand cmd)
        {
            RespuestaGuardado respuesta = new RespuestaGuardado();
            respuesta.ok = false;
            respuesta.mensaje = "No se pudo completar la acción.";
            respuesta.codigo = 0;

            // El lector se cierra por la misma razón que en LeerRespuesta.
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    respuesta.ok = Convert.ToBoolean(reader["ok"]);
                    respuesta.mensaje = Convert.ToString(reader["mensaje"]);
                    respuesta.codigo = Convert.ToInt32(reader["codigo"]);
                }
            }

            return respuesta;
        }

        private static RespuestaGuardado RechazoGuardado(string mensaje)
        {
            return new RespuestaGuardado { ok = false, mensaje = mensaje, codigo = 0 };
        }


        // =============================================================
        //  Encuestas de percepción
        //
        //  La tercera pata del módulo de participación, junto a las
        //  valoraciones y los comentarios. Igual que en las otras dos
        //  secciones, este servicio no escribe SQL: cada método invoca su
        //  procedimiento del script 14, que es donde vive la regla.
        //
        //  Consultar es público y participar exige cuenta, el mismo criterio
        //  del script 05. Por eso las lecturas no comprueban el módulo y el
        //  voto sí: quien administra tiene que poder revisar una encuesta
        //  oculta antes de publicarla.
        // =============================================================

        /// <summary>
        /// Las encuestas abiertas de una campaña, con sus opciones dentro.
        ///
        /// Con la campaña vacía usa la destacada, que es como las pide la
        /// portada. El código de usuario viaja para saber en cuáles respondió
        /// ya esa persona, y con ello en cuáles le corresponde ver el
        /// resultado.
        ///
        /// Dos consultas y no una por encuesta: la segunda trae las opciones de
        /// todas juntas y se reparten acá. La portada dibuja varias tarjetas en
        /// la misma respuesta, así que una llamada por encuesta serían tantos
        /// viajes como preguntas haya publicadas.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<EncuestaPublica> listarEncuestasVigentes(string campanaSlug, int codigoUsuario)
        {
            List<EncuestaPublica> lista = new List<EncuestaPublica>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.spEncuestasVigentes", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@campanaSlug",
                    string.IsNullOrEmpty(campanaSlug) ? (object)DBNull.Value : campanaSlug);
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new EncuestaPublica
                        {
                            codigoEncuesta = Convert.ToInt32(reader["codigoEncuesta"]),
                            campanaSlug = Texto(reader, "campanaSlug"),
                            pregunta = Texto(reader, "pregunta"),
                            descripcion = Texto(reader, "descripcion"),
                            categoria = Texto(reader, "categoria"),
                            fechaInicio = Convert.ToDateTime(reader["fechaInicio"]),
                            fechaCierre = FechaOVacio(reader, "fechaCierre"),
                            estado = Texto(reader, "estado"),
                            votos = Convert.ToInt32(reader["votos"]),
                            miOpcion = Convert.ToInt32(reader["miOpcion"]),
                            opciones = new OpcionEncuesta[0]
                        });
                    }
                }

                if (lista.Count == 0) return lista;

                RepartirOpciones(conn, campanaSlug, codigoUsuario, lista);
            }

            return lista;
        }

        /// <summary>
        /// Reparte entre las encuestas las opciones que llegan en un solo
        /// resultado, agrupadas por su código.
        /// </summary>
        private static void RepartirOpciones(SqlConnection conn, string campanaSlug,
            int codigoUsuario, List<EncuestaPublica> lista)
        {
            Dictionary<int, List<OpcionEncuesta>> porEncuesta =
                new Dictionary<int, List<OpcionEncuesta>>();

            SqlCommand cmd = new SqlCommand("dbo.spEncuestasVigentesOpciones", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@campanaSlug",
                string.IsNullOrEmpty(campanaSlug) ? (object)DBNull.Value : campanaSlug);
            cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int codigo = Convert.ToInt32(reader["codigoEncuesta"]);

                    if (!porEncuesta.ContainsKey(codigo))
                        porEncuesta[codigo] = new List<OpcionEncuesta>();

                    porEncuesta[codigo].Add(new OpcionEncuesta
                    {
                        codigoOpcion = Convert.ToInt32(reader["codigoOpcion"]),
                        texto = Texto(reader, "texto"),
                        orden = Convert.ToInt32(reader["orden"]),
                        votos = Convert.ToInt32(reader["votos"]),
                        miVoto = Convert.ToBoolean(reader["miVoto"])
                    });
                }
            }

            foreach (EncuestaPublica e in lista)
            {
                if (porEncuesta.ContainsKey(e.codigoEncuesta))
                    e.opciones = porEncuesta[e.codigoEncuesta].ToArray();
            }
        }

        /// <summary>
        /// Opciones de una encuesta con su resultado.
        ///
        /// El conteo va completo, se haya respondido o no. El código de usuario
        /// solo sirve para marcar cuál eligió esa persona.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<OpcionEncuesta> listarOpcionesEncuesta(int codigoEncuesta, int codigoUsuario)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                return LeerOpciones(conn, "dbo.spEncuestaOpciones", codigoEncuesta, codigoUsuario);
            }
        }

        /// <summary>
        /// Registra la respuesta de una persona y devuelve el resultado ya
        /// actualizado, para que la página no tenga que pedirlo aparte.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaEncuesta votarEncuesta(int codigoEncuesta, int codigoOpcion, int codigoUsuario)
        {
            RespuestaEncuesta r = new RespuestaEncuesta();
            r.ok = false;
            r.opciones = new OpcionEncuesta[0];

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();

                    if (!ModuloVisible(conn, "encuestas"))
                    {
                        r.mensaje = "Las encuestas están temporalmente cerradas.";
                        return r;
                    }

                    string motivo = MotivoSinParticipacion(conn, codigoUsuario,
                        "Necesitás una cuenta activa para participar.");
                    if (motivo != null)
                    {
                        r.mensaje = motivo;
                        return r;
                    }

                    SqlCommand cmd = new SqlCommand("dbo.spEncuestaVotar", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codigoEncuesta", codigoEncuesta);
                    cmd.Parameters.AddWithValue("@codigoOpcion", codigoOpcion);
                    cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);

                    RespuestaAdmin voto = LeerRespuesta(cmd);

                    r.ok = voto.ok;
                    r.mensaje = voto.mensaje;

                    // El resultado se devuelve siempre, también cuando el voto
                    // se rechazó: si la encuesta cerró mientras la persona la
                    // tenía abierta, mostrarle el resultado explica el rechazo
                    // mejor que el mensaje solo.
                    r.opciones = LeerOpciones(conn, "dbo.spEncuestaOpciones",
                        codigoEncuesta, codigoUsuario).ToArray();

                    SqlCommand total = new SqlCommand(
                        "SELECT votos FROM dbo.vwEncuestas WHERE codigoEncuesta = @e", conn);
                    total.Parameters.AddWithValue("@e", codigoEncuesta);

                    object leido = total.ExecuteScalar();
                    r.votos = leido == null || leido == DBNull.Value ? 0 : Convert.ToInt32(leido);
                }
            }
            catch (Exception ex)
            {
                r.ok = false;
                r.mensaje = "No se pudo registrar tu respuesta: " + ex.Message;
            }

            return r;
        }

        /// <summary>
        /// Lee las opciones desde el procedimiento que se le indique. Los dos
        /// —el público y el de administración— devuelven las mismas columnas,
        /// y se diferencian en si reservan el conteo y en si exigen rol.
        /// </summary>
        private static List<OpcionEncuesta> LeerOpciones(
            SqlConnection conn, string procedimiento, int primerParametro, int segundoParametro)
        {
            List<OpcionEncuesta> lista = new List<OpcionEncuesta>();

            SqlCommand cmd = new SqlCommand(procedimiento, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (procedimiento == "dbo.spAdminEncuestaOpciones")
            {
                cmd.Parameters.AddWithValue("@codigoUsuario", primerParametro);
                cmd.Parameters.AddWithValue("@codigoEncuesta", segundoParametro);
            }
            else
            {
                cmd.Parameters.AddWithValue("@codigoEncuesta", primerParametro);
                cmd.Parameters.AddWithValue("@codigoUsuario", segundoParametro);
            }

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new OpcionEncuesta
                {
                    codigoOpcion = Convert.ToInt32(reader["codigoOpcion"]),
                    texto = Texto(reader, "texto"),
                    orden = Convert.ToInt32(reader["orden"]),
                    votos = Convert.ToInt32(reader["votos"]),
                    miVoto = Convert.ToBoolean(reader["miVoto"])
                });
            }
            reader.Close();

            return lista;
        }

        /// <summary>
        /// Fecha que puede no existir, devuelta como texto. Un DateTime no
        /// tiene manera de decir «ninguna», y el año uno disfrazado de fecha
        /// obligaría a cada página a saber que ese valor es en realidad un
        /// hueco.
        /// </summary>
        private static string FechaOVacio(SqlDataReader reader, string columna)
        {
            object valor = reader[columna];
            if (valor == null || valor == DBNull.Value) return string.Empty;

            return Convert.ToDateTime(valor).ToString("yyyy-MM-dd HH:mm");
        }

        // ------------------------------------------- Administración

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<EncuestaAdmin> listarEncuestasAdmin(
            int codigoUsuario, string campanaSlug, string estado)
        {
            List<EncuestaAdmin> lista = new List<EncuestaAdmin>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminEncuestas", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@campanaSlug",
                    string.IsNullOrEmpty(campanaSlug) ? (object)DBNull.Value : campanaSlug);
                cmd.Parameters.AddWithValue("@estado",
                    string.IsNullOrEmpty(estado) ? (object)DBNull.Value : estado);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new EncuestaAdmin
                    {
                        codigoEncuesta = Convert.ToInt32(reader["codigoEncuesta"]),
                        codigoCampana = Convert.ToInt32(reader["codigoCampana"]),
                        campanaSlug = Texto(reader, "campanaSlug"),
                        campana = Texto(reader, "campana"),
                        pregunta = Texto(reader, "pregunta"),
                        descripcion = Texto(reader, "descripcion"),
                        codigoCategoria = Convert.ToInt32(reader["codigoCategoria"]),
                        categoria = Texto(reader, "categoria"),
                        fechaInicio = Convert.ToDateTime(reader["fechaInicio"]),
                        fechaCierre = FechaOVacio(reader, "fechaCierre"),
                        activo = Convert.ToBoolean(reader["activo"]),
                        motivoBaja = Texto(reader, "motivoBaja"),
                        estado = Texto(reader, "estado"),
                        opciones = Convert.ToInt32(reader["opciones"]),
                        votos = Convert.ToInt32(reader["votos"])
                    });
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<OpcionEncuesta> listarOpcionesEncuestaAdmin(int codigoUsuario, int codigoEncuesta)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                if (!EsAdministrador(conn, codigoUsuario)) return new List<OpcionEncuesta>();

                return LeerOpciones(conn, "dbo.spAdminEncuestaOpciones", codigoUsuario, codigoEncuesta);
            }
        }

        /// <summary>
        /// Alta o edición de una encuesta. Con <paramref name="codigoEncuesta"/>
        /// en cero es alta.
        ///
        /// Las opciones llegan en un solo texto, una por línea, y las separa el
        /// procedimiento. Va así porque el número de opciones lo decide quien
        /// escribe la pregunta, y un parámetro por opción obligaría a fijar un
        /// tope arbitrario en el contrato del servicio.
        ///
        /// La fecha de cierre viaja como texto para poder venir vacía. Una
        /// encuesta sin cierre programado es un caso normal, no un dato que
        /// falte.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaGuardado guardarEncuesta(
            int codigoUsuario, int codigoEncuesta, int codigoCampana,
            string pregunta, string descripcion, int codigoCategoria,
            DateTime fechaInicio, string fechaCierre, string opciones)
        {
            object cierre = DBNull.Value;

            if (!string.IsNullOrEmpty(fechaCierre))
            {
                DateTime leida;
                if (!DateTime.TryParse(fechaCierre,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out leida))
                {
                    return RechazoGuardado("La fecha de cierre no se entiende. Usá el formato aaaa-mm-dd.");
                }

                cierre = leida;
            }

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return RechazoGuardado("La cuenta no tiene permiso para administrar encuestas.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminGuardarEncuesta", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoEncuesta", codigoEncuesta);
                cmd.Parameters.AddWithValue("@codigoCampana", codigoCampana);
                cmd.Parameters.AddWithValue("@pregunta", (object)pregunta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@codigoCategoria", codigoCategoria);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaCierre", cierre);
                cmd.Parameters.AddWithValue("@opciones", (object)opciones ?? DBNull.Value);

                return LeerGuardado(cmd);
            }
        }

        /// <summary>
        /// Cierra, reabre, retira o restaura una encuesta.
        ///
        /// Cerrar y retirar no son lo mismo: una encuesta cerrada terminó su
        /// votación y sigue a la vista con su resultado, una retirada
        /// desaparece del sitio público. Las dos exigen motivo, y el
        /// procedimiento es quien lo exige.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin cambiarEstadoEncuesta(
            int codigoUsuario, int codigoEncuesta, string accion, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para administrar encuestas.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminEstadoEncuesta", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoEncuesta", codigoEncuesta);
                cmd.Parameters.AddWithValue("@accion", (object)accion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }


        // =============================================================
        //  Módulos
        //
        //  Qué está visible en el sitio público y qué está oculto.
        // =============================================================

        /// <summary>
        /// Estado efectivo de cada elemento apagable, para que el sitio sepa
        /// qué mostrar.
        ///
        /// No exige rol a propósito: lo consulta cada página en cada carga,
        /// también las que ve un visitante anónimo. Saber que el tablero está
        /// oculto no es información reservada, y pedir credenciales obligaría a
        /// autenticar a quien solo está consultando.
        ///
        /// Devuelve solo la clave y si está visible. El nombre, la descripción
        /// y quién lo cambió son parte de la administración y viajan por el otro
        /// método.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<EstadoModulo> listarModulosVisibles()
        {
            List<EstadoModulo> lista = new List<EstadoModulo>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                SqlCommand cmd = new SqlCommand("dbo.spModulosVisibles", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new EstadoModulo
                    {
                        clave = Texto(reader, "clave"),
                        visible = Convert.ToBoolean(reader["visible"])
                    });
                }
            }

            return lista;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<ModuloAdmin> listarModulosAdmin(int codigoUsuario)
        {
            List<ModuloAdmin> lista = new List<ModuloAdmin>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();
                if (!EsAdministrador(conn, codigoUsuario)) return lista;

                SqlCommand cmd = new SqlCommand("dbo.spAdminModulos", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new ModuloAdmin
                    {
                        codigoModulo = Convert.ToInt32(reader["codigoModulo"]),
                        clave = Texto(reader, "clave"),
                        nombre = Texto(reader, "nombre"),
                        descripcion = Texto(reader, "descripcion"),
                        grupo = Texto(reader, "grupo"),
                        clavePadre = Texto(reader, "clavePadre"),
                        habilitado = Convert.ToBoolean(reader["habilitado"]),
                        visible = Convert.ToBoolean(reader["visible"]),
                        apagadoPorPadre = Convert.ToBoolean(reader["apagadoPorPadre"]),
                        fechaCambio = reader["fechaCambio"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(reader["fechaCambio"]),
                        cambiadoPor = Texto(reader, "cambiadoPor")
                    });
                }
            }

            return lista;
        }

        /// <summary>
        /// Oculta un elemento del sitio público, o lo devuelve a la vista.
        /// Ocultar exige motivo, encender no: volver al estado normal no
        /// necesita justificación.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin cambiarEstadoModulo(
            int codigoUsuario, string clave, bool habilitado, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para configurar los módulos.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminCambiarModulo", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@clave", (object)clave ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@habilitado", habilitado);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Lee una columna de texto tratando NULL como cadena vacía, para que
        /// el frontend nunca reciba nulos que tenga que verificar.
        /// </summary>
        private static string Texto(IDataRecord reader, string columna)
        {
            object valor = reader[columna];
            return valor == DBNull.Value ? string.Empty : Convert.ToString(valor);
        }

        // =============================================================
        //  Iniciativas ciudadanas
        // =============================================================

        /// <summary>
        /// Las iniciativas activas, de la más popular a la menos. Es lo que
        /// dibuja la portada.
        ///
        /// Devuelve todas: cuántas se ven y de a cuántas se despliegan es una
        /// decisión de la página, y todas se dibujan en la misma respuesta
        /// para que «Ver más» no cueste un viaje. El código de usuario sirve
        /// para marcar el voto propio y cuáles son suyas. Con cero es un
        /// visitante sin cuenta.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<IniciativaPublica> listarIniciativas(int codigoUsuario)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.spIniciativasPublicas", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);

                return LeerIniciativas(cmd);
            }
        }

        /// <summary>
        /// Las iniciativas de una persona, activas y retiradas, para «Mi
        /// cuenta». Cada una dice si su texto todavía se puede editar.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<IniciativaPublica> listarIniciativasDeUsuario(int codigoUsuario)
        {
            if (codigoUsuario <= 0) return new List<IniciativaPublica>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.spIniciativasDeUsuario", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);

                return LeerIniciativas(cmd);
            }
        }

        /// <summary>
        /// Alta o edición de una iniciativa por una cuenta ciudadana.
        ///
        /// Las mismas tres puertas que votar y comentar, en el mismo orden:
        /// módulo visible, cuenta que puede participar (existe, está activa y
        /// confirmó su correo) y, además, rol Ciudadano. El procedimiento
        /// vuelve a comprobar el rol, la autoría y que la iniciativa no tenga
        /// reacciones: lo que el frontend sabe de su sesión decide qué
        /// formulario muestra, nunca qué se acepta.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaGuardado guardarIniciativa(int codigoUsuario, int codigoIniciativa,
            string titulo, string descripcion, int codigoCategoria, int codigoDepartamento)
        {
            RespuestaGuardado r = new RespuestaGuardado { ok = false, codigo = 0 };

            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();

                    if (!ModuloVisible(conn, "iniciativas"))
                    {
                        r.mensaje = "Las iniciativas ciudadanas están temporalmente cerradas.";
                        return r;
                    }

                    string motivo = MotivoSinParticipacion(conn, codigoUsuario,
                        "Necesitás una cuenta activa para proponer una iniciativa.");
                    if (motivo != null)
                    {
                        r.mensaje = motivo;
                        return r;
                    }

                    if (!EsCiudadano(conn, codigoUsuario))
                    {
                        r.mensaje = "Las iniciativas las proponen las cuentas ciudadanas.";
                        return r;
                    }

                    SqlCommand cmd = new SqlCommand("dbo.spIniciativaGuardar", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                    cmd.Parameters.AddWithValue("@codigoIniciativa", codigoIniciativa);
                    cmd.Parameters.AddWithValue("@titulo", (object)titulo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigoCategoria", codigoCategoria);
                    cmd.Parameters.AddWithValue("@codigoDepartamento", codigoDepartamento);

                    return LeerGuardado(cmd);
                }
            }
            catch (Exception ex)
            {
                r.mensaje = "No se pudo guardar la iniciativa: " + ex.Message;
                return r;
            }
        }

        /// <summary>
        /// Retiro de una iniciativa por quien la propuso. Baja lógica: las
        /// reacciones que recibió se conservan. La autoría la confirma el
        /// procedimiento.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin retirarIniciativaPropia(int codigoUsuario, int codigoIniciativa)
        {
            if (codigoUsuario <= 0)
                return Rechazo("Necesitás una cuenta activa para retirar una iniciativa.");

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.spIniciativaRetirarPropia", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoIniciativa", codigoIniciativa);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Listado para moderación, con las retiradas. <paramref name="estado"/>
        /// acepta «Activas», «Retiradas» o vacío para todas.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<IniciativaPublica> listarIniciativasAdmin(int codigoUsuario, string estado)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario)) return new List<IniciativaPublica>();

                SqlCommand cmd = new SqlCommand("dbo.spAdminIniciativas", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@estado",
                    string.IsNullOrEmpty(estado) ? (object)DBNull.Value : estado);

                return LeerIniciativas(cmd);
            }
        }

        /// <summary>
        /// Retira o restaura una iniciativa desde administración, con motivo
        /// obligatorio y fila en la bitácora. Gemelo de moderarPublicacion.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAdmin moderarIniciativa(
            int codigoUsuario, int codigoIniciativa, bool activo, string motivo)
        {
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!EsAdministrador(conn, codigoUsuario))
                    return Rechazo("La cuenta no tiene permiso para moderar iniciativas.");

                SqlCommand cmd = new SqlCommand("dbo.spAdminModerarIniciativa", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@codigoIniciativa", codigoIniciativa);
                cmd.Parameters.AddWithValue("@activo", activo);
                cmd.Parameters.AddWithValue("@motivo", (object)motivo ?? DBNull.Value);

                return LeerRespuesta(cmd);
            }
        }

        /// <summary>
        /// Confirma que la cuenta existe, está activa y tiene el rol Ciudadano.
        /// Gemelo de EsAdministrador, por la misma razón: la sesión del
        /// frontend no es una credencial.
        /// </summary>
        private static bool EsCiudadano(SqlConnection conn, int codigoUsuario)
        {
            if (codigoUsuario <= 0) return false;

            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) " +
                "FROM dbo.Usuarios u " +
                "INNER JOIN dbo.Roles r ON r.codigoRol = u.codigoRol " +
                "WHERE u.codigoUsuario = @u AND u.activo = 1 AND r.nombre = @rol", conn);
            cmd.Parameters.AddWithValue("@u", codigoUsuario);
            cmd.Parameters.AddWithValue("@rol", RolCiudadano);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Lee iniciativas desde cualquiera de los tres procedimientos, que
        /// devuelven las mismas columnas.
        /// </summary>
        private static List<IniciativaPublica> LeerIniciativas(SqlCommand cmd)
        {
            List<IniciativaPublica> lista = new List<IniciativaPublica>();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new IniciativaPublica
                    {
                        codigoIniciativa = Convert.ToInt32(reader["codigoIniciativa"]),
                        autora = Texto(reader, "autora"),
                        titulo = Texto(reader, "titulo"),
                        descripcion = Texto(reader, "descripcion"),
                        codigoCategoria = Convert.ToInt32(reader["codigoCategoria"]),
                        categoria = Texto(reader, "categoria"),
                        codigoDepartamento = Convert.ToInt32(reader["codigoDepartamento"]),
                        departamento = Texto(reader, "departamento"),
                        meGusta = Convert.ToInt32(reader["meGusta"]),
                        noMeGusta = Convert.ToInt32(reader["noMeGusta"]),
                        comentarios = Convert.ToInt32(reader["comentarios"]),
                        saldo = Convert.ToInt32(reader["saldo"]),
                        miValoracion = Convert.ToInt32(reader["miValoracion"]),
                        esMia = Convert.ToBoolean(reader["esMia"]),
                        activo = Convert.ToBoolean(reader["activo"]),
                        motivoBaja = Texto(reader, "motivoBaja"),
                        puedeEditar = Convert.ToBoolean(reader["puedeEditar"]),
                        fechaRegistro = Convert.ToDateTime(reader["fechaRegistro"]),
                        fechaEdicion = FechaOVacio(reader, "fechaEdicion")
                    });
                }
            }

            return lista;
        }

        // =============================================================
        //  Asistente de consulta en lenguaje natural
        // =============================================================

        /// <summary>
        /// Responde una pregunta en lenguaje natural sobre lo registrado en
        /// la plataforma.
        ///
        /// Las tres comprobaciones previas van acá y no en la página. El
        /// frontend decide qué botones muestra, nunca qué se permite: quien
        /// llame al servicio directamente manda el código de usuario que
        /// quiera, y este método es el que cuesta dinero de verdad.
        ///
        /// El registro se escribe siempre, responda el modelo o falle. Una
        /// bitácora que solo guarda los casos buenos no sirve para revisar
        /// los malos, que son los que hay que revisar.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public RespuestaAsistente preguntarAsistente(int codigoUsuario, string pregunta,
                                                     string campanaSlug)
        {
            RespuestaAsistente r = new RespuestaAsistente();
            r.fuentes = new string[0];
            r.mensaje = string.Empty;
            r.respuesta = string.Empty;

            string texto = Limpio(pregunta);

            if (texto.Length == 0)
            {
                r.mensaje = "Escribí una pregunta.";
                return r;
            }

            if (texto.Length > 1000) texto = texto.Substring(0, 1000);

            int limiteDiario;
            if (!int.TryParse(ConfigurationManager.AppSettings["AsistenteCuotaDiaria"],
                              out limiteDiario) || limiteDiario <= 0)
                limiteDiario = 20;

            // --- Comprobaciones, con la conexión de siempre ---------------
            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                conn.Open();

                if (!ModuloVisible(conn, "analitica.asistente"))
                {
                    r.mensaje = "El asistente está fuera de servicio en este momento.";
                    return r;
                }

                string motivo = MotivoSinParticipacion(conn, codigoUsuario,
                    "Para preguntarle al asistente hay que iniciar sesión.");
                if (motivo != null)
                {
                    r.mensaje = motivo;
                    return r;
                }

                SqlCommand cmd = new SqlCommand("dbo.spIACuotaDisponible", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                cmd.Parameters.AddWithValue("@limiteDiario", limiteDiario);

                using (SqlDataReader lector = cmd.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        r.restantes = Convert.ToInt32(lector["restantes"]);

                        if (!Convert.ToBoolean(lector["permitido"]))
                        {
                            r.mensaje = "Llegaste al límite de " + limiteDiario
                                      + " consultas por día. Vuelve mañana.";
                            return r;
                        }
                    }
                }
            }

            // --- La llamada al modelo, ya sin conexión abierta ------------
            //
            // Tarda entre unos pocos segundos y medio minuto. Sostener una
            // conexión de SQL Server durante ese rato desperdicia una del
            // pool por cada pregunta en curso, sin ninguna necesidad.

            System.Diagnostics.Stopwatch reloj = System.Diagnostics.Stopwatch.StartNew();
            ResultadoIA salida = null;
            string error = null;

            try
            {
                salida = AsistenteIA.Preguntar(texto, campanaSlug);
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name + ": " + ex.Message;
                System.Diagnostics.Trace.TraceError("Asistente: " + ex);
            }

            reloj.Stop();

            // --- Registro y respuesta ------------------------------------
            bool respondio = error == null && salida != null
                          && !string.IsNullOrEmpty(salida.texto);

            if (respondio)
            {
                r.ok = true;
                r.respuesta = salida.texto;
                // Sin fuentes significa que el modelo respondió sin consultar
                // la base, que pasa cuando la pregunta se contesta con las
                // reglas —un pedido de ranking, por ejemplo—. Decirlo importa:
                // callarlo dejaría una respuesta sin respaldo con el mismo
                // aspecto que una respaldada.
                r.fuentes = salida.fuentes.Count > 0
                          ? salida.fuentes.ToArray()
                          : new string[] { "Esta respuesta no consultó datos de la plataforma" };
                r.restantes = r.restantes - 1;
            }
            else
            {
                r.mensaje = "No se pudo consultar al asistente en este momento. "
                          + "Los gráficos del tablero siguen disponibles.";
            }

            RegistrarConsulta(codigoUsuario, texto, salida, error,
                              (int)reloj.ElapsedMilliseconds, respondio);

            return r;
        }

        /// <summary>
        /// Deja la consulta en ConsultasIA. Usa la conexión normal y no la del
        /// asistente, que no tiene permiso de escritura: la bitácora es una
        /// decisión del servicio, no algo que el modelo pueda provocar.
        ///
        /// Si el registro falla no se le arruina la respuesta a la persona.
        /// Queda en el rastro de la aplicación y ya.
        /// </summary>
        private static void RegistrarConsulta(int codigoUsuario, string pregunta,
                                              ResultadoIA salida, string error,
                                              int milisegundos, bool respondio)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cadenaConexion))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spIARegistrarConsulta", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                    cmd.Parameters.AddWithValue("@pregunta", pregunta);
                    cmd.Parameters.AddWithValue("@respondio", respondio);
                    cmd.Parameters.AddWithValue("@milisegundos", milisegundos);
                    cmd.Parameters.AddWithValue("@modelo",
                        (object)ConfigurationManager.AppSettings["AsistenteModelo"] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@error",
                        error == null ? (object)DBNull.Value
                                      : (error.Length > 300 ? error.Substring(0, 300) : error));

                    if (salida == null)
                    {
                        cmd.Parameters.AddWithValue("@respuesta", DBNull.Value);
                        cmd.Parameters.AddWithValue("@herramientas", DBNull.Value);
                        cmd.Parameters.AddWithValue("@tokensEntrada", DBNull.Value);
                        cmd.Parameters.AddWithValue("@tokensSalida", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@respuesta",
                            (object)salida.texto ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@herramientas",
                            salida.herramientas == null || salida.herramientas.Count == 0
                                ? (object)DBNull.Value
                                : string.Join(", ", salida.herramientas.ToArray()));
                        cmd.Parameters.AddWithValue("@tokensEntrada", salida.tokensEntrada);
                        cmd.Parameters.AddWithValue("@tokensSalida", salida.tokensSalida);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Registro de consulta IA: " + ex);
            }
        }
    }
}
