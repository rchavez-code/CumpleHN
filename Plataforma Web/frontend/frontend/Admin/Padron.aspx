<%@ Page Title="Padrón" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Padron.aspx.cs" Inherits="frontend.Admin.PadronPagina" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Padrón de <%: Sesion.EspacioNombre %></h2>
            <p class="gc-muted" style="margin: 0;">
                Quién puede participar en este espacio: valorar, comentar y responder encuestas. Se
                carga por correo, antes o después de que cada persona tenga cuenta, y la pertenencia se
                resuelve al participar cruzando el correo del padrón con el de la cuenta.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <%-- ============================================ Padrón abierto --%>

    <asp:PlaceHolder ID="phAbierto" runat="server" Visible="false">
        <div class="gc-note gc-mb">
            <span>
                Este espacio tiene el <strong>padrón abierto</strong>: participa cualquier cuenta con correo
                confirmado, como en el sitio público de CumpleHN. Para restringir la participación a una
                lista de miembros hay que cerrar el padrón en la ficha del espacio.
            </span>
        </div>
    </asp:PlaceHolder>

    <%-- ================================================== Carga --%>

    <asp:PlaceHolder ID="phCarga" runat="server">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3>Agregar correos al padrón</h3>
            </div>
            <div class="gc-card__body">

                <div class="gc-field">
                    <label for="<%= txtCorreos.ClientID %>">Un correo por línea</label>
                    <asp:TextBox ID="txtCorreos" runat="server" CssClass="gc-input"
                                 TextMode="MultiLine" Rows="7"
                                 placeholder="maria.perez@ejemplo.hn&#10;jose.lopez@ejemplo.hn" />
                    <span class="gc-muted gc-small">
                        Podés pegar la columna de correos de una hoja de cálculo. Las mayúsculas, los espacios y
                        los repetidos no importan. Los que ya estaban se conservan, y los que se habían quitado
                        vuelven al padrón.
                    </span>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnCargar" runat="server" CssClass="gc-btn" Text="Agregar al padrón"
                                OnClick="btnCargar_Click" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ================================================== Lista --%>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Correo</th>
                            <th scope="col">Cuenta</th>
                            <th scope="col">Estado</th>
                            <th scope="col">Cargado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptMiembros" runat="server" OnItemCommand="rptMiembros_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 220px;"><code><%#: Eval("Correo") %></code></td>
                                    <td class="gc-muted"><%#: string.IsNullOrEmpty((string)Eval("Nombre")) ? "—" : Eval("Nombre") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span>
                                    </td>
                                    <td class="gc-muted gc-nowrap gc-small"><%#: ((DateTime)Eval("FechaAlta")).ToString("d MMM yyyy") %></td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small"
                                                        Text='<%# (bool)Eval("Activo") ? "Quitar" : "Restaurar" %>'
                                                        CommandName="estado" CommandArgument='<%# Eval("Codigo") %>' />
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>

        <p class="gc-muted gc-small" style="margin-top: 12px;">
            El padrón dice que un buzón está en la lista, no quién lo controla. Quitar un correo no borra
            lo que esa persona ya votó: solo impide participar de nuevo.
        </p>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phVacio" runat="server" Visible="false">
        <div class="gc-card gc-empty">
            <h3>El padrón está vacío</h3>
            <p>Mientras no haya correos cargados, nadie puede participar en este espacio.</p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
