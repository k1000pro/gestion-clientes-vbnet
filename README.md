# Sistema de Gestión de Clientes

Aplicación web para el mantenimiento de información de clientes con control de acceso y
bitácora de auditoría.

Prueba técnica — Kamilo Martínez

## Tecnologías

- ASP.NET Web Forms (VB.NET) sobre .NET Framework 4.8
- SQL Server 2022 Express
- ADO.NET con procedimientos almacenados
- MSTest
- Bootstrap 5.3 (servido localmente)
- log4net

## Requisitos previos

- Visual Studio 2022 con la carga de trabajo **Desarrollo de ASP.NET y web**
- .NET Framework 4.8 Developer Pack
- SQL Server 2016 SP1 o superior (el script usa `CREATE OR ALTER`, `THROW` y `FOR JSON`)
- SQL Server Management Studio (opcional, para inspeccionar los datos)

## Instalación

### 1. Base de datos

Ejecutar `database/01_CrearBaseDatos.sql` en SSMS, o desde la línea de comandos:

```
sqlcmd -S .\SQLEXPRESS -E -i database\01_CrearBaseDatos.sql
```

El script crea la base `GestionClientesDB`, las tablas `Usuarios`, `Clientes` y `Bitacora`, dos
funciones, los procedimientos almacenados y el usuario administrador inicial. Es idempotente:
puede ejecutarse varias veces sin duplicar datos, y migra una base creada con una versión anterior
agregando las columnas que le falten.

El script declara `SET QUOTED_IDENTIFIER ON` al inicio y lo necesita: el índice único de
`Documento` es filtrado, y SQL Server exige esa opción tanto para crearlo como para escribir en la
tabla. SSMS conecta con esa opción activa y `sqlcmd` no, así que sin la declaración el script
funcionaría desde SSMS y fallaría desde la línea de comandos.

### 2. Cadena de conexión

Está en `src/GestionClientes.Web/Web.config`, con el nombre `GestionClientes`:

```xml
<add name="GestionClientes"
     connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=GestionClientesDB;Integrated Security=True;Application Name=GestionClientes"
     providerName="System.Data.SqlClient" />
```

Usa autenticación de Windows, por lo que no hay credenciales almacenadas en el archivo de
configuración. Para una instancia con autenticación de SQL Server:

```xml
connectionString="Data Source=SERVIDOR\INSTANCIA;Initial Catalog=GestionClientesDB;User ID=usuario;Password=contrasena"
```

Ajustar `Data Source` si la instancia no es `.\SQLEXPRESS`.

### 3. Ejecutar

Abrir `src/GestionClientes.sln` en Visual Studio 2022, establecer `GestionClientes.Web` como
proyecto de inicio y ejecutar con F5. La aplicación abre en `http://localhost:8080/`. Si ese
puerto ya está en uso, Visual Studio lo informa; puede cambiarse en las propiedades web del
proyecto.

### Credenciales de acceso

```
Usuario:    admin
Contraseña: Admin123$
```

## Estructura del proyecto

| Proyecto | Responsabilidad |
|---|---|
| `GestionClientes.Entidades` | Modelos del dominio. Sin lógica. |
| `GestionClientes.Datos` | Acceso a datos con ADO.NET. Único proyecto que conoce SQL. |
| `GestionClientes.Negocio` | Validaciones, hashing y orquestación. |
| `GestionClientes.Web` | Páginas Web Forms. No conoce SQL. |
| `GestionClientes.Pruebas` | Pruebas unitarias de hashing, validación y paginación. |

Las dependencias van en una sola dirección: `Web` → `Negocio` → `Datos` → `Entidades`.

## Pruebas

```
dotnet test src\GestionClientes.Pruebas\GestionClientes.Pruebas.vbproj
```

34 pruebas. Cubren la derivación y verificación de contraseñas, las reglas de validación de
clientes y el cálculo de totales de la paginación, que es donde vive la lógica que puede fallar
sin dar señales visibles.

