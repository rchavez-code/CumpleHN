<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PublicacionCard.ascx.cs" Inherits="frontend.Controles.PublicacionCard" %>

<article class="gc-card gc-post">

    <div class="gc-post__head">
        <a href="<%= UrlCandidato %>" aria-label="Perfil de <%: Item.CandidatoNombre %>">
            <span class="gc-avatar" style="<%= EstiloAvatar %>" aria-hidden="true"><%: Iniciales %></span>
        </a>

        <div class="gc-post__who">
            <a class="gc-post__name" href="<%= UrlCandidato %>"><%: Item.CandidatoNombre %></a>
            <div class="gc-post__sub">
                <span><%: Item.CandidatoCargo %></span>
                <span aria-hidden="true">·</span>
                <span><%: Item.TiempoRelativo %></span>
            </div>
        </div>

        <span class="<%= Item.VerificacionClase %>" title="<%: Item.VerificacionTexto %>"><%: Item.VerificacionTexto %></span>
    </div>

    <p class="gc-post__text"><%: Item.Texto %></p>

    <% if (Item.TieneImagen)
       { %>
    <div class="gc-post__media" role="img" aria-label="Material adjunto de la publicación"></div>
    <% } %>

    <div class="gc-cand__tags" style="margin-top: 13px;">
        <% if (Item.TienePropuesta)
           { %>
        <a class="gc-chip" href="<%= UrlPropuesta %>">
            <span class="gc-chip__dot" aria-hidden="true"></span>
            <%: Item.PropuestaNombre %>
        </a>
        <% } %>
        <% if (!string.IsNullOrEmpty(Item.Categoria))
           { %>
        <span class="<%= Item.CategoriaClase %>"><%: Item.Categoria %></span>
        <% } %>
    </div>

    <%-- ======================================= Participación --%>

    <div class="gc-post__foot">
        <gc:Interaccion ID="inter" runat="server" />
    </div>

    <%-- Hilo de comentarios, desplegable desde el botón de comentar --%>
    <asp:PlaceHolder ID="phComentarios" runat="server" Visible="false">
        <gc:Comentarios ID="coments" runat="server" />
    </asp:PlaceHolder>

</article>
