Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports GestionClientes.Entidades

''' <summary>
''' Utilidades compartidas por la capa de acceso a datos: obtención de la cadena de conexión,
''' creación de comandos y traducción de errores del motor.
'''
''' Es Friend a propósito: nada fuera de esta capa debe poder crear comandos SQL.
''' </summary>
Friend NotInheritable Class SqlHelper

    ''' <summary>Nombre de la entrada en connectionStrings del archivo de configuración.</summary>
    Private Const NombreCadenaConexion As String = "GestionClientes"

    ''' <summary>Errores personalizados definidos en los procedimientos almacenados.</summary>
    Private Const ErrorDocumentoDuplicado As Integer = 50001
    Private Const ErrorClienteNoEncontrado As Integer = 50002
    Private Const ErrorConflictoConcurrencia As Integer = 50003

    ''' <summary>Violaciones de unicidad que reporta el propio motor.</summary>
    Private Const ErrorIndiceUnicoDuplicado As Integer = 2601
    Private Const ErrorClaveDuplicada As Integer = 2627

    Private Sub New()
    End Sub

    ''' <summary>Lee la cadena de conexión del archivo de configuración.</summary>
    ''' <exception cref="ConfigurationErrorsException">Si la entrada no existe o está vacía.</exception>
    Friend Shared Function ObtenerCadenaConexion() As String
        Dim configuracion = ConfigurationManager.ConnectionStrings(NombreCadenaConexion)

        If configuracion Is Nothing OrElse String.IsNullOrWhiteSpace(configuracion.ConnectionString) Then
            Throw New ConfigurationErrorsException(
                $"No se encontró la cadena de conexión '{NombreCadenaConexion}' en el archivo de configuración.")
        End If

        Return configuracion.ConnectionString
    End Function

    ''' <summary>
    ''' Crea un comando de procedimiento almacenado. Todos los accesos a datos pasan por aquí,
    ''' lo que garantiza que ninguno pueda ejecutar texto SQL arbitrario.
    ''' </summary>
    Friend Shared Function CrearComando(conexion As SqlConnection, nombreProcedimiento As String) As SqlCommand
        Dim comando As New SqlCommand(nombreProcedimiento, conexion) With {
            .CommandType = CommandType.StoredProcedure,
            .CommandTimeout = 30
        }

        Return comando
    End Function

    ''' <summary>
    ''' Traduce los errores de negocio lanzados por los procedimientos almacenados a una
    ''' excepción del dominio. Así la capa de negocio no necesita conocer SqlException ni los
    ''' códigos de error de SQL Server. Los errores de infraestructura se dejan pasar tal cual.
    ''' </summary>
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

    ''' <summary>Lee una columna de texto tratando NULL como cadena vacía.</summary>
    Friend Shared Function LeerTexto(lector As IDataRecord, columna As String) As String
        Dim indice = lector.GetOrdinal(columna)
        If lector.IsDBNull(indice) Then Return String.Empty
        Return lector.GetString(indice)
    End Function

    ''' <summary>Lee una columna binaria (hash o salt).</summary>
    Friend Shared Function LeerBytes(lector As IDataRecord, columna As String) As Byte()
        Dim indice = lector.GetOrdinal(columna)
        If lector.IsDBNull(indice) Then Return Nothing
        Return CType(lector.GetValue(indice), Byte())
    End Function

End Class
