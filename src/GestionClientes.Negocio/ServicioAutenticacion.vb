Imports GestionClientes.Datos
Imports GestionClientes.Entidades

''' <summary>Autenticación de usuarios contra la base de datos.</summary>
Public Class ServicioAutenticacion

    Private ReadOnly _usuarios As New UsuarioDAL()

    ''' <summary>
    ''' Valida credenciales. Devuelve el usuario si son correctas y Nothing en cualquier otro
    ''' caso.
    '''
    ''' Se devuelve el mismo resultado ante usuario inexistente, contraseña incorrecta y usuario
    ''' inactivo. Distinguirlos en el mensaje permitiría enumerar qué cuentas existen.
    ''' </summary>
    Public Function Autenticar(nombreUsuario As String, contrasena As String) As Usuario
        If String.IsNullOrWhiteSpace(nombreUsuario) OrElse String.IsNullOrEmpty(contrasena) Then
            Return Nothing
        End If

        Dim usuario = _usuarios.ObtenerPorNombre(nombreUsuario)

        If usuario Is Nothing Then Return Nothing
        If Not usuario.Activo Then Return Nothing
        If Not Hash.Verificar(contrasena, usuario.PasswordSalt, usuario.PasswordHash) Then Return Nothing

        Return usuario
    End Function

End Class
