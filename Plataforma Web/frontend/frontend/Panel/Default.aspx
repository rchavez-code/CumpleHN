<%@ Page Title="Resumen" Language="C#" MasterPageFile="~/Panel/Panel.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="frontend.Panel.PanelDefault" %>

<asp:Content ContentPlaceHolderID="TopActions" runat="server">
    <a class="gc-btn gc-btn--ghost gc-btn--sm" href="<%= ResolveUrl("~/Panel/Perfil") %>">Editar perfil</a>
    <a class="gc-btn gc-btn--sm" href="<%= ResolveUrl("~/Panel/Proyecto") %>">Nuevo proyecto</a>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <%-- ================================================ Estado del perfil --%>

    <div class="gc-card gc-mb">
        <div class="gc-card__body">
            <div style="display: flex; flex-wrap: wrap; align-items: flex-end; justify-content: space-between; gap: 14px; margin-bottom: 13px;">
                <div>
                    <h2 style="margin: 0 0 3px;">Tu perfil está <%: PerfilCompleto %> % completo</h2>
                    <p class="gc-muted gc-small" style="margin: 0;"><%: MensajePerfil %></p>
                </div>
                <a class="gc-btn gc-btn--ghost gc-btn--sm gc-nowrap" href="<%= ResolveUrl("~/Panel/Perfil") %>">Completar perfil</a>
            </div>

            <div class="gc-meter" role="img" aria-label="Avance del perfil: <%: PerfilCompleto %> por ciento">
                <div class="gc-meter__fill" style="width: <%: PerfilCompleto %>%;"></div>
            </div>
        </div>
    </div>

    <%-- ========================================================= Cifras --%>

    <div class="row gc-mb">
        <div class="col-lg-3 col-md-6 gc-mb">
            <div class="gc-card gc-card__body">
                <div class="gc-stat__n"><%: TotalProyectos %></div>
                <div class="gc-stat__l">proyectos registrados</div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 gc-mb">
            <div class="gc-card gc-card__body">
                <div class="gc-stat__n"><%: TotalPublicaciones %></div>
                <div class="gc-stat__l">publicaciones</div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 gc-mb">
            <div class="gc-card gc-card__body">
                <div class="gc-stat__n"><%: TotalApoyos %></div>
                <div class="gc-stat__l">apoyos recibidos</div>
            </div>
        </div>
        <div class="col-lg-3 col-md-6 gc-mb">
            <div class="gc-card gc-card__body">
                <div class="gc-stat__n"><%: TotalComentarios %></div>
                <div class="gc-stat__l">comentarios</div>
            </div>
        </div>
    </div>

    <div class="row">

        <%-- ================================================ Proyectos --%>

        <div class="col-lg-8">
            <div class="gc-card gc-mb">
                <div class="gc-card__head" style="display: flex; align-items: center; justify-content: space-between; gap: 12px;">
                    <h3>Proyectos recientes</h3>
                    <a class="gc-small gc-nowrap" href="<%= ResolveUrl("~/Panel/Proyectos") %>">Ver todos &rarr;</a>
                </div>

                <asp:PlaceHolder ID="phProyectos" runat="server">
                    <div class="gc-tablewrap">
                        <table class="gc-table">
                            <thead>
                                <tr>
                                    <th>Proyecto</th>
                                    <th>Categoría</th>
                                    <th>Estado</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptProyectos" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><strong><%#: Eval("Nombre") %></strong></td>
                                            <td><span class="<%#: Eval("CategoriaClase") %>"><%#: Eval("Categoria") %></span></td>
                                            <td><span class="<%#: Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span></td>
                                            <td style="text-align: right;">
                                                <a class="gc-small gc-nowrap" href="<%#: ResolveUrl("~/Panel/Proyecto?id=" + Eval("Id")) %>">Editar</a>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="phProyectosVacio" runat="server" Visible="false">
                    <div class="gc-empty">
                        <h3>Todavía no registrás proyectos</h3>
                        <p>
                            Los proyectos de campaña son el corazón de tu perfil público. Cada uno indica qué
                            problema resuelve, cuál es su objetivo y a quién beneficia.
                        </p>
                        <a class="gc-btn" href="<%= ResolveUrl("~/Panel/Proyecto") %>">Registrar mi primer proyecto</a>
                    </div>
                </asp:PlaceHolder>
            </div>
        </div>

        <%-- ============================================ Riel derecho --%>

        <div class="col-lg-4">

            <div class="gc-card gc-mb">
                <div class="gc-card__head">
                    <h3>Verificación</h3>
                </div>
                <div class="gc-card__body">
                    <p style="margin-bottom: 10px;">
                        <span class="<%= VerificacionClase %>"><%: VerificacionTexto %></span>
                    </p>
                    <p class="gc-muted gc-small" style="margin: 0;">
                        Lo que registrás se publica identificado como declarado por tu candidatura. Cuando la
                        plataforma asocia fuentes verificables a un contenido, ese contenido pasa a mostrarse
                        como verificado.
                    </p>
                </div>
            </div>

            <div class="gc-card gc-mb">
                <div class="gc-card__head">
                    <h3>Tu campaña</h3>
                </div>
                <div class="gc-card__body">
                    <dl class="gc-datalist">
                        <div>
                            <dt>Campaña</dt>
                            <dd><%: NombreCampana %></dd>
                        </div>
                        <div>
                            <dt>Cargo</dt>
                            <dd><%: Cargo %></dd>
                        </div>
                        <div>
                            <dt>Elección</dt>
                            <dd><%: FechaEleccion %></dd>
                        </div>
                        <div>
                            <dt>Faltan</dt>
                            <dd><%: CuentaRegresiva %></dd>
                        </div>
                    </dl>
                </div>
            </div>

            <div class="gc-card">
                <div class="gc-card__head">
                    <h3>Siguiente paso</h3>
                </div>
                <div class="gc-card__body" style="display: grid; gap: 9px;">
                    <a class="gc-btn gc-btn--ghost gc-btn--block" href="<%= ResolveUrl("~/Panel/Perfil") %>">Completar mi perfil</a>
                    <a class="gc-btn gc-btn--ghost gc-btn--block" href="<%= ResolveUrl("~/Panel/Proyecto") %>">Agregar un proyecto</a>
                    <a class="gc-btn gc-btn--ghost gc-btn--block" href="<%= UrlPerfilPublico %>">Ver cómo me ven</a>
                </div>
            </div>

        </div>
    </div>

</asp:Content>
