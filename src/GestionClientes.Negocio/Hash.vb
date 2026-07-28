Imports System.Security.Cryptography

''' <summary>
''' Derivación y verificación de contraseñas con PBKDF2-HMAC-SHA256.
'''
''' Se usa PBKDF2 y no un SHA-256 con salt porque un hash rápido, aunque lleve salt, se puede
''' atacar por fuerza bruta a gran velocidad con GPU. El costo por iteración es lo que protege
''' la contraseña; 100000 iteraciones hacen inviable ese ataque manteniendo un tiempo de login
''' imperceptible para el usuario.
''' </summary>
Public NotInheritable Class Hash

    ''' <summary>Iteraciones de PBKDF2. Debe coincidir con el valor usado al sembrar usuarios.</summary>
    Private Const Iteraciones As Integer = 100000

    ''' <summary>Tamaño del salt en bytes.</summary>
    Private Const TamanoSalt As Integer = 16

    ''' <summary>Tamaño del hash derivado en bytes.</summary>
    Private Const TamanoHash As Integer = 32

    ''' <summary>Clase de utilidades: no se instancia.</summary>
    Private Sub New()
    End Sub

    ''' <summary>Genera un salt criptográficamente aleatorio, distinto para cada usuario.</summary>
    Public Shared Function GenerarSalt() As Byte()
        Dim salt(TamanoSalt - 1) As Byte

        Using generador = RandomNumberGenerator.Create()
            generador.GetBytes(salt)
        End Using

        Return salt
    End Function

    ''' <summary>Deriva el hash de una contraseña con el salt indicado.</summary>
    ''' <exception cref="ArgumentException">Si la contraseña o el salt están vacíos.</exception>
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

    ''' <summary>
    ''' Verifica una contraseña contra su hash almacenado. Devuelve False ante datos ausentes o
    ''' de longitud inesperada en lugar de lanzar: un registro corrupto en base de datos no debe
    ''' tumbar el login.
    ''' </summary>
    Public Shared Function Verificar(contrasena As String, salt As Byte(), hashEsperado As Byte()) As Boolean
        If String.IsNullOrEmpty(contrasena) Then Return False
        If salt Is Nothing OrElse salt.Length = 0 Then Return False
        If hashEsperado Is Nothing OrElse hashEsperado.Length <> TamanoHash Then Return False

        Return SonIguales(Calcular(contrasena, salt), hashEsperado)
    End Function

    ''' <summary>
    ''' Compara dos arreglos en tiempo constante. Una comparación que corta en el primer byte
    ''' distinto revela, por el tiempo de respuesta, cuántos bytes iniciales acertó el atacante.
    ''' </summary>
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
