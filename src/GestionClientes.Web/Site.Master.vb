Imports System.Web.Security

Public Class SiteMaster
    Inherits MasterPage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            litUsuario.Text = HttpUtility.HtmlEncode(ObtenerNombreParaMostrar())
        End If
    End Sub

    ''' <summary>
    ''' Cierra la sesión por completo: invalida el ticket de autenticación y abandona la sesión,
    ''' de modo que no quede estado del usuario anterior reutilizable.
    ''' </summary>
    Protected Sub lnkSalir_Click(sender As Object, e As EventArgs) Handles lnkSalir.Click
        FormsAuthentication.SignOut()
        Session.Clear()
        Session.Abandon()
        Response.Redirect(FormsAuthentication.LoginUrl, False)
        Context.ApplicationInstance.CompleteRequest()
    End Sub

    ''' <summary>
    ''' Nombre a mostrar en la barra superior: el nombre completo si el ticket lo trae, y si no el
    ''' nombre de usuario.
    ''' </summary>
    Private Function ObtenerNombreParaMostrar() As String
        ' Se accede a través de Page.User y no de User a secas: MasterPage no expone la propiedad
        ' User, solo la expone Page. Escribirlo sin el prefijo no compila (BC30451).
        Dim nombreCompleto = IdentidadUsuario.ObtenerNombreCompleto(Page.User)
        If Not String.IsNullOrWhiteSpace(nombreCompleto) Then Return nombreCompleto

        If Page.User IsNot Nothing AndAlso Page.User.Identity IsNot Nothing Then
            Return Page.User.Identity.Name
        End If

        Return String.Empty
    End Function

End Class
