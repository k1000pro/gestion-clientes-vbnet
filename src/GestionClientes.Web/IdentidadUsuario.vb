Imports System.Globalization
Imports System.Security.Principal
Imports System.Web.Security

''' <summary>
''' Lee el identificador y el nombre completo del usuario desde el ticket de autenticación.
'''
''' Estos datos no se guardan en Session por dos razones. La primera es que al iniciar sesión la
''' sesión anterior se descarta a propósito para impedir la fijación de sesión, y cualquier valor
''' escrito en ella durante esa misma petición se perdería. La segunda es más importante: la
''' cookie de autenticación tiene expiración deslizante y la sesión InProc muere cuando IIS
''' recicla el grupo de aplicaciones, de modo que un usuario puede seguir autenticado con la
''' sesión ya vacía. Leyendo del ticket no existe ese estado intermedio.
'''
''' El ticket viaja firmado y cifrado, así que el identificador no se puede manipular desde el
''' navegador.
''' </summary>
Public NotInheritable Class IdentidadUsuario

    Private Const Separador As Char = "|"c

    Private Sub New()
    End Sub

    ''' <summary>Identificador del usuario autenticado, o 0 si no hay identidad válida.</summary>
    Public Shared Function ObtenerUsuarioId(usuario As IPrincipal) As Integer
        Dim partes = ObtenerPartes(usuario)
        If partes Is Nothing Then Return 0

        Dim usuarioId As Integer
        If Not Integer.TryParse(partes(0), NumberStyles.Integer, CultureInfo.InvariantCulture, usuarioId) Then
            Return 0
        End If

        Return usuarioId
    End Function

    ''' <summary>Nombre completo para mostrar, o cadena vacía si no hay identidad válida.</summary>
    Public Shared Function ObtenerNombreCompleto(usuario As IPrincipal) As String
        Dim partes = ObtenerPartes(usuario)
        If partes Is Nothing OrElse partes.Length < 2 Then Return String.Empty
        Return partes(1)
    End Function

    ''' <summary>
    ''' Extrae los datos del ticket. El nombre completo puede contener el separador, así que la
    ''' división se limita a dos partes y todo lo que sigue al primer separador se conserva.
    ''' </summary>
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
