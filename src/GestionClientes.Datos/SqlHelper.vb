Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports GestionClientes.Entidades

' Friend a propósito: nada fuera de la capa de datos debe poder crear comandos SQL.
Friend NotInheritable Class SqlHelper

    Private Const NombreCadenaConexion As String = "GestionClientes"

    ' Errores personalizados definidos en los procedimientos almacenados.
    Private Const ErrorDocumentoDuplicado As Integer = 50001
    Private Const ErrorClienteNoEncontrado As Integer = 50002
    Private Const ErrorConflictoConcurrencia As Integer = 50003

    ' Violaciones de unicidad que reporta el propio motor.
    Private Const ErrorIndiceUnicoDuplicado As Integer = 2601
    Private Const ErrorClaveDuplicada As Integer = 2627

    Private Sub New()
    End Sub

    Friend Shared Function ObtenerCadenaConexion() As String
        Dim configuracion = ConfigurationManager.ConnectionStrings(NombreCadenaConexion)

        If configuracion Is Nothing OrElse String.IsNullOrWhiteSpace(configuracion.ConnectionString) Then
            Throw New ConfigurationErrorsException(
                $"No se encontró la cadena de conexión '{NombreCadenaConexion}' en el archivo de configuración.")
        End If

        Return configuracion.ConnectionString
    End Function

    ' Todo acceso a datos pasa por aquí, así ninguno puede ejecutar texto SQL arbitrario.
    Friend Shared Function CrearComando(conexion As SqlConnection, nombreProcedimiento As String) As SqlCommand
        Dim comando As New SqlCommand(nombreProcedimiento, conexion) With {
            .CommandType = CommandType.StoredProcedure,
            .CommandTimeout = 30
        }

        Return comando
    End Function

    ' Traduce errores del motor a excepciones del dominio, para que la capa de negocio no
    ' necesite conocer SqlException ni los códigos de SQL Server.
    Friend Shared Function Traducir(ex As SqlException) As Exception
        Select Case ex.Number

            Case ErrorDocumentoDuplicado, ErrorClienteNoEncontrado, ErrorConflictoConcurrencia
                ' Errores que los procedimientos lanzan a propósito: su mensaje ya está
                ' redactado para el usuario final.
                Return New ReglaNegocioException(ex.Message)

            Case ErrorIndiceUnicoDuplicado, ErrorClaveDuplicada
                ' El procedimiento comprueba el documento duplicado antes de escribir, pero entre
                ' esa comprobación y la escritura cabe otra transacción concurrente. En esa
                ' carrera quien detecta el duplicado es el índice UX_Clientes_Documento, y su
                ' mensaje es técnico y viene en el idioma del motor. Se sustituye por el mismo
                ' texto que habría producido la comprobación, para que el usuario reciba una
                ' única respuesta coherente venga por donde venga el error.
                Return New ReglaNegocioException("Ya existe un cliente registrado con ese documento.")

            Case Else
                ' Fallos de infraestructura (red, permisos, tiempo de espera): no son del dominio
                ' y deben propagarse tal cual hasta el manejador global.
                Return ex

        End Select
    End Function

    ' Trata NULL como cadena vacía.
    Friend Shared Function LeerTexto(lector As IDataRecord, columna As String) As String
        Dim indice = lector.GetOrdinal(columna)
        If lector.IsDBNull(indice) Then Return String.Empty
        Return lector.GetString(indice)
    End Function

    ' Devuelve Nothing y no cero: los campos de auditoría son nulos en los registros anteriores
    ' a que existieran, y un cero apuntaría a un usuario inexistente.
    Friend Shared Function LeerEnteroOpcional(lector As IDataRecord, columna As String) As Integer?
        Dim indice = lector.GetOrdinal(columna)
        If lector.IsDBNull(indice) Then Return Nothing
        Return lector.GetInt32(indice)
    End Function

    Friend Shared Function LeerFechaOpcional(lector As IDataRecord, columna As String) As DateTime?
        Dim indice = lector.GetOrdinal(columna)
        If lector.IsDBNull(indice) Then Return Nothing
        Return lector.GetDateTime(indice)
    End Function

    Friend Shared Function LeerBytes(lector As IDataRecord, columna As String) As Byte()
        Dim indice = lector.GetOrdinal(columna)
        If lector.IsDBNull(indice) Then Return Nothing
        Return CType(lector.GetValue(indice), Byte())
    End Function

End Class