## Decisiones de diseño

**La bitácora es un requisito de trazabilidad típico de las instituciones financieras
supervisadas**, donde debe poder demostrarse quién tocó qué registro y cuándo. Por eso se escribe
dentro del procedimiento almacenado y no desde la aplicación: cada procedimiento de escritura
modifica al cliente y registra la acción en la misma transacción. Si el registro se hiciera con
una llamada separada, bastaría con que una ruta nueva olvidara invocarla para que la auditoría
tuviera huecos, y un fallo entre ambas operaciones dejaría la base inconsistente. Así la garantía
es estructural: se aplican ambas o ninguna.

**`Bitacora.ClienteId` no tiene clave foránea.** La bitácora es el registro de auditoría del
sistema, no el histórico de una entidad concreta. Clientes es lo primero que se audita, no lo
único que se auditará: una acción futura sobre configuración o sobre un acceso no tiene cliente
asociado, y una clave foránea ataría la tabla para siempre a la primera entidad que se le ocurrió
auditar a alguien. Además, la auditoría debe sobrevivir incluso a una purga física que haga
soporte algún día, y por eso `Detalle` guarda el snapshot completo del registro: la bitácora no
depende de que la fila original siga existiendo.

**`NombreUsuario` está desnormalizado en la bitácora.** Preserva el valor histórico aunque el
usuario se renombre o se elimine. `UsuarioId` se mantiene para poder unir con `Usuarios`.

**Bitácora por procedimiento almacenado y no por trigger.** Un trigger no puede saber qué
usuario *de la aplicación* originó el cambio: solo ve la cuenta con la que se conecta el pool de
conexiones.

**Control de concurrencia optimista en `Clientes`.** No es una preocupación teórica: varios
operadores trabajando a la vez sobre el mismo registro de cliente es el modo de operación normal
en ese tipo de institución. La columna `ROWVERSION` que compara el procedimiento de actualización
evita que el segundo guardado sobrescriba en silencio el cambio del primero; el que llega después
recibe un aviso en lugar de perder su información sin saberlo.

**PBKDF2 en lugar de un hash simple con salt.** Un hash rápido, aunque lleve salt, se ataca por
fuerza bruta con GPU. Se usa `Rfc2898DeriveBytes` con HMAC-SHA256 y 100000 iteraciones, salt de
16 bytes por usuario y hash de 32 bytes. La comparación es en tiempo constante para no filtrar
información por el tiempo de respuesta.

**Autorización declarada en `Web.config`.** Con `<deny users="?" />` toda página nueva queda
protegida por omisión. El patrón alternativo de comprobar la sesión al inicio de cada página
falla el día que alguien agrega una página y olvida la comprobación.

**Validación duplicada en cliente y servidor.** Los validadores del navegador son una comodidad
para el usuario; la validación del servidor es el control real, porque un cliente HTTP puede
enviar el formulario sin ejecutar ningún script.

**La identidad del usuario viaja en el ticket de autenticación, no en la sesión.** El
identificador se guarda en el `UserData` del `FormsAuthenticationTicket`, no en `Session`. La
cookie de autenticación y la sesión en memoria tienen ciclos de vida distintos: la cookie
sobrevive mientras dure su expiración deslizante, pero la sesión InProc se pierde si se recicla
el grupo de aplicaciones o si el usuario tarda en volver. Guardar el identificador en `Session`
crearía un estado en el que el usuario está autenticado pero sin sesión, y el identificador
llegaría como cero justo cuando se necesita para escribir en la bitácora.

**Sin `<machineKey>` fijo, a propósito.** Fijar una clave de máquina permitiría que las cookies
de autenticación sobrevivieran a un reciclaje del proceso, pero este repositorio es público: una
clave publicada en el código deja falsificar tickets de autenticación y ViewState en cualquier
despliegue que copie la configuración tal cual. Se prefiere que un reinicio del proceso obligue a
volver a iniciar sesión. En un despliegue real la clave se fija fuera del control de versiones,
por configuración del servidor.

