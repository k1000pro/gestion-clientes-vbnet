Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports GestionClientes.Entidades
Imports GestionClientes.Negocio

<TestClass()>
Public Class PruebasValidadorCliente

    ''' <summary>Cliente válido de referencia; cada prueba altera solo el campo que examina.</summary>
    Private Shared Function ClienteValido() As Cliente
        Return New Cliente With {
            .Nombres = "Ana Maria",
            .Apellidos = "Perez Rivas",
            .Documento = "01234567-8",
            .Email = "ana.perez@ejemplo.com",
            .Telefono = "7777-7777",
            .Direccion = "Colonia Escalon, San Salvador"
        }
    End Function

    <TestMethod()>
    Public Sub Validar_AceptaUnClienteCompletoYCorrecto()
        Dim errores = ValidadorCliente.Validar(ClienteValido())

        Assert.AreEqual(0, errores.Count, String.Join(" | ", errores))
    End Sub

    <TestMethod()>
    Public Sub Validar_AceptaClienteSinCamposOpcionales()
        Dim cliente = ClienteValido()
        cliente.Email = String.Empty
        cliente.Telefono = String.Empty
        cliente.Direccion = String.Empty

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(0, errores.Count, String.Join(" | ", errores))
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaClienteNothing()
        Dim errores = ValidadorCliente.Validar(Nothing)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_ExigeNombres()
        Dim cliente = ClienteValido()
        cliente.Nombres = "   "

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
        StringAssert.Contains(errores(0), "nombres")
    End Sub

    <TestMethod()>
    Public Sub Validar_ExigeApellidos()
        Dim cliente = ClienteValido()
        cliente.Apellidos = String.Empty

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
        StringAssert.Contains(errores(0), "apellidos")
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaNombresDemasiadoLargos()
        Dim cliente = ClienteValido()
        cliente.Nombres = New String("a"c, 101)

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_ExigeDocumento()
        Dim cliente = ClienteValido()
        cliente.Documento = String.Empty

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
        StringAssert.Contains(errores(0), "documento")
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaDocumentoSinGuion()
        Dim cliente = ClienteValido()
        cliente.Documento = "012345678"

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaDocumentoConLetras()
        Dim cliente = ClienteValido()
        cliente.Documento = "0123456A-8"

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaDocumentoConCantidadIncorrectaDeDigitos()
        Dim cliente = ClienteValido()
        cliente.Documento = "1234567-8"

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaEmailSinArroba()
        Dim cliente = ClienteValido()
        cliente.Email = "ana.perez.ejemplo.com"

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
        StringAssert.Contains(errores(0), "correo")
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaEmailSinDominio()
        Dim cliente = ClienteValido()
        cliente.Email = "ana@"

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaTelefonoConFormatoInvalido()
        Dim cliente = ClienteValido()
        cliente.Telefono = "77777777"

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
        StringAssert.Contains(errores(0), "teléfono")
    End Sub

    <TestMethod()>
    Public Sub Validar_RechazaDireccionDemasiadoLarga()
        Dim cliente = ClienteValido()
        cliente.Direccion = New String("a"c, 251)

        Dim errores = ValidadorCliente.Validar(cliente)

        Assert.AreEqual(1, errores.Count)
    End Sub

    <TestMethod()>
    Public Sub Validar_AcumulaTodosLosErroresEnLugarDeCortarEnElPrimero()
        ' La variable NO puede llamarse "cliente": los identificadores de VB.NET no distinguen
        ' mayúsculas, y una variable inferida cuyo inicializador menciona el tipo Cliente
        ' colisiona con él (BC30980).
        Dim clienteInvalido = New Cliente With {
            .Nombres = String.Empty,
            .Apellidos = String.Empty,
            .Documento = "invalido",
            .Email = "sin-arroba",
            .Telefono = "123"
        }

        Dim errores = ValidadorCliente.Validar(clienteInvalido)

        Assert.AreEqual(5, errores.Count, String.Join(" | ", errores))
    End Sub

End Class
