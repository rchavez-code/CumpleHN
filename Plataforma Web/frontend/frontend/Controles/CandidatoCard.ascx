<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CandidatoCard.ascx.cs" Inherits="frontend.Controles.CandidatoCard" %>

<div class="gc-card gc-cand gc-card--int">

    <div class="gc-cand__body">
        <a href="<%= Url %>" aria-label="Perfil de <%: Item.NombreCompleto %>">
            <span class="gc-avatar gc-avatar--lg" style="<%= EstiloAvatar %>" aria-hidden="true"><%: Iniciales %></span>
        </a>

        <div class="gc-cand__info">
            <h3><a href="<%= Url %>" style="color: inherit;"><%: Item.NombreCompleto %></a></h3>
            <p class="gc-cand__cargo"><%: Item.Cargo %> · <%: Item.Territorio %></p>

            <div class="gc-cand__tags">
                <% if (Item.TienePartido)
                   { %>
                <a class="gc-chip" href="<%= UrlPartido %>"><%: Item.PartidoTexto %></a>
                <% }
                   else
                   { %>
                <span class="gc-chip"><%: Item.PartidoTexto %></span>
                <% } %>
                <span class="<%= Item.VerificacionClase %>"><%: Item.VerificacionTexto %></span>
            </div>

            <p class="gc-muted gc-small" style="margin: 10px 0 0;">
                <b><%: Item.TotalPropuestas %></b> <%: TextoPropuestas %>
                <span class="gc-faint" aria-hidden="true">·</span>
                <b><%: Item.TotalPublicaciones %></b> <%: TextoPublicaciones %>
            </p>
        </div>
    </div>

    <div class="gc-card__inter">
        <gc:Interaccion ID="inter" runat="server" MostrarApoyo="false" />
    </div>

</div>
