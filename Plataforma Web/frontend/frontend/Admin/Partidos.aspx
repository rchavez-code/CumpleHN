<%@ Page Title="Partidos" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Partidos.aspx.cs" Inherits="frontend.Admin.Partidos" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Partidos políticos</h2>
            <p class="gc-muted" style="margin: 0;">
                Los partidos a los que se puede afiliar una candidatura. No se borran: se desactivan,
                para no dejar sin partido a quien ya se presentó por él.
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
                        <label for="<%= txtNombre.ClientID %>">Nombre del partido</label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="gc-input" MaxLength="120" />
                        <span class="gc-muted gc-small">
                            La dirección pública de la ficha se deriva del nombre al registrarlo, y después
                            no cambia aunque el nombre se corrija.
                        </span>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtSiglas.ClientID %>">Siglas</label>
                        <asp:TextBox ID="txtSiglas" runat="server" CssClass="gc-input" MaxLength="20" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtDescripcion.ClientID %>">Descripción</label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="4" />
                        <span class="gc-muted gc-small">
                            Datos de registro y trayectoria. La plataforma no describe la posición política
                            de un partido.
                        </span>
                    </div>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn" Text="Guardar"
                                OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="gc-btn gc-btn--quiet" Text="Cancelar"
                                OnClick="btnCancelar_Click" CausesValidation="false" />
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
                    <asp:TextBox ID="txtMotivo" runat="server" CssClass="gc-input"
                                 TextMode="MultiLine" Rows="2" />
                    <span class="gc-muted gc-small">Queda en la bitácora junto a tu nombre.</span>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnConfirmarEstado" runat="server" CssClass="gc-btn" Text="Confirmar"
                                OnClick="btnConfirmarEstado_Click" />
                    <asp:Button ID="btnCancelarEstado" runat="server" CssClass="gc-btn gc-btn--quiet"
                                Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- =================================================== Lista --%>

    <div class="gc-card gc-filters">
        <asp:Button ID="btnNuevo" runat="server" CssClass="gc-btn" Text="Registrar partido"
                    OnClick="btnNuevo_Click" CausesValidation="false" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Partido</th>
                            <th scope="col">Siglas</th>
                            <th scope="col">Candidaturas</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPartidos" runat="server" OnItemCommand="rptPartidos_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 220px;">
                                        <strong><%#: Eval("Nombre") %></strong>
                                        <div class="gc-muted gc-small"><%#: Eval("Slug") %></div>
                                    </td>
                                    <td class="gc-nowrap"><%#: Eval("SiglasTexto") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Candidaturas") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span>
                                    </td>
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
            <h3>No hay partidos registrados</h3>
            <p>Registrá el primer partido para poder afiliarle candidaturas.</p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
