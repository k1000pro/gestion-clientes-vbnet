Imports System.Web.Security
Imports GestionClientes.Negocio

''' <summary>Pantalla de autenticación.</summary>
Public Class PaginaLogin
    Inherits Page

    Private ReadOnly _autenticacion As New ServicioAutenticacion()

    ''' <summary>Vincula el ViewState a la sesión para proteger el postback del login.</summary>
    Protected Overrides Sub OnInit(e As EventArgs)
        ViewStateUserKey = Session.SessionID
        ' El proyecto no incluye jQuery, así que el modo de validación "unobtrusive" (que lo
        ' requiere mediante un ScriptResourceMapping) fallaría en tiempo de ejecución. Se usa el
        ' script de validación clásico, incluido como recurso embebido en System.Web.
        Me.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None
        MyBase.OnInit(e)
    End Sub

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            txtUsuario.Focus()
        End If
    End Sub

    Protected Sub btnIngresar_Click(sender As Object, e As EventArgs) Handles btnIngresar.Click
        If Not Page.IsValid Then Return

        Dim usuario = _autenticacion.Autenticar(txtUsuario.Text, txtContrasena.Text)

        If usuario Is Nothing Then
            ' Mensaje deliberadamente genérico: distinguir "usuario inexistente" de "contraseña
            ' incorrecta" permitiría averiguar qué cuentas existen probando nombres.
            MostrarError("Usuario o contraseña incorrectos.")
            txtContrasena.Text = String.Empty
            Return
        End If

        IniciarSesion(usuario.UsuarioId, usuario.NombreUsuario, usuario.NombreCompleto)
    End Sub

    ''' <summary>
    ''' Establece la sesión autenticada. Se descarta la sesión anterior antes de crear la nueva
    ''' para evitar la fijación de sesión: un identificador conocido por un tercero deja de ser
    ''' válido en el momento en que el usuario se autentica.
    ''' </summary>
    Private Sub IniciarSesion(usuarioId As Integer, nombreUsuario As String, nombreCompleto As String)
        Session.Clear()
        Session.Abandon()

        FormsAuthentication.SetAuthCookie(nombreUsuario, False)

        Session(PaginaBase.ClaveUsuarioId) = usuarioId
        Session(PaginaBase.ClaveNombreCompleto) = nombreCompleto

        Dim destino = FormsAuthentication.GetRedirectUrl(nombreUsuario, False)
        Response.Redirect(destino, False)
        Context.ApplicationInstance.CompleteRequest()
    End Sub

    Private Sub MostrarError(mensaje As String)
        litError.Text = HttpUtility.HtmlEncode(mensaje)
        pnlError.Visible = True
    End Sub

End Class
