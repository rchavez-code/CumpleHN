namespace frontend.Modelos
{
    /// <summary>
    /// Nivel de gobierno del cargo al que aspira el candidato.
    /// </summary>
    public enum NivelGobierno
    {
        Nacional,
        Departamental,
        Municipal
    }

    /// <summary>
    /// Situación de una campaña electoral en la línea de tiempo.
    /// </summary>
    public enum EstadoCampana
    {
        Proxima,
        Activa,
        Cerrada
    }

    /// <summary>
    /// Estado de una propuesta. Los primeros valores corresponden al momento en
    /// que el candidato la declara. Los siguientes replican el esquema de
    /// seguimiento del rastreador de promesas de PolitiFact (2018), que es el
    /// que adopta el marco teórico del proyecto, y solo aplican una vez que la
    /// plataforma evalúa el cumplimiento con evidencia.
    /// </summary>
    public enum EstadoPropuesta
    {
        Declarada,
        SinAvance,
        EnProceso,
        Estancada,
        CumplidaAMedias,
        Cumplida,
        Incumplida
    }

    /// <summary>
    /// Distingue lo que el candidato afirma de lo que la plataforma respalda.
    /// Es la garantía de neutralidad: el contenido autoral nunca se presenta
    /// como verificado hasta que exista una fuente que lo sostenga.
    /// </summary>
    public enum NivelVerificacion
    {
        Declarado,
        EnRevision,
        Verificado
    }
}
