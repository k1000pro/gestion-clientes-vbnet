Imports System.Data
Imports System.Data.SqlClient
Imports GestionClientes.Entidades

''' <summary>
''' Acceso de solo lectura a la bitácora. No expone métodos de escritura ni de borrado a
''' propósito: las entradas las genera exclusivamente el procedimiento almacenado que modifica al
''' cliente, y un registro de auditoría que la aplicación pueda alterar no sirve como auditoría.
''' </summary>
Public Class BitacoraDAL

    ''' <summary>Consulta la bitácora aplicando los filtros indicados.</summary>
    Public Function Listar(filtro As FiltroBitacora) As List(Of RegistroBitacora)
        Dim registros As New List(Of RegistroBitacora)()
        Dim criterios = If(filtro, New FiltroBitacora())

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

                conexion.Open()

                Using lector = comando.ExecuteReader()
                    While lector.Read()
                        registros.Add(New RegistroBitacora With {
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
            End Using
        End Using

        Return registros
    End Function

    ''' <summary>Nombres de usuario distintos presentes en la bitácora, para poblar el filtro.</summary>
    Public Function ObtenerUsuarios() As List(Of String)
        Dim usuarios As New List(Of String)()

        For Each registro In Listar(Nothing)
            If Not usuarios.Contains(registro.NombreUsuario) Then
                usuarios.Add(registro.NombreUsuario)
            End If
        Next

        usuarios.Sort()
        Return usuarios
    End Function

End Class
