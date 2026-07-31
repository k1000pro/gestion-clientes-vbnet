Imports System.Globalization
Imports System.Security.Principal
Imports System.Web.Security

''' <summary>
''' Lee el identificador y el nombre completo del usuario desde el ticket de autenticación, que
''' viaja firmado y cifrado. Ver "Decisiones de diseño" en el README sobre por qué no van en
''' Session.
''' </summary>
Public NotInheritable Class IdentidadUsuario

    Private Const Separador As Char = "|"c

    Private Sub New()
    End Sub

    ' 0 si no hay identidad válida.
    Public Shared Function ObtenerUsuarioId(usuario As IPrincipal) As Integer
        Dim partes = ObtenerPartes(usuario)
        If partes Is Nothing Then Return 0

        Dim usuarioId As Integer
        If Not Integer.TryParse(partes(0), NumberStyles.Integer, CultureInfo.InvariantCulture, usuarioId) Then
            Return 0
        End If

        Return usuarioId
    End Function

    Public Shared Function ObtenerNombreCompleto(usuario As IPrincipal) As String
        Dim partes = ObtenerPartes(usuario)
        If partes Is Nothing OrElse partes.Length < 2 Then Return String.Empty
        Return partes(1)
    End Function

    ' El nombre completo puede contener el separador, así que la división se limita a dos partes
    ' y todo lo que sigue al primero se conserva.
    Private Shared Function ObtenerPartes(usuario As IPrincipal) As String()
        If usuario Is Nothing OrElse usuario.Identity Is Nothing Then Return Nothing
        If Not usuario.Identity.IsAuthenticated Then Return Nothing

        Dim identidad = TryCast(usuario.Identity, FormsIdentity)
        If identidad Is Nothing OrElse identidad.Ticket Is Nothing Then Return Nothing

        Dim datos = identidad.Ticket.UserData
        If String.IsNullOrEmpty(datos) Then Return Nothing

        Return datos.Split(New Char() {Separador}, 2)
    End Function

End Class
