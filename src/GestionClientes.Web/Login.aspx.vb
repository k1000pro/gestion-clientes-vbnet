Imports System.Globalization
Imports System.Web.Security
Imports GestionClientes.Negocio

''' <summary>Pantalla de autenticación.</summary>
Public Class PaginaLogin
    Inherits Page

    ''' <summary>Debe coincidir con el timeout declarado en &lt;forms&gt; de Web.config.</summary>
    Private Const DuracionTicketEnMinutos As Integer = 30

    Private ReadOnly _autenticacion As New ServicioAutenticacion()

    ''' <summary>Vincula el ViewState a la sesión para proteger el postback del login.</summary>
    Protected Overrides Sub OnInit(e As EventArgs)
        ViewStateUserKey = Session.SessionID
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
    ''' Establece la sesión autenticada.
    '''
    ''' La sesión anterior se descarta y su cookie se borra antes de emitir la identidad nueva:
    ''' un identificador de sesión que un tercero ya conociera deja así de ser válido en el
    ''' momento en que el usuario se autentica, que es la defensa contra la fijación de sesión.
    '''
    ''' Por eso mismo el identificador y el nombre del usuario NO se guardan en Session: la sesión
    ''' acaba de descartarse y cualquier valor escrito ahora se perdería al terminar la petición.
    ''' Viajan dentro del ticket de autenticación, que va firmado y cifrado.
    ''' </summary>
    Private Sub IniciarSesion(usuarioId As Integer, nombreUsuario As String, nombreCompleto As String)
        Session.Clear()
        Session.Abandon()
        Response.Cookies.Add(New HttpCookie("ASP.NET_SessionId", String.Empty))

        Dim ticket As New FormsAuthenticationTicket(
            2,
            nombreUsuario,
            DateTime.Now,
            DateTime.Now.AddMinutes(DuracionTicketEnMinutos),
            False,
            usuarioId.ToString(CultureInfo.InvariantCulture) & "|" & nombreCompleto,
            FormsAuthentication.FormsCookiePath)

        Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) With {
            .HttpOnly = True,
            .Path = FormsAuthentication.FormsCookiePath,
            .Secure = FormsAuthentication.RequireSSL
        }

        Response.Cookies.Add(cookie)

        Dim destino = FormsAuthentication.GetRedirectUrl(nombreUsuario, False)
        Response.Redirect(destino, False)
        Context.ApplicationInstance.CompleteRequest()
    End Sub

    Private Sub MostrarError(mensaje As String)
        litError.Text = HttpUtility.HtmlEncode(mensaje)
        pnlError.Visible = True
    End Sub

End Class
