<%@ Page Title="Mis publicaciones" Language="C#" MasterPageFile="~/Panel/Panel.Master" AutoEventWireup="true" CodeBehind="Publicaciones.aspx.cs" Inherits="frontend.Panel.Publicaciones" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Mis publicaciones</h2>
            <p class="gc-muted" style="margin: 0;">
                Actualizaciones que aparecen en el feed de la campaña y en tu perfil público.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">
        <div class="gc-feed">
            <asp:Repeater ID="rptPublicaciones" runat="server">
                <ItemTemplate>
                    <gc:PublicacionCard runat="server" Item="<%# (Publicacion)Container.DataItem %>" />
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <div class="gc-card gc-empty">
            <h3>Todavía no tenés publicaciones</h3>
            <p>Las publicaciones te permiten informar avances y responder a la ciudadanía durante la campaña.</p>
        </div>
    </asp:PlaceHolder>

    <div class="gc-note" style="margin-top: 20px;">
        <span>
            La redacción y edición de publicaciones se habilita en la siguiente etapa del proyecto, junto con
            los comentarios y las votaciones de percepción. Acá ya se muestran las publicaciones existentes tal
            como las ve la ciudadanía.
        </span>
    </div>

</asp:Content>
