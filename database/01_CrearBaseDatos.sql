/*
    Sistema de Gestión de Clientes
    Script de creación de base de datos, tablas, índices, procedimientos y datos iniciales.

    Motor:     SQL Server 2016 o superior (usa CREATE OR ALTER, THROW y FOR JSON).
    Ejecución: sqlcmd -S .\SQLEXPRESS -E -i 01_CrearBaseDatos.sql
               o abrir en SSMS y ejecutar completo.

    El script es idempotente: puede ejecutarse varias veces sin error y sin duplicar datos.
*/

/*
    QUOTED_IDENTIFIER y ANSI_NULLS deben estar activos para todo el script.

    El índice único de Documento es filtrado, y SQL Server exige QUOTED_IDENTIFIER ON tanto para
    crearlo como para cualquier INSERT o UPDATE sobre la tabla que lo tiene. Además, los
    procedimientos almacenados congelan estas opciones en el momento de crearse: uno creado con
    QUOTED_IDENTIFIER OFF falla en ejecución con el error 1934 al tocar esa tabla.

    Hace falta declararlo porque los valores por omisión no coinciden entre herramientas: SSMS
    conecta con QUOTED_IDENTIFIER ON y sqlcmd con OFF. Sin estas líneas el script funciona al
    ejecutarlo desde SSMS y falla desde la línea de comandos.
*/
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
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
        FechaRegistro     DATETIME2(0) NOT NULL CONSTRAINT DF_Clientes_FechaRegistro DEFAULT (SYSDATETIME()),
        CreadoPor         INT          NULL,
        FechaModificacion DATETIME2(0) NULL,
        ModificadoPor     INT          NULL,
        Eliminado         BIT          NOT NULL CONSTRAINT DF_Clientes_Eliminado DEFAULT (0),
        FechaEliminacion  DATETIME2(0) NULL,
        EliminadoPor      INT          NULL,
        [RowVersion]      ROWVERSION   NOT NULL,
        CONSTRAINT PK_Clientes PRIMARY KEY CLUSTERED (ClienteId)
    );

    PRINT 'Tabla Clientes creada.';
END
GO

/*
    Las columnas se agregan aparte para que el script repare una tabla creada por una corrida que
    se interrumpió. Las de "quién" son NULL y no llevan clave foránea a Usuarios: registran quién
    hizo la acción en el momento en que se hizo, y ese hecho no debe dejar de ser cierto si el
    usuario se elimina después.
*/
IF COL_LENGTH('dbo.Clientes', 'Eliminado') IS NULL
BEGIN
    ALTER TABLE dbo.Clientes ADD
        CreadoPor         INT          NULL,
        FechaModificacion DATETIME2(0) NULL,
        ModificadoPor     INT          NULL,
        Eliminado         BIT          NOT NULL CONSTRAINT DF_Clientes_Eliminado DEFAULT (0),
        FechaEliminacion  DATETIME2(0) NULL,
        EliminadoPor      INT          NULL;

    PRINT 'Columnas de auditoria y borrado logico agregadas a Clientes.';
END
GO

/*
    El índice de documento es único solo entre los clientes vigentes.

    Sin el filtro, borrar lógicamente a un cliente dejaría su DUI bloqueado para siempre: nadie
    podría volver a registrarse con ese documento aunque el registro anterior ya no exista para el
    negocio. Con el filtro, el documento se libera al borrar y sigue siendo único entre los vivos.

    La guarda comprueba que el índice exista Y que esté filtrado: un índice sin filtro no sirve
    aquí, así que se reemplaza en lugar de darlo por bueno.
*/
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Clientes_Documento'
                     AND object_id = OBJECT_ID('dbo.Clientes')
                     AND has_filter = 1)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_Clientes_Documento' AND object_id = OBJECT_ID('dbo.Clientes'))
    BEGIN
        DROP INDEX UX_Clientes_Documento ON dbo.Clientes;
        PRINT 'Indice UX_Clientes_Documento anterior eliminado para agregarle el filtro.';
    END

    CREATE UNIQUE INDEX UX_Clientes_Documento
        ON dbo.Clientes (Documento)
        WHERE Eliminado = 0;

    PRINT 'Indice UX_Clientes_Documento creado.';
END
GO

