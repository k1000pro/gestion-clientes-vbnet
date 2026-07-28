Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports GestionClientes.Negocio

<TestClass()>
Public Class PruebasHash

    Private Const ContrasenaValida As String = "Admin123$"

    <TestMethod()>
    Public Sub GenerarSalt_DevuelveDieciseisBytes()
        Dim salt = Hash.GenerarSalt()

        Assert.IsNotNull(salt)
        Assert.AreEqual(16, salt.Length)
    End Sub

    <TestMethod()>
    Public Sub GenerarSalt_NoRepiteElMismoValor()
        Dim primero = Hash.GenerarSalt()
        Dim segundo = Hash.GenerarSalt()

        CollectionAssert.AreNotEqual(primero, segundo,
            "Dos salts consecutivos no deben coincidir; si coinciden, el generador no es aleatorio.")
    End Sub

    <TestMethod()>
    Public Sub Calcular_DevuelveTreintaYDosBytes()
        Dim hashCalculado = Hash.Calcular(ContrasenaValida, Hash.GenerarSalt())

        Assert.AreEqual(32, hashCalculado.Length)
    End Sub

    <TestMethod()>
    Public Sub Calcular_EsDeterministaConElMismoSalt()
        Dim salt = Hash.GenerarSalt()

        Dim primero = Hash.Calcular(ContrasenaValida, salt)
        Dim segundo = Hash.Calcular(ContrasenaValida, salt)

        CollectionAssert.AreEqual(primero, segundo)
    End Sub

    <TestMethod()>
    Public Sub Calcular_ProduceHashesDistintosConSaltsDistintos()
        Dim primero = Hash.Calcular(ContrasenaValida, Hash.GenerarSalt())
        Dim segundo = Hash.Calcular(ContrasenaValida, Hash.GenerarSalt())

        CollectionAssert.AreNotEqual(primero, segundo,
            "La misma contraseña con salts distintos debe producir hashes distintos.")
    End Sub

    <TestMethod()>
    Public Sub Verificar_AceptaLaContrasenaCorrecta()
        Dim salt = Hash.GenerarSalt()
        Dim hashCalculado = Hash.Calcular(ContrasenaValida, salt)

        Assert.IsTrue(Hash.Verificar(ContrasenaValida, salt, hashCalculado))
    End Sub

    <TestMethod()>
    Public Sub Verificar_RechazaLaContrasenaIncorrecta()
        Dim salt = Hash.GenerarSalt()
        Dim hashCalculado = Hash.Calcular(ContrasenaValida, salt)

        Assert.IsFalse(Hash.Verificar("otraContrasena", salt, hashCalculado))
    End Sub

    <TestMethod()>
    Public Sub Verificar_DistingueMayusculasDeMinusculas()
        Dim salt = Hash.GenerarSalt()
        Dim hashCalculado = Hash.Calcular(ContrasenaValida, salt)

        Assert.IsFalse(Hash.Verificar("admin123$", salt, hashCalculado))
    End Sub

    <TestMethod()>
    Public Sub Verificar_RechazaCuandoElHashEsperadoEsNothing()
        Assert.IsFalse(Hash.Verificar(ContrasenaValida, Hash.GenerarSalt(), Nothing))
    End Sub

    <TestMethod()>
    Public Sub Verificar_RechazaCuandoElHashEsperadoTieneOtraLongitud()
        Dim salt = Hash.GenerarSalt()

        Assert.IsFalse(Hash.Verificar(ContrasenaValida, salt, New Byte() {1, 2, 3}))
    End Sub

    <TestMethod()>
    <ExpectedException(GetType(ArgumentException))>
    Public Sub Calcular_RechazaContrasenaVacia()
        Hash.Calcular(String.Empty, Hash.GenerarSalt())
    End Sub

    <TestMethod()>
    <ExpectedException(GetType(ArgumentException))>
    Public Sub Calcular_RechazaSaltVacio()
        Hash.Calcular(ContrasenaValida, New Byte() {})
    End Sub

End Class
