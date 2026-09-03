<%@ Page Title="Analítica" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Analitica.aspx.cs" Inherits="frontend.AnaliticaPagina" %>

<asp:Content ID="Cabeza" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= Recurso("~/Content/cumplehn-analitica.css") %>" rel="stylesheet" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- Velo de carga. La respuesta al filtro es un postback, y una espera
         sin señal visible parece que el clic no hizo nada. --%>
    <div class="gc-cargando" id="gcCargando" aria-hidden="true">
        <div class="gc-cargando__caja">
            <span class="gc-spin"></span>
            <span>Consultando los datos…</span>
        </div>
    </div>

    <%-- ================================================ Encabezado --%>

    <section class="gc-tablero">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <span>Analítica</span>
            </nav>

            <h1>Tablero analítico</h1>
            <p class="gc-tablero__lead">
                Convierte los registros de la plataforma en indicadores comparables, para que cada
                persona pondere la evidencia según el criterio que considere de mayor peso. No emite
                un veredicto sobre quién cumple mejor: organiza lo que hay para que el juicio sea
                de quien lee.
            </p>

            <div class="gc-tablero__meta">
                <span class="gc-vivo">
                    <span class="gc-vivo__p" aria-hidden="true"></span>
                    Datos consultados en vivo contra la base
                </span>
                <span><strong><%: UltimaActualizacion %></strong></span>
                <span>Campaña: <strong><%: CampanaNombre %></strong></span>
                <span class="gc-muted">Consulta ejecutada <%: Consultado %></span>

                <asp:Button ID="btnActualizar" runat="server" CssClass="gc-btn gc-btn--sm gc-btn--claro"
                    Text="Actualizar datos" OnClick="btnActualizar_Click" />
            </div>
        </div>
    </section>

    <div class="container gc-cuerpo" style="padding-top: 22px;">

        <%-- =================================================== Filtros --%>

        <div class="gc-filtros">
            <div class="gc-filtros__rejilla">

                <div class="gc-filtros__c">
                    <label for="<%= ddlCampana.ClientID %>">Campaña</label>
                    <asp:DropDownList ID="ddlCampana" runat="server" CssClass="gc-select"
                        AutoPostBack="true" OnSelectedIndexChanged="Filtro_Cambiado" />
                </div>

                <div class="gc-filtros__c">
                    <label for="<%= ddlCategoria.ClientID %>">Categoría</label>
                    <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="gc-select"
                        AutoPostBack="true" OnSelectedIndexChanged="Filtro_Cambiado" />
                </div>

                <div class="gc-filtros__c">
                    <label for="<%= ddlPartido.ClientID %>">Partido</label>
                    <asp:DropDownList ID="ddlPartido" runat="server" CssClass="gc-select"
                        AutoPostBack="true" OnSelectedIndexChanged="Filtro_Cambiado" />
                </div>

                <div class="gc-filtros__c">
                    <label for="<%= ddlDepartamento.ClientID %>">Departamento</label>
                    <asp:DropDownList ID="ddlDepartamento" runat="server" CssClass="gc-select"
                        AutoPostBack="true" OnSelectedIndexChanged="Filtro_Cambiado" />
                </div>

                <div class="gc-filtros__c">
                    <label for="<%= ddlNivel.ClientID %>">Nivel de gobierno</label>
                    <asp:DropDownList ID="ddlNivel" runat="server" CssClass="gc-select"
                        AutoPostBack="true" OnSelectedIndexChanged="Filtro_Cambiado" />
                </div>

                <div class="gc-filtros__c">
                    <label for="<%= txtDesde.ClientID %>">Desde</label>
                    <asp:TextBox ID="txtDesde" runat="server" CssClass="gc-input" TextMode="Date"
                        AutoPostBack="true" OnTextChanged="Filtro_Cambiado" />
                </div>

                <div class="gc-filtros__c">
                    <label for="<%= txtHasta.ClientID %>">Hasta</label>
                    <asp:TextBox ID="txtHasta" runat="server" CssClass="gc-input" TextMode="Date"
                        AutoPostBack="true" OnTextChanged="Filtro_Cambiado" />
                </div>

            </div>

            <div class="gc-filtros__pie">
                <asp:PlaceHolder runat="server">
                    <asp:Repeater ID="rptFichas" runat="server">
                        <HeaderTemplate><span class="gc-muted gc-small">Filtrando por:</span></HeaderTemplate>
                        <ItemTemplate>
                            <span class="gc-ficha"><b><%#: Container.DataItem %></b></span>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:PlaceHolder>

                <asp:LinkButton ID="btnLimpiar" runat="server" CssClass="gc-btn gc-btn--quiet gc-btn--sm gc-limpiar"
                    Text="Limpiar filtros" OnClick="btnLimpiar_Click" CausesValidation="false" />
            </div>
        </div>

        <%-- ============================================ Estado: error --%>

        <asp:PlaceHolder ID="phError" runat="server" Visible="false">
            <div class="gc-estado gc-estado--error">
                <div class="gc-estado__ico" aria-hidden="true">!</div>
                <h3>No fue posible obtener la información</h3>
                <p>
                    El servicio de datos no respondió. Volvé a intentarlo en un momento. Si el
                    problema persiste, el tablero se restablece solo cuando el servicio vuelve.
                </p>
                <asp:Button runat="server" CssClass="gc-btn gc-btn--sm" Text="Reintentar"
                    OnClick="btnActualizar_Click" />
            </div>
        </asp:PlaceHolder>

        <%-- ======================================== Estado: sin datos --%>

        <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
            <div class="gc-estado gc-estado--vacio">
                <div class="gc-estado__ico" aria-hidden="true">∅</div>
                <h3>No existen datos disponibles para los filtros seleccionados</h3>
                <p>
                    La selección es válida pero no contiene registros. Puede ser una campaña que
                    todavía no tiene candidaturas inscritas, o un cruce de filtros demasiado
                    estrecho. Quitá alguno para ampliar la vista.
                </p>
                <asp:LinkButton runat="server" CssClass="gc-btn gc-btn--sm" Text="Limpiar filtros"
                    OnClick="btnLimpiar_Click" CausesValidation="false" />
            </div>
        </asp:PlaceHolder>

        <%-- =================================================== Tablero --%>

        <%-- Aviso para la administración: lo que ve acá no es lo que ve el
             público. Sin decirlo, quien administra creería que el sitio se ve
             como lo está viendo él. --%>
        <asp:PlaceHolder ID="phAvisoOculto" runat="server" Visible="false">
            <div class="gc-note gc-note--ambar gc-mb">
                <span>
                    <strong>Esta sección está oculta al público.</strong>
                    La estás viendo porque administrás la plataforma. Quien consulte el sitio
                    no la encuentra ni por el menú ni escribiendo la dirección.
                </span>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phTablero" runat="server">

            <%-- --------------------------------------------- Indicadores --%>

            <div id="grafKpi" runat="server" class="gc-kpis">
                <asp:Repeater ID="rptKpis" runat="server">
                    <ItemTemplate>
                        <div class="gc-kpi <%# Eval("ClaseTono") %>">
                            <div class="gc-kpi__t"><%#: Eval("Titulo") %></div>
                            <div class="gc-kpi__n"><%#: Eval("Valor") %></div>
                            <div class="gc-kpi__c"><%#: Eval("Contexto") %></div>

                            <asp:PlaceHolder runat="server"
                                Visible='<%# (decimal)Eval("Proporcion") >= 0 %>'>
                                <div class="gc-kpi__m" role="img"
                                     aria-label='<%#: Eval("Proporcion", "{0:0.#}") %> por ciento'>
                                    <span style='width:<%# Eval("Proporcion", "{0:0.##}").Replace(",", ".") %>%'></span>
                                </div>
                            </asp:PlaceHolder>

                            <div class="gc-kpi__l"><%#: Eval("Lectura") %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <%-- -------------------------------------------------- Pestañas --%>

            <div class="gc-pest" role="tablist" aria-label="Vistas del tablero">
                <a class="gc-pest__b <%= ClaseTab("hallazgos") %>" href="#" role="tab" data-panel="hallazgos"
                   aria-selected="<%= SeleccionTab("hallazgos") %>">Hallazgos</a>
                <a class="gc-pest__b <%= ClaseTab("oferta") %>" href="#" role="tab" data-panel="oferta"
                   aria-selected="<%= SeleccionTab("oferta") %>">Oferta programática</a>
                <a class="gc-pest__b <%= ClaseTab("participacion") %>" href="#" role="tab" data-panel="participacion"
                   aria-selected="<%= SeleccionTab("participacion") %>">Participación</a>
                <a class="gc-pest__b <%= ClaseTab("cobertura") %>" href="#" role="tab" data-panel="cobertura"
                   aria-selected="<%= SeleccionTab("cobertura") %>">Cobertura y verificación</a>
            </div>

            <%-- ==================================== Panel 1: Hallazgos --%>

            <div class="gc-vista" data-panel="hallazgos" role="tabpanel" <%= OcultarTab("hallazgos") %>>

                <div id="grafHallazgos" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Principales hallazgos</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__nota">
                            Conclusiones calculadas sobre la selección activa. Cambian al cambiar los
                            filtros, y cuando una comparación no tiene sustento en los datos no se
                            muestra en lugar de mostrarse vacía.
                        </p>

                        <div class="gc-halls">
                            <asp:Repeater ID="rptHallazgos" runat="server">
                                <ItemTemplate>
                                    <div class="gc-hall <%# Eval("ClaseTono") %>">
                                        <span class="gc-hall__e"><%#: Eval("Etiqueta") %></span>
                                        <p class="gc-hall__t"><%#: Eval("Titulo") %></p>
                                        <p class="gc-hall__x"><%#: Eval("Texto") %></p>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <asp:PlaceHolder ID="phSinHallazgos" runat="server" Visible="false">
                            <p class="gc-muted gc-small">
                                La selección no tiene suficientes registros para derivar conclusiones.
                            </p>
                        </asp:PlaceHolder>
                    </div>
                </div>

                <div id="grafBrecha" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Lo que la ciudadanía prioriza y lo que las candidaturas proponen</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__preg">¿Coincide la oferta programática con la demanda ciudadana?</p>
                        <p class="gc-graf__nota">
                            Las dos medidas están en porcentaje sobre su propio total, de modo que se
                            lean en una sola escala. El interés proviene de la encuesta del proyecto
                            (n = 150) y la oferta, de las propuestas registradas en la selección.
                        </p>

                        <div class="gc-leyenda">
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--b"></span>Interés ciudadano</span>
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--a"></span>Propuestas registradas</span>
                        </div>

                        <div class="gc-bars">
                            <asp:Repeater ID="rptOferta" runat="server">
                                <ItemTemplate>
                                    <div class="gc-bar">
                                        <div class="gc-bar__et">
                                            <%#: Eval("Categoria") %>
                                            <small><%#: TextoPropuestas(Container.DataItem) %></small>
                                        </div>
                                        <div class="gc-bar__p">
                                            <span class="gc-bar__b gc-bar__b--b" style="<%# AnchoInteres(Container.DataItem) %>"
                                                  title="Interés ciudadano: <%#: TextoInteres(Container.DataItem) %>"></span>
                                            <span class="gc-bar__b gc-bar__b--a" style="<%# AnchoOferta(Container.DataItem) %>"
                                                  title="Propuestas registradas: <%#: TextoOferta(Container.DataItem) %>"></span>
                                        </div>
                                        <div class="gc-bar__v">
                                            <%#: TextoInteres(Container.DataItem) %><br />
                                            <small><%#: TextoOferta(Container.DataItem) %></small>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <p class="gc-lectura"><%: LecturaBrechaTexto %></p>

                        <details class="gc-datos">
                            <summary>Ver los datos</summary>
                            <div class="gc-tablewrap">
                                <table class="gc-table">
                                    <thead>
                                        <tr>
                                            <th>Categoría</th>
                                            <th>Interés ciudadano</th>
                                            <th>Propuestas</th>
                                            <th>Oferta</th>
                                            <th>Brecha</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptOfertaTabla" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%#: Eval("Categoria") %></td>
                                                    <td><%#: TextoInteres(Container.DataItem) %></td>
                                                    <td><%#: Eval("Propuestas") %></td>
                                                    <td><%#: TextoOferta(Container.DataItem) %></td>
                                                    <td><%#: TextoBrecha(Container.DataItem) %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                        </details>
                    </div>
                </div>

            </div>

            <%-- ============================ Panel 2: Oferta programática --%>

            <div class="gc-vista" data-panel="oferta" role="tabpanel" <%= OcultarTab("oferta") %>>

                <div id="grafEstados" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Estado de cumplimiento de las propuestas</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__preg">¿En qué punto del seguimiento está cada compromiso?</p>
                        <p class="gc-graf__nota">
                            Los siete estados del esquema aparecen siempre, incluso los que están en
                            cero: un estado ausente del gráfico se leería como que no existe, cuando
                            lo que ocurre es que ninguna propuesta llegó todavía a él.
                        </p>

                        <div class="gc-pila" role="img" aria-label="Distribución de propuestas por estado">
                            <asp:Literal ID="litPilaEstados" runat="server" />
                        </div>

                        <div class="gc-bars" style="margin-top: 17px;">
                            <asp:Repeater ID="rptEstados" runat="server">
                                <ItemTemplate>
                                    <div class="gc-pila-fila__c">
                                        <span>
                                            <span class="gc-leyenda__m <%# ClaseLeyendaEstado(Container.DataItem) %>"
                                                  style="display: inline-block; vertical-align: -1px; margin-right: 8px;"></span>
                                            <b><%#: Eval("Estado") %></b>
                                        </span>
                                        <span><%#: TextoEstado(Container.DataItem) %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <p class="gc-lectura"><%: LecturaEstados %></p>
                    </div>
                </div>

                <div class="row">
                    <div id="grafPartidos" runat="server" class="col-lg-6 gc-mb">
                        <div class="gc-card" style="height: 100%;">
                            <div class="gc-card__head">
                                <h3>Propuestas por partido</h3>
                            </div>
                            <div class="gc-card__body">
                                <p class="gc-graf__preg">¿Cuánta oferta aporta cada partido en total?</p>
                                <p class="gc-graf__nota">
                                    Conteo bruto. Depende de cuántas candidaturas inscribió cada partido,
                                    así que por sí solo no compara productividad.
                                </p>

                                <div class="gc-bars">
                                    <asp:Repeater ID="rptPartidos" runat="server">
                                        <ItemTemplate>
                                            <div class="gc-bar">
                                                <div class="gc-bar__et">
                                                    <%#: Eval("Siglas") %>
                                                    <small><%#: SubPartido(Container.DataItem) %></small>
                                                </div>
                                                <div class="gc-bar__p">
                                                    <span class="gc-bar__b gc-bar__b--uno" style="<%# AnchoPartido(Container.DataItem) %>"
                                                          title="<%#: Eval("Partido") %>"></span>
                                                </div>
                                                <div class="gc-bar__v"><%#: Eval("Propuestas") %></div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div id="grafDensidad" runat="server" class="col-lg-6 gc-mb">
                        <div class="gc-card" style="height: 100%;">
                            <div class="gc-card__head">
                                <h3>Densidad programática</h3>
                            </div>
                            <div class="gc-card__body">
                                <p class="gc-graf__preg">¿Cuánto propone cada candidatura, en promedio?</p>
                                <p class="gc-graf__nota">
                                    Propuestas divididas entre candidaturas. Es la cifra comparable entre
                                    partidos de distinto tamaño, y va en su propio gráfico porque tiene
                                    otra unidad que el conteo de al lado.
                                </p>

                                <div class="gc-bars">
                                    <asp:Repeater ID="rptDensidad" runat="server">
                                        <ItemTemplate>
                                            <div class="gc-bar">
                                                <div class="gc-bar__et">
                                                    <%#: Eval("Siglas") %>
                                                    <small><%#: SubPartido(Container.DataItem) %></small>
                                                </div>
                                                <div class="gc-bar__p">
                                                    <span class="gc-bar__b gc-bar__b--uno" style="<%# AnchoDensidad(Container.DataItem) %>"
                                                          title="<%#: Eval("Partido") %>"></span>
                                                </div>
                                                <div class="gc-bar__v"><%#: TextoDensidad(Container.DataItem) %></div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="gc-card gc-graf">
                    <div class="gc-card__body">
                        <details class="gc-datos" style="margin-top: 0; border-top: 0; padding-top: 0;">
                            <summary>Ver los datos de partidos</summary>
                            <div class="gc-tablewrap">
                                <table class="gc-table">
                                    <thead>
                                        <tr>
                                            <th>Partido</th>
                                            <th>Candidaturas</th>
                                            <th>Propuestas</th>
                                            <th>Por candidatura</th>
                                            <th>A favor</th>
                                            <th>En contra</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptPartidosTabla" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%#: Eval("Partido") %></td>
                                                    <td><%#: Eval("Candidaturas") %></td>
                                                    <td><%#: Eval("Propuestas") %></td>
                                                    <td><%#: TextoDensidad(Container.DataItem) %></td>
                                                    <td><%#: Eval("MeGusta") %></td>
                                                    <td><%#: Eval("NoMeGusta") %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                        </details>
                    </div>
                </div>

            </div>

            <%-- ================================ Panel 3: Participación --%>

            <div class="gc-vista" data-panel="participacion" role="tabpanel" <%= OcultarTab("participacion") %>>

                <div class="row">
                    <div id="grafSigno" runat="server" class="col-lg-5 gc-mb">
                        <div class="gc-card" style="height: 100%;">
                            <div class="gc-card__head">
                                <h3>Signo de la participación</h3>
                            </div>
                            <div class="gc-card__body">
                                <p class="gc-graf__preg">¿Con qué signo reacciona la ciudadanía?</p>

                                <div class="gc-dona-caja">
                                    <asp:Literal ID="litDona" runat="server" />

                                    <div class="gc-dona-caja__l">
                                        <div class="gc-dona-caja__it">
                                            <span class="gc-leyenda__m gc-leyenda__m--si"></span>
                                            <span>A favor</span>
                                            <b style="margin-left: auto;"><%: Numero(Datos.Resumen.MeGusta) %></b>
                                        </div>
                                        <div class="gc-dona-caja__it">
                                            <span class="gc-leyenda__m gc-leyenda__m--no"></span>
                                            <span>En contra</span>
                                            <b style="margin-left: auto;"><%: Numero(Datos.Resumen.NoMeGusta) %></b>
                                        </div>
                                        <div class="gc-dona-caja__it gc-muted gc-small" style="display: block;">
                                            <%: Numero(Datos.Resumen.PersonasParticipando) %> personas ·
                                            <%: Numero(Datos.Resumen.Comentarios) %> comentarios
                                        </div>
                                    </div>
                                </div>

                                <p class="gc-lectura">
                                    La proporción mide reacción ciudadana ante el contenido publicado. No es
                                    una medición de cumplimiento ni una calificación que emita la plataforma.
                                </p>
                            </div>
                        </div>
                    </div>

                    <div id="grafTipos" runat="server" class="col-lg-7 gc-mb">
                        <div class="gc-card" style="height: 100%;">
                            <div class="gc-card__head">
                                <h3>Participación por tipo de contenido</h3>
                            </div>
                            <div class="gc-card__body">
                                <p class="gc-graf__preg">¿Dónde se concentra la conversación?</p>

                                <div class="gc-leyenda">
                                    <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--si"></span>A favor</span>
                                    <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--no"></span>En contra</span>
                                </div>

                                <div class="gc-bars">
                                    <asp:Repeater ID="rptParticipacion" runat="server">
                                        <ItemTemplate>
                                            <div class="gc-bar">
                                                <div class="gc-bar__et">
                                                    <%#: Eval("Nombre") %>
                                                    <small><%#: SubParticipacion(Container.DataItem) %></small>
                                                </div>
                                                <div class="gc-bar__p">
                                                    <span class="gc-bar__b gc-bar__b--si" style="<%# AnchoAFavor(Container.DataItem) %>"
                                                          title="A favor: <%#: Eval("MeGusta") %>"></span>
                                                    <span class="gc-bar__b gc-bar__b--no" style="<%# AnchoEnContra(Container.DataItem) %>"
                                                          title="En contra: <%#: Eval("NoMeGusta") %>"></span>
                                                </div>
                                                <div class="gc-bar__v"><%#: TextoParticipacion(Container.DataItem) %></div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>

                                <p class="gc-muted gc-small" style="margin-top: 14px;">
                                    Los partidos no pertenecen a una campaña, así que sus valoraciones quedan
                                    fuera cuando hay una campaña seleccionada.
                                </p>

                                <details class="gc-datos">
                                    <summary>Ver los datos</summary>
                                    <div class="gc-tablewrap">
                                        <table class="gc-table">
                                            <thead>
                                                <tr>
                                                    <th>Tipo</th>
                                                    <th>A favor</th>
                                                    <th>En contra</th>
                                                    <th>Comentarios</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <asp:Repeater ID="rptParticipacionTabla" runat="server">
                                                    <ItemTemplate>
                                                        <tr>
                                                            <td><%#: Eval("Nombre") %></td>
                                                            <td><%#: Eval("MeGusta") %></td>
                                                            <td><%#: Eval("NoMeGusta") %></td>
                                                            <td><%#: Eval("Comentarios") %></td>
                                                        </tr>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </tbody>
                                        </table>
                                    </div>
                                </details>
                            </div>
                        </div>
                    </div>
                </div>

                <div id="grafRanking" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Ranking de candidaturas por saldo de valoraciones</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__preg">¿Qué candidaturas concentran respaldo y cuáles rechazo?</p>
                        <p class="gc-graf__nota">
                            Ordenado por saldo, que es a favor menos en contra. Un orden por total de
                            valoraciones premiaría igual a quien genera respaldo y a quien genera
                            rechazo. Las dos mitades comparten una sola escala.
                        </p>

                        <div class="gc-leyenda">
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--no"></span>En contra</span>
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--si"></span>A favor</span>
                        </div>

                        <div class="gc-rank">
                            <div class="gc-rank__f gc-rank__f--head">
                                <span></span>
                                <span>Candidatura</span>
                                <span class="gc-rank__num">Propuestas</span>
                                <span class="gc-rank__num gc-rank__coment">Comentarios</span>
                                <span class="gc-rank__num">Saldo</span>
                                <span class="gc-rank__saldo">En contra · a favor</span>
                            </div>

                            <asp:Repeater ID="rptRanking" runat="server">
                                <ItemTemplate>
                                    <div class="gc-rank__f">
                                        <span class="gc-rank__pos"><%#: Posicion(Container) %></span>
                                        <span class="gc-rank__n">
                                            <b><%#: Eval("Candidato") %></b>
                                            <small><%#: SubCandidato(Container.DataItem) %></small>
                                        </span>
                                        <span class="gc-rank__num"><%#: Eval("Propuestas") %></span>
                                        <span class="gc-rank__num gc-rank__coment"><%#: Eval("Comentarios") %></span>
                                        <span class="gc-rank__num"><%#: TextoSaldo(Container.DataItem) %></span>
                                        <span class="gc-rank__saldo">
                                            <span class="gc-div">
                                                <span class="gc-div__i">
                                                    <span class="gc-div__b gc-div__b--no" style="<%# AnchoNo(Container.DataItem) %>"
                                                          title="En contra: <%#: Eval("NoMeGusta") %>"></span>
                                                </span>
                                                <span class="gc-div__e"></span>
                                                <span class="gc-div__d">
                                                    <span class="gc-div__b gc-div__b--si" style="<%# AnchoSi(Container.DataItem) %>"
                                                          title="A favor: <%#: Eval("MeGusta") %>"></span>
                                                </span>
                                            </span>
                                        </span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <details class="gc-datos">
                            <summary>Ver los datos</summary>
                            <div class="gc-tablewrap">
                                <table class="gc-table">
                                    <thead>
                                        <tr>
                                            <th>Candidatura</th>
                                            <th>Partido</th>
                                            <th>Departamento</th>
                                            <th>Propuestas</th>
                                            <th>A favor</th>
                                            <th>En contra</th>
                                            <th>Saldo</th>
                                            <th>Apoyo</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptRankingTabla" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%#: Eval("Candidato") %></td>
                                                    <td><%#: Eval("PartidoSiglas") %></td>
                                                    <td><%#: Eval("Departamento") %></td>
                                                    <td><%#: Eval("Propuestas") %></td>
                                                    <td><%#: Eval("MeGusta") %></td>
                                                    <td><%#: Eval("NoMeGusta") %></td>
                                                    <td><%#: Eval("Saldo") %></td>
                                                    <td><%#: TextoApoyo(Container.DataItem) %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                        </details>
                    </div>
                </div>

                <div id="grafActividad" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Actividad registrada por día</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__preg">¿Cómo se distribuyó la participación en el tiempo?</p>
                        <p class="gc-graf__nota">
                            Las dos series comparten una sola escala vertical porque miden lo mismo,
                            interacciones por día. Los días sin actividad aparecen en cero y no se
                            omiten de la línea.
                        </p>

                        <div class="gc-leyenda">
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--a"></span>Valoraciones</span>
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--b"></span>Comentarios</span>
                        </div>

                        <asp:Literal ID="litActividad" runat="server" />

                        <p class="gc-lectura"><%: LecturaActividad %></p>
                    </div>
                </div>

            </div>

            <%-- ============================ Panel 4: Cobertura y verificación --%>

            <div class="gc-vista" data-panel="cobertura" role="tabpanel" <%= OcultarTab("cobertura") %>>

                <div id="grafTerritorio" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Cobertura territorial</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__preg">¿En qué departamentos hay candidaturas registradas?</p>
                        <p class="gc-graf__nota">
                            Los 18 departamentos del país, ordenados de mayor a menor cobertura. No es
                            un mapa: trazar fronteras aproximadas a mano produciría un mapa falso, y
                            lo que el indicador necesita responder no es dónde queda cada
                            departamento sino cuáles tienen candidaturas y cuáles no.
                        </p>

                        <div class="gc-mapa">
                            <asp:Repeater ID="rptTerritorio" runat="server">
                                <ItemTemplate>
                                    <div class="gc-mapa__f <%# ClaseDepartamento(Container.DataItem) %>"
                                         title="<%#: TituloDepartamento(Container.DataItem) %>">
                                        <span class="gc-mapa__n"><%#: Eval("Departamento") %></span>
                                        <span class="gc-mapa__v"><%#: Eval("Candidaturas") %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <div class="gc-escala">
                            <span>Candidaturas</span>
                            <span class="gc-escala__c">
                                <span class="gc-escala__p gc-escala__p--n0" title="Ninguna"></span>
                                <span class="gc-escala__p gc-escala__p--n1" title="Pocas"></span>
                                <span class="gc-escala__p gc-escala__p--n2" title="Varias"></span>
                                <span class="gc-escala__p gc-escala__p--n3" title="El máximo"></span>
                            </span>
                            <span>ninguna → el máximo. La cifra va escrita en cada ficha.</span>
                        </div>

                        <p class="gc-lectura"><%: LecturaTerritorio %></p>

                        <details class="gc-datos">
                            <summary>Ver los datos</summary>
                            <div class="gc-tablewrap">
                                <table class="gc-table">
                                    <thead>
                                        <tr>
                                            <th>Departamento</th>
                                            <th>Candidaturas</th>
                                            <th>Propuestas</th>
                                            <th>Valoraciones</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptTerritorioTabla" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%#: Eval("Departamento") %></td>
                                                    <td><%#: Eval("Candidaturas") %></td>
                                                    <td><%#: Eval("Propuestas") %></td>
                                                    <td><%#: Eval("Valoraciones") %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                        </details>
                    </div>
                </div>

                <div id="grafVerificacion" runat="server" class="gc-card gc-graf">
                    <div class="gc-card__head">
                        <h3>Verificación por tipo de contenido</h3>
                    </div>
                    <div class="gc-card__body">
                        <p class="gc-graf__preg">¿Cuánto de lo publicado está respaldado por una fuente?</p>
                        <p class="gc-graf__nota">
                            Distingue lo que la candidatura declara de lo que la plataforma respalda
                            con una fuente verificable. Es el indicador que sostiene la neutralidad
                            del sitio, y por eso está a la vista y no escondido en cada ficha.
                        </p>

                        <div class="gc-leyenda">
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--ok"></span>Verificado</span>
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--rev"></span>En revisión</span>
                            <span class="gc-leyenda__it"><span class="gc-leyenda__m gc-leyenda__m--dec"></span>Declarado</span>
                        </div>

                        <asp:Repeater ID="rptVerificacion" runat="server">
                            <ItemTemplate>
                                <div class="gc-pila-fila">
                                    <div class="gc-pila-fila__c">
                                        <b><%#: Container.DataItem %></b>
                                        <span><%#: TextoVerificacion(Container.DataItem) %></span>
                                    </div>
                                    <div class="gc-pila" role="img"
                                         aria-label="<%#: DetalleVerificacion(Container.DataItem) %>">
                                        <%# PilaVerificacion(Container.DataItem) %>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <p class="gc-lectura"><%: LecturaVerificacion %></p>
                    </div>
                </div>

            </div>

        </asp:PlaceHolder>

        <%-- ============================================ Asistente IA --%>

        <div id="grafAsistente" runat="server" class="gc-ia gc-mb">
            <div class="gc-ia__cabeza">
                <span class="gc-msg__ico" style="background: rgba(255,255,255,.18); color: #fff;">IA</span>
                <h3>Asistente de consulta en lenguaje natural</h3>
                <span class="gc-ia__proto">Prototipo</span>
            </div>

            <div class="gc-ia__cuerpo">

                <div class="gc-note gc-note--ambar" style="margin-bottom: 18px;">
                    <span>
                        <strong>Esto es una maqueta funcional, no el asistente terminado.</strong>
                        Las respuestas se arman con las cifras reales del tablero que está en pantalla,
                        pero las preguntas no se procesan con un modelo de lenguaje todavía. Sirve para
                        mostrar la forma que va a tener la respuesta, incluidas sus fuentes.
                    </span>
                </div>

                <p class="gc-muted gc-small">Preguntas de ejemplo:</p>
                <div class="gc-ia__sug">
                    <asp:Repeater ID="rptSugerencias" runat="server" OnItemCommand="rptSugerencias_ItemCommand">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="gc-chip" CommandName="preguntar"
                                CommandArgument="<%# Container.ItemIndex %>"><%#: Container.DataItem %></asp:LinkButton>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:PlaceHolder ID="phConversacion" runat="server" Visible="false">
                    <div class="gc-msg gc-msg--yo">
                        <span class="gc-msg__ico"><%: InicialesUsuario %></span>
                        <div class="gc-msg__b"><asp:Literal ID="litPregunta" runat="server" /></div>
                    </div>

                    <div class="gc-msg gc-msg--ia">
                        <span class="gc-msg__ico">IA</span>
                        <div class="gc-msg__b">
                            <asp:Literal ID="litRespuesta" runat="server" />

                            <div class="gc-fuentes">
                                <div class="gc-fuentes__t">Fuentes consultadas</div>
                                <ol>
                                    <asp:Repeater ID="rptFuentes" runat="server">
                                        <ItemTemplate>
                                            <li><%#: Container.DataItem %></li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ol>
                            </div>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-coment-form" style="margin-top: 6px;">
                    <span class="gc-msg__ico"><%: InicialesUsuario %></span>
                    <div style="flex: 1 1 auto; min-width: 0;">
                        <asp:TextBox ID="txtPregunta" runat="server" CssClass="gc-input"
                            placeholder="Escribí tu pregunta sobre las candidaturas o sus propuestas" />
                        <div class="gc-coment-form__pie">
                            <span class="gc-muted gc-small">
                                En esta etapa solo responden las preguntas de ejemplo de arriba.
                            </span>
                            <asp:Button ID="btnPreguntar" runat="server" CssClass="gc-btn gc-btn--sm"
                                Text="Preguntar" OnClick="btnPreguntar_Click" />
                        </div>
                    </div>
                </div>

            </div>
        </div>

        <p class="gc-neutral">
            Los indicadores de esta página describen lo que está registrado en la plataforma. No
            constituyen una evaluación de las candidaturas ni un pronóstico electoral. El estado de
            cumplimiento de una propuesta lo asigna la plataforma únicamente con evidencia
            documentada, nunca la propia candidatura.
        </p>

    </div>

    <input type="hidden" id="gcPestana" name="gcPestana" value="<%: PestanaActiva %>" />

    <script src="<%= Recurso("~/Scripts/cumplehn-analitica.js") %>"></script>

</asp:Content>
