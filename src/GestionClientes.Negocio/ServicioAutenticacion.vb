Imports GestionClientes.Datos
Imports GestionClientes.Entidades

''' <summary>Autenticación de usuarios contra la base de datos.</summary>
Public Class ServicioAutenticacion

    Private ReadOnly _usuarios As New UsuarioDAL()

    ''' <summary>
    ''' Salt de relleno para la verificación señuelo. No protege ninguna contraseña real: existe
    ''' únicamente para que exista trabajo criptográfico que hacer cuando el usuario no existe.
    ''' </summary>
    Private Shared ReadOnly SaltSenuelo As Byte() = New Byte(15) {}

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

        If usuario Is Nothing OrElse Not usuario.Activo Then
            ' Se calcula igualmente un hash señuelo antes de rechazar. Devolver el mismo mensaje
            ' no basta: sin esta línea, la ruta de "usuario inexistente" respondería en
            ' microsegundos mientras que la de "contraseña incorrecta" tardaría lo que tardan
            ' 100000 iteraciones de PBKDF2. Esa diferencia de tiempo, medible desde fuera, revela
            ' qué cuentas existen — exactamente lo que el mensaje genérico pretende ocultar.
            Hash.Calcular(contrasena, SaltSenuelo)
            Return Nothing
        End If

        If Not Hash.Verificar(contrasena, usuario.PasswordSalt, usuario.PasswordHash) Then
            Return Nothing
        End If

        Return usuario
    End Function

End Class
