Imports System.Globalization
Imports System.Web.Configuration
Imports System.Web.Security
Imports GestionClientes.Negocio

''' <summary>Pantalla de autenticación.</summary>
Public Class PaginaLogin
    Inherits Page

    Private Shared ReadOnly Registro As log4net.ILog = log4net.LogManager.GetLogger(GetType(PaginaLogin))

    Private ReadOnly _autenticacion As New ServicioAutenticacion()

    ' Vincula el ViewState a la sesión para proteger el postback del login.
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
            ' Se registra el nombre intentado, nunca la contraseña: un registro que contenga
            ' credenciales convierte el archivo de log en un objetivo.
            Registro.Warn($"Intento de inicio de sesión fallido para el usuario '{ParaRegistro(txtUsuario.Text)}'.")

            ' Mensaje deliberadamente genérico: distinguir "usuario inexistente" de "contraseña
            ' incorrecta" permitiría averiguar qué cuentas existen probando nombres.
            MostrarAviso("Usuario o contraseña incorrectos.")
            txtContrasena.Text = String.Empty
            Return
        End If

        IniciarSesion(usuario.UsuarioId, usuario.NombreUsuario, usuario.NombreCompleto)
    End Sub

    ' La sesión anterior se descarta y su cookie se borra antes de emitir la identidad nueva:
    ' un identificador que un tercero ya conociera deja de ser válido justo al autenticarse, que
    ' es la defensa contra la fijación de sesión. Por eso la identidad viaja en el ticket y no
    ' en Session: lo que se escribiera ahora se perdería al terminar la petición.
    Private Sub IniciarSesion(usuarioId As Integer, nombreUsuario As String, nombreCompleto As String)
        Session.Clear()
        Session.Abandon()
        CaducarCookieDeSesion()

        ' El primer argumento es la versión del formato del ticket: es informativo y no afecta al
        ' cifrado ni a la validación. La vigencia se toma de <forms timeout> para no declararla
        ' dos veces.
        Dim ticket As New FormsAuthenticationTicket(
            2,
            nombreUsuario,
            DateTime.Now,
            DateTime.Now.Add(FormsAuthentication.Timeout),
            False,
            usuarioId.ToString(CultureInfo.InvariantCulture) & "|" & nombreCompleto,
            FormsAuthentication.FormsCookiePath)

        Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) With {
            .HttpOnly = True,
            .Path = FormsAuthentication.FormsCookiePath,
            .Secure = FormsAuthentication.RequireSSL
        }

        Response.Cookies.Add(cookie)

        Registro.Info($"Inicio de sesión correcto del usuario '{nombreUsuario}'.")

        Dim destino = FormsAuthentication.GetRedirectUrl(nombreUsuario, False)
        Response.Redirect(destino, False)
        Context.ApplicationInstance.CompleteRequest()
    End Sub

    ' Session.Abandon() por sí solo puede dejar que el navegador reenvíe el mismo identificador,
    ' así que la cookie se caduca explícitamente: es lo que cierra la fijación de sesión. El
    ' nombre se lee de la configuración porque <sessionState cookieName> puede cambiarlo.
    Private Sub CaducarCookieDeSesion()
        Dim seccion = TryCast(WebConfigurationManager.GetSection("system.web/sessionState"), SessionStateSection)
        Dim nombre = If(seccion Is Nothing OrElse String.IsNullOrEmpty(seccion.CookieName),
                        "ASP.NET_SessionId", seccion.CookieName)

        Response.Cookies.Add(New HttpCookie(nombre, String.Empty) With {
            .Expires = DateTime.Now.AddDays(-1),
            .HttpOnly = True,
            .Path = "/"
        })
    End Sub

    Private Sub MostrarAviso(mensaje As String)
        litAviso.Text = HttpUtility.HtmlEncode(mensaje)
        pnlAviso.Visible = True
    End Sub

    ' Sin esto, un salto de línea en el nombre de usuario permitiría fabricar entradas de log
    ' falsas.
    Private Shared Function ParaRegistro(valor As String) As String
        If String.IsNullOrEmpty(valor) Then Return String.Empty

        Dim limpio = valor.Replace(ControlChars.Cr, " "c).Replace(ControlChars.Lf, " "c)
        If limpio.Length > 50 Then limpio = limpio.Substring(0, 50)

        Return limpio
    End Function

End Class
