<%@ Page Title="Configuración" Language="C#" MasterPageFile="~/Panel/Panel.Master" AutoEventWireup="true" CodeBehind="Configuracion.aspx.cs" Inherits="frontend.Panel.Configuracion" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2>Configuración de la cuenta</h2>
            <p class="gc-muted" style="margin: 0;">
                Datos de acceso y visibilidad de tu candidatura.
            </p>
        </div>
    </div>

    <div class="row">
        <div class="col-lg-8">
            <div class="gc-card gc-mb">
                <div class="gc-card__head">
                    <h3>Cuenta</h3>
                </div>
                <div class="gc-card__body">
                    <dl class="gc-datalist">
                        <div>
                            <dt>Correo de acceso</dt>
                            <dd><%: CorreoAcceso %></dd>
                        </div>
                        <div>
                            <dt>Tipo de cuenta</dt>
                            <dd>Candidato</dd>
                        </div>
                        <div>
                            <dt>Campaña</dt>
                            <dd><%: NombreCampana %></dd>
                        </div>
                        <div>
                            <dt>Estado de verificación</dt>
                            <dd><span class="<%= VerificacionClase %>"><%: VerificacionTexto %></span></dd>
                        </div>
                    </dl>
                </div>
            </div>

            <div class="gc-card gc-empty">
                <h3>Opciones pendientes de esta etapa</h3>
                <p>
                    El cambio de contraseña, la baja de la cuenta y las preferencias de notificación se
                    habilitan junto con el Web Service de usuarios, que es el que administra las credenciales.
                </p>
                <a class="gc-btn gc-btn--ghost" href="<%= ResolveUrl("~/Panel/Perfil") %>">Editar mi perfil</a>
            </div>
        </div>

        <div class="col-lg-4">
            <div class="gc-note">
                <span>
                    Tu perfil y tus proyectos son públicos por diseño: cualquier persona puede consultarlos sin
                    crear una cuenta. Los únicos datos de contacto visibles son los que llenaste en tu perfil.
                </span>
            </div>
        </div>
    </div>

</asp:Content>
