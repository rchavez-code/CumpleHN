<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Comentarios.ascx.cs" Inherits="frontend.Controles.Comentarios" %>

<div class="gc-coments">

    <%-- ================================== Escribir un comentario --%>

    <asp:PlaceHolder ID="phFormulario" runat="server">
        <div class="gc-coment-form">
            <span class="gc-avatar gc-avatar--sm" aria-hidden="true"><%: Iniciales %></span>
            <div style="flex: 1 1 auto; min-width: 0;">
                <asp:TextBox ID="txtComentario" runat="server" CssClass="gc-textarea" TextMode="MultiLine"
                    Rows="2" placeholder="Escribí tu comentario" />
                <div class="gc-coment-form__pie">
                    <span class="gc-muted gc-small">Tu comentario se publica con tu nombre.</span>
                    <asp:Button ID="btnPublicar" runat="server" CssClass="gc-btn gc-btn--sm"
                        Text="Publicar" OnClick="btnPublicar_Click" />
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <%-- ============================ Invitación a crear una cuenta --%>

    <%-- La participación quedó cerrada por la administración. El hilo se sigue
         leyendo: cerrar la participación no borra lo que ya se dijo. --%>
    <asp:PlaceHolder ID="phCerrado" runat="server" Visible="false">
        <div class="gc-gate">
            <span>La participación está temporalmente cerrada. Los comentarios registrados siguen visibles.</span>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phInvitacion" runat="server" Visible="false">
        <div class="gc-gate">
            <span>Para comentar necesitás una cuenta. Leer los comentarios es libre.</span>
            <a class="gc-btn gc-btn--ghost gc-btn--sm gc-nowrap" href="<%= UrlAcceso %>">Acceder</a>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <p class="gc-alert" style="margin-top: 12px;"><asp:Literal ID="litMensaje" runat="server" /></p>
    </asp:PlaceHolder>

    <%-- ================================================ El hilo --%>

    <asp:Repeater ID="rptComentarios" runat="server">
        <HeaderTemplate>
            <div class="gc-coment-lista">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="gc-coment">
                <span class="gc-avatar gc-avatar--sm" aria-hidden="true"><%#: Eval("Iniciales") %></span>
                <div class="gc-coment__cuerpo">
                    <div class="gc-coment__meta">
                        <strong><%#: Eval("Autor") %></strong>
                        <%# EtiquetaRol(Container.DataItem) %>
                        <span class="gc-faint" aria-hidden="true">·</span>
                        <span class="gc-muted"><%#: Eval("TiempoRelativo") %></span>
                    </div>
                    <p class="gc-coment__texto"><%#: Eval("Texto") %></p>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <p class="gc-muted gc-small" style="margin: 14px 0 0;">
            Todavía no hay comentarios. Podés ser la primera persona en opinar.
        </p>
    </asp:PlaceHolder>

</div>
