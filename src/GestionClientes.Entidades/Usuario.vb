''' <summary>
''' Usuario del sistema. El hash y el salt se manejan como arreglos de bytes y nunca se
''' convierten a texto: no hay motivo para que la contraseña o su derivado existan como String
''' más allá del momento de la verificación.
''' </summary>
Public Class Usuario

    Public Property UsuarioId As Integer
    Public Property NombreUsuario As String = String.Empty
    Public Property NombreCompleto As String = String.Empty
    Public Property PasswordHash As Byte()
    Public Property PasswordSalt As Byte()
    Public Property Activo As Boolean
    Public Property FechaCreacion As DateTime

End Class
