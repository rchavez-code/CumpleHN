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
                string filtro = string.IsNullOrEmpty(campanaSlug) ? "" : "WHERE ca.slug = @campana ";

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
            "LEFT JOIN dbo.Propuestas p ON p.codigoPropuesta = b.codigoPropuesta ";

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<Publicacion> listarFeed(string campanaSlug)
        {
            List<Publicacion> lista = new List<Publicacion>();

            using (SqlConnection conn = new SqlConnection(cadenaConexion))
            {
                string filtro = string.IsNullOrEmpty(campanaSlug) ? "" : "WHERE ca.slug = @campana ";

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
                    SelectPublicacion + "WHERE k.slug = @slug ORDER BY b.fecha DESC", conn);
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
                || tipoObjeto == "Propuesta";
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
                default:
                    return false;
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@codigo", codigoObjeto);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>Confirma que la cuenta existe y está activa.</summary>
        private static bool UsuarioActivo(SqlConnection conn, int codigoUsuario)
        {
            if (codigoUsuario <= 0) return false;

            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Usuarios WHERE codigoUsuario = @u AND activo = 1", conn);
            cmd.Parameters.AddWithValue("@u", codigoUsuario);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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

                    if (!UsuarioActivo(conn, codigoUsuario))
                    {
                        r.mensaje = "Necesitás una cuenta activa para participar.";
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

                    if (!UsuarioActivo(conn, codigoUsuario))
                    {
                        r.mensaje = "Necesitás una cuenta activa para comentar.";
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
        private static string Limpio(string texto)
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

        private static OpcionFiltro[] LeerOpciones(SqlConnection conn)
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

        private static ResumenAnalitica LeerResumen(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaCategoria[] LeerCategorias(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaEstado[] LeerEstados(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaVerificacion[] LeerVerificacion(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaParticipacion[] LeerParticipacion(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaCandidato[] LeerCandidatos(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaPartido[] LeerPartidos(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaDepartamento[] LeerTerritorio(SqlConnection conn, FiltroAnalitica f)
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

        private static FilaActividad[] LeerActividad(SqlConnection conn, FiltroAnalitica f)
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

        /// <summary>
        /// Lee una columna de texto tratando NULL como cadena vacía, para que
        /// el frontend nunca reciba nulos que tenga que verificar.
        /// </summary>
        private static string Texto(IDataRecord reader, string columna)
        {
            object valor = reader[columna];
            return valor == DBNull.Value ? string.Empty : Convert.ToString(valor);
        }
    }
}
