<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IniciativaCard.ascx.cs" Inherits="frontend.Controles.IniciativaCard" %>

<article class="<%= ClaseTarjeta %>">

    <div class="gc-post__head">
        <span class="gc-avatar" aria-hidden="true"><%: Item.AutoraIniciales %></span>

        <div class="gc-post__who">
            <span class="gc-post__name"><%: Item.Autora %></span>
            <div class="gc-post__sub">
                <span>Cuenta ciudadana</span>
                <span aria-hidden="true">·</span>
                <span><%: Item.TiempoRelativo %></span>
                <% if (Item.FueEditada)
                   { %>
                <span aria-hidden="true">·</span>
                <span>editada</span>
                <% } %>
            </div>
        </div>

        <%-- Ocupa el lugar del nivel de verificación de las otras tarjetas, y
             dice lo único que la plataforma puede afirmar de este texto: quién
             lo escribió. No es una promesa de nadie y no hay nada que
             verificar. --%>
        <span class="gc-verif" title="Escrita por una persona con cuenta ciudadana. La plataforma no la respalda ni la verifica.">Propuesta ciudadana</span>
    </div>

    <h3 class="gc-inic__titulo"><%: Item.Titulo %></h3>

    <p class="gc-post__text gc-inic__texto"><%: Item.Descripcion %></p>

    <div class="gc-cand__tags" style="margin-top: 13px;">
        <span class="<%= Item.CategoriaClase %>"><%: Item.Categoria %></span>
        <% if (Item.TieneDepartamento)
           { %>
        <span class="gc-chip"><%: Item.Departamento %></span>
        <% } %>
    </div>

    <asp:PlaceHolder ID="phRetirada" runat="server" Visible="false">
        <p class="gc-inic__retirada">
            Retirada. <asp:Literal ID="litMotivo" runat="server" />
        </p>
    </asp:PlaceHolder>

    <%-- ======================================= Participación --%>

    <div class="gc-post__foot">
        <gc:Interaccion ID="inter" runat="server" />
    </div>

    <asp:PlaceHolder ID="phComentarios" runat="server" Visible="false">
        <gc:Comentarios ID="coments" runat="server" />
    </asp:PlaceHolder>

</article>
