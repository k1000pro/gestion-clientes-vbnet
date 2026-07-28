/*
    Sistema de Gestión de Clientes
    Script de creación de base de datos, tablas, índices, procedimientos y datos iniciales.

    Motor:     SQL Server 2016 o superior (usa CREATE OR ALTER, THROW y FOR JSON).
    Ejecución: sqlcmd -S .\SQLEXPRESS -E -i 01_CrearBaseDatos.sql
               o abrir en SSMS y ejecutar completo.

    El script es idempotente: puede ejecutarse varias veces sin error y sin duplicar datos.
*/

SET NOCOUNT ON;
GO

IF DB_ID('GestionClientesDB') IS NULL
BEGIN
    CREATE DATABASE GestionClientesDB;
    PRINT 'Base de datos GestionClientesDB creada.';
END
ELSE
    PRINT 'Base de datos GestionClientesDB ya existe; se conserva.';
GO

USE GestionClientesDB;
GO

/* ------------------------------------------------------------------ Usuarios */
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        UsuarioId      INT            IDENTITY(1,1) NOT NULL,
        NombreUsuario  NVARCHAR(50)   NOT NULL,
        NombreCompleto NVARCHAR(150)  NOT NULL,
        PasswordHash   VARBINARY(32)  NOT NULL,
        PasswordSalt   VARBINARY(16)  NOT NULL,
        Activo         BIT            NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        FechaCreacion  DATETIME2(0)   NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (UsuarioId)
    );

    PRINT 'Tabla Usuarios creada.';
END
GO

/*
    Los índices se crean fuera del guard de su tabla, cada uno con su propia comprobación.
    Si estuvieran dentro, una corrida previa que hubiera creado la tabla pero se hubiera
    interrumpido antes del índice dejaría la base sin la restricción de unicidad, y toda
    reejecución posterior saltaría el bloque entero al ver que la tabla ya existe. El script
    diría "ya existe" y la base quedaría sin su índice único, en silencio.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Usuarios_NombreUsuario' AND object_id = OBJECT_ID('dbo.Usuarios'))
BEGIN
    CREATE UNIQUE INDEX UX_Usuarios_NombreUsuario ON dbo.Usuarios (NombreUsuario);
    PRINT 'Indice UX_Usuarios_NombreUsuario creado.';
END
GO

/* ------------------------------------------------------------------ Clientes */
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        ClienteId     INT            IDENTITY(1,1) NOT NULL,
        Nombres       NVARCHAR(100)  NOT NULL,
        Apellidos     NVARCHAR(100)  NOT NULL,
        Documento     NVARCHAR(20)   NOT NULL,
        Email         NVARCHAR(150)  NULL,
        Telefono      NVARCHAR(20)   NULL,
        Direccion     NVARCHAR(250)  NULL,
        FechaRegistro DATETIME2(0)   NOT NULL CONSTRAINT DF_Clientes_FechaRegistro DEFAULT (SYSDATETIME()),
        CONSTRAINT PK_Clientes PRIMARY KEY CLUSTERED (ClienteId)
    );

    PRINT 'Tabla Clientes creada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Clientes_Documento' AND object_id = OBJECT_ID('dbo.Clientes'))
BEGIN
    CREATE UNIQUE INDEX UX_Clientes_Documento ON dbo.Clientes (Documento);
    PRINT 'Indice UX_Clientes_Documento creado.';
END
GO

/* ------------------------------------------------------------------ Bitacora */
/*
    Bitacora.ClienteId NO lleva clave foránea hacia Clientes de forma deliberada.
    El borrado de clientes es físico; una FK obligaría a borrar en cascada (destruyendo el
    historial que esta tabla existe para conservar) o a bloquear el borrado. El snapshot del
    registro eliminado queda en la columna Detalle.

    NombreUsuario se desnormaliza a propósito: preserva el valor histórico aunque el usuario
    se renombre o se elimine después.
*/
IF OBJECT_ID('dbo.Bitacora', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bitacora
    (
        BitacoraId    BIGINT         IDENTITY(1,1) NOT NULL,
        Accion        NVARCHAR(10)   NOT NULL,
        ClienteId     INT            NOT NULL,
        UsuarioId     INT            NOT NULL,
        NombreUsuario NVARCHAR(50)   NOT NULL,
        FechaHora     DATETIME2(0)   NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (SYSDATETIME()),
        Detalle       NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_Bitacora PRIMARY KEY CLUSTERED (BitacoraId),
        CONSTRAINT CK_Bitacora_Accion CHECK (Accion IN ('AGREGAR', 'EDITAR', 'ELIMINAR')),
        CONSTRAINT FK_Bitacora_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (UsuarioId)
    );

    PRINT 'Tabla Bitacora creada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Bitacora_FechaHora' AND object_id = OBJECT_ID('dbo.Bitacora'))
BEGIN
    CREATE INDEX IX_Bitacora_FechaHora ON dbo.Bitacora (FechaHora DESC);
    PRINT 'Indice IX_Bitacora_FechaHora creado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_Bitacora_ClienteId' AND object_id = OBJECT_ID('dbo.Bitacora'))
BEGIN
    CREATE INDEX IX_Bitacora_ClienteId ON dbo.Bitacora (ClienteId);
    PRINT 'Indice IX_Bitacora_ClienteId creado.';
END
GO
