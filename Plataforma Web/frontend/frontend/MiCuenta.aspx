<%@ Page Title="Mi cuenta" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MiCuenta.aspx.cs" Inherits="frontend.MiCuenta" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container" style="padding-top: 40px; padding-bottom: 60px;">

        <div class="gc-pagehead">
            <div>
                <p class="gc-eyebrow">Cuenta ciudadana</p>
                <h1 style="font-size: 1.7rem; margin: 0;">Mi cuenta</h1>
            </div>
        </div>

        <div class="row">

            <%-- ================================================ Cuenta --%>

            <div class="col-lg-4 gc-mb">
                <div class="gc-card gc-cuenta">
                    <span class="gc-avatar gc-avatar--lg" aria-hidden="true"><%: Iniciales %></span>
                    <h2 class="gc-cuenta__nombre"><%: Nombre %></h2>
                    <p class="gc-cuenta__correo"><%: Correo %></p>

                    <asp:PlaceHolder ID="phConfirmada" runat="server">
                        <span class="gc-verif gc-verif--ok">Correo confirmado</span>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="phPendiente" runat="server" Visible="false">
                        <span class="gc-verif gc-verif--rev">Correo sin confirmar</span>
                        <p class="gc-muted gc-small" style="margin: 10px 0 0;">
                            Podés consultar todo el sitio. Para valorar, comentar y proponer,
                            abrí el enlace que te enviamos o pedilo de nuevo desde el aviso de arriba.
                        </p>
                    </asp:PlaceHolder>

                    <p class="gc-cuenta__nota">
                        Lo que propongás se publica con tu nombre. Tu correo no se muestra a nadie.
                    </p>
                </div>
            </div>

            <%-- ============================================ Formulario --%>

            <div class="col-lg-8 gc-mb">

                <asp:PlaceHolder ID="phCerrado" runat="server" Visible="false">
                    <div class="gc-card gc-empty">
                        <h3>Las iniciativas están temporalmente cerradas</h3>
                        <p>
                            La administración cerró por ahora el módulo de iniciativas ciudadanas.
                            Las que ya propusiste se conservan y las vas a ver abajo.
                        </p>
                    </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="phFormulario" runat="server">
                    <div class="gc-card">
                        <div class="gc-card__body">

                            <div class="gc-pagehead" style="margin-bottom: 16px;">
                                <div>
                                    <h2 style="font-size: 1.25rem;"><%: TituloFormulario %></h2>
                                    <p class="gc-muted" style="margin: .25rem 0 0; max-width: 56ch;">
                                        Una propuesta de proyecto para tu comunidad o para el país. Se publica en la
                                        portada, identificada como propuesta ciudadana, y el resto la valora y la comenta.
                                    </p>
                                </div>
                            </div>

                            <asp:PlaceHolder ID="phOk" runat="server" Visible="false">
                                <p class="gc-ok gc-mb"><asp:Literal ID="litOk" runat="server" /></p>
                            </asp:PlaceHolder>

                            <asp:PlaceHolder ID="phError" runat="server" Visible="false">
                                <p class="gc-alert gc-mb"><asp:Literal ID="litError" runat="server" /></p>
                            </asp:PlaceHolder>

                            <div class="gc-form">

                                <div class="gc-field">
                                    <label for="<%= txtTitulo.ClientID %>">Título</label>
                                    <asp:TextBox ID="txtTitulo" runat="server" CssClass="gc-input" MaxLength="120"
                                        placeholder="Por ejemplo: Alumbrado en el parque central" />
                                    <span class="gc-hint">Concreto y corto. Al menos ocho caracteres.</span>
                                </div>

                                <div class="gc-field">
                                    <label for="<%= txtDescripcion.ClientID %>">Descripción</label>
                                    <asp:TextBox ID="txtDescripcion" runat="server" CssClass="gc-textarea" TextMode="MultiLine" Rows="6" />
                                    <span class="gc-hint">
                                        Qué problema resuelve, en qué consiste y a quién beneficia. Entre veinte y mil quinientos caracteres.
                                    </span>
                                </div>

                                <div class="gc-row2">
                                    <div class="gc-field">
                                        <label for="<%= ddlCategoria.ClientID %>">Categoría</label>
                                        <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="gc-select" />
                                        <span class="gc-hint">Es lo que permite contrastarla con las propuestas de las candidaturas.</span>
                                    </div>
                                    <div class="gc-field">
                                        <label for="<%= ddlDepartamento.ClientID %>">Departamento</label>
                                        <asp:DropDownList ID="ddlDepartamento" runat="server" CssClass="gc-select" />
                                        <span class="gc-hint">Dejalo en «Todo el país» si no es de un lugar concreto.</span>
                                    </div>
                                </div>

                                <div class="gc-formfoot">
                                    <asp:LinkButton ID="lnkCancelar" runat="server" CssClass="gc-btn gc-btn--quiet"
                                        Text="Cancelar edición" OnClick="lnkCancelar_Click" Visible="false" CausesValidation="false" />
                                    <asp:Button ID="btnGuardar" runat="server" CssClass="gc-btn"
                                        Text="Publicar iniciativa" OnClick="btnGuardar_Click" />
                                </div>

                            </div>

                            <asp:HiddenField ID="hdnId" runat="server" Value="0" />
                        </div>
                    </div>
                </asp:PlaceHolder>
            </div>
        </div>

        <%-- ========================================== Mis iniciativas --%>

        <div class="gc-sechead" style="margin-top: 30px;">
            <div>
                <h2>Mis iniciativas</h2>
                <p class="gc-muted">
                    <asp:Literal ID="litResumen" runat="server" />
                </p>
            </div>
        </div>

        <asp:PlaceHolder ID="phAvisoLista" runat="server" Visible="false">
            <p class="gc-ok gc-mb"><asp:Literal ID="litAvisoLista" runat="server" /></p>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="phSinIniciativas" runat="server" Visible="false">
            <div class="gc-card gc-empty">
                <h3>Todavía no propusiste ninguna</h3>
                <p>Cuando publiques una iniciativa, acá vas a ver cómo la recibe el resto.</p>
            </div>
        </asp:PlaceHolder>

        <%-- Se enlaza en cada carga, también en los postbacks: la tarjeta lleva
             los botones de participación y necesita su modelo al procesar el
             clic. Las acciones de la autora (editar, retirar) son de esta
             página y van fuera de la tarjeta, que es la misma que ve el
             público en la portada. --%>

        <div class="row">
            <asp:Repeater ID="rptMias" runat="server" OnItemCommand="rptMias_ItemCommand">
                <ItemTemplate>
                    <div class="col-lg-6 gc-mb">
                        <div class="gc-mia">
                            <gc:IniciativaCard runat="server" Item="<%# (Iniciativa)Container.DataItem %>" />

                            <asp:PlaceHolder runat="server" Visible='<%# ((Iniciativa)Container.DataItem).Activa %>'>
                                <div class="gc-mia__acciones">
                                    <asp:LinkButton runat="server" CssClass="gc-btn gc-btn--ghost gc-btn--sm"
                                        CommandName="editar" CommandArgument='<%# ((Iniciativa)Container.DataItem).Id %>'
                                        Visible='<%# ((Iniciativa)Container.DataItem).PuedeEditar %>'
                                        Text="Editar" />
                                    <span class="gc-mia__fija" runat="server" visible='<%# !((Iniciativa)Container.DataItem).PuedeEditar %>'>
                                        Ya recibió reacciones: el texto queda fijo.
                                    </span>
                                    <asp:LinkButton runat="server" CssClass="gc-btn gc-btn--quiet gc-btn--sm"
                                        CommandName="retirar" CommandArgument='<%# ((Iniciativa)Container.DataItem).Id %>'
                                        OnClientClick="return confirm('¿Retirar esta iniciativa? Deja de verse en la portada. Las reacciones que recibió se conservan.');"
                                        Text="Retirar" />
                                </div>
                            </asp:PlaceHolder>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

    </div>

</asp:Content>
