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
    End Sub

    Private Function ObtenerNombreParaMostrar() As String
        Dim nombreCompleto = TryCast(Session(PaginaBase.ClaveNombreCompleto), String)
        If Not String.IsNullOrWhiteSpace(nombreCompleto) Then Return nombreCompleto

        If Page.User IsNot Nothing AndAlso Page.User.Identity IsNot Nothing Then Return Page.User.Identity.Name

        Return String.Empty
    End Function

End Class
