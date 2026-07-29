<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Error.aspx.vb" Inherits="GestionClientes.Web.PaginaError" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Error &mdash; Gestión de Clientes</title>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/bootstrap.min.css") %>" />
    <link rel="stylesheet" href="<%= ResolveUrl("~/Content/sitio.css") %>" />
</head>
<body>
    <div class="container">
        <div class="tarjeta-login card shadow-sm">
            <div class="card-body p-4 text-center">
                <h1 class="h4 mb-3">Ocurrió un error</h1>
                <p class="text-muted mb-4">
                    <asp:Literal ID="litMensaje" runat="server" />
                </p>
                <a class="btn btn-primary" href="<%= ResolveUrl("~/Clientes.aspx") %>">Volver al inicio</a>
            </div>
        </div>
    </div>
</body>
</html>
