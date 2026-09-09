using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using backend.Modelos;

namespace backend.Servicios
{
    /// <summary>
    /// Único punto por el que el asistente llega a la base de datos.
    ///
    /// Todo lo que el modelo puede consultar pasa por acá, y acá se abre
    /// siempre la conexión del login cumplehn_ia, que solo tiene EXECUTE
    /// sobre los procedimientos del script 12 y los diez del tablero. No
    /// puede escribir, no puede leer Usuarios, y no puede leer quién
    /// valoró a quién.
    ///
    /// La razón de que sea una clase y no unos métodos sueltos en el Web
    /// Service es la misma por la que el control de acceso del frontend
    /// vive en PaginaSegura y no repetido en cada página: si la conexión
    /// restringida se abriera en cada método que la necesita, bastaría
    /// con que uno nuevo se olvidara para perder la garantía, y el
    /// síntoma sería que todo funciona. Acá el descuido no tiene dónde
    /// ocurrir, porque ningún método de esta clase abre otra cosa.
    ///
    /// Corolario que conviene no romper: si alguna vez hace falta que el
    /// asistente lea algo nuevo, se le agrega un procedimiento y su
    /// GRANT, nunca una consulta suelta con la conexión de siempre.
    /// </summary>
    internal static class AsistenteDatos
    {
        /// <summary>
        /// Cadena del login restringido. Vive en secrets.config, fuera de
        /// git, así que puede faltar en una máquina recién clonada. El
        /// mensaje dice exactamente qué crear, porque el síntoma sin él
        /// sería una referencia nula sin ninguna pista.
        /// </summary>
        private static string Cadena
        {
            get
            {
                string c = ConfigurationManager.AppSettings["CnxCumpleHN_IA"];

                if (string.IsNullOrEmpty(c))
                    throw new ConfigurationErrorsException(
                        "Falta la clave CnxCumpleHN_IA. Se define en secrets.config, " +
                        "junto al Web.config del backend. Hay una plantilla con el " +
                        "formato en secrets.config.ejemplo, y el login lo crea el " +
                        "script 13_permisos_ia.sql.");

                return c;
            }
        }

        /// <summary>
        /// La única apertura de conexión de toda la clase. Si aparece una
        /// segunda en algún método, la garantía de aislamiento se perdió.
        /// </summary>
        private static SqlConnection Abrir()
        {
            SqlConnection conn = new SqlConnection(Cadena);
            conn.Open();
            return conn;
        }

        // =============================================================
        //  Herramientas que se le ofrecen al modelo
        // =============================================================

