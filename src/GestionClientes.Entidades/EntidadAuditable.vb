''' <summary>
''' Campos de auditoría comunes a cualquier entidad que se persista: quién la creó, quién la
''' modificó por última vez, y si fue borrada lógicamente.
''' </summary>
Public MustInherit Class EntidadAuditable

    ' La asigna la base de datos.
    Public Property FechaRegistro As DateTime

    Public Property CreadoPor As Integer?

    ' Nothing si el registro nunca se modificó.
    Public Property FechaModificacion As DateTime?

    Public Property ModificadoPor As Integer?

    ' Borrado lógico: un registro con Eliminado en True no existe para la aplicación.
    Public Property Eliminado As Boolean

    Public Property FechaEliminacion As DateTime?

    Public Property EliminadoPor As Integer?

End Class
