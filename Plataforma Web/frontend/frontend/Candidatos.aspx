<%@ Page Title="Candidatos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Candidatos.aspx.cs" Inherits="frontend.CandidatosPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <span>Candidatos</span>
            </nav>

            <h1>Candidatos</h1>
            <p class="gc-lead" style="max-width: 66ch;">
                Perfiles de las candidaturas registradas en la plataforma. Cada perfil reúne la información
                personal y profesional, las propuestas de campaña y las publicaciones que la propia
                candidatura declara.
            </p>
            <div style="height: 26px;"></div>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">

        <div class="gc-card gc-filters">
            <div class="gc-filters__grow">
                <asp:TextBox ID="txtBuscar" runat="server" CssClass="gc-input" placeholder="Buscar por nombre o cargo" />
            </div>

            <asp:DropDownList ID="ddlCampana" runat="server" CssClass="gc-select" />
            <asp:DropDownList ID="ddlNivel" runat="server" CssClass="gc-select" />

            <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar" OnClick="btnFiltrar_Click" />
            <a class="gc-btn gc-btn--quiet" href="<%= ResolveUrl("~/Candidatos") %>">Limpiar</a>
        </div>

        <div class="gc-sechead">
            <div>
                <h2><%: TotalTexto %></h2>
                <p class="gc-muted"><%: DescripcionFiltro %></p>
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

        <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
            <div class="gc-card gc-empty">
                <h3>No se encontraron candidaturas</h3>
                <p>Probá con otro nombre o quitá los filtros aplicados para ver todos los perfiles registrados.</p>
                <a class="gc-btn gc-btn--ghost" href="<%= ResolveUrl("~/Candidatos") %>">Quitar filtros</a>
            </div>
        </asp:PlaceHolder>

    </div>

</asp:Content>
