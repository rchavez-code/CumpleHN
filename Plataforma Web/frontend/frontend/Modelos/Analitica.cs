using System;
using System.Collections.Generic;

namespace frontend.Modelos
{
    /* ================================================================
       Modelo del tablero analítico.

       La página no debe decidir qué significa una cifra. Acá viven las
       tres cosas que convierten datos en información: el denominador de
       cada magnitud, la conversión a proporciones comparables y la
       redacción de los hallazgos.

       Todos los textos se arman con las cifras que vienen de la base. No
       hay ninguna frase con un número escrito a mano.
       ================================================================ */

    /// <summary>
    /// Carga semántica de un indicador. Decide el color y el ícono, y por
    /// eso nunca viaja solo: cada elemento que lo usa lleva además su
    /// texto, para que la información no dependa del color.
    /// </summary>
    public enum Tono
    {
        /// <summary>Dato descriptivo, sin juicio.</summary>
        Neutro,

        /// <summary>Cobertura o respaldo suficiente.</summary>
        Favorable,

        /// <summary>Vacío o brecha que conviene mirar.</summary>
        Atencion
    }

    /// <summary>Selección activa del tablero.</summary>
    public class FiltroAnalitica
    {
        public FiltroAnalitica()
        {
            CampanaSlug = string.Empty;
            NivelGobierno = string.Empty;
            Desde = string.Empty;
            Hasta = string.Empty;
        }

        public string CampanaSlug { get; set; }
        public int Categoria { get; set; }
        public int Partido { get; set; }
        public int Departamento { get; set; }
        public string NivelGobierno { get; set; }
        public string Desde { get; set; }
        public string Hasta { get; set; }

        /// <summary>
        /// Verdadero cuando solo está elegida la campaña. Lo usa el botón de
        /// limpiar filtros para saber si tiene algo que limpiar.
        /// </summary>
        public bool EsVistaGeneral
        {
            get
            {
                return Categoria <= 0 && Partido <= 0 && Departamento <= 0
                    && string.IsNullOrEmpty(NivelGobierno)
                    && string.IsNullOrEmpty(Desde) && string.IsNullOrEmpty(Hasta);
            }
        }

        /// <summary>Cantidad de filtros activos además de la campaña.</summary>
        public int Activos
        {
            get
            {
                int n = 0;
                if (Categoria > 0) n++;
                if (Partido > 0) n++;
                if (Departamento > 0) n++;
                if (!string.IsNullOrEmpty(NivelGobierno)) n++;
                if (!string.IsNullOrEmpty(Desde) || !string.IsNullOrEmpty(Hasta)) n++;
                return n;
            }
        }
    }

    /// <summary>Opción de un desplegable de filtro.</summary>
    public class OpcionFiltro
    {
        public string Grupo { get; set; }
        public string Valor { get; set; }
        public string Texto { get; set; }
    }

    // ================================================================
    //  Filas de los indicadores
    // ================================================================

    public class FilaCategoria
    {
        public int Codigo { get; set; }
        public string Categoria { get; set; }

        /// <summary>Porcentaje de interés medido en la encuesta (n = 150).</summary>
        public decimal InteresEncuesta { get; set; }

        public int Propuestas { get; set; }
        public int Candidaturas { get; set; }

        /// <summary>Porcentaje de la oferta total. Lo fija la colección.</summary>
        public decimal PorcentajeOferta { get; set; }

        /// <summary>
        /// Interés menos oferta, en puntos porcentuales. Positivo significa
        /// que se pide más de lo que se propone.
        /// </summary>
        public decimal Brecha
        {
            get { return Math.Round(InteresEncuesta - PorcentajeOferta, 1); }
        }
    }

    public class FilaEstado
    {
        public string Estado { get; set; }
        public string Descripcion { get; set; }
        public decimal Ponderacion { get; set; }
        public int Propuestas { get; set; }
    }

    public class FilaVerificacion
    {
        public string Entidad { get; set; }
        public string Nivel { get; set; }
        public int Total { get; set; }
    }

    public class FilaParticipacion
    {
        public string TipoObjeto { get; set; }
        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }

        public int Valoraciones
        {
            get { return MeGusta + NoMeGusta; }
        }