## Mejoras posteriores a la primera versión

**Paginación y ordenamiento en el motor.** El listado de clientes y la bitácora paginan con
`OFFSET/FETCH` y devuelven el total en un parámetro de salida, en lugar de traer todas las filas
y descartar las que no caben. El ordenamiento por columna no usa SQL dinámico: el nombre de
columna llega como parámetro y se resuelve con expresiones `CASE`, de modo que un valor
arbitrario no puede ejecutarse. El orden lleva siempre un desempate por clave primaria, sin el
cual una fila podría intercambiarse entre páginas y no aparecer nunca.

**Control de concurrencia optimista.** `Clientes` tiene una columna `ROWVERSION` que el
procedimiento de actualización compara. Si dos usuarios abren el mismo cliente y ambos guardan,
el segundo recibe un aviso en lugar de sobrescribir en silencio el cambio del primero.

**Transformaciones de configuración.** `Web.Release.config` quita `debug="true"` y fuerza
`customErrors` a `On` al publicar. Se aplican al **publicar**, no al compilar: ejecutar en Release
desde Visual Studio no las dispara.

**Confirmación de borrado con modal.** Sustituye al diálogo nativo del navegador e identifica al
cliente por nombre. Usa el Bootstrap que el proyecto ya sirve localmente, sin agregar
dependencias. El nombre se inserta con `textContent` y no con `innerHTML`.

**Registro con log4net.** Reemplaza la escritura manual a archivo. Rota por tamaño, conserva
cinco archivos y permite cambiar destino y nivel sin recompilar. Registra los intentos de inicio
de sesión fallidos con el nombre de usuario intentado, nunca con la contraseña.

**Arranque reproducible.** El puerto y la URL de IIS Express viven en el archivo de proyecto
versionado en lugar del `.user`, que está ignorado. Cualquiera que clone el repositorio y presione
F5 abre en la misma dirección, sin depender de un perfil de arranque local.

**Respuesta entendible ante contenido no permitido.** La validación de petición de ASP.NET
rechaza entradas que parecen marcado; la aplicación ahora explica el motivo en lugar de mostrar
una página de error genérica. La validación no se relajó en ningún punto: no hay
`validateRequest="false"` y `requestValidationMode` no se tocó.

**Indicador de ordenamiento.** La cabecera de la rejilla muestra por qué columna se está
ordenando y en qué dirección, con una flecha y el atributo `aria-sort`.

**Filtro de listados en funciones en línea.** El predicado de filtro se escribe una sola vez, en
una función de tabla en línea, y lo comparten la consulta de conteo y la de página: un paginador
que no coincida con lo que muestra la rejilla deja de ser posible. La función es en línea (no
multiinstrucción) para que el optimizador la expanda dentro del plan de ejecución en lugar de
materializar un resultado intermedio.

**Campos de auditoría y borrado lógico.** `Clientes` registra quién creó el registro, quién lo
modificó por última vez y cuándo, y si fue borrado. Los campos viven en una clase base,
`EntidadAuditable`, de la que hereda `Cliente`: no describen a un cliente, describen el hecho de
haber sido guardado, así que una entidad nueva los obtiene sin volver a declararlos.

El borrado es lógico. `usp_Cliente_Eliminar` marca la fila en lugar de ejecutar `DELETE`: el
registro deja de existir para la aplicación —ningún listado ni `ObtenerPorId` lo devuelven— pero
la fila permanece y solo soporte puede recuperarla desde la base. En un sistema que maneja cartera
de crédito, un borrado físico destruye el extremo de relaciones que quizá ya no se pueden
reconstruir.

