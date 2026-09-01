<%@ Page Title="Propuestas y proyectos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Propuestas.aspx.cs" Inherits="frontend.PropuestasPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <span>Propuestas</span>
            </nav>

            <h1>Propuestas y proyectos</h1>
            <p class="gc-lead" style="max-width: 68ch;">
                Todos los compromisos registrados en la plataforma, clasificados por área temática. La
                clasificación común es lo que permite comparar candidaturas, partidos y períodos sobre la
                misma base.
            </p>
            <div style="height: 26px;"></div>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">

        <div class="gc-card gc-filters">
            <div class="gc-filters__grow">
                <asp:TextBox ID="txtBuscar" runat="server" CssClass="gc-input" placeholder="Buscar en propuestas" />
            </div>

            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="gc-select" />
            <asp:DropDownList ID="ddlCampana" runat="server" CssClass="gc-select" />

            <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar" OnClick="btnFiltrar_Click" />
            <a class="gc-btn gc-btn--quiet" href="<%= ResolveUrl("~/Propuestas") %>">Limpiar</a>
        </div>

        <div class="gc-sechead">
            <div>
                <h2><%: TotalTexto %></h2>
                <p class="gc-muted"><%: DescripcionFiltro %></p>
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

        <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
            <div class="gc-card gc-empty">
                <h3>No se encontraron propuestas</h3>
                <p>Probá con otros términos o quitá los filtros para ver todos los compromisos registrados.</p>
                <a class="gc-btn gc-btn--ghost" href="<%= ResolveUrl("~/Propuestas") %>">Quitar filtros</a>
            </div>
        </asp:PlaceHolder>

    </div>

</asp:Content>