/*
    Igual que las columnas de auditoría: quien ejecute el script desde cero obtiene RowVersion en
    el CREATE TABLE, y sobre una tabla ya creada se agrega sin perder datos. SQL Server rellena el
    valor inicial de cada fila existente.
*/
IF COL_LENGTH('dbo.Clientes', 'RowVersion') IS NULL
BEGIN
    ALTER TABLE dbo.Clientes ADD [RowVersion] ROWVERSION NOT NULL;
    PRINT 'Columna RowVersion agregada a Clientes.';
END
GO

/* ------------------------------------------------------------------ Bitacora */
/*
    Bitacora.ClienteId NO lleva clave foránea hacia Clientes de forma deliberada.

    Esta tabla es el registro de auditoría del sistema, no el histórico de una entidad concreta.
    Clientes es lo primero que se audita, no lo único que se auditará: mañana registra un cambio
    de configuración o un acceso, acciones que no tienen cliente asociado. Una clave foránea la
    ataría para siempre a la primera entidad que se le ocurrió auditar a alguien.

    Y aunque el borrado de clientes hoy es lógico, la auditoría debe sobrevivir incluso a una
    purga física que haga soporte algún día. Por eso el snapshot completo del registro queda en la
    columna Detalle: la bitácora no depende de que la fila original siga existiendo.

    NombreUsuario se desnormaliza a propósito: preserva el nombre con el que el usuario actuó
    aunque después se renombre. UsuarioId sí lleva clave foránea, porque el actor de una acción
    auditada debe existir: la integridad importa más aquí que poder borrar usuarios, y el sistema
    no ofrece esa operación. Si algún día la ofreciera, sería con baja lógica, no con DELETE.
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
    Clientes que cumplen el filtro de búsqueda.

    Existe para que el predicado se escriba una sola vez: el procedimiento de listado lo necesita
    dos veces, una para contar y otra para traer la página, y dos copias pueden desincronizarse y
    producir un paginador que no concuerda con la rejilla.

    Es una función en línea a propósito. El optimizador la expande dentro del plan de la consulta
    que la invoca, así que no materializa nada. Una función multi-sentencia o una tabla temporal
    sí materializarían el conjunto completo, que es exactamente lo que la paginación evita.
*/
CREATE OR ALTER FUNCTION dbo.fn_Clientes_Filtrados
(
    @Busqueda NVARCHAR(100)
)
RETURNS TABLE
AS
RETURN
(
    SELECT  c.ClienteId,
            c.Nombres,
            c.Apellidos,
            c.Documento,
            c.Email,
            c.Telefono,
            c.Direccion,
            c.FechaRegistro,
            c.CreadoPor,
            c.FechaModificacion,
            c.ModificadoPor,
            c.Eliminado,
            c.FechaEliminacion,
            c.EliminadoPor,
            c.[RowVersion]
    FROM    dbo.Clientes AS c
            /*
                Los comodines que el usuario escriba se tratan como texto literal. Sin esto,
                teclear % en el buscador devuelve todos los clientes y _ actúa como comodín de un
                carácter, que no es lo que nadie espera de una caja de búsqueda. Se escapa primero
                la propia barra invertida, porque hacerlo después volvería a escapar las que
                introducen los reemplazos siguientes.

                El patrón se calcula una sola vez con CROSS APPLY en lugar de repetirlo en cada
                comparación: una función en línea no admite DECLARE.
            */
            CROSS APPLY (VALUES (
                '%' +
                REPLACE(REPLACE(REPLACE(REPLACE(
                    LTRIM(RTRIM(@Busqueda)),
                    '\', '\\'),
                    '%', '\%'),
                    '_', '\_'),
                    '[', '\[')
                + '%'
            )) AS f(Patron)
    /*
        Los clientes borrados lógicamente no existen para el negocio. El paréntesis alrededor de
        las alternativas de búsqueda es obligatorio: AND liga más fuerte que OR, así que sin él el
        filtro de borrado solo aplicaría a la primera alternativa y los borrados reaparecerían en
        cuanto alguien buscara por apellido.
    */
    WHERE   c.Eliminado = 0
            AND (
                @Busqueda IS NULL
                OR LTRIM(RTRIM(@Busqueda)) = ''
                OR c.Nombres   LIKE f.Patron ESCAPE '\'
                OR c.Apellidos LIKE f.Patron ESCAPE '\'
                OR c.Documento LIKE f.Patron ESCAPE '\'
            )
);
GO

/*
    Lista clientes con filtro, ordenamiento y paginación en el motor.

    La paginación se hace aquí y no en la interfaz: devolver todas las filas para mostrar diez
    obliga a transportarlas y materializarlas enteras. @TotalRegistros sale por parámetro de
    salida para que la interfaz pueda dibujar el paginador sin una segunda consulta.

    El ordenamiento NO usa SQL dinámico. @Orden llega como texto pero nunca se concatena en la
    consulta: se resuelve con expresiones CASE, de modo que un valor arbitrario no puede
    ejecutarse. Se usa una expresión por columna y dirección porque un CASE devuelve un solo
    tipo, y mezclar NVARCHAR con DATETIME2 forzaría conversiones implícitas.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Cliente_Listar
    @Busqueda       NVARCHAR(100) = NULL,
    @Orden          NVARCHAR(20)  = 'Apellidos',
    @Descendente    BIT           = 0,
    @Pagina         INT           = 1,
    @TamanoPagina   INT           = 10,
    @TotalRegistros INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 10;

    SELECT @TotalRegistros = COUNT(*) FROM dbo.fn_Clientes_Filtrados(@Busqueda);

    /*
        La lista de columnas debe coincidir con la de usp_Cliente_ObtenerPorId: ClienteDAL.Mapear
        es una sola función compartida por ambos caminos y lee todas por nombre. Si una falta
        aquí, el listado revienta en ejecución con IndexOutOfRangeException, no al compilar.
    */
    SELECT  ClienteId,
            Nombres,
            Apellidos,
            Documento,
            Email,
            Telefono,
            Direccion,
            FechaRegistro,
            CreadoPor,
            FechaModificacion,
            ModificadoPor,
            Eliminado,
            FechaEliminacion,
            EliminadoPor,
            [RowVersion]
    FROM    dbo.fn_Clientes_Filtrados(@Busqueda)
    ORDER BY
            CASE WHEN @Descendente = 0 AND @Orden = 'Documento'     THEN Documento     END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'Documento'     THEN Documento     END DESC,
            CASE WHEN @Descendente = 0 AND @Orden = 'Nombres'       THEN Nombres       END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'Nombres'       THEN Nombres       END DESC,
            CASE WHEN @Descendente = 0 AND @Orden = 'Apellidos'     THEN Apellidos     END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'Apellidos'     THEN Apellidos     END DESC,
            CASE WHEN @Descendente = 0 AND @Orden = 'FechaRegistro' THEN FechaRegistro END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'FechaRegistro' THEN FechaRegistro END DESC,
            /* Desempate obligatorio: sin un orden total, dos filas con el mismo apellido
               podrían intercambiarse entre páginas y una de ellas no aparecer nunca. */
            ClienteId
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
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
            FechaRegistro,
            CreadoPor,
            FechaModificacion,
            ModificadoPor,
            Eliminado,
            FechaEliminacion,
            EliminadoPor,
            [RowVersion]
    FROM    dbo.Clientes
    WHERE   ClienteId = @ClienteId
            AND Eliminado = 0;
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

    /* Un documento que perteneció a un cliente borrado vuelve a estar libre. */
    IF EXISTS (SELECT 1 FROM dbo.Clientes WHERE Documento = @Documento AND Eliminado = 0)
        THROW 50001, 'Ya existe un cliente registrado con ese documento.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Clientes (Nombres, Apellidos, Documento, Email, Telefono, Direccion, CreadoPor)
        VALUES (@Nombres, @Apellidos, @Documento, @Email, @Telefono, @Direccion, @UsuarioId);

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
    Actualiza un cliente y registra el cambio en la bitácora, en la misma transacción.

    El UPDATE filtra además por RowVersion. Si otra sesión modificó la fila desde que este usuario
    la cargó, el valor ya no coincide, no se actualiza ninguna fila y se lanza 50003. Sin esa
    comparación, el segundo en guardar sobrescribe el cambio del primero con los valores que tenía
    en pantalla, y nadie se entera.
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
    @NombreUsuario NVARCHAR(50),
    @RowVersion    BINARY(8)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE ClienteId = @ClienteId AND Eliminado = 0)
        THROW 50002, 'El cliente indicado no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Clientes
               WHERE Documento = @Documento AND ClienteId <> @ClienteId AND Eliminado = 0)
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
        SET     Nombres           = @Nombres,
                Apellidos         = @Apellidos,
                Documento         = @Documento,
                Email             = @Email,
                Telefono          = @Telefono,
                Direccion         = @Direccion,
                FechaModificacion = SYSDATETIME(),
                ModificadoPor     = @UsuarioId
        WHERE   ClienteId = @ClienteId
                AND [RowVersion] = @RowVersion;

        IF @@ROWCOUNT = 0
            THROW 50003, 'Este cliente fue modificado por otro usuario mientras usted lo editaba. Vuelva a abrirlo para ver los datos actuales.', 1;

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

    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE ClienteId = @ClienteId AND Eliminado = 0)
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

        /*
            Borrado lógico. El registro deja de existir para la aplicación —no lo devuelve ningún
            listado ni ObtenerPorId— pero la fila permanece y solo soporte puede recuperarla desde
            la base. Un DELETE físico destruiría el extremo de relaciones que quizá ya no se pueden
            reconstruir.

            El snapshot se toma antes del cambio: registra el estado exacto en el momento de
            borrar, que es lo que interesa auditar.
        */
        UPDATE  dbo.Clientes
        SET     Eliminado        = 1,
                FechaEliminacion = SYSDATETIME(),
                EliminadoPor     = @UsuarioId
        WHERE   ClienteId = @ClienteId;

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
    Entradas de bitácora que cumplen los filtros. Existe por la misma razón que
    dbo.fn_Clientes_Filtrados: que el predicado se escriba una sola vez.