        /// <summary>
        /// Valores válidos de los filtros. El modelo la llama primero
        /// cuando la pregunta nombra una categoría o un partido, para no
        /// inventarse un código que no existe.
        /// </summary>
        public static List<CatalogoIA> Catalogos()
        {
            List<CatalogoIA> lista = new List<CatalogoIA>();

            using (SqlConnection conn = Abrir())
            {
                SqlCommand cmd = new SqlCommand("dbo.spAnaliticaCatalogos", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        CatalogoIA c = new CatalogoIA();
                        c.grupo = Texto(r, "grupo");
                        c.valor = Texto(r, "valor");
                        c.texto = Texto(r, "texto");
                        lista.Add(c);
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// El tablero completo para una selección de filtros. Reusa los
        /// mismos lectores que obtenerAnalitica, así que las cifras que
        /// cita el asistente son las que muestra la página — si se
        /// calcularan aparte podrían discrepar, y ahí el asistente
        /// dejaría de ser confiable aunque las dos cuentas fueran
        /// correctas por separado.
        /// </summary>
        public static Analitica Tablero(FiltroAnalitica filtro)
        {
            if (filtro == null) filtro = new FiltroAnalitica();

            Analitica a = new Analitica();
            a.generado = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            using (SqlConnection conn = Abrir())
            {
                // Slug y nombre en una sola ida, por procedimiento. El login
                // del asistente no puede leer Campanas de forma directa.
                string[] campana = Campana(conn, filtro.campanaSlug);

                filtro.campanaSlug = campana[0];
                a.campanaSlug      = campana[0];
                a.campanaNombre    = campana[1];

                a.opciones      = WebServiceGlobal.LeerOpciones(conn);
                a.resumen       = WebServiceGlobal.LeerResumen(conn, filtro);
                a.categorias    = WebServiceGlobal.LeerCategorias(conn, filtro);
                a.estados       = WebServiceGlobal.LeerEstados(conn, filtro);
                a.verificacion  = WebServiceGlobal.LeerVerificacion(conn, filtro);
                a.participacion = WebServiceGlobal.LeerParticipacion(conn, filtro);
                a.candidatos    = WebServiceGlobal.LeerCandidatos(conn, filtro);
                a.partidos      = WebServiceGlobal.LeerPartidos(conn, filtro);
                a.territorio    = WebServiceGlobal.LeerTerritorio(conn, filtro);
                a.actividad     = WebServiceGlobal.LeerActividad(conn, filtro);

                a.sinDatos = a.resumen.candidaturas == 0
                          && a.resumen.propuestas == 0
                          && a.resumen.valoraciones == 0;
            }

            return a;
        }

        /// <summary>
        /// Busca propuestas, o devuelve una sola con su texto completo si
        /// se indica codigoPropuesta. El tope de filas lo impone el
        /// procedimiento y no este método, para que valga también si
        /// alguien lo llama desde otro lado.
        /// </summary>
        public static List<PropuestaIA> BuscarPropuestas(
            int codigoPropuesta, string texto, string campanaSlug,
            int codigoCategoria, string estado, string candidatoSlug,
            int codigoPartido, int limite)
        {
            List<PropuestaIA> lista = new List<PropuestaIA>();

            using (SqlConnection conn = Abrir())
            {
                SqlCommand cmd = new SqlCommand("dbo.spIABuscarPropuestas", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                Opcional(cmd, "@codigoPropuesta", codigoPropuesta);
                Opcional(cmd, "@texto",           texto);
                Opcional(cmd, "@campanaSlug",     campanaSlug);
                Opcional(cmd, "@codigoCategoria", codigoCategoria);
                Opcional(cmd, "@estado",          estado);
                Opcional(cmd, "@candidatoSlug",   candidatoSlug);
                Opcional(cmd, "@codigoPartido",   codigoPartido);
                cmd.Parameters.AddWithValue("@limite", limite <= 0 ? 20 : limite);

                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(LeerPropuesta(r));
            }

            return lista;
        }

        /// <summary>
        /// Ficha pública de una candidatura con sus propuestas. El
        /// procedimiento devuelve dos resultados y se leen los dos en la
        /// misma ida, porque casi toda pregunta sobre una candidatura
        /// termina siendo una pregunta sobre lo que propuso.
        /// </summary>
        public static FichaIA FichaCandidato(string candidatoSlug)
        {
            FichaIA ficha = null;

            using (SqlConnection conn = Abrir())
            {
                SqlCommand cmd = new SqlCommand("dbo.spIAFichaCandidato", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@candidatoSlug",
                    (object)WebServiceGlobal.Limpio(candidatoSlug) ?? DBNull.Value);

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        ficha = new FichaIA();
                        ficha.candidatoSlug          = Texto(r, "candidatoSlug");
                        ficha.candidato              = Texto(r, "candidato");
                        ficha.campanaSlug            = Texto(r, "campanaSlug");
                        ficha.partido                = Texto(r, "partido");
                        ficha.partidoSiglas          = Texto(r, "partidoSiglas");
                        ficha.cargo                  = Texto(r, "cargo");
                        ficha.nivelGobierno          = Texto(r, "nivelGobierno");
                        ficha.departamento           = Texto(r, "departamento");
                        ficha.municipio              = Texto(r, "municipio");
                        ficha.verificacion           = Texto(r, "verificacion");
                        ficha.titular                = Texto(r, "titular");
                        ficha.biografia              = Texto(r, "biografia");
                        ficha.informacionProfesional = Texto(r, "informacionProfesional");
                        ficha.descripcionCandidatura = Texto(r, "descripcionCandidatura");
                    }

                    // El segundo resultado se lee siempre, exista o no la
                    // ficha, porque dejar el lector a medias mantiene la
                    // conexión ocupada hasta que el using la cierre.
                    List<PropuestaIA> propuestas = new List<PropuestaIA>();

                    if (r.NextResult())
                        while (r.Read()) propuestas.Add(LeerPropuesta(r));

                    if (ficha != null) ficha.propuestas = propuestas.ToArray();
                }
            }

            return ficha;
        }

        // =============================================================
        //  Auxiliares
        // =============================================================

        /// <summary>
        /// Slug y nombre de la campaña, en ese orden. Sin slug devuelve la
        /// destacada.
        ///
        /// Va por procedimiento y no con un SELECT acá porque el login del
        /// asistente no tiene permiso de lectura sobre ninguna tabla. La
        /// primera versión lo hacía con dos consultas sueltas y falló en la
        /// primera consulta real — el permiso atrapó lo que la regla del
        /// encabezado de esta clase ya prohibía.
        /// </summary>
        private static string[] Campana(SqlConnection conn, string slug)
        {
            SqlCommand cmd = new SqlCommand("dbo.spIACampana", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            string s = WebServiceGlobal.Limpio(slug);
            cmd.Parameters.AddWithValue("@campanaSlug",
                s.Length == 0 ? (object)DBNull.Value : s);

            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                    return new string[] { Texto(r, "slug"), Texto(r, "nombre") };

            // Slug inexistente. Se devuelve vacío y los indicadores salen en
            // cero, que la página ya trata como selección sin datos.
            return new string[] { string.Empty, string.Empty };
        }

        /// <summary>
        /// Una fila de propuesta. La usan la búsqueda y la ficha, que
        /// devuelven columnas distintas — de ahí que cada lectura
        /// compruebe si la columna vino en este resultado.
        /// </summary>
        private static PropuestaIA LeerPropuesta(IDataRecord r)
        {
            PropuestaIA p = new PropuestaIA();
            p.codigoPropuesta   = Entero(r, "codigoPropuesta");
            p.propuesta         = Texto(r, "propuesta");
            p.campanaSlug       = Texto(r, "campanaSlug");
            p.categoria         = Texto(r, "categoria");
            p.estado            = Texto(r, "estado");
            p.verificacion      = Texto(r, "verificacion");
            p.candidato         = Texto(r, "candidato");
            p.candidatoSlug     = Texto(r, "candidatoSlug");
            p.partido           = Texto(r, "partido");
            p.partidoSiglas     = Texto(r, "partidoSiglas");
            p.departamento      = Texto(r, "departamento");
            p.nivelGobierno     = Texto(r, "nivelGobierno");
            p.fecha             = Fecha(r, "fecha");
            p.ubicacion         = Texto(r, "ubicacion");
            p.periodoEjecucion  = Texto(r, "periodoEjecucion");
            p.beneficiarios     = Texto(r, "beneficiarios");
            p.descripcion       = Texto(r, "descripcion");
            p.problema          = Texto(r, "problema");
            p.objetivo          = Texto(r, "objetivo");
            return p;
        }

        /// <summary>
        /// Agrega el parámetro solo si trae valor. Ausente equivale a NULL,
        /// que es «sin filtrar» en todos los procedimientos del proyecto.
        /// </summary>
        private static void Opcional(SqlCommand cmd, string nombre, string valor)
        {
            string v = WebServiceGlobal.Limpio(valor);
            cmd.Parameters.AddWithValue(nombre,
                string.IsNullOrEmpty(v) ? (object)DBNull.Value : v);
        }

        private static void Opcional(SqlCommand cmd, string nombre, int valor)
        {
            cmd.Parameters.AddWithValue(nombre,
                valor <= 0 ? (object)DBNull.Value : valor);
        }

        /// <summary>Índice de la columna, o -1 si no vino en este resultado.</summary>
        private static int Indice(IDataRecord r, string columna)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), columna, StringComparison.OrdinalIgnoreCase))
                    return i;

            return -1;
        }

        private static string Texto(IDataRecord r, string columna)
        {
            int i = Indice(r, columna);
            return i < 0 || r.IsDBNull(i) ? string.Empty : Convert.ToString(r.GetValue(i));
        }

        private static int Entero(IDataRecord r, string columna)
        {
            int i = Indice(r, columna);
            return i < 0 || r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i));
        }

        private static string Fecha(IDataRecord r, string columna)
        {
            int i = Indice(r, columna);
            if (i < 0 || r.IsDBNull(i)) return string.Empty;
            return Convert.ToDateTime(r.GetValue(i)).ToString("yyyy-MM-dd");
        }
    }
}
