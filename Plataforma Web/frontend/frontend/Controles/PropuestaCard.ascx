<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PropuestaCard.ascx.cs" Inherits="frontend.Controles.PropuestaCard" %>

<div class="gc-card gc-prop gc-card--int">

    <a href="<%= Url %>" class="gc-prop__media" aria-hidden="true" tabindex="-1"></a>

    <div class="gc-prop__body">
        <div class="gc-prop__tags">
            <span class="<%= Item.CategoriaClase %>"><%: Item.Categoria %></span>
            <span class="<%= Item.EstadoClase %>"><%: Item.EstadoTexto %></span>
        </div>

        <h3><a href="<%= Url %>" style="color: inherit;"><%: Item.Nombre %></a></h3>
        <p class="gc-prop__desc"><%: Item.Descripcion %></p>
    </div>

    <div class="gc-prop__meta">
        <span><%: Item.UbicacionTexto %></span>
        <span aria-hidden="true">·</span>
        <span><%: Item.PeriodoEjecucion %></span>
    </div>

    <div class="gc-card__inter">
        <gc:Interaccion ID="inter" runat="server" MostrarApoyo="false" />
    </div>

</div>
