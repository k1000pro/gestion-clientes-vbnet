<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="GestionClientes.Web.PaginaLogin" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Iniciar sesión &mdash; Gestión de Clientes</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/bootstrap.min.css") %>" />
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/sitio.css") %>" />
</head>
<body>
    <form id="formLogin" runat="server" autocomplete="off">
        <div class="container">
            <div class="tarjeta-login card shadow-sm">
                <div class="card-body p-4">

                    <h1 class="h4 mb-1 marca">Gestión de Clientes</h1>
                    <p class="text-muted mb-4">Ingrese sus credenciales para continuar.</p>

                    <asp:Panel ID="pnlError" runat="server" Visible="False"
                               CssClass="alert alert-danger py-2" role="alert">
                        <asp:Literal ID="litError" runat="server" />
                    </asp:Panel>

                    <div class="mb-3">
                        <label class="form-label" for="<%= txtUsuario.ClientID %>">Usuario</label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control"
                                     MaxLength="50" autocomplete="username" />
                        <asp:RequiredFieldValidator ID="valUsuario" runat="server"
                                                    ControlToValidate="txtUsuario"
                                                    CssClass="text-danger small"
                                                    Display="Dynamic"
                                                    ErrorMessage="El usuario es obligatorio." />
                    </div>

                    <div class="mb-4">
                        <label class="form-label" for="<%= txtContrasena.ClientID %>">Contraseña</label>
                        <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control"
                                     TextMode="Password" MaxLength="128" autocomplete="current-password" />
                        <asp:RequiredFieldValidator ID="valContrasena" runat="server"
                                                    ControlToValidate="txtContrasena"
                                                    CssClass="text-danger small"
                                                    Display="Dynamic"
                                                    ErrorMessage="La contraseña es obligatoria." />
                    </div>

                    <asp:Button ID="btnIngresar" runat="server" Text="Ingresar"
                                CssClass="btn btn-primary w-100" />

                </div>
            </div>
        </div>
    </form>
</body>
</html>