        /// <summary>Nombre en plural para mostrar.</summary>
        public string Nombre
        {
            get
            {
                switch (TipoObjeto)
                {
                    case "Publicacion": return "Publicaciones";
                    case "Candidato": return "Candidaturas";
                    case "Partido": return "Partidos";
                    case "Propuesta": return "Propuestas";
                    default: return TipoObjeto;
                }
            }
        }
    }

    public class FilaCandidato
    {
        public string Slug { get; set; }
        public string Candidato { get; set; }
        public string PartidoSiglas { get; set; }
        public string Departamento { get; set; }
        public string Cargo { get; set; }
        public string NivelGobierno { get; set; }
        public string Verificacion { get; set; }

        public int Propuestas { get; set; }
        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Saldo { get; set; }
        public int Comentarios { get; set; }

        public int Valoraciones
        {
            get { return MeGusta + NoMeGusta; }
        }

        /// <summary>Proporción a favor. Devuelve -1 cuando no hay valoraciones.</summary>
        public int PorcentajeApoyo
        {
            get
            {
                if (Valoraciones == 0) return -1;
                return (int)Math.Round((MeGusta * 100m) / Valoraciones);
            }
        }
    }

    public class FilaPartido
    {
        public int Codigo { get; set; }
        public string Siglas { get; set; }
        public string Partido { get; set; }
        public int Candidaturas { get; set; }
        public int Propuestas { get; set; }

        /// <summary>Propuestas divididas entre candidaturas.</summary>
        public decimal PropuestasPorCandidatura { get; set; }

        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }

        public int Valoraciones
        {
            get { return MeGusta + NoMeGusta; }
        }
    }

    public class FilaDepartamento
    {
        public int Codigo { get; set; }
        public string Departamento { get; set; }
        public int Candidaturas { get; set; }
        public int Propuestas { get; set; }
        public int Valoraciones { get; set; }

        public bool TieneCobertura
        {
            get { return Candidaturas > 0; }
        }
    }

    public class FilaActividad
    {
        /// <summary>Formato aaaa-MM-dd.</summary>
        public string Fecha { get; set; }

        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }

        public int Valoraciones
        {
            get { return MeGusta + NoMeGusta; }
        }

        /// <summary>Total de interacciones del día.</summary>
        public int Total
        {
            get { return MeGusta + NoMeGusta + Comentarios; }
        }

        public DateTime Dia
        {
            get
            {
                DateTime d;
                if (DateTime.TryParseExact(Fecha, "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out d))
                    return d;

                return DateTime.MinValue;
            }
        }
    }

    public class ResumenAnalitica
    {
        public int Propuestas { get; set; }
        public int Candidaturas { get; set; }
        public int Partidos { get; set; }
        public int Publicaciones { get; set; }

        public int CategoriasConOferta { get; set; }
        public int CategoriasTotal { get; set; }

        public int PropuestasEvaluadas { get; set; }
        public int PropuestasVerificadas { get; set; }
        public int CandidaturasVerificadas { get; set; }

        public int DepartamentosConCandidatura { get; set; }
        public int DepartamentosTotal { get; set; }

        public int Valoraciones { get; set; }
        public int MeGusta { get; set; }
        public int NoMeGusta { get; set; }
        public int Comentarios { get; set; }
        public int PersonasParticipando { get; set; }

        /// <summary>Fecha del registro más reciente, aaaa-MM-dd. Puede venir vacía.</summary>
        public string UltimoRegistro { get; set; }
    }

    // ================================================================
    //  Objetos de lectura
    // ================================================================

    /// <summary>
    /// Tarjeta de indicador.
    ///
    /// Lleva siempre <see cref="Contexto"/> con el denominador. El prompt del
    /// tablero pedía comparar cada cifra contra el período anterior, pero los
    /// registros de participación abarcan tres días: una variación calculada
    /// sobre eso sería una invención con forma de dato. La comparación que sí
    /// sostienen los datos es la de la parte contra su total, y es la que se
    /// muestra.
    /// </summary>
    public class Kpi
    {
        public string Titulo { get; set; }
        public string Valor { get; set; }

        /// <summary>Denominador o desglose. Nunca va vacío.</summary>
        public string Contexto { get; set; }

        /// <summary>Qué concluir del indicador. Puede ir vacío.</summary>
        public string Lectura { get; set; }

        public Tono Tono { get; set; }

        /// <summary>
        /// Proporción que representa el indicador sobre su total, de 0 a 100.
        /// Un valor negativo oculta la barra, para los indicadores que no son
        /// una parte de nada.
        /// </summary>
        public decimal Proporcion { get; set; }

        public string ClaseTono
        {
            get
            {
                switch (Tono)
                {
                    case Tono.Favorable: return "gc-t--ok";
                    case Tono.Atencion: return "gc-t--at";
                    default: return "gc-t--nu";
                }
            }
        }
    }

    /// <summary>Conclusión calculada sobre los datos de la selección.</summary>
    public class Hallazgo
    {
        /// <summary>Dimensión a la que pertenece: Brecha, Verificación, Territorio…</summary>
        public string Etiqueta { get; set; }

        public string Titulo { get; set; }
        public string Texto { get; set; }
        public Tono Tono { get; set; }

        public string ClaseTono
        {
            get
            {
                switch (Tono)
                {
                    case Tono.Favorable: return "gc-h--ok";
                    case Tono.Atencion: return "gc-h--at";
                    default: return "gc-h--nu";
                }
            }
        }
    }

    // ================================================================
    //  Tablero
    // ================================================================

    public class Analitica
    {
        public Analitica()
        {
            Resumen = new ResumenAnalitica();
            Opciones = new List<OpcionFiltro>();
            Categorias = new List<FilaCategoria>();
            Estados = new List<FilaEstado>();
            Verificacion = new List<FilaVerificacion>();
            Participacion = new List<FilaParticipacion>();
            Candidatos = new List<FilaCandidato>();
            Partidos = new List<FilaPartido>();
            Territorio = new List<FilaDepartamento>();
            Actividad = new List<FilaActividad>();

            CampanaSlug = string.Empty;
            CampanaNombre = string.Empty;
            Generado = string.Empty;
        }

        public string CampanaSlug { get; set; }
        public string CampanaNombre { get; set; }

        /// <summary>Momento en que el backend resolvió la consulta.</summary>
        public string Generado { get; set; }

        /// <summary>La selección es válida pero no contiene registros.</summary>
        public bool SinDatos { get; set; }

        /// <summary>El backend no respondió. Distinto de <see cref="SinDatos"/>.</summary>
        public bool Error { get; set; }

        public ResumenAnalitica Resumen { get; set; }
        public IList<OpcionFiltro> Opciones { get; set; }
        public IList<FilaCategoria> Categorias { get; set; }
        public IList<FilaEstado> Estados { get; set; }
        public IList<FilaVerificacion> Verificacion { get; set; }
        public IList<FilaParticipacion> Participacion { get; set; }
        public IList<FilaCandidato> Candidatos { get; set; }
        public IList<FilaPartido> Partidos { get; set; }
        public IList<FilaDepartamento> Territorio { get; set; }
        public IList<FilaActividad> Actividad { get; set; }

        // ------------------------------------------------------ Utilidades

        /// <summary>Porcentaje con una decimal. Total cero devuelve cero.</summary>
        public static decimal Pct(decimal parte, decimal total)
        {
            if (total <= 0) return 0;
            return Math.Round((parte * 100m) / total, 1);
        }

        private static string Pp(decimal valor)
        {
            return valor.ToString("0.#") + " puntos porcentuales";
        }

        /// <summary>
        /// Reparte la oferta en porcentajes sobre el total de la selección.
        ///
        /// Es la razón de existir del método: el interés de la encuesta llega
        /// como porcentaje y las propuestas como conteo. Graficar ambos en un
        /// mismo eje sin convertir inventaría una relación que no está en los
        /// datos, y usar dos ejes distintos es peor.
        /// </summary>
        public void CalcularProporciones()
        {
            int total = 0;
            foreach (FilaCategoria c in Categorias) total += c.Propuestas;

            foreach (FilaCategoria c in Categorias)
                c.PorcentajeOferta = Pct(c.Propuestas, total);
        }

        public IList<OpcionFiltro> OpcionesDe(string grupo)
        {
            List<OpcionFiltro> lista = new List<OpcionFiltro>();
            foreach (OpcionFiltro o in Opciones)
                if (o.Grupo == grupo) lista.Add(o);

            return lista;
        }

        /// <summary>Total de valoraciones dentro de un nivel de verificación.</summary>
        public int VerificacionDe(string entidad, string nivel)
        {
            foreach (FilaVerificacion v in Verificacion)
                if (v.Entidad == entidad && v.Nivel == nivel) return v.Total;

            return 0;
        }

        public int TotalEntidad(string entidad)
        {
            int n = 0;
            foreach (FilaVerificacion v in Verificacion)
                if (v.Entidad == entidad) n += v.Total;

            return n;
        }

        /// <summary>Categoría donde más se pide y menos se propone.</summary>
        public FilaCategoria MayorBrecha
        {
            get
            {
                FilaCategoria peor = null;
                foreach (FilaCategoria c in Categorias)
                    if (peor == null || c.Brecha > peor.Brecha) peor = c;

                return peor;
            }
        }

        /// <summary>Categoría con más propuestas en relación con el interés.</summary>
        public FilaCategoria MayorExcedente
        {
            get
            {
                FilaCategoria mejor = null;
                foreach (FilaCategoria c in Categorias)
                    if (mejor == null || c.Brecha < mejor.Brecha) mejor = c;

                return mejor;
            }
        }

        /// <summary>Partido con más propuestas por candidatura, con al menos una.</summary>
        public FilaPartido MasDenso
        {
            get
            {
                FilaPartido mejor = null;
                foreach (FilaPartido p in Partidos)
                {
                    if (p.Candidaturas == 0) continue;
                    if (mejor == null || p.PropuestasPorCandidatura > mejor.PropuestasPorCandidatura)
                        mejor = p;
                }

                return mejor;
            }
        }

        /// <summary>Tipo de contenido que concentra más valoraciones.</summary>
        public FilaParticipacion TipoMasValorado
        {
            get
            {
                FilaParticipacion mejor = null;
                foreach (FilaParticipacion p in Participacion)
                    if (mejor == null || p.Valoraciones > mejor.Valoraciones) mejor = p;

                return mejor != null && mejor.Valoraciones > 0 ? mejor : null;
            }
        }

        /// <summary>Departamento con más candidaturas.</summary>
        public FilaDepartamento TerritorioLider
        {
            get
            {
                FilaDepartamento mejor = null;
                foreach (FilaDepartamento d in Territorio)
                    if (mejor == null || d.Candidaturas > mejor.Candidaturas) mejor = d;

                return mejor != null && mejor.Candidaturas > 0 ? mejor : null;
            }
        }

        /// <summary>Día con más interacciones registradas.</summary>
        public FilaActividad DiaMasActivo
        {
            get
            {
                FilaActividad mejor = null;
                foreach (FilaActividad a in Actividad)
                    if (mejor == null || a.Total > mejor.Total) mejor = a;

                return mejor != null && mejor.Total > 0 ? mejor : null;
            }
        }

        public decimal ApoyoGeneral
        {
            get { return Pct(Resumen.MeGusta, Resumen.Valoraciones); }
        }

        // ================================================================
        //  Indicadores de encabezado
        // ================================================================

        /// <summary>
        /// Las seis cifras que responden «¿cuál es la situación actual?».
        /// Se construyen acá y no en el marcado para que la interpretación
        /// quede junto al cálculo que la sostiene.
        /// </summary>
        public IList<Kpi> Kpis
        {
            get
            {
                List<Kpi> lista = new List<Kpi>();
                ResumenAnalitica r = Resumen;

                // --- 1. Volumen de la oferta programática
                decimal porCandidatura = r.Candidaturas == 0
                    ? 0 : Math.Round((decimal)r.Propuestas / r.Candidaturas, 1);

                lista.Add(new Kpi
                {
                    Titulo = "Propuestas documentadas",
                    Valor = Vista.Numero(r.Propuestas),
                    Contexto = r.Candidaturas == 0
                        ? "sin candidaturas en la selección"
                        : "de " + Vista.Plural(r.Candidaturas, "candidatura", "candidaturas")
                          + " · " + porCandidatura.ToString("0.#") + " por candidatura",
                    Tono = Tono.Neutro,
                    Proporcion = -1
                });

                // --- 2. Cobertura temática
                decimal coberturaTema = Pct(r.CategoriasConOferta, r.CategoriasTotal);

                lista.Add(new Kpi
                {
                    Titulo = "Cobertura temática",
                    Valor = r.CategoriasConOferta + " de " + r.CategoriasTotal,
                    Contexto = "categorías con al menos una propuesta",
                    Lectura = r.CategoriasConOferta == r.CategoriasTotal
                        ? "Todas las áreas temáticas tienen oferta."
                        : (r.CategoriasTotal - r.CategoriasConOferta)
                          + " sin ninguna propuesta registrada.",
                    Tono = coberturaTema >= 100 ? Tono.Favorable
                         : coberturaTema >= 60 ? Tono.Neutro : Tono.Atencion,
                    Proporcion = coberturaTema
                });

                // --- 3. Respaldo documental de las propuestas
                decimal verifPct = Pct(r.PropuestasVerificadas, r.Propuestas);

                lista.Add(new Kpi
                {
                    Titulo = "Propuestas con fuente verificable",
                    Valor = r.PropuestasVerificadas + " de " + r.Propuestas,
                    Contexto = verifPct.ToString("0.#") + " % del total documentado",
                    Lectura = r.Propuestas == 0
                        ? string.Empty
                        : r.PropuestasVerificadas == 0
                          ? "Todas se muestran como declaradas por la propia candidatura."
                          : "El resto se muestra como declaración de la candidatura.",
                    Tono = r.Propuestas == 0 ? Tono.Neutro
                         : verifPct >= 50 ? Tono.Favorable
                         : verifPct > 0 ? Tono.Neutro : Tono.Atencion,
                    Proporcion = r.Propuestas == 0 ? -1 : verifPct
                });

                // --- 4. Volumen de participación
                lista.Add(new Kpi
                {
                    Titulo = "Participación ciudadana",
                    Valor = Vista.Numero(r.Valoraciones),
                    Contexto = "valoraciones de "
                        + Vista.Plural(r.PersonasParticipando, "persona", "personas")
                        + " · " + Vista.Plural(r.Comentarios, "comentario", "comentarios"),
                    Tono = Tono.Neutro,
                    Proporcion = -1
                });

                // --- 5. Signo de la participación
                decimal apoyo = ApoyoGeneral;

                lista.Add(new Kpi
                {
                    Titulo = "Valoraciones a favor",
                    Valor = r.Valoraciones == 0 ? "—" : apoyo.ToString("0.#") + " %",
                    Contexto = r.Valoraciones == 0
                        ? "todavía sin valoraciones"
                        : Vista.Numero(r.MeGusta) + " a favor · " + Vista.Numero(r.NoMeGusta) + " en contra",
                    Lectura = "Mide reacción ciudadana, no cumplimiento.",
                    Tono = Tono.Neutro,
                    Proporcion = r.Valoraciones == 0 ? -1 : apoyo
                });

                // --- 6. Alcance territorial
                decimal territorio = Pct(r.DepartamentosConCandidatura, r.DepartamentosTotal);

                lista.Add(new Kpi
                {
                    Titulo = "Cobertura territorial",
                    Valor = r.DepartamentosConCandidatura + " de " + r.DepartamentosTotal,
                    Contexto = "departamentos con alguna candidatura",
                    Lectura = r.DepartamentosConCandidatura == 0
                        ? string.Empty
                        : (r.DepartamentosTotal - r.DepartamentosConCandidatura)
                          + " departamentos sin representación en la plataforma.",
                    Tono = territorio >= 75 ? Tono.Favorable
                         : territorio >= 40 ? Tono.Neutro : Tono.Atencion,
                    Proporcion = territorio
                });

                return lista;
            }
        }

        // ================================================================
        //  Hallazgos
        // ================================================================

        /// <summary>
        /// Las conclusiones que el tablero puede sostener con los datos de la
        /// selección, ordenadas de más a menos relevante.
        ///
        /// Todas se calculan. Cuando una comparación no tiene sustento —una
        /// categoría sin propuestas, una selección sin valoraciones— el
        /// hallazgo correspondiente no se agrega, en lugar de mostrarse con
        /// una cifra vacía.
        /// </summary>
        public IList<Hallazgo> Hallazgos
        {
            get
            {
                List<Hallazgo> lista = new List<Hallazgo>();
                ResumenAnalitica r = Resumen;

                if (SinDatos) return lista;

                // --- Brecha entre lo que se pide y lo que se propone
                FilaCategoria brecha = MayorBrecha;
                if (brecha != null && brecha.Brecha > 0 && r.Propuestas > 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Brecha temática",
                        Titulo = "La mayor distancia está en " + brecha.Categoria.ToLowerInvariant(),
                        Texto = "Concentra " + brecha.InteresEncuesta.ToString("0.#")
                              + " % del interés ciudadano medido en la encuesta y solo "
                              + brecha.PorcentajeOferta.ToString("0.#")
                              + " % de las propuestas registradas, una diferencia de "
                              + Pp(brecha.Brecha) + ".",
                        Tono = brecha.Brecha >= 10 ? Tono.Atencion : Tono.Neutro
                    });
                }

                // --- La cara opuesta: lo que se propone por encima de lo que se pide
                FilaCategoria excedente = MayorExcedente;
                if (excedente != null && excedente.Brecha < 0 && excedente.Propuestas > 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Sobreoferta",
                        Titulo = excedente.Categoria + " concentra más oferta que demanda",
                        Texto = "Reúne " + excedente.PorcentajeOferta.ToString("0.#")
                              + " % de las propuestas frente a " + excedente.InteresEncuesta.ToString("0.#")
                              + " % de interés ciudadano, " + Pp(Math.Abs(excedente.Brecha))
                              + " por encima.",
                        Tono = Tono.Neutro
                    });
                }

                // --- Respaldo documental
                if (r.Propuestas > 0 && r.PropuestasVerificadas == 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Verificación",
                        Titulo = "Ninguna propuesta tiene todavía fuente verificable",
                        Texto = "Las " + r.Propuestas + " propuestas de la selección se muestran como "
                              + "declaradas por la candidatura que las registró. La plataforma no "
                              + "respalda su contenido mientras no exista una fuente documentada, y "
                              + "así lo indica en cada ficha.",
                        Tono = Tono.Atencion
                    });
                }
                else if (r.Propuestas > 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Verificación",
                        Titulo = r.PropuestasVerificadas + " de " + r.Propuestas
                               + " propuestas tienen fuente verificable",
                        Texto = "Equivale al " + Pct(r.PropuestasVerificadas, r.Propuestas).ToString("0.#")
                              + " % de la oferta documentada. El resto se presenta como afirmación "
                              + "de la candidatura y no como hecho comprobado.",
                        Tono = Tono.Neutro
                    });
                }

                // --- Estado del seguimiento de cumplimiento
                if (r.Propuestas > 0 && r.PropuestasEvaluadas == 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Cumplimiento",
                        Titulo = "El seguimiento de cumplimiento aún no comienza",
                        Texto = "Las " + r.Propuestas + " propuestas están en estado declarada, que es "
                              + "el estado en que nacen. Los estados de cumplimiento los asigna la "
                              + "plataforma con evidencia, nunca la propia candidatura, así que "
                              + "todavía no hay porcentaje de cumplimiento que mostrar.",
                        Tono = Tono.Neutro
                    });
                }

                // --- Concentración territorial
                FilaDepartamento lider = TerritorioLider;
                if (lider != null && r.Candidaturas > 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Territorio",
                        Titulo = "La oferta se concentra en " + lider.Departamento,
                        Texto = lider.Departamento + " reúne "
                              + Vista.Plural(lider.Candidaturas, "candidatura", "candidaturas")
                              + " y " + Vista.Plural(lider.Propuestas, "propuesta", "propuestas") + ". "
                              + "En total la plataforma cubre " + r.DepartamentosConCandidatura
                              + " de los " + r.DepartamentosTotal + " departamentos del país.",
                        Tono = r.DepartamentosConCandidatura * 2 < r.DepartamentosTotal
                             ? Tono.Atencion : Tono.Neutro
                    });
                }

                // --- Densidad programática por partido
                FilaPartido denso = MasDenso;
                if (denso != null && Partidos.Count > 1 && denso.Propuestas > 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Partidos",
                        Titulo = denso.Siglas + " tiene la mayor densidad programática",
                        Texto = "Registra " + denso.PropuestasPorCandidatura.ToString("0.#")
                              + " propuestas por candidatura. El conteo total por partido depende de "
                              + "cuántas candidaturas inscribió cada uno, así que el cociente es la "
                              + "cifra comparable entre partidos de distinto tamaño.",
                        Tono = Tono.Neutro
                    });
                }

                // --- Dónde se concentra la conversación
                FilaParticipacion tipo = TipoMasValorado;
                if (tipo != null && r.Valoraciones > 0)
                {
                    lista.Add(new Hallazgo
                    {
                        Etiqueta = "Participación",
                        Titulo = tipo.Nombre + " concentran la participación",
                        Texto = "Reúnen " + Vista.Numero(tipo.Valoraciones) + " de las "
                              + Vista.Numero(r.Valoraciones) + " valoraciones de la selección, el "
                              + Pct(tipo.Valoraciones, r.Valoraciones).ToString("0.#")
                              + " %, con " + Pct(tipo.MeGusta, tipo.Valoraciones).ToString("0.#")
                              + " % a favor.",
                        Tono = Tono.Neutro
                    });
                }

                return lista;
            }
        }
    }
}
