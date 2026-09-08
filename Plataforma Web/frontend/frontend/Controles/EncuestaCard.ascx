<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EncuestaCard.ascx.cs" Inherits="frontend.Controles.EncuestaCard" %>

<section class="gc-card gc-enc <%= ClaseOculta %>" aria-labelledby="<%= ClientID %>_p">

    <div class="gc-enc__head">
        <p class="gc-eyebrow">Encuesta de la plataforma</p>

        <h3 class="gc-enc__q" id="<%= ClientID %>_p"><%: Item.Pregunta %></h3>

        <% if (Item.TieneDescripcion)
           { %>
        <p class="gc-enc__d"><%: Item.Descripcion %></p>
        <% } %>

        <div class="gc-enc__meta">
            <% if (Item.TieneCategoria)
               { %>
            <span class="<%= Item.CategoriaClase %>"><%: Item.Categoria %></span>
            <% } %>
            <span class="gc-muted gc-small"><%: Item.TotalTexto %></span>
            <% if (!string.IsNullOrEmpty(Item.CierreTexto))
               { %>
            <span class="gc-faint" aria-hidden="true">·</span>
            <span class="gc-muted gc-small"><%: Item.CierreTexto %></span>
            <% } %>
        </div>
    </div>

    <%-- ================================================== Opciones --%>

    <asp:Repeater ID="rptOpciones" runat="server" OnItemCommand="rptOpciones_ItemCommand">
        <HeaderTemplate>
            <ul class="gc-enc__lista">
        </HeaderTemplate>
        <ItemTemplate>
            <li>
                <asp:LinkButton runat="server" CssClass='<%# ClaseOpcion(Container.DataItem) %>'
                                CommandName="votar" CommandArgument='<%# Eval("Id") %>'
                                Enabled='<%# PuedeResponder %>'>
                    <span class="gc-enc__pista" style='<%# EstiloPista(Container.DataItem) %>' aria-hidden="true"></span>
                    <span class="gc-enc__marca" aria-hidden="true"></span>
                    <span class="gc-enc__txt"><%#: Eval("Texto") %></span>
                    <span class="gc-enc__n"><%# TextoResultado(Container.DataItem) %></span>
                </asp:LinkButton>
            </li>
        </ItemTemplate>
        <FooterTemplate>
            </ul>
        </FooterTemplate>
    </asp:Repeater>

    <%-- ==================================================== Pie --%>

    <%-- El aviso es el propio párrafo con runat, y no un contenedor alrededor,
         porque la clase cambia según el resultado y un PlaceHolder no lleva
         atributos al marcado. --%>
    <p id="phAviso" runat="server" visible="false" class="gc-ok gc-enc__aviso" role="status">
        <asp:Literal ID="litAviso" runat="server" />
    </p>

    <p class="gc-enc__pie">
        <asp:Literal ID="litPie" runat="server" />
    </p>

</section>
