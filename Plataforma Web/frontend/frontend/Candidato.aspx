<%@ Page Title="Candidato" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Candidato.aspx.cs" Inherits="frontend.CandidatoPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ================================================= Encabezado --%>

    <section class="gc-profile">
        <div class="gc-profile__cover" aria-hidden="true"></div>

        <div class="container">
            <div class="gc-profile__in">
                <span class="gc-avatar gc-avatar--xl" style="<%= EstiloAvatar %>" aria-hidden="true"><%: Iniciales %></span>

                <div class="gc-profile__id">
                    <h1><%: Item.NombreCompleto %></h1>
                    <p class="gc-profile__cargo"><%: Item.Cargo %> · <%: Item.Territorio %></p>

                    <div class="gc-cand__tags">
                        <span class="gc-chip"><%: Item.PartidoTexto %></span>
                        <span class="gc-chip"><%: NivelTexto %></span>
                        <span class="<%= Item.VerificacionClase %>"><%: Item.VerificacionTexto %></span>
                    </div>
                </div>

                <div class="gc-profile__acts">
                    <a class="gc-btn gc-btn--ghost" href="<%= UrlCampana %>">Ver la campaña</a>
                    <a class="gc-btn" href="<%= ResolveUrl("~/Acceso") %>">Seguir</a>
                </div>
            </div>

            <div class="gc-card__inter" style="border: 0; padding: 0 0 14px;">
                <gc:Interaccion ID="interPerfil" runat="server" UrlComentarios="" />
            </div>

            <ul class="gc-tabs">
                <li><a class="<%= ClaseTab("propuestas") %>" href="<%= UrlTab("propuestas") %>">Propuestas<span class="gc-tabs__n"><%: Item.TotalPropuestas %></span></a></li>
                <li><a class="<%= ClaseTab("publicaciones") %>" href="<%= UrlTab("publicaciones") %>">Publicaciones<span class="gc-tabs__n"><%: Item.TotalPublicaciones %></span></a></li>
                <li><a class="<%= ClaseTab("opiniones") %>" href="<%= UrlTab("opiniones") %>">Opiniones<span class="gc-tabs__n"><%: Item.Comentarios %></span></a></li>
                <li><a class="<%= ClaseTab("info") %>" href="<%= UrlTab("info") %>">Información</a></li>
            </ul>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">
        <div class="gc-layout">

            <div>
                <%-- ==================================== Propuestas --%>

                <asp:PlaceHolder ID="phPropuestas" runat="server">
                    <div class="gc-sechead">
                        <div>
                            <h2>Propuestas de campaña</h2>
                            <p class="gc-muted">Cada propuesta indica el problema que busca resolver, su objetivo y a quién beneficia.</p>
                        </div>
                    </div>

                    <div class="row">
                        <asp:Repeater ID="rptPropuestas" runat="server">
                            <ItemTemplate>
                                <div class="col-lg-6 gc-mb">
                                    <gc:PropuestaCard runat="server" Item="<%# (Propuesta)Container.DataItem %>" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:PlaceHolder ID="phPropuestasVacio" runat="server" Visible="false">
                        <div class="gc-card gc-empty">
                            <h3>Esta candidatura todavía no registra propuestas</h3>
                            <p>Cuando publique sus proyectos de campaña aparecerán acá, con su categoría y estado.</p>
                        </div>
                    </asp:PlaceHolder>
                </asp:PlaceHolder>

                <%-- =================================== Publicaciones --%>

                <asp:PlaceHolder ID="phPublicaciones" runat="server" Visible="false">
                    <div class="gc-sechead">
                        <div>
                            <h2>Publicaciones</h2>
                            <p class="gc-muted">Actualizaciones que la candidatura ha publicado en la plataforma.</p>
                        </div>
                    </div>

                    <div class="gc-feed">
                        <asp:Repeater ID="rptPublicaciones" runat="server">
                            <ItemTemplate>
                                <gc:PublicacionCard runat="server" Item="<%# (Publicacion)Container.DataItem %>" />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <asp:PlaceHolder ID="phPublicacionesVacio" runat="server" Visible="false">
                        <div class="gc-card gc-empty">
                            <h3>Todavía no hay publicaciones</h3>
                            <p>Esta candidatura aún no ha publicado actualizaciones de campaña.</p>
                        </div>
                    </asp:PlaceHolder>
                </asp:PlaceHolder>

                <%-- ====================================== Opiniones --%>

                <asp:PlaceHolder ID="phOpiniones" runat="server" Visible="false">
                    <div class="gc-sechead">
                        <div>
                            <h2>Opiniones sobre esta candidatura</h2>
                            <p class="gc-muted">
                                Lo que la ciudadanía comenta. Leer es libre, opinar requiere cuenta.
                            </p>
                        </div>
                    </div>

                    <div class="gc-card">
                        <div class="gc-card__body">
                            <gc:Comentarios ID="comentsPerfil" runat="server" />
                        </div>
                    </div>

                    <p class="gc-neutral" style="margin-top: 18px;">
                        Los comentarios y las valoraciones expresan la opinión de quien los publica.
                        No son verificados por CumpleHN ni forman parte de la evidencia sobre el
                        cumplimiento de las propuestas.
                    </p>
                </asp:PlaceHolder>

                <%-- ==================================== Información --%>

                <asp:PlaceHolder ID="phInfo" runat="server" Visible="false">
                    <div class="gc-card gc-mb">
                        <div class="gc-card__body">
                            <h2>Sobre la candidatura</h2>
                            <p class="gc-lead"><%: Item.Titular %></p>
                            <p><%: Item.DescripcionCandidatura %></p>
                        </div>
                    </div>

                    <div class="gc-card gc-mb">
                        <div class="gc-card__head">
                            <h3>Biografía</h3>
                        </div>
                        <div class="gc-card__body">
                            <p><%: Item.Biografia %></p>
                        </div>
                    </div>

                    <div class="gc-card">
                        <div class="gc-card__head">
                            <h3>Información profesional</h3>
                        </div>
                        <div class="gc-card__body">
                            <p><%: Item.InformacionProfesional %></p>
                        </div>
                    </div>
                </asp:PlaceHolder>
            </div>

            <%-- ========================================= Riel lateral --%>

            <aside class="gc-rail">
                <div class="gc-card gc-mb">
                    <div class="gc-card__head">
                        <h3>Candidatura</h3>
                    </div>
                    <div class="gc-card__body">
                        <dl class="gc-datalist">
                            <div>
                                <dt>Cargo</dt>
                                <dd><%: Item.Cargo %></dd>
                            </div>
                            <div>
                                <dt>Nivel</dt>
                                <dd><%: NivelTexto %></dd>
                            </div>
                            <div>
                                <dt>Territorio</dt>
                                <dd><%: Item.Territorio %></dd>
                            </div>
                            <div>
                                <dt>Partido</dt>
                                <dd><%: Item.PartidoTexto %></dd>
                            </div>
                            <div>
                                <dt>Campaña</dt>
                                <dd><a href="<%= UrlCampana %>"><%: NombreCampana %></a></dd>
                            </div>
                        </dl>
                    </div>
                </div>

                <asp:PlaceHolder ID="phContacto" runat="server">
                    <div class="gc-card gc-mb">
                        <div class="gc-card__head">
                            <h3>Contacto público</h3>
                        </div>
                        <div class="gc-card__body">
                            <dl class="gc-datalist">
                                <asp:PlaceHolder ID="phCorreo" runat="server">
                                    <div>
                                        <dt>Correo</dt>
                                        <dd><%: Item.CorreoPublico %></dd>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phTelefono" runat="server">
                                    <div>
                                        <dt>Teléfono</dt>
                                        <dd><%: Item.Telefono %></dd>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="phSitio" runat="server">
                                    <div>
                                        <dt>Sitio web</dt>
                                        <dd><%: Item.SitioWeb %></dd>
                                    </div>
                                </asp:PlaceHolder>
                            </dl>

                            <asp:PlaceHolder ID="phRedes" runat="server">
                                <div class="gc-social" style="margin-top: 14px;">
                                    <asp:Repeater ID="rptRedes" runat="server">
                                        <ItemTemplate>
                                            <span class="gc-chip"><%#: Container.DataItem %></span>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </asp:PlaceHolder>

                            <p class="gc-muted gc-small" style="margin: 14px 0 0;">
                                Datos de contacto que la propia candidatura decidió hacer públicos.
                            </p>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-note">
                    <span>
                        La información de este perfil fue registrada por la candidatura.
                        <strong><%: Item.VerificacionTexto %>.</strong>
                        CumpleHN la presenta organizada, sin avalarla ni cuestionarla.
                    </span>
                </div>
            </aside>

        </div>
    </div>

</asp:Content>
