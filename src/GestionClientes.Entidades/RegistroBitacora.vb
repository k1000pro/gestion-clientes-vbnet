''' <summary>Una entrada de la bitácora de auditoría.</summary>
Public Class RegistroBitacora

    Public Property BitacoraId As Long
    Public Property Accion As String = String.Empty
    Public Property ClienteId As Integer
    Public Property UsuarioId As Integer
    Public Property NombreUsuario As String = String.Empty
    Public Property FechaHora As DateTime
    Public Property Detalle As String = String.Empty

End Class
