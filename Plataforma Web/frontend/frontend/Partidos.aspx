<%@ Page Title="Partidos políticos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Partidos.aspx.cs" Inherits="frontend.PartidosPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <span>Partidos políticos</span>
            </nav>

            <h1>Partidos políticos</h1>
            <p class="gc-lead" style="max-width: 68ch;">
                Partidos con candidaturas registradas en la plataforma. Cada uno reúne a sus
                candidaturas y las propuestas que estas han documentado.
            </p>
            <div style="height: 26px;"></div>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">

        <div class="row">
            <asp:Repeater ID="rptPartidos" runat="server" OnItemDataBound="rptPartidos_ItemDataBound">
                <ItemTemplate>
                    <div class="col-lg-6 gc-mb">
                        <div class="gc-card gc-card--int">
                            <div class="gc-card__body">
                                <div style="display: flex; align-items: flex-start; gap: 14px;">
                                    <span class="gc-avatar gc-avatar--lg" aria-hidden="true"><%#: Eval("SiglasONombre") %></span>
                                    <div style="min-width: 0; flex: 1 1 auto;">
                                        <h3 style="margin-bottom: 4px;">
                                            <a href="<%#: ResolveUrl("~/Partido?id=" + Eval("Slug")) %>" style="color: inherit;"><%#: Eval("Nombre") %></a>
                                        </h3>
                                        <p class="gc-muted gc-small" style="margin: 0;">
                                            <b><%#: Eval("TotalCandidatos") %></b> candidaturas
                                            <span class="gc-faint" aria-hidden="true">·</span>
                                            <b><%#: Eval("TotalPropuestas") %></b> propuestas
                                        </p>
                                    </div>
                                </div>
                            </div>
                            <div class="gc-card__inter">
                                <gc:Interaccion ID="inter" runat="server" MostrarApoyo="false" />
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
            <div class="gc-card gc-empty">
                <h3>Todavía no hay partidos registrados</h3>
                <p>Los partidos aparecen acá cuando una candidatura afiliada registra su perfil.</p>
            </div>
        </asp:PlaceHolder>

        <p class="gc-neutral">
            Las valoraciones sobre un partido expresan la opinión de quien las emite. CumpleHN no
            califica a los partidos políticos ni establece comparaciones entre ellos.
        </p>

    </div>

</asp:Content>
