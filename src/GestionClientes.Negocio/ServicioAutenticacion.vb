Imports GestionClientes.Datos
Imports GestionClientes.Entidades

''' <summary>Autenticación de usuarios contra la base de datos.</summary>
Public Class ServicioAutenticacion

    Private ReadOnly _usuarios As New UsuarioDAL()

    ' Relleno para la verificación señuelo. No protege ninguna contraseña real.
    Private Shared ReadOnly SaltSenuelo As Byte() = New Byte(15) {}

    ''' <summary>
    ''' Devuelve el usuario si las credenciales son correctas y Nothing en cualquier otro caso.
    ''' Usuario inexistente, contraseña incorrecta y usuario inactivo dan el mismo resultado:
    ''' distinguirlos permitiría enumerar qué cuentas existen.
    ''' </summary>
    Public Function Autenticar(nombreUsuario As String, contrasena As String) As Usuario
        If String.IsNullOrWhiteSpace(nombreUsuario) OrElse String.IsNullOrEmpty(contrasena) Then
            Return Nothing
        End If

        Dim usuario = _usuarios.ObtenerPorNombre(nombreUsuario)

        If usuario Is Nothing OrElse Not usuario.Activo Then
            ' Hash señuelo antes de rechazar. Sin él esta ruta respondería en microsegundos y la
            ' de contraseña incorrecta tardaría 100000 iteraciones de PBKDF2: esa diferencia de
            ' tiempo revela qué cuentas existen, que es lo que el mensaje genérico oculta.
            Hash.Calcular(contrasena, SaltSenuelo)
            Return Nothing
        End If

        If Not Hash.Verificar(contrasena, usuario.PasswordSalt, usuario.PasswordHash) Then
            Return Nothing
        End If

        Return usuario
    End Function

End Class
