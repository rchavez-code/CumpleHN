<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="frontend._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ==================================================== Encabezado --%>

    <section class="gc-hero">
        <div class="container">
            <p class="gc-eyebrow">Seguimiento ciudadano de compromisos políticos</p>

            <h1>Lo que se promete, ordenado y a la vista.</h1>

            <p class="gc-hero__lead">
                CumpleHN reúne en un solo lugar las candidaturas, sus propuestas y el estado de cumplimiento
                de cada compromiso. No emitimos un veredicto sobre quién cumple mejor: ordenamos la evidencia
                para que cada persona pondere lo que considere de mayor peso y decida por su cuenta.
            </p>

            <div class="gc-hero__actions">
                <asp:TextBox ID="txtBuscar" runat="server" CssClass="gc-input" Style="max-width: 320px;"
                    placeholder="Buscar un candidato por nombre" />
                <asp:Button ID="btnBuscar" runat="server" CssClass="gc-btn" Text="Buscar" OnClick="btnBuscar_Click" />
                <a class="gc-btn gc-btn--ghost" href="<%= ResolveUrl("~/Campanas") %>">Ver campañas</a>
            </div>

            <div class="gc-figures">
                <div class="gc-figure">
                    <div class="gc-figure__n"><%: TotalCampanas %></div>
                    <div class="gc-figure__l">campañas electorales registradas</div>
                </div>
                <div class="gc-figure">
                    <div class="gc-figure__n"><%: TotalCandidatos %></div>
                    <div class="gc-figure__l">candidaturas con perfil público</div>
                </div>
                <div class="gc-figure">
                    <div class="gc-figure__n"><%: TotalPropuestas %></div>
                    <div class="gc-figure__l">propuestas documentadas</div>
                </div>
                <div class="gc-figure">
                    <div class="gc-figure__n">6</div>
                    <div class="gc-figure__l">categorías temáticas de clasificación</div>
                </div>
            </div>
        </div>
    </section>

    <div class="container" style="padding-top: 40px;">

        <%-- ============================================== Campaña actual --%>

        <asp:PlaceHolder ID="phActual" runat="server">
            <div class="gc-sechead">
                <div>
                    <h2>Campaña electoral actual</h2>
                    <p class="gc-muted">La campaña abierta en este momento para consulta y registro.</p>
                </div>
                <a class="gc-btn gc-btn--quiet gc-btn--sm gc-nowrap" href="<%= ResolveUrl("~/Campanas") %>">Ver todas &rarr;</a>
            </div>

            <div class="gc-feature gc-mb">
                <div class="gc-feature__banner">
                    <span class="gc-live">
                        <span class="gc-live__dot" aria-hidden="true"></span>
                        En curso
                    </span>

                    <h2><%: NombreActual %></h2>
                    <p><%: ResumenActual %></p>
                </div>

                <div class="gc-feature__body">
                    <div class="gc-feature__stats">
                        <div class="gc-stat">
                            <div class="gc-stat__n"><%: CandidatosActual %></div>
                            <div class="gc-stat__l">candidatos</div>
                        </div>
                        <div class="gc-stat">
                            <div class="gc-stat__n"><%: PropuestasActual %></div>
                            <div class="gc-stat__l">propuestas</div>
                        </div>
                        <div class="gc-stat">
                            <div class="gc-stat__n"><%: PublicacionesActual %></div>
                            <div class="gc-stat__l">publicaciones</div>
                        </div>
                        <div class="gc-stat">
                            <div class="gc-stat__n"><%: FechaEleccion %></div>
                            <div class="gc-stat__l"><%: CuentaRegresiva %></div>
                        </div>
                    </div>

                    <a class="gc-btn gc-btn--lg gc-nowrap" href="<%= UrlActual %>">Explorar la campaña</a>
                </div>
            </div>
        </asp:PlaceHolder>

        <%-- ================================================== Encuesta --%>

        <%-- Va después de la campaña destacada y antes de las candidaturas:
             es donde cae la vista de quien llega por primera vez, sin que
             desplace al contenido que la plataforma existe para mostrar. --%>

        <asp:PlaceHolder ID="phEncuestas" runat="server" Visible="false">
            <div class="gc-sechead" style="margin-top: 42px;">
                <div>
                    <h2>Tu opinión</h2>
                    <p class="gc-muted">
                        Preguntas abiertas a la ciudadanía. Una respuesta por cuenta, y podés
                        cambiarla mientras la encuesta siga abierta.
                    </p>
                </div>
            </div>

            <%-- Las encuestas que pasan de la segunda se dibujan igual, solo que
                 ocultas: si «Ver más» pidiera otra carga al servidor, desplegar la
                 lista costaría un viaje para mostrar algo ya consultado. El estado
                 viaja en el input oculto para sobrevivir al postback de responder,
                 igual que la pestaña activa del tablero. --%>

            <div id="zonaEncuestas" runat="server" class="gc-encs">

                <div class="row">
                    <asp:Repeater ID="rptEncuestas" runat="server">
                        <ItemTemplate>
                            <div class="<%# ClaseColumnaEncuesta(Container.ItemIndex) %>">
                                <gc:EncuestaCard runat="server" Item="<%# (Encuesta)Container.DataItem %>" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:PlaceHolder ID="phVerMas" runat="server" Visible="false">
                    <div class="gc-encs__mas">
                        <button type="button" class="gc-btn gc-btn--ghost" id="btnVerMasEncuestas"
                                data-mas="<%: TextoVerMas %>" data-menos="Ver menos"
                                aria-expanded="<%: EncuestasDesplegadas ? "true" : "false" %>"
                                onclick="cumplehnVerMasEncuestas(this)"><%: TextoBotonEncuestas %></button>
                    </div>
                </asp:PlaceHolder>

                <asp:HiddenField ID="hdnEncuestas" runat="server" Value="0" />
            </div>

            <script type="text/javascript">
                // Quince líneas para un botón de una sola página no justifican un
                // archivo aparte con su huella de versión.
                function cumplehnVerMasEncuestas(boton) {
                    var zona = document.getElementById('<%= zonaEncuestas.ClientID %>');
                    var estado = document.getElementById('<%= hdnEncuestas.ClientID %>');
                    var abierta = zona.className.indexOf('is-abierta') === -1;

                    zona.className = abierta ? 'gc-encs is-abierta' : 'gc-encs';
                    estado.value = abierta ? '1' : '0';

                    boton.textContent = abierta ? boton.getAttribute('data-menos')
                                                : boton.getAttribute('data-mas');
                    boton.setAttribute('aria-expanded', abierta ? 'true' : 'false');
                }
            </script>
        </asp:PlaceHolder>

        <%-- =========================================== Iniciativas --%>

        <%-- Después de las encuestas: las dos son participación ciudadana, y
             estas son las propuestas que escribe la gente. Ordenadas por apoyo
             (me gusta menos no me gusta). La plataforma no las respalda, y el
             encabezado lo dice: es texto de cualquier persona con cuenta. --%>

        <asp:PlaceHolder ID="phIniciativas" runat="server" Visible="false">
            <div class="gc-sechead" style="margin-top: 42px;">
                <div>
                    <h2>Iniciativas ciudadanas</h2>
                    <p class="gc-muted">
                        Propuestas de proyecto que escribe la ciudadanía, ordenadas por el apoyo que reciben.
                        La plataforma no las respalda ni las verifica: las ordena para que las veas y opines.
                    </p>
                </div>
                <asp:PlaceHolder ID="phProponer" runat="server">
                    <a class="gc-btn gc-btn--quiet gc-btn--sm gc-nowrap" href="<%= UrlProponer %>">Proponer una &rarr;</a>
                </asp:PlaceHolder>
            </div>

            <asp:PlaceHolder ID="phIniciativasOcultas" runat="server" Visible="false">
                <p class="gc-oculto-aviso">
                    Este módulo está oculto al público. Solo vos lo ves, como administrador.
                </p>
            </asp:PlaceHolder>

            <%-- Todas las activas se dibujan en la misma respuesta, cuatro a la
                 vista y el resto ocultas por bloques de cuatro. «Ver más» solo
                 les quita el ocultamiento: si pidiera otra carga, desplegar la
                 lista costaría un viaje para mostrar algo ya consultado. Cuántos
                 bloques están abiertos viaja en el input oculto, como la pestaña
                 activa del tablero, para sobrevivir al postback de votar o
                 comentar. --%>

            <div id="zonaIniciativas" runat="server" class="gc-inics">
                <div class="row">
                    <asp:Repeater ID="rptIniciativas" runat="server">
                        <ItemTemplate>
                            <div class="<%# ClaseColumnaIniciativa(Container.ItemIndex) %>" data-bloque="<%# BloqueDe(Container.ItemIndex) %>">
                                <gc:IniciativaCard runat="server" Item="<%# (Iniciativa)Container.DataItem %>" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:PlaceHolder ID="phVerMasInic" runat="server" Visible="false">
                    <div class="gc-inics__mas">
                        <span class="gc-inics__cont" id="contIniciativas" data-total="<%: TotalIniciativas %>"><%: TextoContadorInic %></span>
                        <button type="button" class="gc-btn gc-btn--ghost" id="btnVerMasInic"
                                data-porbloque="<%: PorBloqueInic %>" data-menos="Ver menos"
                                onclick="cumplehnVerMasIniciativas(this)"><%: TextoBotonInic %></button>
                    </div>
                </asp:PlaceHolder>

                <asp:HiddenField ID="hdnIniciativas" runat="server" Value="1" />
            </div>

            <script type="text/javascript">
                // Despliega las iniciativas de cuatro en cuatro y mantiene el
                // contador «8 de 24». El estado vive en el input oculto —cuántos
                // bloques están abiertos— y se reaplica al cargar, también
                // después de un postback de votar o comentar, porque el servidor
                // siempre rinde solo el primer bloque. Igual que el «Ver más» de
                // las encuestas, son pocas líneas para una sola página y no
                // justifican un archivo aparte con su huella de versión.
                function cumplehnAplicarIniciativas() {
                    var zona = document.getElementById('<%= zonaIniciativas.ClientID %>');
                    var estado = document.getElementById('<%= hdnIniciativas.ClientID %>');
                    var boton = document.getElementById('btnVerMasInic');
                    var cont = document.getElementById('contIniciativas');
                    if (!zona || !estado || !boton || !cont) return;

                    var total = parseInt(cont.getAttribute('data-total'), 10);
                    var porBloque = parseInt(boton.getAttribute('data-porbloque'), 10);
                    var abiertos = parseInt(estado.value, 10) || 1;

                    var columnas = zona.querySelectorAll('[data-bloque]');
                    for (var i = 0; i < columnas.length; i++) {
                        var bloque = parseInt(columnas[i].getAttribute('data-bloque'), 10);
                        if (bloque < abiertos) columnas[i].classList.remove('is-oculta');
                        else columnas[i].classList.add('is-oculta');
                    }

                    var visibles = Math.min(abiertos * porBloque, total);
                    cont.textContent = visibles + ' de ' + total + ' iniciativas';

                    boton.textContent = (visibles >= total)
                        ? boton.getAttribute('data-menos')
                        : 'Ver ' + Math.min(porBloque, total - visibles) + ' más';
                }

                function cumplehnVerMasIniciativas(boton) {
                    var estado = document.getElementById('<%= hdnIniciativas.ClientID %>');
                    var cont = document.getElementById('contIniciativas');

                    var total = parseInt(cont.getAttribute('data-total'), 10);
                    var porBloque = parseInt(boton.getAttribute('data-porbloque'), 10);
                    var abiertos = parseInt(estado.value, 10) || 1;

                    // Con todo a la vista, el botón colapsa a la vista inicial.
                    estado.value = (abiertos * porBloque >= total) ? 1 : abiertos + 1;

                    cumplehnAplicarIniciativas();
                }

                cumplehnAplicarIniciativas();
            </script>
        </asp:PlaceHolder>

        <%-- ================================================= Candidatos --%>

        <div class="gc-sechead" style="margin-top: 42px;">
            <div>
                <h2>Candidaturas de la campaña</h2>
                <p class="gc-muted">Cada perfil reúne la biografía, las propuestas y las publicaciones que la propia candidatura declara.</p>
            </div>
            <a class="gc-btn gc-btn--quiet gc-btn--sm gc-nowrap" href="<%= ResolveUrl("~/Candidatos") %>">Ver todos &rarr;</a>
        </div>

        <div class="row">
            <asp:Repeater ID="rptCandidatos" runat="server">
                <ItemTemplate>
                    <div class="col-lg-4 col-md-6 gc-mb">
                        <gc:CandidatoCard runat="server" Item="<%# (Candidato)Container.DataItem %>" />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <%-- ============================================== Otras campañas --%>

        <div class="gc-sechead" style="margin-top: 42px;">
            <div>
                <h2>Otras campañas</h2>
                <p class="gc-muted">Ciclos próximos y cerrados. En los cerrados se consulta el cumplimiento de lo prometido.</p>
            </div>
        </div>

        <div class="row">
            <asp:Repeater ID="rptCampanas" runat="server">
                <ItemTemplate>
                    <div class="col-lg-4 col-md-6 gc-mb">
                        <gc:CampanaCard runat="server" Item="<%# (Campana)Container.DataItem %>" />
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <%-- =============================================== Cómo funciona --%>

        <div class="gc-sechead" style="margin-top: 42px;">
            <div>
                <h2>Cómo funciona</h2>
                <p class="gc-muted">Tres pasos entre lo que se dijo en campaña y lo que se puede comprobar.</p>
            </div>
        </div>

        <div class="row gc-mb">
            <div class="col-md-4 gc-mb">
                <div class="gc-card gc-quick">
                    <span class="gc-quick__ico" aria-hidden="true"><strong>1</strong></span>
                    <div>
                        <h3>Se declara</h3>
                        <p>La candidatura registra sus propuestas con problema, objetivo, beneficiarios y plazo. Queda marcado como declarado.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 gc-mb">
                <div class="gc-card gc-quick">
                    <span class="gc-quick__ico" aria-hidden="true"><strong>2</strong></span>
                    <div>
                        <h3>Se respalda</h3>
                        <p>La plataforma asocia a cada compromiso las fuentes que lo sostienen: planes de gobierno, discursos, notas de prensa y documentos oficiales.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4 gc-mb">
                <div class="gc-card gc-quick">
                    <span class="gc-quick__ico" aria-hidden="true"><strong>3</strong></span>
                    <div>
                        <h3>Se compara</h3>
                        <p>Con el tiempo cada propuesta recibe un estado de cumplimiento, y eso permite comparar por categoría, partido y período.</p>
                    </div>
                </div>
            </div>
        </div>

        <%-- ============================================ Para candidatos --%>

        <div class="gc-card gc-mb" style="overflow: hidden;">
            <div class="row" style="margin: 0;">
                <div class="col-lg-8" style="padding: 30px;">
                    <p class="gc-eyebrow">Para candidatos</p>
                    <h2 style="margin-bottom: .4em;">Registrá tu candidatura y documentá tus propuestas</h2>
                    <p class="gc-muted" style="max-width: 58ch; margin: 0;">
                        Publicá tu perfil, tus proyectos de campaña y tus actualizaciones. Lo que registrás se
                        muestra siempre identificado como declarado por la candidatura, y cualquier persona
                        puede consultarlo sin necesidad de crear una cuenta.
                    </p>
                </div>
                <div class="col-lg-4" style="padding: 30px; display: flex; align-items: center;">
                    <div style="width: 100%;">
                        <a class="gc-btn gc-btn--lg gc-btn--block" href="<%= ResolveUrl("~/Registro?tipo=candidato") %>">Registrar candidatura</a>
                        <a class="gc-btn gc-btn--ghost gc-btn--block" style="margin-top: 9px;" href="<%= ResolveUrl("~/Acceso") %>">Ya tengo cuenta</a>
                    </div>
                </div>
            </div>
        </div>

        <p class="gc-neutral">
            CumpleHN es una herramienta de carácter genérico y neutral. No promueve ni ataca candidatos ni
            partidos políticos. La información de cada perfil es declarada por la propia candidatura y se
            identifica como tal hasta que exista una fuente verificable que la respalde.
        </p>

    </div>

</asp:Content>
