Imports GestionClientes.Datos
Imports GestionClientes.Entidades

''' <summary>Consulta de la bitácora de auditoría.</summary>
Public Class ServicioBitacora

    Private ReadOnly _bitacora As New BitacoraDAL()

    Public Function Listar(filtro As FiltroBitacora) As ResultadoPaginado(Of RegistroBitacora)
        Return _bitacora.Listar(filtro)
    End Function

    Public Function ObtenerUsuarios() As List(Of String)
        Return _bitacora.ObtenerUsuarios()
    End Function

End Class
