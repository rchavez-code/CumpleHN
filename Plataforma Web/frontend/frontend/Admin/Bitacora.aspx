<%@ Page Title="Bitácora" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Bitacora.aspx.cs" Inherits="frontend.Admin.Bitacora" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Bitácora de administración</h2>
            <p class="gc-muted" style="margin: 0;">
                Cada verificación y cada decisión de moderación, con quién la tomó y con qué motivo.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <div class="gc-card gc-filters">
        <asp:DropDownList ID="ddlAccion" runat="server" CssClass="gc-select" />
        <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar" OnClick="btnFiltrar_Click" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Fecha</th>
                            <th scope="col">Quién</th>
                            <th scope="col">Acción</th>
                            <th scope="col">Contenido</th>
                            <th scope="col">Motivo registrado</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptBitacora" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("FechaTexto") %></td>
                                    <td><%#: Eval("Usuario") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("AccionClase") %>"><%#: Eval("AccionTexto") %></span>
                                    </td>
                                    <td>
                                        <%#: Eval("TipoObjeto") %> #<%#: Eval("CodigoObjeto") %>
                                        <div class="gc-muted gc-small"><%#: Eval("Detalle") %></div>
                                    </td>
                                    <td style="min-width: 240px;"><%#: Eval("Motivo") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <div class="gc-card gc-empty">
            <h3>La bitácora está vacía</h3>
            <p>
                Todavía no se ha registrado ninguna acción de administración. Cada verificación y cada
                retiro de contenido aparecerá acá con su motivo.
            </p>
        </div>
    </asp:PlaceHolder>

    <div class="gc-note" style="margin-top: 18px;">
        <span>
            La bitácora no se puede editar ni borrar desde la plataforma. Una plataforma que le pide
            cuentas a otros tiene que poder responder por lo que hace su propio administrador.
        </span>
    </div>

</asp:Content>
