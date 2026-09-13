<%@ Page Title="Cargos" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Cargos.aspx.cs" Inherits="frontend.Admin.CargosPagina" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Cargos de <%: Sesion.EspacioNombre %></h2>
            <p class="gc-muted" style="margin: 0;">
                A qué se presenta cada candidatura: presidencia, tesorería, vocalías, lo que este espacio
                elija. Son propios del espacio, y el alta de candidaturas solo ofrece los que estén
                disponibles. No se borran: se desactivan, y las candidaturas que ya lo tienen no cambian.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <%-- ================================================ Formulario --%>

    <asp:PlaceHolder ID="phForm" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloForm %></h3>
            </div>
            <div class="gc-card__body">

                <div class="gc-form">
                    <div class="gc-field">
                        <label for="<%= txtNombre.ClientID %>">Nombre del cargo</label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="gc-input" MaxLength="80" placeholder="Presidencia, Secretaría, Vocalía 1" />
                    </div>

                    <div class="gc-row2">
                        <div class="gc-field">
                            <label for="<%= ddlNivel.ClientID %>">Nivel</label>
                            <asp:DropDownList ID="ddlNivel" runat="server" CssClass="gc-input gc-select">
                                <asp:ListItem Text="Institucional" Value="Institucional" />
                                <asp:ListItem Text="Nacional" Value="Nacional" />
                                <asp:ListItem Text="Departamental" Value="Departamental" />
                                <asp:ListItem Text="Municipal" Value="Municipal" />
                            </asp:DropDownList>
                            <span class="gc-muted gc-small">
                                Para una organización es «Institucional». Los otros tres son los niveles de
                                gobierno del sitio público, y el tablero agrupa por ellos.
                            </span>
                        </div>
                        <div class="gc-field">
                            <label for="<%= txtOrden.ClientID %>">Orden</label>
                            <asp:TextBox ID="txtOrden" runat="server" CssClass="gc-input" TextMode="Number" min="0" />
                            <span class="gc-muted gc-small">En qué posición aparece en las listas. Cero va primero.</span>
                        </div>
                    </div>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn" Text="Guardar" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ============================================== Baja / alta --%>

    <asp:PlaceHolder ID="phEstado" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloEstado %></h3>
            </div>
            <div class="gc-card__body">
                <p class="gc-muted" style="margin-top: 0;"><%: TextoEstado %></p>
                <div class="gc-field">
                    <label for="<%= txtMotivo.ClientID %>">Motivo</label>
                    <asp:TextBox ID="txtMotivo" runat="server" CssClass="gc-input" TextMode="MultiLine" Rows="2" />
                    <span class="gc-muted gc-small">Queda en la bitácora junto a tu nombre.</span>
                </div>
                <div class="gc-formfoot">
                    <asp:Button ID="btnConfirmarEstado" runat="server" CssClass="gc-btn" Text="Confirmar" OnClick="btnConfirmarEstado_Click" />
                    <asp:Button ID="btnCancelarEstado" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

    </asp:PlaceHolder>

    <%-- =================================================== Lista --%>

    <div class="gc-card gc-filters">
        <asp:Button ID="btnNuevo" runat="server" CssClass="gc-btn" Text="Registrar cargo" OnClick="btnNuevo_Click" CausesValidation="false" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">
        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Cargo</th>
                            <th scope="col">Nivel</th>
                            <th scope="col">Orden</th>
                            <th scope="col">Candidaturas</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCargos" runat="server" OnItemCommand="rptCargos_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 220px;"><strong><%#: Eval("Nombre") %></strong></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("NivelGobierno") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Orden") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Candidaturas") %></td>
                                    <td class="gc-nowrap"><span class="<%# Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span></td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Editar"
                                                        CommandName="editar" CommandArgument='<%# Eval("Codigo") %>' />
                                        <span class="gc-faint" aria-hidden="true"> · </span>
                                        <asp:LinkButton runat="server" CssClass="gc-small"
                                                        Text='<%# (bool)Eval("Activo") ? "Desactivar" : "Reactivar" %>'
                                                        CommandName="estado" CommandArgument='<%# Eval("Codigo") %>' />
                                    </td>
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
            <h3>Este espacio todavía no tiene cargos</h3>
            <p>Registrá los cargos que se eligen (por ejemplo Presidencia, Secretaría, Tesorería y Vocalías) para poder inscribir candidaturas.</p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
