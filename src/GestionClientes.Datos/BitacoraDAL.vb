Imports System.Data
Imports System.Data.SqlClient
Imports GestionClientes.Entidades

''' <summary>
''' Acceso de solo lectura a la bitácora. Sin métodos de escritura ni de borrado a propósito: las
''' entradas las genera el procedimiento almacenado que modifica al cliente, y un registro de
''' auditoría que la aplicación pueda alterar no sirve como auditoría.
''' </summary>
Public Class BitacoraDAL

    ''' <summary>Devuelve una página de la bitácora junto con el total que cumple el filtro.</summary>
    Public Function Listar(filtro As FiltroBitacora) As ResultadoPaginado(Of RegistroBitacora)
        Dim criterios = If(filtro, New FiltroBitacora())

        Dim resultado As New ResultadoPaginado(Of RegistroBitacora) With {
            .Pagina = criterios.Pagina,
            .TamanoPagina = criterios.TamanoPagina
        }

        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Bitacora_Listar")

                comando.Parameters.Add("@FechaDesde", SqlDbType.DateTime2).Value =
                    If(criterios.FechaDesde.HasValue, CObj(criterios.FechaDesde.Value), DBNull.Value)

                comando.Parameters.Add("@FechaHasta", SqlDbType.DateTime2).Value =
                    If(criterios.FechaHasta.HasValue, CObj(criterios.FechaHasta.Value), DBNull.Value)

                comando.Parameters.Add("@Accion", SqlDbType.NVarChar, 10).Value =
                    If(String.IsNullOrWhiteSpace(criterios.Accion), CObj(DBNull.Value), criterios.Accion.Trim())

                comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 50).Value =
                    If(String.IsNullOrWhiteSpace(criterios.NombreUsuario), CObj(DBNull.Value), criterios.NombreUsuario.Trim())

                comando.Parameters.Add("@Orden", SqlDbType.NVarChar, 20).Value = criterios.Orden
                comando.Parameters.Add("@Descendente", SqlDbType.Bit).Value = criterios.Descendente
                comando.Parameters.Add("@Pagina", SqlDbType.Int).Value = criterios.Pagina
                comando.Parameters.Add("@TamanoPagina", SqlDbType.Int).Value = criterios.TamanoPagina

                Dim parametroTotal = comando.Parameters.Add("@TotalRegistros", SqlDbType.Int)
                parametroTotal.Direction = ParameterDirection.Output

                conexion.Open()

                Using lector = comando.ExecuteReader()
                    While lector.Read()
                        resultado.Elementos.Add(New RegistroBitacora With {
                            .BitacoraId = lector.GetInt64(lector.GetOrdinal("BitacoraId")),
                            .Accion = SqlHelper.LeerTexto(lector, "Accion"),
                            .ClienteId = lector.GetInt32(lector.GetOrdinal("ClienteId")),
                            .UsuarioId = lector.GetInt32(lector.GetOrdinal("UsuarioId")),
                            .NombreUsuario = SqlHelper.LeerTexto(lector, "NombreUsuario"),
                            .FechaHora = lector.GetDateTime(lector.GetOrdinal("FechaHora")),
                            .Detalle = SqlHelper.LeerTexto(lector, "Detalle")
                        })
                    End While
                End Using

                If parametroTotal.Value IsNot Nothing AndAlso Not Convert.IsDBNull(parametroTotal.Value) Then
                    resultado.TotalRegistros = CInt(parametroTotal.Value)
                End If
            End Using
        End Using

        Return resultado
    End Function

    ''' <summary>
    ''' Nombres de usuario distintos presentes en la bitácora, para poblar el filtro. Tiene su
    ''' propio procedimiento porque derivarlos del listado paginado daría solo los de una página.
    ''' </summary>
    Public Function ObtenerUsuarios() As List(Of String)
        Dim usuarios As New List(Of String)()

        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Bitacora_ListarUsuarios")

                conexion.Open()

                Using lector = comando.ExecuteReader()
                    While lector.Read()
                        usuarios.Add(SqlHelper.LeerTexto(lector, "NombreUsuario"))
                    End While
                End Using
            End Using
        End Using

        Return usuarios
    End Function

End Class
