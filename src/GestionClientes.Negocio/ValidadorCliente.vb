Imports System.Text.RegularExpressions
Imports GestionClientes.Entidades

''' <summary>
''' Única fuente de verdad de las reglas de validación de un cliente. Los validadores de la
''' interfaz web son una comodidad para el usuario, no un control de seguridad.
''' </summary>
Public NotInheritable Class ValidadorCliente

    Private Const LargoMaximoNombres As Integer = 100
    Private Const LargoMaximoApellidos As Integer = 100
    Private Const LargoMaximoEmail As Integer = 150
    Private Const LargoMaximoDireccion As Integer = 250

    ' Documento Único de Identidad salvadoreño: ocho dígitos, guion, un dígito.
    Private Shared ReadOnly PatronDocumento As New Regex("^\d{8}-\d$", RegexOptions.Compiled)

    ' Teléfono salvadoreño en formato ####-####.
    Private Shared ReadOnly PatronTelefono As New Regex("^\d{4}-\d{4}$", RegexOptions.Compiled)

    ' Permisiva a propósito: validar RFC 5322 con expresión regular es una fuente conocida de
    ' falsos negativos. Solo se descartan las formas claramente inválidas.
    Private Shared ReadOnly PatronEmail As New Regex("^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled)

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Errores encontrados; una lista vacía significa que el cliente es válido. Se acumulan todos
    ''' en lugar de devolver el primero, para corregir el formulario de una sola vez.
    ''' </summary>
    Public Shared Function Validar(cliente As Cliente) As List(Of String)
        Dim errores As New List(Of String)()

        If cliente Is Nothing Then
            errores.Add("No se recibieron datos del cliente.")
            Return errores
        End If

        ValidarNombres(cliente, errores)
        ValidarApellidos(cliente, errores)
        ValidarDocumento(cliente, errores)
        ValidarEmail(cliente, errores)
        ValidarTelefono(cliente, errores)
        ValidarDireccion(cliente, errores)

        Return errores
    End Function

    Private Shared Sub ValidarNombres(cliente As Cliente, errores As List(Of String))
        If String.IsNullOrWhiteSpace(cliente.Nombres) Then
            errores.Add("Los nombres son obligatorios.")
        ElseIf cliente.Nombres.Trim().Length > LargoMaximoNombres Then
            errores.Add($"Los nombres no pueden exceder {LargoMaximoNombres} caracteres.")
        End If
    End Sub

    Private Shared Sub ValidarApellidos(cliente As Cliente, errores As List(Of String))
        If String.IsNullOrWhiteSpace(cliente.Apellidos) Then
            errores.Add("Los apellidos son obligatorios.")
        ElseIf cliente.Apellidos.Trim().Length > LargoMaximoApellidos Then
            errores.Add($"Los apellidos no pueden exceder {LargoMaximoApellidos} caracteres.")
        End If
    End Sub

    Private Shared Sub ValidarDocumento(cliente As Cliente, errores As List(Of String))
        If String.IsNullOrWhiteSpace(cliente.Documento) Then
            errores.Add("El documento es obligatorio.")
        ElseIf Not PatronDocumento.IsMatch(cliente.Documento.Trim()) Then
            errores.Add("El documento debe tener el formato 00000000-0.")
        End If
    End Sub

    Private Shared Sub ValidarEmail(cliente As Cliente, errores As List(Of String))
        If String.IsNullOrWhiteSpace(cliente.Email) Then Return

        Dim email = cliente.Email.Trim()

        If email.Length > LargoMaximoEmail Then
            errores.Add($"El correo electrónico no puede exceder {LargoMaximoEmail} caracteres.")
        ElseIf Not PatronEmail.IsMatch(email) Then
            errores.Add("El correo electrónico no tiene un formato válido.")
        End If
    End Sub

    Private Shared Sub ValidarTelefono(cliente As Cliente, errores As List(Of String))
        If String.IsNullOrWhiteSpace(cliente.Telefono) Then Return

        If Not PatronTelefono.IsMatch(cliente.Telefono.Trim()) Then
            errores.Add("El teléfono debe tener el formato 0000-0000.")
        End If
    End Sub

    Private Shared Sub ValidarDireccion(cliente As Cliente, errores As List(Of String))
        If String.IsNullOrWhiteSpace(cliente.Direccion) Then Return

        If cliente.Direccion.Trim().Length > LargoMaximoDireccion Then
            errores.Add($"La dirección no puede exceder {LargoMaximoDireccion} caracteres.")
        End If
    End Sub

End Class
