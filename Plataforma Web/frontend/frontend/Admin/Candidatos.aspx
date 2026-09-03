<%@ Page Title="Candidaturas" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Candidatos.aspx.cs" Inherits="frontend.Admin.Candidatos" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Candidaturas</h2>
            <p class="gc-muted" style="margin: 0;">
                La plataforma registra los datos de identificación. El perfil, las propuestas y las
                publicaciones los llena la propia candidatura desde su panel.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <%-- ============================================== Datos --%>

    <asp:PlaceHolder ID="phForm" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloForm %></h3>
            </div>
            <div class="gc-card__body">

                <div class="gc-form">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtNombres.ClientID %>">Nombres</label>
                                <asp:TextBox ID="txtNombres" runat="server" CssClass="gc-input" MaxLength="80" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtApellidos.ClientID %>">Apellidos</label>
                                <asp:TextBox ID="txtApellidos" runat="server" CssClass="gc-input" MaxLength="80" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= ddlCampana.ClientID %>">Campaña</label>
                                <asp:DropDownList ID="ddlCampana" runat="server" CssClass="gc-select" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= ddlCargo.ClientID %>">Cargo al que aspira</label>
                                <asp:DropDownList ID="ddlCargo" runat="server" CssClass="gc-select" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= ddlPartido.ClientID %>">Partido</label>
                                <asp:DropDownList ID="ddlPartido" runat="server" CssClass="gc-select" />
                                <span class="gc-muted gc-small">
                                    Una candidatura independiente no es un dato faltante: se registra como tal.
                                </span>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= ddlDepartamento.ClientID %>">Departamento</label>
                                <asp:DropDownList ID="ddlDepartamento" runat="server" CssClass="gc-select" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtMunicipio.ClientID %>">Municipio</label>
                                <asp:TextBox ID="txtMunicipio" runat="server" CssClass="gc-input" MaxLength="120" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtTitular.ClientID %>">Titular</label>
                                <asp:TextBox ID="txtTitular" runat="server" CssClass="gc-input" MaxLength="200" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="gc-note gc-mb">
                    <span>
                        La candidatura queda registrada como <strong>declarada</strong>. Lo que la plataforma
                        sabe de ella es lo que alguien acaba de escribir, y eso todavía no está contrastado
                        contra ninguna fuente.
                    </span>
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

    <%-- ============================================== Cuenta --%>

    <asp:PlaceHolder ID="phCuenta" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3>Cuenta de acceso</h3>
            </div>
            <div class="gc-card__body">

                <p class="gc-muted" style="margin-top: 0;">
                    Cuenta para <strong><%: SeleccionNombre %></strong>. Sin ella, la candidatura solo existe
                    como ficha que alguien más llenó: no puede editar su perfil ni registrar sus propuestas.
                </p>

                <div class="gc-form">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtLogin.ClientID %>">Usuario</label>
                                <asp:TextBox ID="txtLogin" runat="server" CssClass="gc-input" MaxLength="60" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtCorreo.ClientID %>">Correo</label>
                                <asp:TextBox ID="txtCorreo" runat="server" CssClass="gc-input"
                                             TextMode="Email" MaxLength="160" />
                            </div>
                        </div>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtClave.ClientID %>">Contraseña inicial</label>
                        <asp:TextBox ID="txtClave" runat="server" CssClass="gc-input" TextMode="Password" />
                        <span class="gc-muted gc-small">
                            Mínimo ocho caracteres. Se guarda cifrada y no vuelve a mostrarse: anotala antes
                            de confirmar y entregala por un medio seguro.
                        </span>
                    </div>
                </div>

                <div class="gc-formfoot">
                    <asp:Button ID="btnCrearCuenta" runat="server" CssClass="gc-btn" Text="Crear cuenta"
                                OnClick="btnCrearCuenta_Click" />
                    <asp:Button ID="btnCancelarCuenta" runat="server" CssClass="gc-btn gc-btn--quiet"
                                Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

            </div>
        </div>

    </asp:PlaceHolder>

    <%-- ============================================== Estado --%>

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
                    <span class="gc-muted gc-small">Obligatorio. Queda en la bitácora junto a tu nombre.</span>
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
        <asp:DropDownList ID="ddlFiltroCampana" runat="server" CssClass="gc-select" />
        <asp:Button ID="btnFiltrar" runat="server" CssClass="gc-btn" Text="Filtrar"
                    OnClick="btnFiltrar_Click" CausesValidation="false" />
        <asp:Button ID="btnNuevo" runat="server" CssClass="gc-btn gc-btn--ghost" Text="Registrar candidatura"
                    OnClick="btnNuevo_Click" CausesValidation="false" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Candidatura</th>
                            <th scope="col">Cargo</th>
                            <th scope="col">Partido</th>
                            <th scope="col">Cuenta</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCandidatos" runat="server" OnItemCommand="rptCandidatos_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 220px;">
                                        <strong><%#: Eval("NombreCompleto") %></strong>
                                        <div class="gc-muted gc-small"><%#: Eval("Campana") %></div>
                                    </td>
                                    <td class="gc-muted"><%#: Eval("Cargo") %></td>
                                    <td class="gc-muted"><%#: Eval("PartidoTexto") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("CuentaClase") %>"><%#: Eval("CuentaTexto") %></span>
                                    </td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span>
                                    </td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Editar"
                                                        CommandName="editar" CommandArgument='<%# Eval("Codigo") %>' />
                                        <asp:PlaceHolder runat="server" Visible='<%# !(bool)Eval("TieneCuenta") %>'>
                                            <span class="gc-faint" aria-hidden="true"> · </span>
                                            <asp:LinkButton runat="server" CssClass="gc-small" Text="Crear cuenta"
                                                            CommandName="cuenta" CommandArgument='<%# Eval("Codigo") %>' />
                                        </asp:PlaceHolder>
                                        <span class="gc-faint" aria-hidden="true"> · </span>
                                        <asp:LinkButton runat="server" CssClass="gc-small"
                                                        Text='<%# (bool)Eval("Activo") ? "Retirar" : "Reincorporar" %>'
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
            <h3>No hay candidaturas registradas</h3>
            <p>Registrá la primera candidatura de esta campaña.</p>
        </div>
    </asp:PlaceHolder>

</asp:Content>
