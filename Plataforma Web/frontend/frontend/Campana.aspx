<%@ Page Title="Campaña" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Campana.aspx.cs" Inherits="frontend.CampanaPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============================================ Encabezado de campaña --%>

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <a href="<%= ResolveUrl("~/Campanas") %>">Campañas</a>
                <span aria-hidden="true">/</span>
                <span><%: Item.Nombre %></span>
            </nav>

            <div class="gc-chead__top">
                <div>
                    <p class="gc-eyebrow"><%: Item.EstadoTexto %> · <%: Item.Alcance %></p>
                    <h1><%: Item.Nombre %></h1>
                    <p class="gc-lead" style="max-width: 64ch; margin: 0;"><%: Item.Resumen %></p>
                </div>

                <div style="text-align: right;">
                    <div class="gc-stat__n"><%: FechaEleccion %></div>
                    <div class="gc-stat__l"><%: CuentaRegresiva %></div>
                </div>
            </div>

            <ul class="gc-tabs">
                <li><a class="<%= ClaseTab("feed") %>" href="<%= UrlTab("feed") %>">Publicaciones<span class="gc-tabs__n"><%: Item.TotalPublicaciones %></span></a></li>
                <li><a class="<%= ClaseTab("candidatos") %>" href="<%= UrlTab("candidatos") %>">Candidatos<span class="gc-tabs__n"><%: Item.TotalCandidatos %></span></a></li>
                <li><a class="<%= ClaseTab("propuestas") %>" href="<%= UrlTab("propuestas") %>">Propuestas<span class="gc-tabs__n"><%: Item.TotalPropuestas %></span></a></li>
                <li><a class="<%= ClaseTab("info") %>" href="<%= UrlTab("info") %>">Información</a></li>
            </ul>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">

        <%-- ================================================== Publicaciones --%>

        <asp:PlaceHolder ID="phFeed" runat="server">
            <div class="gc-layout">

                <div>
                    <div class="gc-gate gc-mb">
                        <span>
                            Estás viendo el contenido como visitante. Para apoyar publicaciones o comentar
                            necesitás una cuenta.
                        </span>
                        <a class="gc-btn gc-btn--ghost gc-btn--sm gc-nowrap" href="<%= ResolveUrl("~/Acceso") %>">Acceder</a>
                    </div>

                    <div class="gc-feed">
                        <asp:Repeater ID="rptFeed" runat="server">
                            <ItemTemplate>
                                <gc:PublicacionCard runat="server" Item="<%# (Publicacion)Container.DataItem %>" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:PlaceHolder ID="phFeedVacio" runat="server" Visible="false">
                        <div class="gc-card gc-empty">
                            <h3>Todavía no hay publicaciones</h3>
                            <p>Cuando las candidaturas de esta campaña publiquen actualizaciones, aparecerán acá.</p>
                        </div>
                    </asp:PlaceHolder>
                </div>

                <aside class="gc-rail">
                    <div class="gc-card gc-mb">
                        <div class="gc-card__head">
                            <h3>Sobre la campaña</h3>
                        </div>
                        <div class="gc-card__body">
                            <dl class="gc-datalist">
                                <div>
                                    <dt>Estado</dt>
                                    <dd><%: Item.EstadoTexto %></dd>
                                </div>
                                <div>
                                    <dt>Elección</dt>
                                    <dd><%: FechaEleccionLarga %></dd>
                                </div>
                                <div>
                                    <dt>Alcance</dt>
                                    <dd><%: Item.Alcance %></dd>
                                </div>
                                <div>
                                    <dt>Candidatos</dt>
                                    <dd><%: Item.TotalCandidatos %></dd>
                                </div>
                                <div>
                                    <dt>Propuestas</dt>
                                    <dd><%: Item.TotalPropuestas %></dd>
                                </div>
                            </dl>
                        </div>
                    </div>

                    <div class="gc-card gc-mb">
                        <div class="gc-card__head">
                            <h3>Categorías temáticas</h3>
                        </div>
                        <div class="gc-card__body">
                            <div class="gc-cand__tags">
                                <asp:Repeater ID="rptCategorias" runat="server">
                                    <ItemTemplate>
                                        <span class="<%#: Vista.ClaseCategoria((string)Container.DataItem) %>"><%#: Container.DataItem %></span>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <p class="gc-muted gc-small" style="margin: 13px 0 0;">
                                Clasificación derivada de las funciones de gobierno, para poder comparar
                                propuestas entre candidaturas y períodos.
                            </p>
                        </div>
                    </div>

                    <div class="gc-card">
                        <div class="gc-card__head">
                            <h3>Candidaturas</h3>
                        </div>
                        <div class="gc-card__body" style="display: grid; gap: 13px;">
                            <asp:Repeater ID="rptRail" runat="server">
                                <ItemTemplate>
                                    <a href="<%#: ResolveUrl(((Candidato)Container.DataItem).Url) %>" style="display: flex; align-items: center; gap: 11px; color: inherit;">
                                        <span class="gc-avatar gc-avatar--sm" aria-hidden="true"><%#: ((Candidato)Container.DataItem).Iniciales %></span>
                                        <span style="min-width: 0;">
                                            <strong style="display: block; font-size: .88rem;"><%#: ((Candidato)Container.DataItem).NombreCompleto %></strong>
                                            <span class="gc-muted gc-small"><%#: ((Candidato)Container.DataItem).Cargo %></span>
                                        </span>
                                    </a>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                        <div class="gc-card__foot">
                            <a class="gc-small" href="<%= UrlTab("candidatos") %>">Ver todas las candidaturas &rarr;</a>
                        </div>
                    </div>
                </aside>

            </div>
        </asp:PlaceHolder>

        <%-- ====================================================== Candidatos --%>

        <asp:PlaceHolder ID="phCandidatos" runat="server" Visible="false">
            <div class="gc-sechead">
                <div>
                    <h2>Candidaturas participantes</h2>
                    <p class="gc-muted">La información de cada perfil es declarada por la propia candidatura.</p>
                </div>
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

            <asp:PlaceHolder ID="phCandidatosVacio" runat="server" Visible="false">
                <div class="gc-card gc-empty">
                    <h3>Todavía no hay candidaturas registradas</h3>
                    <p>El registro de esta campaña está abierto. Si sos candidato, podés crear tu perfil público.</p>
                    <a class="gc-btn" href="<%= ResolveUrl("~/Registro?tipo=candidato") %>">Registrar candidatura</a>
                </div>
            </asp:PlaceHolder>
        </asp:PlaceHolder>

        <%-- ====================================================== Propuestas --%>

        <asp:PlaceHolder ID="phPropuestas" runat="server" Visible="false">
            <div class="gc-sechead">
                <div>
                    <h2>Propuestas y proyectos de campaña</h2>
                    <p class="gc-muted">Todo lo que las candidaturas de esta campaña se han comprometido a hacer.</p>
                </div>
            </div>

            <div class="row">
                <asp:Repeater ID="rptPropuestas" runat="server">
                    <ItemTemplate>
                        <div class="col-lg-4 col-md-6 gc-mb">
                            <gc:PropuestaCard runat="server" Item="<%# (Propuesta)Container.DataItem %>" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <asp:PlaceHolder ID="phPropuestasVacio" runat="server" Visible="false">
                <div class="gc-card gc-empty">
                    <h3>Todavía no hay propuestas registradas</h3>
                    <p>Las propuestas aparecen acá en cuanto las candidaturas las documentan desde su panel.</p>
                </div>
            </asp:PlaceHolder>
        </asp:PlaceHolder>

        <%-- ===================================================== Información --%>

        <asp:PlaceHolder ID="phInfo" runat="server" Visible="false">
            <div class="row">
                <div class="col-lg-8">
                    <div class="gc-card">
                        <div class="gc-card__body">
                            <h2>Sobre esta campaña</h2>
                            <p class="gc-lead"><%: Item.Resumen %></p>
                            <p><%: Item.Descripcion %></p>

                            <div class="gc-pob gc-mt">
                                <div>
                                    <div class="gc-pob__l">Inicio del registro</div>
                                    <div><%: FechaInicioLarga %></div>
                                </div>
                                <div>
                                    <div class="gc-pob__l">Fecha de elección</div>
                                    <div><%: FechaEleccionLarga %></div>
                                </div>
                                <div>
                                    <div class="gc-pob__l">Alcance</div>
                                    <div><%: Item.Alcance %></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-lg-4">
                    <div class="gc-note">
                        <span>
                            El contenido de las candidaturas se presenta identificado como declarado hasta que
                            exista una fuente verificable que lo respalde. CumpleHN no avala ni cuestiona
                            ninguna candidatura.
                        </span>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>

    </div>

</asp:Content>
