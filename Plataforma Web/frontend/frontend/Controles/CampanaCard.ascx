<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CampanaCard.ascx.cs" Inherits="frontend.Controles.CampanaCard" %>

<a class="gc-card <%= Item.ClaseTarjeta %>" href="<%= Url %>">

    <div class="gc-campana__cover" aria-hidden="true"></div>

    <div class="gc-campana__body">
        <p class="gc-eyebrow"><%: Item.EstadoTexto %></p>
        <h3><%: Item.Nombre %></h3>
        <p class="gc-muted gc-small" style="margin: 0;"><%: Item.Resumen %></p>

        <div class="gc-campana__meta">
            <span>Elección: <%: FechaEleccion %></span>
            <span><%: TextoCandidatos %></span>
            <span><%: TextoPropuestas %></span>
        </div>
    </div>

</a>
