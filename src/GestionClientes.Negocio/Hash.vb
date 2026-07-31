Imports System.Security.Cryptography

''' <summary>
''' Derivación y verificación de contraseñas con PBKDF2-HMAC-SHA256.
''' </summary>
Public NotInheritable Class Hash

    ' El costo por iteración es lo que protege la contraseña frente a fuerza bruta con GPU.
    ' Debe coincidir con el valor usado al sembrar usuarios.
    Private Const Iteraciones As Integer = 100000

    Private Const TamanoSalt As Integer = 16
    Private Const TamanoHash As Integer = 32

    Private Sub New()
    End Sub

    ' Salt distinto para cada usuario.
    Public Shared Function GenerarSalt() As Byte()
        Dim salt(TamanoSalt - 1) As Byte

        Using generador = RandomNumberGenerator.Create()
            generador.GetBytes(salt)
        End Using

        Return salt
    End Function

    Public Shared Function Calcular(contrasena As String, salt As Byte()) As Byte()
        If String.IsNullOrEmpty(contrasena) Then
            Throw New ArgumentException("La contraseña no puede estar vacía.", NameOf(contrasena))
        End If

        If salt Is Nothing OrElse salt.Length = 0 Then
            Throw New ArgumentException("El salt no puede estar vacío.", NameOf(salt))
        End If

        Using derivador As New Rfc2898DeriveBytes(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256)
            Return derivador.GetBytes(TamanoHash)
        End Using
    End Function

    ' Devuelve False ante datos ausentes o de longitud inesperada en lugar de lanzar: un registro
    ' corrupto en base de datos no debe tumbar el login.
    Public Shared Function Verificar(contrasena As String, salt As Byte(), hashEsperado As Byte()) As Boolean
        If String.IsNullOrEmpty(contrasena) Then Return False
        If salt Is Nothing OrElse salt.Length = 0 Then Return False
        If hashEsperado Is Nothing OrElse hashEsperado.Length <> TamanoHash Then Return False

        Return SonIguales(Calcular(contrasena, salt), hashEsperado)
    End Function

    ' Comparación en tiempo constante: una que corte en el primer byte distinto revela, por el
    ' tiempo de respuesta, cuántos bytes iniciales acertó el atacante.
    Private Shared Function SonIguales(primero As Byte(), segundo As Byte()) As Boolean
        If primero Is Nothing OrElse segundo Is Nothing Then Return False
        If primero.Length <> segundo.Length Then Return False

        Dim diferencia As Integer = 0

        For indice As Integer = 0 To primero.Length - 1
            diferencia = diferencia Or (primero(indice) Xor segundo(indice))
        Next

        Return diferencia = 0
    End Function

End Class