Conviene no confundir dos conceptos que a veces se colapsan en una sola columna: *eliminado*
significa que el registro no debe volver a aparecer y solo soporte lo revierte; un estado de
negocio como *inactivo* significaría que el registro sigue siendo válido pero no debe ofrecerse en
ciertos lugares, y lo alternaría el usuario en ambos sentidos. Aquí solo existe el primero, porque
es el que corresponde a la acción de eliminar que pide el enunciado.

El índice único de `Documento` pasó a ser filtrado por `Eliminado = 0`. Sin el filtro, borrar a un
cliente bloquearía su DUI para siempre y nadie podría volver a registrarse con ese documento.

## Qué generalizaría este diseño al crecer, y por qué todavía no

El sistema tiene una sola entidad de negocio. Eso condiciona qué abstracciones están justificadas
hoy y cuáles serían adivinar.

**Lo que sí se extrajo, porque ya tenía dos consumidores:** `Paginador.ascx` como control
reutilizable en cuanto lo necesitaron las dos rejillas; `MarcarCabeceraOrdenada` en `PaginaBase`
en cuanto el indicador de ordenamiento hizo falta en ambas páginas; `SqlHelper` como único punto
donde se abren conexiones y se crean comandos; y `EntidadAuditable`, porque los campos de
auditoría son transversales por definición.

**Lo que no se construyó:** un repositorio genérico `RepositorioBase(Of T)` con CRUD por
convención, y una rejilla genérica configurable por metadatos de entidad. Ambos son el camino
natural cuando aparecen la tercera y la cuarta entidad y el esqueleto de "abrir conexión, poblar
parámetros, recorrer el lector" se repite lo suficiente como para que el contrato sea evidente.
Con una sola entidad ese contrato habría que inventarlo, y una abstracción diseñada contra un
único caso normalmente acierta con ese caso y falla con el siguiente.

El criterio que se siguió es extraer cuando hay dos usos reales, no cuando podría haberlos.

## Seguridad

- **Inyección SQL:** todo el acceso a datos usa procedimientos almacenados invocados con
  `SqlParameter` tipados y con longitud explícita. No hay concatenación de SQL en ninguna capa.
- **Contraseñas:** PBKDF2-HMAC-SHA256, 100000 iteraciones, salt por usuario.
- **Sesión:** cookies `HttpOnly`; la sesión se descarta y se regenera al autenticarse, para
  impedir la fijación de sesión; `SignOut` y `Abandon` al cerrar.
- **CSRF:** `ViewStateUserKey` vinculado al identificador de sesión, con MAC y cifrado de
  ViewState activos.
- **Enumeración de usuarios:** el login responde el mismo mensaje ante usuario inexistente,
  contraseña incorrecta y usuario inactivo, y además calcula un hash señuelo cuando el usuario no
  existe, de modo que las tres rutas tarden lo mismo. El mensaje uniforme por sí solo no basta:
  sin el señuelo, la diferencia de tiempo entre responder al instante y ejecutar 100000
  iteraciones de PBKDF2 revelaría qué cuentas existen.
- **XSS:** los datos se renderizan con controles que codifican HTML; los mensajes se codifican
  de forma explícita.
- **Fuga de información:** `customErrors` está en `RemoteOnly`: un cliente remoto solo recibe la
  página genérica, mientras que en la máquina local se conserva el detalle para poder
  diagnosticar. En ambos casos la excepción completa queda registrada en
  `App_Data/errores.log`, que es donde debe consultarse.
- **Identidad en el ticket, no en la sesión:** el identificador del usuario viaja en el `UserData`
  del ticket de autenticación, firmado y cifrado, en lugar de en `Session` (ver Decisiones de
  diseño).
- **Sin `<machineKey>` fijo:** decisión deliberada por tratarse de un repositorio público (ver
  Decisiones de diseño).
- **Validación de petición:** permanece activa; el contenido que rechaza se explica al usuario en
  lugar de suprimirse o relajar la validación.
