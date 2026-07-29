''' <summary>
''' Campos de auditoría comunes a cualquier entidad que se persista: quién la creó, quién la
''' modificó por última vez, y si fue borrada lógicamente.
'''
''' Viven aquí y no en cada entidad porque no describen a un cliente ni a un usuario: describen el
''' hecho de haber sido guardados. Una entidad nueva los hereda sin volver a declararlos.
'''
''' El borrado es lógico a propósito. "Eliminado" no es lo mismo que un estado de negocio como
''' "inactivo": inactivo significa que el registro sigue siendo válido pero no debe ofrecerse en
''' ciertos lugares, y lo alterna el usuario en ambos sentidos; eliminado significa que no debe
''' volver a aparecer, y solo soporte puede revertirlo desde la base de datos.
'''
''' Las columnas de "quién" guardan el identificador sin clave foránea hacia Usuarios: registran
''' quién hizo la acción en el momento de hacerla, y ese hecho no deja de ser cierto si el usuario
''' se elimina después. La bitácora aplica el mismo criterio con NombreUsuario.
'''
''' El control de concurrencia optimista NO está aquí: es otra preocupación, con otro ciclo de
''' vida, y no toda entidad necesita compararse por versión de fila.
''' </summary>
Public MustInherit Class EntidadAuditable

    ''' <summary>Fecha de alta. La asigna la base de datos.</summary>
    Public Property FechaRegistro As DateTime

    ''' <summary>Usuario que dio de alta el registro.</summary>
    Public Property CreadoPor As Integer?

    ''' <summary>Fecha de la última modificación. Nothing si nunca se modificó.</summary>
    Public Property FechaModificacion As DateTime?

    ''' <summary>Usuario que modificó el registro por última vez.</summary>
    Public Property ModificadoPor As Integer?

    ''' <summary>Borrado lógico. Un registro con este valor en True no existe para la aplicación.</summary>
    Public Property Eliminado As Boolean

    ''' <summary>Fecha del borrado lógico.</summary>
    Public Property FechaEliminacion As DateTime?

    ''' <summary>Usuario que borró el registro.</summary>
    Public Property EliminadoPor As Integer?

End Class
