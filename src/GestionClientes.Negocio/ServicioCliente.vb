Imports GestionClientes.Datos
Imports GestionClientes.Entidades

''' <summary>
''' Reglas de negocio de clientes. Valida antes de escribir y traduce los errores de regla a
''' resultados con mensaje, dejando pasar únicamente los fallos de infraestructura.
''' </summary>
Public Class ServicioCliente

    Private ReadOnly _clientes As New ClienteDAL()

    Public Function Listar(criterios As CriteriosCliente) As ResultadoPaginado(Of Cliente)
        Return _clientes.Listar(criterios)
    End Function

    Public Function ObtenerPorId(clienteId As Integer) As Cliente
        If clienteId <= 0 Then Return Nothing
        Return _clientes.ObtenerPorId(clienteId)
    End Function

    ''' <summary>
    ''' Guarda un cliente: lo inserta si ClienteId es 0, lo actualiza en caso contrario.
    ''' La validación ocurre aquí y no en la página, porque la página no es el único punto de
    ''' entrada posible y los validadores del navegador se pueden omitir.
    ''' </summary>
    Public Function Guardar(cliente As Cliente, usuarioId As Integer, nombreUsuario As String) As ResultadoOperacion
        Dim errores = ValidadorCliente.Validar(cliente)

        If errores.Count > 0 Then
            Return ResultadoOperacion.Fallo(errores)
        End If

        Try
            If cliente.ClienteId = 0 Then
                cliente.ClienteId = _clientes.Insertar(cliente, usuarioId, nombreUsuario)
                Return ResultadoOperacion.Exito("Cliente agregado correctamente.")
            End If

            _clientes.Actualizar(cliente, usuarioId, nombreUsuario)
            Return ResultadoOperacion.Exito("Cliente actualizado correctamente.")

        Catch ex As ReglaNegocioException
            Return ResultadoOperacion.Fallo(ex.Message)
        End Try
    End Function

    Public Function Eliminar(clienteId As Integer, usuarioId As Integer, nombreUsuario As String) As ResultadoOperacion
        If clienteId <= 0 Then
            Return ResultadoOperacion.Fallo("No se indicó el cliente a eliminar.")
        End If

        Try
            _clientes.Eliminar(clienteId, usuarioId, nombreUsuario)
            Return ResultadoOperacion.Exito("Cliente eliminado correctamente.")

        Catch ex As ReglaNegocioException
            Return ResultadoOperacion.Fallo(ex.Message)
        End Try
    End Function

End Class
