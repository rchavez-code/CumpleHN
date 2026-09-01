<%@ Page Title="Mis proyectos" Language="C#" MasterPageFile="~/Panel/Panel.Master" AutoEventWireup="true" CodeBehind="Proyectos.aspx.cs" Inherits="frontend.Panel.Proyectos" %>

<asp:Content ContentPlaceHolderID="TopActions" runat="server">
    <a class="gc-btn gc-btn--sm" href="<%= ResolveUrl("~/Panel/Proyecto") %>">Nuevo proyecto</a>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Mis proyectos de campaña</h2>
            <p class="gc-muted" style="margin: 0;">
                Cada proyecto se publica en tu perfil con su categoría, su objetivo y a quién beneficia.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card gc-filters">
            <div class="gc-filters__grow">
                <asp:TextBox ID="txtBuscar" runat="server" CssClass="gc-input" placeholder="Buscar en mis proyectos" />
            </div>
            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="gc-select" />
            <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar" OnClick="btnFiltrar_Click" />
            <a class="gc-btn gc-btn--quiet" href="<%= ResolveUrl("~/Panel/Proyectos") %>">Limpiar</a>
        </div>

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th>Proyecto</th>
                            <th>Categoría</th>
                            <th>Estado</th>
                            <th>Ubicación</th>
                            <th>Período</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptProyectos" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 240px;">
                                        <strong><%#: Eval("Nombre") %></strong>
                                        <div class="gc-muted gc-small"><%#: Resumir(Eval("Descripcion")) %></div>
                                    </td>
                                    <td><span class="<%#: Eval("CategoriaClase") %>"><%#: Eval("Categoria") %></span></td>
                                    <td><span class="<%#: Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span></td>
                                    <td class="gc-muted"><%#: Eval("UbicacionTexto") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("PeriodoEjecucion") %></td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <a class="gc-small" href="<%#: ResolveUrl("~/Panel/Proyecto?id=" + Eval("Id")) %>">Editar</a>
                                        <span class="gc-faint" aria-hidden="true"> · </span>
                                        <a class="gc-small" href="<%#: ResolveUrl("~/Propuesta?id=" + Eval("Id")) %>">Ver</a>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <div class="gc-card gc-empty">
            <h3><%: TituloVacio %></h3>
            <p><%: TextoVacio %></p>
            <a class="gc-btn" href="<%= ResolveUrl("~/Panel/Proyecto") %>">Registrar un proyecto</a>
        </div>
    </asp:PlaceHolder>

    <div class="gc-note" style="margin-top: 20px;">
        <span>
            El estado de cumplimiento de un proyecto no lo define la candidatura. Vos registrás el compromiso
            y su avance declarado. La plataforma asigna los estados de cumplimiento cuando existe evidencia
            documentada que los respalde.
        </span>
    </div>

</asp:Content>
