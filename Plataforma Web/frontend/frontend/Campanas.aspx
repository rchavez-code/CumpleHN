<%@ Page Title="Campañas electorales" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Campanas.aspx.cs" Inherits="frontend.Campanas" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <span>Campañas electorales</span>
            </nav>

            <h1>Campañas electorales</h1>
            <p class="gc-lead" style="max-width: 66ch;">
                Cada campaña agrupa las candidaturas que compiten en ella, sus propuestas y sus publicaciones.
                En las campañas cerradas la consulta se centra en el cumplimiento de lo que se prometió.
            </p>
            <div style="height: 26px;"></div>
        </div>
    </section>

    <div class="container" style="padding-top: 30px;">

        <asp:Repeater ID="rptActivas" runat="server">
            <HeaderTemplate>
                <div class="gc-sechead">
                    <div>
                        <h2>En curso</h2>
                        <p class="gc-muted">Abiertas para consulta y registro de candidaturas.</p>
                    </div>
                </div>
                <div class="row">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-lg-4 col-md-6 gc-mb">
                    <gc:CampanaCard runat="server" Item="<%# (Campana)Container.DataItem %>" />
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Repeater ID="rptProximas" runat="server">
            <HeaderTemplate>
                <div class="gc-sechead" style="margin-top: 38px;">
                    <div>
                        <h2>Próximas</h2>
                        <p class="gc-muted">El registro abre con la convocatoria oficial de cada proceso.</p>
                    </div>
                </div>
                <div class="row">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-lg-4 col-md-6 gc-mb">
                    <gc:CampanaCard runat="server" Item="<%# (Campana)Container.DataItem %>" />
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:Repeater ID="rptCerradas" runat="server">
            <HeaderTemplate>
                <div class="gc-sechead" style="margin-top: 38px;">
                    <div>
                        <h2>Cerradas</h2>
                        <p class="gc-muted">Ciclos concluidos, disponibles para contrastar propuestas con resultados.</p>
                    </div>
                </div>
                <div class="row">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-lg-4 col-md-6 gc-mb">
                    <gc:CampanaCard runat="server" Item="<%# (Campana)Container.DataItem %>" />
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
            <div class="gc-card gc-empty">
                <h3>Todavía no hay campañas registradas</h3>
                <p>Cuando se registre una campaña electoral aparecerá acá con sus candidaturas y propuestas.</p>
            </div>
        </asp:PlaceHolder>

    </div>

</asp:Content>
