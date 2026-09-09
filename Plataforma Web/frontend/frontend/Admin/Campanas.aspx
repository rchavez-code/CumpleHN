<%@ Page Title="Campañas" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Campanas.aspx.cs" Inherits="frontend.Admin.Campanas" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Campañas electorales</h2>
            <p class="gc-muted" style="margin: 0;">
                Cada candidatura, propuesta y publicación pertenece a una campaña. Una sola puede estar
                destacada en la portada.
            </p>
        </div>
        <span class="gc-muted gc-small"><%: TotalTexto %></span>
    </div>

    <asp:PlaceHolder ID="phMensaje" runat="server" Visible="false">
        <div class="<%= ClaseMensaje %>" role="status"><%: Mensaje %></div>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phForm" runat="server" Visible="false">

        <div class="gc-card gc-mb">
            <div class="gc-card__head">
                <h3><%: TituloForm %></h3>
            </div>
            <div class="gc-card__body">

                <div class="gc-form">
                    <div class="gc-field">
                        <label for="<%= txtNombre.ClientID %>">Nombre de la campaña</label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="gc-input" MaxLength="160" />
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtInicio.ClientID %>">Inicio de la campaña</label>
                                <asp:TextBox ID="txtInicio" runat="server" CssClass="gc-input"
                                             TextMode="Date" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtEleccion.ClientID %>">Fecha de la elección</label>
                                <asp:TextBox ID="txtEleccion" runat="server" CssClass="gc-input"
                                             TextMode="Date" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= ddlEstado.ClientID %>">Estado</label>
                                <asp:DropDownList ID="ddlEstado" runat="server" CssClass="gc-select" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="gc-field">
                                <label for="<%= txtAlcance.ClientID %>">Alcance</label>
                                <asp:TextBox ID="txtAlcance" runat="server" CssClass="gc-input" MaxLength="200"
                                             placeholder="Nacional, departamental y municipal" />
                            </div>
                        </div>
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtResumen.ClientID %>">Resumen</label>
                        <asp:TextBox ID="txtResumen" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="2" MaxLength="400" />
                    </div>

                    <div class="gc-field">
                        <label for="<%= txtDescripcion.ClientID %>">Descripción</label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="gc-input"
                                     TextMode="MultiLine" Rows="5" />
                    </div>

                    <div class="gc-field">
                        <label>
                            <asp:CheckBox ID="chkActual" runat="server" />
                            Destacar esta campaña en la portada
                        </label>
                        <span class="gc-muted gc-small">
                            Solo una campaña puede estarlo. Al marcar esta, la que estuviera destacada deja
                            de estarlo. Una campaña cerrada no puede destacarse.
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

    <div class="gc-card gc-filters">
        <asp:Button ID="btnNuevo" runat="server" CssClass="gc-btn" Text="Registrar campaña"
                    OnClick="btnNuevo_Click" CausesValidation="false" />
    </div>

    <asp:PlaceHolder ID="phLista" runat="server">

        <div class="gc-card">
            <div class="gc-tablewrap">
                <table class="gc-table">
                    <thead>
                        <tr>
                            <th scope="col">Campaña</th>
                            <th scope="col">Período</th>
                            <th scope="col">Candidaturas</th>
                            <th scope="col">Propuestas</th>
                            <th scope="col">Estado</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCampanas" runat="server" OnItemCommand="rptCampanas_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td style="min-width: 220px;">
                                        <strong><%#: Eval("Nombre") %></strong>
                                        <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("EsActual") %>'>
                                            <span class="gc-chip gc-chip--declarada" style="margin-left: 6px;">En portada</span>
                                        </asp:PlaceHolder>
                                        <div class="gc-muted gc-small"><%#: Eval("Resumen") %></div>
                                    </td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("PeriodoTexto") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Candidaturas") %></td>
                                    <td class="gc-muted gc-nowrap"><%#: Eval("Propuestas") %></td>
                                    <td class="gc-nowrap">
                                        <span class="<%# Eval("EstadoClase") %>"><%#: Eval("EstadoTexto") %></span>
                                    </td>
                                    <td class="gc-nowrap" style="text-align: right;">
                                        <asp:LinkButton runat="server" CssClass="gc-small" Text="Editar"
                                                        CommandName="editar" CommandArgument='<%# Eval("Codigo") %>' />
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
            <h3>No hay campañas registradas</h3>
            <p>Registrá la primera campaña para poder inscribir candidaturas.</p>
        </div>
    </asp:PlaceHolder>

    <div class="gc-note" style="margin-top: 18px;">
        <span>
            Una campaña no se borra. Cuando termina, se cierra: sus candidaturas y sus propuestas siguen
            consultables, que es de lo que se trata dar seguimiento a lo prometido.
        </span>
    </div>

</asp:Content>
