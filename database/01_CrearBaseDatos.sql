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

/* ==================================================================
   PROCEDIMIENTOS ALMACENADOS
   ================================================================== */

/*
    Devuelve el hash y el salt del usuario para que la aplicación verifique la contraseña.
    La comparación NO se hace en SQL: el hash nunca viaja como criterio de búsqueda.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Usuario_ObtenerPorNombre
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  UsuarioId,
            NombreUsuario,
            NombreCompleto,
            PasswordHash,
            PasswordSalt,
            Activo,
            FechaCreacion
    FROM    dbo.Usuarios
    WHERE   NombreUsuario = @NombreUsuario;
END
GO

/*
    Lista clientes. @Busqueda filtra por nombres, apellidos o documento.
    El comodín se aplica con parámetro, nunca concatenando la cadena de búsqueda en el SQL.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Cliente_Listar
    @Busqueda NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Patron NVARCHAR(110) = '%' + ISNULL(LTRIM(RTRIM(@Busqueda)), '') + '%';

    SELECT  ClienteId,
            Nombres,
            Apellidos,
            Documento,
            Email,
            Telefono,
            Direccion,
            FechaRegistro
    FROM    dbo.Clientes
    WHERE   @Busqueda IS NULL
            OR LTRIM(RTRIM(@Busqueda)) = ''
            OR Nombres   LIKE @Patron
            OR Apellidos LIKE @Patron
            OR Documento LIKE @Patron
    ORDER BY Apellidos, Nombres;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Cliente_ObtenerPorId
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  ClienteId,
            Nombres,
            Apellidos,
            Documento,
            Email,
            Telefono,
            Direccion,
            FechaRegistro
    FROM    dbo.Clientes
    WHERE   ClienteId = @ClienteId;
END
GO

/*
    Inserta un cliente y registra la acción en la bitácora dentro de la misma transacción.
    Si cualquiera de las dos operaciones falla, no se aplica ninguna: la auditoría no puede
    quedar desincronizada de los datos.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Cliente_Insertar
    @Nombres       NVARCHAR(100),
    @Apellidos     NVARCHAR(100),
    @Documento     NVARCHAR(20),
    @Email         NVARCHAR(150) = NULL,
    @Telefono      NVARCHAR(20)  = NULL,
    @Direccion     NVARCHAR(250) = NULL,
    @UsuarioId     INT,
    @NombreUsuario NVARCHAR(50),
    @ClienteId     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.Clientes WHERE Documento = @Documento)
        THROW 50001, 'Ya existe un cliente registrado con ese documento.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Clientes (Nombres, Apellidos, Documento, Email, Telefono, Direccion)
        VALUES (@Nombres, @Apellidos, @Documento, @Email, @Telefono, @Direccion);

        SET @ClienteId = CAST(SCOPE_IDENTITY() AS INT);

        DECLARE @Detalle NVARCHAR(MAX) =
        (
            SELECT ClienteId, Nombres, Apellidos, Documento, Email, Telefono, Direccion
            FROM   dbo.Clientes
            WHERE  ClienteId = @ClienteId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.Bitacora (Accion, ClienteId, UsuarioId, NombreUsuario, Detalle)
        VALUES ('AGREGAR', @ClienteId, @UsuarioId, @NombreUsuario, @Detalle);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/*
    Actualiza un cliente y registra en bitácora el estado anterior y el nuevo.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Cliente_Actualizar
    @ClienteId     INT,
    @Nombres       NVARCHAR(100),
    @Apellidos     NVARCHAR(100),
    @Documento     NVARCHAR(20),
    @Email         NVARCHAR(150) = NULL,
    @Telefono      NVARCHAR(20)  = NULL,
    @Direccion     NVARCHAR(250) = NULL,
    @UsuarioId     INT,
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE ClienteId = @ClienteId)
        THROW 50002, 'El cliente indicado no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Clientes WHERE Documento = @Documento AND ClienteId <> @ClienteId)
        THROW 50001, 'Ya existe un cliente registrado con ese documento.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Anterior NVARCHAR(MAX) =
        (
            SELECT ClienteId, Nombres, Apellidos, Documento, Email, Telefono, Direccion
            FROM   dbo.Clientes
            WHERE  ClienteId = @ClienteId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        UPDATE  dbo.Clientes
        SET     Nombres   = @Nombres,
                Apellidos = @Apellidos,
                Documento = @Documento,
                Email     = @Email,
                Telefono  = @Telefono,
                Direccion = @Direccion
        WHERE   ClienteId = @ClienteId;

        DECLARE @Nuevo NVARCHAR(MAX) =
        (
            SELECT ClienteId, Nombres, Apellidos, Documento, Email, Telefono, Direccion
            FROM   dbo.Clientes
            WHERE  ClienteId = @ClienteId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.Bitacora (Accion, ClienteId, UsuarioId, NombreUsuario, Detalle)
        VALUES ('EDITAR', @ClienteId, @UsuarioId, @NombreUsuario,
                CONCAT('{"anterior":', @Anterior, ',"nuevo":', @Nuevo, '}'));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/*
    Elimina un cliente. El snapshot completo del registro se guarda en la bitácora ANTES
    del DELETE, porque después ya no habría de dónde recuperarlo.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Cliente_Eliminar
    @ClienteId     INT,
    @UsuarioId     INT,
    @NombreUsuario NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE ClienteId = @ClienteId)
        THROW 50002, 'El cliente indicado no existe.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Detalle NVARCHAR(MAX) =
        (
            SELECT ClienteId, Nombres, Apellidos, Documento, Email, Telefono, Direccion
            FROM   dbo.Clientes
            WHERE  ClienteId = @ClienteId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT INTO dbo.Bitacora (Accion, ClienteId, UsuarioId, NombreUsuario, Detalle)
        VALUES ('ELIMINAR', @ClienteId, @UsuarioId, @NombreUsuario, @Detalle);

        DELETE FROM dbo.Clientes WHERE ClienteId = @ClienteId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/*
    Consulta la bitácora con filtros opcionales. @FechaHasta se compara con < día siguiente
    para que el filtro incluya el día completo sin depender de la hora.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Bitacora_Listar
    @FechaDesde    DATETIME2(0) = NULL,
    @FechaHasta    DATETIME2(0) = NULL,
    @Accion        NVARCHAR(10) = NULL,
    @NombreUsuario NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  BitacoraId,
            Accion,
            ClienteId,
            UsuarioId,
            NombreUsuario,
            FechaHora,
            Detalle
    FROM    dbo.Bitacora
    WHERE   (@FechaDesde    IS NULL OR FechaHora >= @FechaDesde)
            AND (@FechaHasta    IS NULL OR FechaHora <  DATEADD(DAY, 1, CAST(@FechaHasta AS DATE)))
            AND (@Accion        IS NULL OR @Accion = ''        OR Accion = @Accion)
            AND (@NombreUsuario IS NULL OR @NombreUsuario = '' OR NombreUsuario = @NombreUsuario)
    ORDER BY FechaHora DESC, BitacoraId DESC;
END
GO

/* ==================================================================
   DATOS INICIALES
   ==================================================================
   Usuario administrador para poder ingresar al sistema.

   Usuario: admin
   Contraseña: Admin123$

   El hash es PBKDF2-HMAC-SHA256, 100000 iteraciones, salt de 16 bytes, hash de 32 bytes.
   Se calculó fuera de SQL porque T-SQL no implementa PBKDF2; usar HASHBYTES aquí produciría
   un hash incompatible con el que verifica la aplicación.
   ================================================================== */

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'admin')
BEGIN
    INSERT INTO dbo.Usuarios (NombreUsuario, NombreCompleto, PasswordHash, PasswordSalt, Activo)
    VALUES ('admin', 'Administrador del Sistema', 0xCB653E938368EEED3D767768CFC4223D1F4AFC4AA76DF05EB3076FF21C791540, 0x3B36B91114CB21D1E0B567D066F8A178, 1);

    PRINT 'Usuario administrador creado.';
END
ELSE
    PRINT 'Usuario administrador ya existe; se conserva.';
GO

PRINT 'Instalacion completada.';
GO