*/
CREATE OR ALTER FUNCTION dbo.fn_Bitacora_Filtrada
(
    @FechaDesde    DATETIME2(0),
    @FechaHasta    DATETIME2(0),
    @Accion        NVARCHAR(10),
    @NombreUsuario NVARCHAR(50)
)
RETURNS TABLE
AS
RETURN
(
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
);
GO

/*
    Consulta la bitácora con filtros, ordenamiento y paginación en el motor. Importa más aquí que
    en clientes: cada fila lleva un Detalle NVARCHAR(MAX) con el JSON del cambio, así que traer
    la tabla entera para mostrar quince filas es especialmente costoso.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Bitacora_Listar
    @FechaDesde     DATETIME2(0) = NULL,
    @FechaHasta     DATETIME2(0) = NULL,
    @Accion         NVARCHAR(10) = NULL,
    @NombreUsuario  NVARCHAR(50) = NULL,
    @Orden          NVARCHAR(20) = 'FechaHora',
    @Descendente    BIT          = 1,
    @Pagina         INT          = 1,
    @TamanoPagina   INT          = 15,
    @TotalRegistros INT          OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 15;

    SELECT @TotalRegistros = COUNT(*)
    FROM   dbo.fn_Bitacora_Filtrada(@FechaDesde, @FechaHasta, @Accion, @NombreUsuario);

    SELECT  BitacoraId,
            Accion,
            ClienteId,
            UsuarioId,
            NombreUsuario,
            FechaHora,
            Detalle
    FROM    dbo.fn_Bitacora_Filtrada(@FechaDesde, @FechaHasta, @Accion, @NombreUsuario)
    ORDER BY
            CASE WHEN @Descendente = 0 AND @Orden = 'FechaHora'     THEN FechaHora     END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'FechaHora'     THEN FechaHora     END DESC,
            CASE WHEN @Descendente = 0 AND @Orden = 'Accion'        THEN Accion        END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'Accion'        THEN Accion        END DESC,
            CASE WHEN @Descendente = 0 AND @Orden = 'NombreUsuario' THEN NombreUsuario END ASC,
            CASE WHEN @Descendente = 1 AND @Orden = 'NombreUsuario' THEN NombreUsuario END DESC,
            BitacoraId DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO

/*
    Nombres de usuario distintos presentes en la bitácora, para poblar el filtro de la pantalla.

    Existe como procedimiento propio porque, con la consulta principal ya paginada, derivar la
    lista recorriendo una página daría un desplegable incompleto.
*/
CREATE OR ALTER PROCEDURE dbo.usp_Bitacora_ListarUsuarios
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT NombreUsuario
    FROM   dbo.Bitacora
    ORDER BY NombreUsuario;
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
