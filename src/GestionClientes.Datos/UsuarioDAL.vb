Imports System.Data
Imports System.Data.SqlClient
Imports GestionClientes.Entidades

''' <summary>Acceso a datos de la tabla Usuarios.</summary>
Public Class UsuarioDAL

    ''' <summary>
    ''' Busca un usuario por su nombre. Devuelve Nothing si no existe.
    '''
    ''' Trae el hash y el salt para que la verificación ocurra en la aplicación. Enviar la
    ''' contraseña a SQL Server para compararla allí obligaría a transportarla y la expondría en
    ''' los planes de ejecución y en cualquier traza del motor.
    ''' </summary>
    Public Function ObtenerPorNombre(nombreUsuario As String) As Usuario
        If String.IsNullOrWhiteSpace(nombreUsuario) Then Return Nothing

        Using conexion As New SqlConnection(SqlHelper.ObtenerCadenaConexion())
            Using comando = SqlHelper.CrearComando(conexion, "dbo.usp_Usuario_ObtenerPorNombre")

                comando.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 50).Value = nombreUsuario.Trim()

                conexion.Open()

                Using lector = comando.ExecuteReader(CommandBehavior.SingleRow)
                    If Not lector.Read() Then Return Nothing

                    Return New Usuario With {
                        .UsuarioId = lector.GetInt32(lector.GetOrdinal("UsuarioId")),
                        .NombreUsuario = SqlHelper.LeerTexto(lector, "NombreUsuario"),
                        .NombreCompleto = SqlHelper.LeerTexto(lector, "NombreCompleto"),
                        .PasswordHash = SqlHelper.LeerBytes(lector, "PasswordHash"),
                        .PasswordSalt = SqlHelper.LeerBytes(lector, "PasswordSalt"),
                        .Activo = lector.GetBoolean(lector.GetOrdinal("Activo")),
                        .FechaCreacion = lector.GetDateTime(lector.GetOrdinal("FechaCreacion"))
                    }
                End Using
            End Using
        End Using
    End Function

End Class
