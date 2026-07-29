Imports System.Data
Imports System.Data.SqlClient
Imports GestionClientes.Entidades

''' <summary>
''' Acceso a datos de la tabla Clientes.
'''
''' Los métodos de escritura reciben el usuario que realiza la acción y lo pasan al procedimiento
''' almacenado, que registra la bitácora dentro de la misma transacción que el cambio. Por eso no
''' existe aquí ningún método "RegistrarEnBitacora": no es posible modificar un cliente sin que
''' quede auditado.
''' </summary>
Public Class ClienteDAL

    ''' <summary>
    ''' Devuelve una página de clientes junto con el total que cumple el filtro.
    '''
    ''' El parámetro de salida se lee DESPUÉS de cerrar el lector: SQL Server no entrega los
    ''' parámetros de salida hasta que el conjunto de resultados se ha consumido por completo, así
    ''' que leerlo antes devolvería Nothing.
    ''' </summary>
    Public Function Listar(criterios As CriteriosCliente) As ResultadoPaginado(Of Cliente)
        Dim consulta = If(criterios, New CriteriosCliente())

        Dim resultado As New ResultadoPaginado(Of Cliente) With {
            .Pagina = consulta.Pagina,
            .TamanoPagina = consulta.TamanoPagina
        }

        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Cliente_Listar")

                comando.Parameters.Add("@Busqueda", SqlDbType.NVarChar, 100).Value =
                    If(String.IsNullOrWhiteSpace(consulta.Busqueda), CObj(DBNull.Value), consulta.Busqueda.Trim())

                comando.Parameters.Add("@Orden", SqlDbType.NVarChar, 20).Value = consulta.Orden
                comando.Parameters.Add("@Descendente", SqlDbType.Bit).Value = consulta.Descendente
                comando.Parameters.Add("@Pagina", SqlDbType.Int).Value = consulta.Pagina
                comando.Parameters.Add("@TamanoPagina", SqlDbType.Int).Value = consulta.TamanoPagina

                Dim parametroTotal = comando.Parameters.Add("@TotalRegistros", SqlDbType.Int)
                parametroTotal.Direction = ParameterDirection.Output

                conexion.Open()

                Using lector = comando.ExecuteReader()
                    While lector.Read()
                        resultado.Elementos.Add(Mapear(lector))
                    End While
                End Using

                If parametroTotal.Value IsNot Nothing AndAlso Not Convert.IsDBNull(parametroTotal.Value) Then
                    resultado.TotalRegistros = CInt(parametroTotal.Value)
                End If
            End Using
        End Using

        Return resultado
    End Function

    ''' <summary>Obtiene un cliente por su identificador. Devuelve Nothing si no existe.</summary>
    Public Function ObtenerPorId(clienteId As Integer) As Cliente
        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Cliente_ObtenerPorId")

                comando.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId

                conexion.Open()

                Using lector = comando.ExecuteReader(CommandBehavior.SingleRow)
                    If Not lector.Read() Then Return Nothing
                    Return Mapear(lector)
                End Using
            End Using
        End Using
    End Function

    ''' <summary>Inserta un cliente y devuelve el identificador asignado.</summary>
    ''' <exception cref="ReglaNegocioException">Si el documento ya está registrado.</exception>
    Public Function Insertar(cliente As Cliente, usuarioId As Integer, nombreUsuario As String) As Integer
        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Cliente_Insertar")

                AgregarParametrosDeCliente(comando, cliente)
                AgregarParametrosDeAuditoria(comando, usuarioId, nombreUsuario)

                Dim parametroId = comando.Parameters.Add("@ClienteId", SqlDbType.Int)
                parametroId.Direction = ParameterDirection.Output

                Try
                    conexion.Open()
                    comando.ExecuteNonQuery()
                Catch ex As SqlException
                    Throw SqlHelper.Traducir(ex)
                End Try

                Return CInt(parametroId.Value)
            End Using
        End Using
    End Function

    ''' <summary>Actualiza un cliente existente.</summary>
    ''' <exception cref="ReglaNegocioException">Si el cliente no existe o el documento se repite.</exception>
    Public Sub Actualizar(cliente As Cliente, usuarioId As Integer, nombreUsuario As String)
        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Cliente_Actualizar")

                comando.Parameters.Add("@ClienteId", SqlDbType.Int).Value = cliente.ClienteId
                AgregarParametrosDeCliente(comando, cliente)
                AgregarParametrosDeAuditoria(comando, usuarioId, nombreUsuario)

                Try
                    conexion.Open()
                    comando.ExecuteNonQuery()
                Catch ex As SqlException
                    Throw SqlHelper.Traducir(ex)
                End Try
            End Using
        End Using
    End Sub

    ''' <summary>Elimina un cliente. El snapshot del registro queda en la bitácora.</summary>
    ''' <exception cref="ReglaNegocioException">Si el cliente no existe.</exception>
    Public Sub Eliminar(clienteId As Integer, usuarioId As Integer, nombreUsuario As String)
        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Cliente_Eliminar")

                comando.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId
                AgregarParametrosDeAuditoria(comando, usuarioId, nombreUsuario)

                Try
                    conexion.Open()
                    comando.ExecuteNonQuery()
                Catch ex As SqlException
                    Throw SqlHelper.Traducir(ex)
                End Try
            End Using
        End Using
    End Sub

    ''' <summary>Agrega los parámetros de datos del cliente, convirtiendo cadenas vacías a NULL.</summary>
    Private Shared Sub AgregarParametrosDeCliente(comando As SqlCommand, cliente As Cliente)
        comando.Parameters.Add("@Nombres", SqlDbType.NVarChar, 100).Value = cliente.Nombres.Trim()
        comando.Parameters.Add("@Apellidos", SqlDbType.NVarChar, 100).Value = cliente.Apellidos.Trim()
        comando.Parameters.Add("@Documento", SqlDbType.NVarChar, 20).Value = cliente.Documento.Trim()
        comando.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = ValorOpcional(cliente.Email)
        comando.Parameters.Add("@Telefono", SqlDbType.NVarChar, 20).Value = ValorOpcional(cliente.Telefono)
        comando.Parameters.Add("@Direccion", SqlDbType.NVarChar, 250).Value = ValorOpcional(cliente.Direccion)
    End Sub

    Private Shared Sub AgregarParametrosDeAuditoria(comando As SqlCommand, usuarioId As Integer, nombreUsuario As String)
        comando.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId
        comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 50).Value = nombreUsuario
    End Sub

    ''' <summary>Un campo opcional vacío se guarda como NULL, no como cadena vacía.</summary>
    Private Shared Function ValorOpcional(valor As String) As Object
        If String.IsNullOrWhiteSpace(valor) Then Return DBNull.Value
        Return valor.Trim()
    End Function

    Private Shared Function Mapear(lector As IDataRecord) As Cliente
        Return New Cliente With {
            .ClienteId = lector.GetInt32(lector.GetOrdinal("ClienteId")),
            .Nombres = SqlHelper.LeerTexto(lector, "Nombres"),
            .Apellidos = SqlHelper.LeerTexto(lector, "Apellidos"),
            .Documento = SqlHelper.LeerTexto(lector, "Documento"),
            .Email = SqlHelper.LeerTexto(lector, "Email"),
            .Telefono = SqlHelper.LeerTexto(lector, "Telefono"),
            .Direccion = SqlHelper.LeerTexto(lector, "Direccion"),
            .FechaRegistro = lector.GetDateTime(lector.GetOrdinal("FechaRegistro"))
        }
    End Function

End Class
