<%@ Page Title="Resumen" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="frontend.Admin.AdminDefault" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="gc-pagehead">
        <div>
            <h2><%: Titulo %></h2>
            <p class="gc-muted" style="margin: 0;"><%: Subtitulo %></p>
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
                            <dt>Nombre</dt>
                            <dd><%: NombreUsuario %></dd>
                        </div>
                        <div>
                            <dt>Tipo de cuenta</dt>
                            <dd>Administrador</dd>
                        </div>
                        <div>
                            <dt>Alcance</dt>
                            <dd><%: Alcance %></dd>
                        </div>
                    </dl>
                </div>
            </div>

            <div class="gc-card gc-mb">
                <div class="gc-card__head">
                    <h3>Qué administra esta cuenta</h3>
                </div>
                <div class="gc-card__body">
                    <p class="gc-muted gc-small" style="margin-top: 0;">
                        Cada facultad tiene su procedimiento en la base de datos, su método en el Web
                        Service y su registro en la bitácora. El permiso se comprueba en las dos capas:
                        llamar al servicio directamente sin el rol tampoco funciona.
                    </p>

                    <table class="gc-table">
                        <thead>
                            <tr>
                                <th scope="col">Facultad</th>
                                <th scope="col">Estado</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>Consultar el tablero de analítica completo</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <tr>
                                <td>Declarar contenido como verificado</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <tr>
                                <td>Retirar publicaciones inadecuadas</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <tr>
                                <td>Consultar la bitácora de administración</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <tr>
                                <td>Administrar partidos, campañas y candidaturas</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <tr>
                                <td>Crear cuentas de acceso de candidaturas</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <% if (Sesion.AdministraPlataforma) { %>
                            <tr>
                                <td>Ocultar módulos y gráficos del sitio público</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <tr>
                                <td>Registrar espacios de clientes y crear sus cuentas</td>
                                <td><span class="gc-chip gc-chip--cumplida">Habilitado</span></td>
                            </tr>
                            <% } %>
                        </tbody>
                    </table>
                </div>
            </div>

        </div>

        <div class="col-lg-4">

            <div class="gc-note gc-mb">
                <span>
                    La verificación de contenido es lo que sostiene la neutralidad de la plataforma. Todo lo
                    que publica una candidatura se muestra identificado como declarado por ella hasta que
                    esta cuenta lo marque como verificado con una fuente que lo respalde.
                </span>
            </div>

            <div class="gc-note gc-note--ambar">
                <span>
                    Ninguna acción de administración queda sin registro. Cada verificación y cada retiro de
                    contenido guarda quién lo hizo, cuándo y con qué motivo, para que la propia plataforma
                    pueda ser auditada.
                </span>
            </div>

        </div>
    </div>

</asp:Content>
