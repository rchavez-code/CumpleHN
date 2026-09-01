<%@ Page Title="Partido" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Partido.aspx.cs" Inherits="frontend.PartidoPagina" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <section class="gc-chead">
        <div class="container">
            <nav class="gc-crumbs" aria-label="Ruta de navegación">
                <a href="<%= ResolveUrl("~/") %>">Inicio</a>
                <span aria-hidden="true">/</span>
                <a href="<%= ResolveUrl("~/Partidos") %>">Partidos</a>
                <span aria-hidden="true">/</span>
                <span><%: Item.Nombre %></span>
            </nav>

            <div class="gc-chead__top">
                <div style="display: flex; align-items: flex-start; gap: 18px;">
                    <span class="gc-avatar gc-avatar--lg" aria-hidden="true"><%: Item.SiglasONombre %></span>
                    <div>
                        <p class="gc-eyebrow">Partido político</p>
                        <h1 style="margin-bottom: .25em;"><%: Item.Nombre %></h1>
                        <p class="gc-muted" style="margin: 0;">
                            <b><%: Item.TotalCandidatos %></b> <%: TextoCandidatos %>
                            <span class="gc-faint" aria-hidden="true">·</span>
                            <b><%: Item.TotalPropuestas %></b> <%: TextoPropuestas %>
                        </p>
                    </div>
                </div>
            </div>

            <div class="gc-card__inter" style="border: 0; padding: 16px 0 14px;">
                <gc:Interaccion ID="interPartido" runat="server" />
            </div>
            <div style="height: 12px;"></div>
        </div>
    </section>

    <div class="container" style="padding-top: 26px;">
        <div class="gc-layout">

            <div>
                <div class="gc-sechead">
                    <div>
                        <h2>Candidaturas del partido</h2>
                        <p class="gc-muted">Perfiles registrados en la plataforma bajo este partido.</p>
                    </div>
                </div>

                <div class="row">
                    <asp:Repeater ID="rptCandidatos" runat="server">
                        <ItemTemplate>
                            <div class="col-lg-6 gc-mb">
                                <gc:CandidatoCard runat="server" Item="<%# (Candidato)Container.DataItem %>" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:PlaceHolder ID="phSinCandidatos" runat="server" Visible="false">
                    <div class="gc-card gc-empty">
                        <h3>Este partido todavía no tiene candidaturas registradas</h3>
                        <p>Los perfiles aparecen acá cuando una candidatura afiliada se registra en la plataforma.</p>
                    </div>
                </asp:PlaceHolder>

                <div class="gc-card gc-mb" style="margin-top: 8px;">
                    <div class="gc-card__head">
                        <h3>Opiniones de la ciudadanía</h3>
                    </div>
                    <div class="gc-card__body">
                        <gc:Comentarios ID="comentsPartido" runat="server" />
                    </div>
                </div>
            </div>

            <aside class="gc-rail">
                <div class="gc-card gc-mb">
                    <div class="gc-card__head">
                        <h3>Ficha</h3>
                    </div>
                    <div class="gc-card__body">
                        <dl class="gc-datalist">
                            <div>
                                <dt>Nombre</dt>
                                <dd><%: Item.Nombre %></dd>
                            </div>
                            <div>
                                <dt>Siglas</dt>
                                <dd><%: SiglasTexto %></dd>
                            </div>
                            <div>
                                <dt>Candidaturas</dt>
                                <dd><%: Item.TotalCandidatos %></dd>
                            </div>
                            <div>
                                <dt>Propuestas</dt>
                                <dd><%: Item.TotalPropuestas %></dd>
                            </div>
                        </dl>
                    </div>
                </div>

                <div class="gc-note">
                    <span>
                        CumpleHN no califica a los partidos políticos. Las valoraciones y los
                        comentarios de esta página expresan la opinión de quien los emite, y no
                        constituyen una evaluación de la plataforma.
                    </span>
                </div>
            </aside>

        </div>
    </div>

</asp:Content>
