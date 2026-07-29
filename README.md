# Sistema de Gestión de Clientes

Aplicación web para el mantenimiento de información de clientes con control de acceso y
bitácora de auditoría.

Prueba técnica — Kamilo Martinez

## Tecnologías

- ASP.NET Web Forms (VB.NET) sobre .NET Framework 4.8
- SQL Server 2022 Express
- ADO.NET con procedimientos almacenados
- MSTest
- Bootstrap 5.3 (servido localmente)

## Requisitos previos

- Visual Studio 2022 con la carga de trabajo **Desarrollo de ASP.NET y web**
- .NET Framework 4.8 Developer Pack
- SQL Server 2016 o superior (el script usa `CREATE OR ALTER`, `THROW` y `FOR JSON`)
- SQL Server Management Studio (opcional, para inspeccionar los datos)

## Instalación

### 1. Base de datos

Ejecutar `database/01_CrearBaseDatos.sql` en SSMS, o desde la línea de comandos:

```
sqlcmd -S .\SQLEXPRESS -E -i database\01_CrearBaseDatos.sql
```

El script crea la base `GestionClientesDB`, las tablas `Usuarios`, `Clientes` y `Bitacora`, los
procedimientos almacenados y el usuario administrador inicial. Es idempotente: puede ejecutarse
varias veces sin duplicar datos.

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
proyecto de inicio y ejecutar con F5.

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
| `GestionClientes.Pruebas` | Pruebas unitarias de hashing y validación. |

Las dependencias van en una sola dirección: `Web` → `Negocio` → `Datos` → `Entidades`.

## Pruebas

```
dotnet test src\GestionClientes.Pruebas\GestionClientes.Pruebas.vbproj
```

Cubren la derivación y verificación de contraseñas y las reglas de validación de clientes, que
es donde vive la lógica que puede fallar sin dar señales visibles.

## Decisiones de diseño

**La bitácora se escribe dentro del procedimiento almacenado, no desde la aplicación.**
Cada procedimiento de escritura modifica al cliente y registra la acción en la misma
transacción. Si el registro se hiciera con una llamada separada, bastaría con que una ruta nueva
olvidara invocarla para que la auditoría tuviera huecos, y un fallo entre ambas operaciones
dejaría la base inconsistente. Así la garantía es estructural: se aplican ambas o ninguna.

**`Bitacora.ClienteId` no tiene clave foránea.** El borrado de clientes es físico. Una clave
foránea obligaría a borrar en cascada, destruyendo el historial que la bitácora existe para
conservar, o a bloquear el borrado. La columna `Detalle` guarda el snapshot completo del
registro eliminado.

**`NombreUsuario` está desnormalizado en la bitácora.** Preserva el valor histórico aunque el
usuario se renombre o se elimine. `UsuarioId` se mantiene para poder unir con `Usuarios`.

**Bitácora por procedimiento almacenado y no por trigger.** Un trigger no puede saber qué
usuario *de la aplicación* originó el cambio: solo ve la cuenta con la que se conecta el pool de
conexiones.

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
- **Fuga de información:** `customErrors` redirige a una página genérica y el detalle de la
  excepción se registra en `App_Data/errores.log`, nunca se envía al navegador.
- **Identidad en el ticket, no en la sesión:** el identificador del usuario viaja en el `UserData`
  del ticket de autenticación, firmado y cifrado, en lugar de en `Session` (ver Decisiones de
  diseño).
- **Sin `<machineKey>` fijo:** decisión deliberada por tratarse de un repositorio público (ver
  Decisiones de diseño).
