<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Interaccion.ascx.cs" Inherits="frontend.Controles.Interaccion" %>

<div class="gc-inter">

    <asp:LinkButton ID="lnkMeGusta" runat="server" CssClass="gc-act gc-act--si"
        OnClick="lnkMeGusta_Click" ToolTip="Me gusta">
        <span class="gc-act__ico gc-act__ico--si" aria-hidden="true"></span>
        Me gusta <span class="gc-act__n"><%: TextoMeGusta %></span>
    </asp:LinkButton>

    <asp:LinkButton ID="lnkNoMeGusta" runat="server" CssClass="gc-act gc-act--no"
        OnClick="lnkNoMeGusta_Click" ToolTip="No me gusta">
        <span class="gc-act__ico gc-act__ico--no" aria-hidden="true"></span>
        No me gusta <span class="gc-act__n"><%: TextoNoMeGusta %></span>
    </asp:LinkButton>

    <asp:LinkButton ID="lnkComentar" runat="server" CssClass="gc-act"
        OnClick="lnkComentar_Click" ToolTip="Ver comentarios">
        <span class="gc-act__ico gc-act__ico--com" aria-hidden="true"></span>
        Comentar <span class="gc-act__n"><%: TextoComentarios %></span>
    </asp:LinkButton>

    <asp:HyperLink ID="lnkComentarEnlace" runat="server" CssClass="gc-act" Visible="false">
        <span class="gc-act__ico gc-act__ico--com" aria-hidden="true"></span>
        Comentar <span class="gc-act__n"><%: TextoComentarios %></span>
    </asp:HyperLink>

    <asp:PlaceHolder ID="phApoyo" runat="server">
        <span class="gc-apoyo" title="Porcentaje de apoyo sobre el total de valoraciones">
            <span class="gc-apoyo__barra"><span class="gc-apoyo__fill" style="<%= EstiloApoyo %>"></span></span>
            <span class="gc-apoyo__n"><%: TextoApoyo %></span>
        </span>
    </asp:PlaceHolder>

</div>

<asp:PlaceHolder ID="phAviso" runat="server" Visible="false">
    <p class="gc-alert" style="margin: 10px 0 0;"><asp:Literal ID="litAviso" runat="server" /></p>
</asp:PlaceHolder>
