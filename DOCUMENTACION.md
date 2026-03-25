# Documentación Completa — SoporteDeTickets

> **Propósito:** Documentación técnica completa del proyecto — arquitectura, base de datos, flujo de datos, y notas de mejora.
>
> **Cómo leerlo:** De inicio a fin, en orden. Cada parte depende de la anterior.

---

## Parte 0 — ¿Qué es este proyecto y por qué existe?

### El problema que resuelve

Imagina que trabajas en soporte técnico. Un cliente llama y dice "mi sistema no funciona". Tú lo apuntas en un papel. Otro técnico toma el papel y empieza a trabajarlo. Un tercero lo resuelve. ¿Cómo saben todos quién lo está atendiendo, en qué estado está, y quién lo reportó?

Con papel no puedes. Necesitas un sistema de tickets.

Un **ticket de soporte** es un registro digital de un problema:

- ¿Quién lo reportó? (cliente)
- ¿Qué pasó? (título + descripción)
- ¿En qué estado está? (abierto, en proceso, resuelto)
- ¿Cuándo llegó? (fecha)

Este proyecto, **SoporteDeTickets**, hace exactamente eso para una empresa llamada "Zynapse".

### Qué puede hacer la aplicación

| Sección      | Qué hace                                                           |
| ------------ | ------------------------------------------------------------------ |
| **Tickets**  | Listar, crear, modificar y eliminar tickets                        |
| **Clientes** | Listar, crear, modificar y eliminar clientes                       |
| **Estados**  | Ver un reporte de todos los tickets con su estado actual y cliente |

### Cómo fluye la información

```
[Tú en el navegador]
        |
        | HTTP (GET, POST)
        v
[ASP.NET Core — el servidor web]
        |
        | C# / LINQ
        v
[Entity Framework Core — el traductor]
        |
        | SQL
        v
[SQL Server — la base de datos]
```

Cuando abres `/Tickets` en el navegador, estás haciendo una petición HTTP GET al servidor. El servidor la procesa, le pide los datos a la base de datos, construye el HTML, y te lo devuelve.

---

## Parte 1 — El Stack (por qué se usaron estas tecnologías)

### ASP.NET Core

**ASP.NET Core** es el framework de Microsoft para construir aplicaciones web con C#. Es el equivalente de Express (Node.js) o Django (Python) pero para el ecosistema .NET.

**Por qué se usó:** Es el estándar en empresas que trabajan con Microsoft. Si vienes de Unity (que también usa C#), la sintaxis del lenguaje ya la conoces. La diferencia es el "mundo" donde corre: en Unity corres en un motor de juego, aquí corres en un servidor web.

### Razor Pages

Dentro de ASP.NET Core hay dos formas principales de hacer apps web:

| Enfoque         | Qué es                                     | Cuándo usarlo               |
| --------------- | ------------------------------------------ | --------------------------- |
| **MVC**         | Separa en 3 capas: Model, View, Controller | Apps grandes y complejas    |
| **Razor Pages** | Cada página tiene su propia lógica         | Apps CRUD simples como esta |

**Razor Pages** es más directo: tienes una página (`Create.cshtml`) y su lógica (`Create.cshtml.cs`). Todo lo que necesita esa página está junto. No tienes que buscar en tres carpetas distintas.

**Por qué se usó aquí:** La app es CRUD (Create, Read, Update, Delete) puro. No necesita la complejidad de MVC.

### Entity Framework Core (EF Core)

Normalmente, para leer datos de SQL Server tendrías que escribir SQL directamente:

```sql
SELECT t.Id, t.Titulo, c.Nombre FROM Tickets t
INNER JOIN Clientes c ON t.ClienteId = c.Id
```

Con **Entity Framework Core** puedes escribir eso en C#:

```csharp
_context.Tickets.Include(t => t.Cliente).ToListAsync()
```

EF Core traduce ese C# a SQL por ti. A esto se le llama **ORM** (Object-Relational Mapper) — mapea objetos de C# a tablas de SQL.

**Analogía:** EF Core es como Google Translate entre C# y SQL. Tú hablas C#, la base de datos habla SQL, y EF Core hace la traducción.

**Por qué se usó:** No tienes que memorizar sintaxis SQL para operaciones simples. C# es suficiente.

### SQL Server

La base de datos donde viven todos los datos. Microsoft SQL Server es la base de datos empresarial de Microsoft.

La app también usa **Stored Procedures** (procedimientos almacenados) — bloques de SQL que viven dentro de la base de datos y se llaman por nombre desde la app. Son como funciones, pero escritas en SQL y guardadas en el servidor de base de datos.

**Por qué stored procedures para algunas operaciones:** Mayor control, mejor rendimiento para operaciones complejas, y en entornos empresariales es common policy que ciertas operaciones pasen por SPs y no por ORM directo.

### Resumen del stack

```
Navegador  →  ASP.NET Core (C#)  →  EF Core (traductor)  →  SQL Server
           ↑                    ↑
      Razor Pages          Stored Procedures
   (HTML + C# mezclados)  (para Update/Delete)
```

**Analogía del restaurante:**

- El navegador es el cliente que hace el pedido
- Razor Pages es el mesero que toma el pedido y trae la comida
- EF Core es el cajero que comunica al mesero con la cocina
- SQL Server es la cocina donde se prepara (y guarda) todo

---

## Parte 2 — La Base de Datos

Esta es la parte más importante para entender el resto. Todo lo demás gira en torno a cómo están organizados los datos.

### Diagrama de tablas y relaciones

```
┌─────────────┐       ┌──────────────────────────────────────┐
│   Estados   │       │              Tickets                 │
├─────────────┤       ├──────────────────────────────────────┤
│ Id (PK)     │◄──────│ EstadoId (FK)                        │
│ Nombre      │       │ Id (PK)                              │
└─────────────┘       │ Titulo                               │
                      │ Descripcion                          │
┌─────────────┐       │ ClienteId (FK) ──────────────────────┼──┐
│  Clientes   │◄──────┘ Prioridad                            │  │
├─────────────┤         FechaCreacion                        │  │
│ Id (PK)     │◄─────────────────────────────────────────────┘  │
│ Nombre      │         FechaActualizacion (nullable)            │
│ Email       │                                                  │
│ Telefono    │                                                  │
│FechaRegistro│                                                  │
└─────────────┘
```

Un ticket pertenece a un cliente (ClienteId → Clientes.Id) y tiene un estado (EstadoId → Estados.Id). Eso es una **relación de clave foránea (FK)**.

### Script SQL — Crear la base de datos desde cero

Copia y ejecuta esto en SQL Server Management Studio (SSMS) para recrear la base de datos completa:

```sql
-- =============================================
-- PASO 1: Crear la base de datos
-- =============================================
CREATE DATABASE SoporteTickets;
GO

USE SoporteTickets;
GO

-- =============================================
-- PASO 2: Crear las tablas
-- El orden importa: primero las tablas que
-- no dependen de otras (sin FK)
-- =============================================

-- Estados: sin dependencias externas
CREATE TABLE Estados (
    Id    INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL
);
GO

-- Clientes: sin dependencias externas
CREATE TABLE Clientes (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    Nombre         NVARCHAR(100) NOT NULL,
    Email          NVARCHAR(100) NOT NULL,
    Telefono       NVARCHAR(20)  NOT NULL,
    FechaRegistro  DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- Tickets: depende de Clientes y Estados
CREATE TABLE Tickets (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Titulo              NVARCHAR(200) NOT NULL,
    Descripcion         NVARCHAR(MAX),
    ClienteId           INT NOT NULL,
    EstadoId            INT NOT NULL,
    Prioridad           TINYINT NOT NULL DEFAULT 1,
    FechaCreacion       DATETIME NOT NULL DEFAULT GETDATE(),
    FechaActualizacion  DATETIME,

    CONSTRAINT FK_Tickets_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    CONSTRAINT FK_Tickets_Estados  FOREIGN KEY (EstadoId)  REFERENCES Estados(Id)
);
GO

-- =============================================
-- PASO 3: Insertar datos de ejemplo
-- =============================================

INSERT INTO Estados (Nombre) VALUES
    ('Abierto'),
    ('En proceso'),
    ('Resuelto'),
    ('Cerrado');
GO

INSERT INTO Clientes (Nombre, Email, Telefono) VALUES
    ('Carlos Mendoza',   'carlos@empresa.com',  '555-1001'),
    ('Laura Ríos',       'laura@empresa.com',   '555-1002'),
    ('Pedro Gómez',      'pedro@empresa.com',   '555-1003');
GO

INSERT INTO Tickets (Titulo, Descripcion, ClienteId, EstadoId, Prioridad) VALUES
    ('No puedo iniciar sesión',     'Al ingresar mis credenciales aparece error 403', 1, 1, 3),
    ('Impresora no responde',       'La impresora de Contabilidad dejó de funcionar', 2, 2, 2),
    ('Correo no llega',             'Desde ayer no recibo correos externos',          3, 1, 2),
    ('PC muy lenta',                'El equipo tarda 10 minutos en arrancar',         1, 3, 1);
GO
```

### Script SQL — Los 6 Stored Procedures

La aplicación llama a estos 6 procedimientos por nombre. Si no existen, la app falla.

```sql
USE SoporteTickets;
GO

-- =============================================
-- 1. sp_Tickets_Delete
--    Elimina un ticket por su Id
-- =============================================
CREATE OR ALTER PROCEDURE sp_Tickets_Delete
    @Id INT
AS
BEGIN
    DELETE FROM Tickets WHERE Id = @Id;
END
GO

-- =============================================
-- 2. sp_Tickets_Update
--    Modifica los campos editables de un ticket
-- =============================================
CREATE OR ALTER PROCEDURE sp_Tickets_Update
    @Id           INT,
    @Titulo       NVARCHAR(200),
    @Descripcion  NVARCHAR(MAX),
    @ClienteId    INT,
    @EstadoId     INT,
    @Prioridad    TINYINT
AS
BEGIN
    UPDATE Tickets
    SET
        Titulo             = @Titulo,
        Descripcion        = @Descripcion,
        ClienteId          = @ClienteId,
        EstadoId           = @EstadoId,
        Prioridad          = @Prioridad,
        FechaActualizacion = GETDATE()   -- se actualiza automáticamente
    WHERE Id = @Id;
END
GO

-- =============================================
-- 3. sp_Clientes_Delete
--    Elimina un cliente por su Id
-- =============================================
CREATE OR ALTER PROCEDURE sp_Clientes_Delete
    @Id INT
AS
BEGIN
    DELETE FROM Clientes WHERE Id = @Id;
END
GO

-- =============================================
-- 4. sp_Clientes_Insert
--    Inserta un nuevo cliente
--    (FechaRegistro se pone automáticamente)
-- =============================================
CREATE OR ALTER PROCEDURE sp_Clientes_Insert
    @Nombre   NVARCHAR(100),
    @Email    NVARCHAR(100),
    @Telefono NVARCHAR(20)
AS
BEGIN
    INSERT INTO Clientes (Nombre, Email, Telefono)
    VALUES (@Nombre, @Email, @Telefono);
END
GO

-- =============================================
-- 5. sp_Clientes_Update
--    Modifica los datos de un cliente
-- =============================================
CREATE OR ALTER PROCEDURE sp_Clientes_Update
    @Id       INT,
    @Nombre   NVARCHAR(100),
    @Email    NVARCHAR(100),
    @Telefono NVARCHAR(20)
AS
BEGIN
    UPDATE Clientes
    SET
        Nombre   = @Nombre,
        Email    = @Email,
        Telefono = @Telefono
    WHERE Id = @Id;
END
GO

-- =============================================
-- 6. sp_Estados_List
--    Devuelve todos los tickets con el nombre
--    del cliente y del estado (JOIN)
--    Resultado: Id, Cliente, Titulo, Estado
-- =============================================
CREATE OR ALTER PROCEDURE sp_Estados_List
AS
BEGIN
    SELECT
        t.Id,
        c.Nombre  AS Cliente,
        t.Titulo,
        e.Nombre  AS Estado
    FROM Tickets   t
    INNER JOIN Clientes c ON t.ClienteId = c.Id
    INNER JOIN Estados  e ON t.EstadoId  = e.Id
    ORDER BY t.Id;
END
GO
```

### Por qué stored procedures

1. **Seguridad:** Los parámetros (`@Id`, `@Nombre`) están separados del SQL. No hay forma de hacer SQL Injection porque los valores no se concatenan al texto del query.

2. **Rendimiento:** SQL Server compila y guarda el plan de ejecución del SP. La primera vez lo calcula, las siguientes veces lo reutiliza.

3. **Separación:** Si mañana cambias la base de datos, solo cambias el SP — la app de C# no necesita tocar.

---

## Parte 3 — La Arquitectura del Proyecto

### Diagrama de capas

```
┌──────────────────────────────────────────────┐
│            CAPA DE PRESENTACIÓN              │
│         Pages/ (.cshtml + .cshtml.cs)        │
│   Tickets/  |  Clientes/  |  Estados/        │
└─────────────────────┬────────────────────────┘
                      │ usa
┌─────────────────────▼────────────────────────┐
│             CAPA DE DATOS                    │
│   Models/TicketsEfRepo.cs  (repositorio)     │
│   Data/ApplicationDbContext.cs (EF Core)     │
└─────────────────────┬────────────────────────┘
                      │ habla con
┌─────────────────────▼────────────────────────┐
│               SQL SERVER                     │
│       Tablas: Tickets, Clientes, Estados     │
│       Stored Procedures (6 en total)         │
└──────────────────────────────────────────────┘
```

### Qué hace cada carpeta

| Carpeta/Archivo | Responsabilidad                                          |
| --------------- | -------------------------------------------------------- |
| `Program.cs`    | Punto de entrada. Configura y arranca la app             |
| `Data/`         | El DbContext — la conexión entre C# y SQL Server         |
| `Models/`       | Las entidades (Ticket, Cliente, Estado) y el repositorio |
| `Pages/`        | Las páginas web — HTML + lógica C#                       |
| `wwwroot/`      | Archivos estáticos — CSS, JavaScript, Bootstrap          |

### El patrón Repository

El repositorio (`TicketsEfRepo`) es una capa intermedia entre las Pages y la base de datos. Su trabajo es responder preguntas como:

- "Dame todos los tickets" → `ListAsync()`
- "Dame el ticket con ID 5" → `GetAsync(5)`
- "Guarda este nuevo ticket" → `InsertAsync(ticket)`

**Por qué no consultar la BD directamente desde cada página?**

Imagina que tienes 3 páginas que necesitan listar tickets. Sin repositorio, las 3 páginas tendrían la misma consulta. Si el día de mañana cambia algo en esa consulta, tienes que cambiarla en 3 lugares. Con el repositorio, la cambias en 1 solo lugar.

**Analogía:** El repositorio es como el mostrador de un almacén. Tú pides "necesito 3 tornillos" y el mostrador sabe dónde están, cómo agarrarlos, y te los trae. No entras al almacén directamente.

### Dependency Injection (DI) — cómo Program.cs conecta todo

En `Program.cs` hay estas tres líneas clave:

```csharp
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(...);
builder.Services.AddScoped<TicketsEfRepo>();
```

Esto es **Inyección de Dependencias**. En lugar de que cada página cree su propio `TicketsEfRepo` con `new TicketsEfRepo(...)`, le dices al sistema: "cuando alguien pida un `TicketsEfRepo`, créaselo automáticamente y dáselo".

**Analogía:** Es como un sistema de valet parking. No tienes que ir a buscar tu carro — le dices al valet "necesito mi carro" y él lo trae. Tú no sabes ni dónde está, simplemente lo pides.

Cuando `TicketsPageModel` tiene en su constructor:

```csharp
public TicketsPageModel(TicketsEfRepo ticketsRepo)
```

...está diciendo "necesito un `TicketsEfRepo`". El sistema de DI lo crea y se lo inyecta automáticamente.

`AddScoped` significa que se crea una instancia nueva por cada request HTTP (por cada vez que alguien carga una página).

---

## Parte 4 — Explicación Archivo por Archivo

### 4.1 Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<TicketsEfRepo>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
```

**Línea por línea:**

- `WebApplication.CreateBuilder(args)` — crea el "constructor" de la app. Todavía no arranca nada.
- `builder.Services.AddRazorPages()` — registra el sistema de Razor Pages en el contenedor de DI.
- `builder.Services.AddDbContext<ApplicationDbContext>(...)` — registra el DbContext. Le dice que use SQL Server y que lea la connection string del `appsettings.json`.
- `builder.Services.AddScoped<TicketsEfRepo>()` — registra el repositorio de tickets.
- `var app = builder.Build()` — aquí sí se construye la app con todo lo registrado.
- `app.UseStaticFiles()` — activa el servicio de archivos estáticos (CSS, JS de wwwroot/).
- `app.MapRazorPages()` — activa el enrutamiento de páginas (que `/Tickets` lleve a `Pages/Tickets/Index`).
- `app.Run()` — arranca el servidor. La app queda escuchando peticiones.

**El ciclo de vida de un request:**

```
1. Navegador hace GET /Tickets
2. ASP.NET Core recibe la petición
3. MapRazorPages() identifica que /Tickets → Pages/Tickets/Index.cshtml
4. Crea una instancia de TicketsPageModel (inyectando TicketsEfRepo)
5. Llama a OnGetAsync()
6. OnGetAsync() carga los tickets de la BD
7. Razor combina Index.cshtml + los datos → HTML
8. HTML se envía de vuelta al navegador
```

---

### 4.2 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ROKOVZKY;Password=123456; Database=SoporteTickets;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

La **connection string** es como la dirección + credenciales para llegar a la base de datos. Cada parte tiene un significado:

| Parte                         | Significado                                                                             |
| ----------------------------- | --------------------------------------------------------------------------------------- |
| `Server=ROKOVZKY`             | Nombre o IP del servidor SQL Server                                                     |
| `Password=123456`             | Contraseña (⚠️ ver Parte 6 — problema de seguridad)                                     |
| `Database=SoporteTickets`     | Nombre de la base de datos                                                              |
| `Trusted_Connection=True`     | Usa la autenticación de Windows (el usuario de Windows actual)                          |
| `TrustServerCertificate=True` | No valida el certificado SSL del servidor (necesario en entornos locales de desarrollo) |

`builder.Configuration.GetConnectionString("DefaultConnection")` lee esta cadena automáticamente desde el archivo.

---

### 4.3 Data/ApplicationDbContext.cs

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Estado> Estados { get; set; }
    public DbSet<EstadoClienteVM> EstadosClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<EstadoClienteVM>().HasNoKey();
    }
}
```

El `DbContext` es el corazón de EF Core. Es el objeto que mantiene la conexión con la base de datos.

**`DbSet<T>`** — cada `DbSet` representa una tabla:

- `DbSet<Ticket> Tickets` → tabla `Tickets`
- `DbSet<Cliente> Clientes` → tabla `Clientes`
- `DbSet<Estado> Estados` → tabla `Estados`

Cuando escribes `_context.Tickets.ToListAsync()`, EF Core traduce eso a `SELECT * FROM Tickets`.

**`HasNoKey()`** — `EstadoClienteVM` no es una tabla real, es el resultado de un stored procedure (`sp_Estados_List`). Los resultados de SPs no tienen clave primaria, así que hay que decirle a EF Core que no busque ninguna con `.HasNoKey()`.

---

### 4.4 Models/Ticket.cs, Cliente.cs, Estado.cs

```csharp
public class Ticket
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }   // navigation property

    public int EstadoId { get; set; }
    public Estado? Estado { get; set; }     // navigation property

    public byte Prioridad { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
```

Estas clases son **entidades** — el espejo en C# de las tablas SQL. EF Core mapea cada propiedad a una columna.

**Navigation properties** (`Cliente?` y `Estado?`):

`ClienteId` es la clave foránea (un número entero). Pero `Cliente?` es el objeto completo del cliente. Cuando usas `.Include(t => t.Cliente)` en EF Core, le estás diciendo "cuando cargues el ticket, también carga el objeto Cliente completo".

El `?` significa nullable — la propiedad puede ser `null`. En C# con `<Nullable>enable</Nullable>` en el .csproj, todas las referencias son non-nullable por defecto. El `?` indica explícitamente que este campo puede no tener valor (cuando no haces `.Include()`).

**`FechaActualizacion` es nullable** (`DateTime?`) porque un ticket recién creado no ha sido modificado — ese campo está vacío en la BD.

---

### 4.5 Models/EstadoClienteVM.cs

```csharp
public class EstadoClienteVM
{
    public int Id { get; set; }
    public string Cliente { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Estado { get; set; } = "";
}
```

**ViewModel (VM)** — no es una tabla de la BD. Es una clase diseñada específicamente para mostrar datos en una página.

`sp_Estados_List` hace un JOIN de 3 tablas y devuelve: Id del ticket, nombre del cliente, título del ticket, nombre del estado. Eso no encaja exactamente en ningún modelo existente, así que se crea `EstadoClienteVM` que tiene exactamente esas 4 columnas.

**`= ""`** — inicializa las strings como cadena vacía en lugar de null. Así evitas `NullReferenceException` si llega un resultado vacío.

---

### 4.6 Models/TicketsEfRepo.cs

```csharp
public class TicketsEfRepo
{
    private readonly ApplicationDbContext _context;

    public TicketsEfRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    // Listar todos los tickets con sus relaciones
    public async Task<List<Ticket>> ListAsync()
    {
        return await _context.Tickets
            .Include(t => t.Cliente)
            .Include(t => t.Estado)
            .ToListAsync();
    }

    // Obtener un ticket por ID
    public async Task<Ticket?> GetAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Cliente)
            .Include(t => t.Estado)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // Insertar — EF Core puro (no usa SP)
    public async Task<int> InsertAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket.Id;
    }

    // Eliminar — usa SP
    public async Task<int> DeleteAsync(int id)
    {
        var param = new SqlParameter("@Id", id);
        return await _context.Database.ExecuteSqlRawAsync("EXEC sp_Tickets_Delete @Id", param);
    }

    // Actualizar — usa SP
    public async Task<int> UpdateAsync(Ticket ticket)
    {
        var parameters = new[] {
            new SqlParameter("@Id",          ticket.Id),
            new SqlParameter("@Titulo",      ticket.Titulo),
            new SqlParameter("@Descripcion", (object?)ticket.Descripcion ?? DBNull.Value),
            new SqlParameter("@ClienteId",   ticket.ClienteId),
            new SqlParameter("@EstadoId",    ticket.EstadoId),
            new SqlParameter("@Prioridad",   ticket.Prioridad)
        };
        return await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_Tickets_Update @Id, @Titulo, @Descripcion, @ClienteId, @EstadoId, @Prioridad",
            parameters
        );
    }
}
```

**`Include(t => t.Cliente)`** — le dice a EF Core que haga un JOIN con la tabla Clientes. Sin esto, `ticket.Cliente` sería null. Esto genera SQL así:

```sql
SELECT t.*, c.* FROM Tickets t
INNER JOIN Clientes c ON t.ClienteId = c.Id
```

**`FirstOrDefaultAsync(t => t.Id == id)`** — busca el primer ticket donde `Id == id`. Si no existe, devuelve `null` (el `?` en `Task<Ticket?>`).

**`async/await`** — las operaciones de base de datos toman tiempo. `async` marca el método como asíncrono. `await` le dice "espera aquí hasta que termine, pero mientras esperas no bloquees el servidor para otras peticiones". Es como dejar un pedido en el mostrador y ir a sentarte — el mesero te llama cuando está listo, en vez de quedarte parado bloqueando la caja.

**`(object?)ticket.Descripcion ?? DBNull.Value`** — si `Descripcion` es null, pasar `DBNull.Value` a SQL Server. SQL no entiende `null` de C# directamente en este contexto — necesita `DBNull.Value`.

**Por qué InsertAsync usa EF Core pero Delete/Update usan SPs:**

Inconsistencia del proyecto. EF Core puede hacer los tres. Probablemente se usaron SPs para Delete/Update porque el profesor lo pedía así, y EF Core para Insert porque es más simple. En un proyecto real, se elegiría uno y se seguiría consistentemente.

---

### 4.7 Pages/Tickets/Index — Listar y Eliminar

**`Index.cshtml.cs`** (el "code-behind" de la página):

```csharp
public class TicketsPageModel : PageModel
{
    private readonly TicketsEfRepo _ticketsRepo;

    public TicketsPageModel(TicketsEfRepo ticketsRepo)
    {
        _ticketsRepo = ticketsRepo;
    }

    public IList<Ticket> TicketList { get; set; } = new List<Ticket>();

    // Se ejecuta cuando alguien hace GET /Tickets
    public async Task OnGetAsync()
    {
        TicketList = await _ticketsRepo.ListAsync();
    }

    // Se ejecuta cuando alguien hace POST /Tickets?handler=Delete&id=X
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _ticketsRepo.DeleteAsync(id);
        return RedirectToPage();
    }
}
```

**`PageModel`** — la clase base de todas las páginas Razor. Tiene acceso a Request, Response, etc.

**`OnGetAsync()`** — convención de nombres. ASP.NET Core llama automáticamente a `OnGetAsync` cuando llega un GET a esta página.

**`OnPostDeleteAsync(int id)`** — el sufijo `Delete` es el handler. Se activa cuando llega un POST con `?handler=Delete`. El parámetro `id` viene de `?id=X` en la URL.

**En `Index.cshtml`**, el truco del formulario oculto:

```html
<!-- Botón visible que el usuario hace clic -->
<a
  href="javascript:void(0);"
  onclick="document.getElementById('deleteForm-@ticket.Id').submit();"
>
  Eliminar
</a>

<!-- Formulario oculto que hace el POST real -->
<form
  id="deleteForm-@ticket.Id"
  method="post"
  asp-page-handler="Delete"
  asp-route-id="@ticket.Id"
  style="display:none;"
></form>
```

**Por qué esto?** No se puede hacer un DELETE de base de datos con un simple `<a href>` porque esos generan GET, no POST. Los formularios generan POST. La solución aquí es: el enlace visible activa un `submit()` en un formulario invisible que sí hace POST.

`asp-page-handler="Delete"` se traduce en la URL como `?handler=Delete`. ASP.NET Core lo usa para llamar `OnPostDeleteAsync`.

`asp-route-id="@ticket.Id"` agrega `&id=5` a la URL (donde 5 es el ID del ticket).

---

### 4.8 Pages/Tickets/Create — Crear Ticket

**`Create.cshtml.cs`**:

```csharp
[IgnoreAntiforgeryToken]  // ⚠️ PROBLEMA — ver Parte 6
public class CreateTicketModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly TicketsEfRepo _ticketsRepo;

    [BindProperty]
    public Ticket Ticket { get; set; } = new();

    public List<Cliente> Clientes { get; set; } = new();
    public List<Estado> Estados { get; set; } = new();

    private async Task CargarCombosAsync()
    {
        Clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
        Estados = await _context.Estados.OrderBy(e => e.Nombre).ToListAsync();
    }

    public async Task OnGetAsync()
    {
        await CargarCombosAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarCombosAsync();

        if (!ModelState.IsValid)
            return Page();

        Ticket.Prioridad = 1;
        Ticket.FechaCreacion = DateTime.Now;

        await _ticketsRepo.InsertAsync(Ticket);
        return RedirectToPage("/Tickets/Index");
    }
}
```

**`[BindProperty]`** — marca la propiedad `Ticket` para que ASP.NET Core la llene automáticamente con los datos del formulario cuando llega un POST. Sin esto, `Ticket` tendría sus valores en null/default después de un POST.

**¿Cómo funciona el bind?** En el formulario HTML:

```html
<input asp-for="Ticket.Titulo" />
```

El tag helper `asp-for` genera `<input name="Ticket.Titulo">`. Cuando llega el POST, ASP.NET Core ve `Ticket.Titulo=valor` en el body y lo asigna a `Ticket.Titulo` automáticamente.

**`CargarCombosAsync()`** — carga la lista de clientes y estados para los dropdowns. Se llama en `OnGetAsync()` (para mostrar el formulario vacío) y en `OnPostAsync()` (por si hay errores de validación y hay que redibujar el formulario — necesita los combos de nuevo).

**`ModelState.IsValid`** — comprueba que los datos del formulario cumplen las reglas de validación. Si el modelo tiene `[Required]` en un campo y el campo llegó vacío, `IsValid` será `false`.

**`Ticket.Prioridad = 1`** — la prioridad no está en el formulario de creación (se pone en edición). Aquí se fuerza a 1 (Baja) como valor por defecto.

**`RedirectToPage("/Tickets/Index")`** — después de guardar, redirige a la lista. Esto evita el problema de "doble submit" si el usuario recarga la página.

---

### 4.9 Pages/Tickets/Edit — Modificar Ticket

**`Edit.cshtml`**:

```html
@page "{id:int}"
```

Esta línea dice: "esta página espera un parámetro `id` de tipo entero en la URL". Así, `/Tickets/Edit/5` es válida, pero `/Tickets/Edit/abc` da error 400.

**`Edit.cshtml.cs`**:

```csharp
public async Task<IActionResult> OnGetAsync(int id)
{
    await CargarCombosAsync();

    var ticket = await _repo.GetAsync(id);
    if (ticket == null)
        return NotFound();    // devuelve HTTP 404

    Ticket = ticket;

    if (Ticket.Prioridad == 0)
        Ticket.Prioridad = 1;

    return Page();
}
```

`OnGetAsync(int id)` — ASP.NET Core toma el `id` de la URL (`/Tickets/Edit/5`) y lo pasa como parámetro automáticamente.

`return NotFound()` — si no existe el ticket con ese ID, devuelve un error 404. Esto es importante para que no se muestre un formulario vacío o con error.

**En el formulario de edición:**

```html
<input type="hidden" asp-for="Ticket.Id" />
```

Este campo oculto guarda el ID del ticket en el formulario. Cuando se hace POST, ese ID viaja de vuelta al servidor. Sin esto, el servidor no sabría qué ticket está siendo editado.

---

### 4.10 Pages/Clientes — Gestión de Clientes

Las páginas de clientes siguen el mismo patrón que las de tickets, con una diferencia: **no usan el repositorio**. Usan `ApplicationDbContext` directamente en el PageModel.

**`ClientesPageModel`** carga la lista con:

```csharp
Clientes = await _context.Clientes.ToListAsync();
```

Y elimina con:

```csharp
await _context.Database.ExecuteSqlRawAsync("EXEC sp_Clientes_Delete @Id", parameter);
```

**`CreateClienteModel`** inserta con:

```csharp
await _context.Database.ExecuteSqlRawAsync("EXEC sp_Clientes_Insert @Nombre, @Email, @Telefono", parameters);
```

**`EditClienteModel`** carga con `FindAsync(id)`:

```csharp
var cliente = await _context.Clientes.FindAsync(id);
```

`FindAsync` busca por clave primaria directamente. Es más eficiente que `FirstOrDefaultAsync` cuando buscas por PK.

**Inconsistencia con el formulario de CreateCliente:**

```html
<!-- CreateCliente.cshtml usa inputs manuales SIN asp-for -->
<input type="text" id="Nombre" name="Nombre" required />
```

En cambio, EditCliente usa TagHelpers:

```html
<input asp-for="Cliente.Nombre" class="form-control" />
```

La diferencia es que `asp-for` activa la validación del lado del cliente y genera los atributos necesarios. `name="Nombre"` funciona para el bind, pero no activa la validación integrada de ASP.NET. Ver Parte 6.

---

### 4.11 Pages/Estados — Reporte con PDF

**`EstadosPageModel`**:

```csharp
public async Task OnGetAsync()
{
    Estados = await _context.EstadosClientes
        .FromSqlRaw("EXEC sp_Estados_List")
        .ToListAsync();
}
```

`FromSqlRaw("EXEC sp_Estados_List")` ejecuta el stored procedure y mapea el resultado a `List<EstadoClienteVM>`. EF Core lee cada columna devuelta por el SP y la asigna a la propiedad correspondiente de `EstadoClienteVM` (por nombre).

**Generación de PDF (client-side):**

La función `generarPdfEstados()` en el JavaScript de la página usa **jsPDF** — una librería JavaScript que corre en el navegador. Lee la tabla HTML visible, extrae los datos, y genera un PDF que se descarga directamente.

Esto es **client-side PDF**: el servidor no hace nada para generar el PDF. Todo ocurre en el navegador del usuario. Es más simple pero tiene una limitación: el PDF solo puede contener lo que está visible en pantalla.

---

### 4.12 Pages/Shared/\_Layout.cshtml

```html
@{ Layout = null; }

<!DOCTYPE html>
<html lang="es">
  <head>
    <link rel="stylesheet" href="~/css/styles.css" />
  </head>
  <body>
    <header>...</header>

    <main>
      <div class="container">@RenderBody()</div>
    </main>

    <footer>...</footer>

    @RenderSection("Scripts", required: false)
  </body>
</html>
```

El **Layout** es la plantilla que comparten todas las páginas. Contiene el header, navegación, y footer.

**`@RenderBody()`** — aquí se "inyecta" el contenido de cada página. Cuando abres `/Tickets`, el contenido de `Tickets/Index.cshtml` reemplaza `@RenderBody()`.

**`@RenderSection("Scripts", required: false)`** — permite que páginas individuales agreguen JavaScript al final del `<body>`. La página de Estados lo usa para cargar jsPDF:

```html
@section Scripts {
<script src="...jspdf..."></script>
<script>
  function generarPdfEstados() {...}
</script>
}
```

El `required: false` significa que si una página no define esa sección, no hay error.

**`~/css/styles.css`** — el `~` en Razor es un alias para la raíz de `wwwroot/`. Razor lo transforma en la URL correcta.

---

### 4.13 wwwroot/ — Archivos Estáticos

```
wwwroot/
├── css/
│   └── styles.css        ← Tu CSS personalizado
├── js/
│   └── site.js           ← JavaScript personalizado (vacío)
├── lib/
│   ├── bootstrap/        ← Framework CSS de Bootstrap
│   ├── jquery/           ← Librería JavaScript de jQuery
│   └── jquery-validation/ ← Validación de formularios en el cliente
└── favicon.ico
```

**`wwwroot/`** es la única carpeta a la que el navegador puede acceder directamente. Todo lo demás (Models, Data, Pages) vive en el servidor y el usuario nunca lo ve.

`app.UseStaticFiles()` en Program.cs es lo que activa este servicio. Sin esa línea, el navegador no podría cargar el CSS ni el JavaScript.

---

## Parte 5 — Flujo Completo de un Request

### Ejemplo: El usuario crea un ticket

Vamos a seguir paso a paso qué ocurre cuando alguien llena el formulario de "Nuevo Ticket" y presiona el botón "Crear Ticket".

```
PASO 1: El usuario abre /Tickets/Create
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Navegador → GET /Tickets/Create → servidor

ASP.NET Core:
  1. Identifica que /Tickets/Create → Pages/Tickets/Create.cshtml
  2. Crea CreateTicketModel (inyecta ApplicationDbContext y TicketsEfRepo)
  3. Llama a OnGetAsync()

OnGetAsync():
  1. Llama a CargarCombosAsync()
  2. CargarCombosAsync() consulta la BD:
     SELECT * FROM Clientes ORDER BY Nombre
     SELECT * FROM Estados ORDER BY Nombre
  3. Guarda los resultados en Model.Clientes y Model.Estados

Razor renderiza Create.cshtml:
  1. Genera el HTML del formulario
  2. Los <select> se llenan con los clientes y estados cargados
  3. Devuelve el HTML completo al navegador

Resultado: El usuario ve el formulario vacío con los combos llenos.


PASO 2: El usuario llena el formulario y presiona "Crear Ticket"
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Navegador → POST /Tickets/Create
  Body: Ticket.Titulo=No%20funciona&Ticket.Descripcion=...&Ticket.ClienteId=2&Ticket.EstadoId=1

ASP.NET Core:
  1. Recibe el POST
  2. Ve que la propiedad Ticket tiene [BindProperty]
  3. Lee el body del request y llena Ticket automáticamente:
     Ticket.Titulo       = "No funciona"
     Ticket.Descripcion  = "..."
     Ticket.ClienteId    = 2
     Ticket.EstadoId     = 1
  4. Llama a OnPostAsync()

OnPostAsync():
  1. Llama a CargarCombosAsync() (por si hay que redibujar)
  2. Revisa ModelState.IsValid
     - ¿Titulo tiene valor? ✓
     - ¿ClienteId tiene valor? ✓
     - → IsValid = true
  3. Asigna valores que no vienen del form:
     Ticket.Prioridad     = 1
     Ticket.FechaCreacion = DateTime.Now
  4. Llama a _ticketsRepo.InsertAsync(Ticket)

InsertAsync():
  1. _context.Tickets.Add(ticket) — marca el ticket para inserción
  2. _context.SaveChangesAsync() — EF Core genera y ejecuta:
     INSERT INTO Tickets (Titulo, Descripcion, ClienteId, EstadoId, Prioridad, FechaCreacion)
     VALUES ('No funciona', '...', 2, 1, 1, '2025-01-15 10:30:00')
  3. EF Core lee el Id generado y lo asigna a Ticket.Id
  4. Devuelve el Id del nuevo ticket

OnPostAsync() continúa:
  5. return RedirectToPage("/Tickets/Index")
     → Devuelve HTTP 302 (redirección)

Navegador recibe 302:
  6. Hace automáticamente GET /Tickets/Index

Resultado: El usuario ve la lista de tickets con el nuevo ticket incluido.
```

---

## Parte 6 — Problemas Encontrados y Cómo Corregirlos

Aquí están todos los problemas del proyecto, explicados con el POR QUÉ son un problema y CÓMO los corriges tú mismo.

---

### Problema 1 — `[IgnoreAntiforgeryToken]` en Create.cshtml.cs

**Archivo:** `Pages/Tickets/Create.cshtml.cs` línea 14

**Qué hay ahora:**

```csharp
[IgnoreAntiforgeryToken]
public class CreateTicketModel : PageModel
```

**Por qué es un problema — CSRF:**

CSRF (Cross-Site Request Forgery) es un tipo de ataque. Imagina que alguien crea una web maliciosa con un formulario invisible que apunta a `/Tickets/Create`. Si un usuario de tu app visita esa web mientras tiene sesión activa, el formulario se puede enviar automáticamente en su nombre, creando tickets falsos.

Para defenderse, ASP.NET Core genera un **token antifalsificación** — un número secreto que se incluye en cada formulario legítimo. Si el POST no incluye ese token, el servidor lo rechaza.

`[IgnoreAntiforgeryToken]` desactiva esa protección. Fue puesto probablemente para "solucionar" un error 400 que aparecía durante el desarrollo.

**Cómo corregirlo:**

1. Elimina la línea `[IgnoreAntiforgeryToken]`
2. Agrega el token al formulario en `Create.cshtml`:

```html
<form method="post">@Html.AntiForgeryToken() ← agregar esta línea ...</form>
```

En realidad, los Tag Helpers lo incluyen automáticamente cuando usas `<form method="post">` junto con `_ViewImports.cshtml` que tiene `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`. Pero para estar seguro, agrégalo explícitamente.

---

### Problema 2 — Contraseña hardcodeada en appsettings.json

**Archivo:** `appsettings.json`

**Qué hay ahora:**

```json
"DefaultConnection": "Server=ROKOVZKY;Password=123456;..."
```

**Por qué es un problema:**

Si subes este archivo a GitHub (o cualquier repositorio), la contraseña queda pública para siempre. Incluso si después la borras, git guarda el historial.

**Cómo corregirlo — User Secrets (solo para desarrollo):**

```bash
# En la terminal, dentro del directorio del proyecto:
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=ROKOVZKY;Password=123456;Database=SoporteTickets;Trusted_Connection=True;TrustServerCertificate=True;"
```

Los User Secrets se guardan en tu máquina local (fuera del proyecto) y nunca van a git. En producción se usarían variables de entorno o Azure Key Vault.

**Agrega appsettings.json a .gitignore si tiene credenciales**, o borra la contraseña del archivo antes de hacer commit.

---

### Problema 3 — Debug.WriteLine en código de producción

**Archivo:** `Pages/Tickets/Create.cshtml.cs`

**Qué hay ahora:**

```csharp
Debug.WriteLine("🟢 OnGetAsync(CreateTicket) ejecutado");
Debug.WriteLine("🔵 OnPostAsync(CreateTicket) EJECUTADO");
Debug.WriteLine($"📌 Título: {Ticket.Titulo}");
```

**Por qué es un problema:**

`Debug.WriteLine` solo funciona en modo Debug de Visual Studio — en producción no aparece en ningún lado. No es un error, pero es código sucio que no sirve para nada en el proyecto final.

**Cómo corregirlo — eliminar o reemplazar con ILogger:**

```csharp
// Opción A: simplemente eliminar las líneas

// Opción B: usar el sistema de logging de ASP.NET Core
public class CreateTicketModel : PageModel
{
    private readonly ILogger<CreateTicketModel> _logger;

    public CreateTicketModel(ApplicationDbContext context, TicketsEfRepo repo, ILogger<CreateTicketModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        _logger.LogInformation("CreateTicket: OnGetAsync ejecutado");
        await CargarCombosAsync();
    }
}
```

`ILogger` escribe en el sistema de logs de ASP.NET Core, que sí funciona en producción y se puede configurar para escribir en archivos, bases de datos, o servicios como Application Insights.

---

### Problema 4 — Archivo duplicado con nombre incorrecto

**Archivo:** `Models/TicketsPageModel..cs` (doble punto)

**Qué hay ahora:** Una clase `TicketsPage` que es prácticamente igual a la `TicketsPageModel` que ya existe en `Pages/Tickets/Index.cshtml.cs`.

**Por qué es un problema:**

El archivo tiene un nombre con doble punto (bug tipográfico). La clase `TicketsPage` en `Models/` está en el namespace incorrecto y no se usa en ningún lado. Es código muerto.

**Cómo corregirlo:** Elimina el archivo `Models/TicketsPageModel..cs`.

---

### Problema 5 — CreateCliente.cshtml no usa TagHelpers

**Archivo:** `Pages/Clientes/CreateCliente.cshtml`

**Qué hay ahora:**

```html
<input type="text" id="Nombre" name="Nombre" required />
```

**Qué debería ser:**

```html
<input asp-for="Cliente.Nombre" class="form-control" />
<span asp-validation-for="Cliente.Nombre" class="text-danger"></span>
```

**Por qué es un problema:**

`name="Nombre"` funciona para el bind, pero:

- No activa la validación del lado del cliente (jQuery Validation)
- No muestra mensajes de error automáticamente
- El valor no se rellena si hay error y hay que redibujar el form

**Cómo corregirlo:** Reemplaza los `<input>` manuales por `asp-for` y agrega `<span asp-validation-for>` para los mensajes de error. Asegúrate de que `CreateClienteModel` tiene `[BindProperty]` en `Cliente`.

---

### Problema 6 — Ticket sin validaciones de datos

**Archivo:** `Models/Ticket.cs`

**Qué hay ahora:**

```csharp
public string Titulo { get; set; }
public string Descripcion { get; set; }
```

**Qué debería ser:**

```csharp
using System.ComponentModel.DataAnnotations;

[Required(ErrorMessage = "El título es obligatorio")]
[MaxLength(200, ErrorMessage = "Máximo 200 caracteres")]
public string Titulo { get; set; } = "";

[MaxLength(2000, ErrorMessage = "Máximo 2000 caracteres")]
public string? Descripcion { get; set; }
```

**Por qué:** Sin `[Required]`, `ModelState.IsValid` siempre será `true` para esos campos, incluso si llegan vacíos. Los Data Annotations agregan validación automática tanto en el servidor como (con jQuery Validation) en el cliente.

---

## Parte 7 — Mejoras Posibles

Estas no son correcciones urgentes — son ideas para que el proyecto crezca. Las pones en práctica cuando quieras, cuando ya te sientas cómodo con el código actual.

### 7.1 Confirmación antes de eliminar

Ahora mismo, hacer clic en "Eliminar" elimina el ticket/cliente al instante sin pedir confirmación. En producción esto es peligroso.

**Mejora:** Agregar un diálogo de confirmación con JavaScript:

```javascript
onclick =
  "if(confirm('¿Seguro que deseas eliminar este ticket?')) document.getElementById('deleteForm-@ticket.Id').submit();";
```

### 7.2 Mover lógica de Clientes al repositorio

Los PageModels de Clientes llaman directamente a `_context.Database.ExecuteSqlRawAsync(...)`. Eso viola el principio del repositorio.

**Mejora:** Crear un `ClientesEfRepo` similar a `TicketsEfRepo` con métodos `InsertAsync`, `UpdateAsync`, `DeleteAsync`, y `ListAsync`. Los PageModels de Clientes lo usan igual que los de Tickets.

### 7.3 FechaActualizacion nunca se actualiza en el Insert

El SP `sp_Tickets_Update` ya actualiza `FechaActualizacion = GETDATE()`. Pero cuando se crea un ticket con EF Core en `InsertAsync()`, `FechaActualizacion` queda como `NULL` en la BD — lo cual está bien (un ticket nuevo no ha sido modificado).

Sin embargo, la tabla de tickets en `Index.cshtml` no muestra `FechaActualizacion`. Podría ser útil mostrarlo si se modificó.

### 7.4 Paginación en la lista de tickets

Si hay 1000 tickets, `ListAsync()` los carga todos. EF Core permite paginar fácilmente:

```csharp
return await _context.Tickets
    .Include(t => t.Cliente)
    .Include(t => t.Estado)
    .Skip((pagina - 1) * tamanoPagina)
    .Take(tamanoPagina)
    .ToListAsync();
```

### 7.5 Búsqueda y filtrado

Agregar un `<input type="search">` en la página de tickets y filtrar por cliente, estado, o texto en el título.

### 7.6 Página de confirmación de eliminación (Delete.cshtml)

Existe `Delete.cshtml.cs` pero no hay un `Delete.cshtml` visible. El proyecto actualmente usa el formulario oculto directo (sin página de confirmación). Una mejora sería crear la página `Delete.cshtml` que muestre el ticket y pida confirmación antes de eliminarlo.

---

## Resumen de Archivos del Proyecto

Para tener todo claro, aquí está cada archivo y para qué sirve:

| Archivo                                    | Qué es        | Para qué                                   |
| ------------------------------------------ | ------------- | ------------------------------------------ |
| `Program.cs`                               | Entry point   | Configura y arranca la app                 |
| `appsettings.json`                         | Configuración | Connection string y logging                |
| `Data/ApplicationDbContext.cs`             | DbContext     | Conexión C# ↔ SQL Server                   |
| `Models/Ticket.cs`                         | Entidad       | Representa la tabla Tickets                |
| `Models/Cliente.cs`                        | Entidad       | Representa la tabla Clientes               |
| `Models/Estado.cs`                         | Entidad       | Representa la tabla Estados                |
| `Models/EstadoClienteVM.cs`                | ViewModel     | Resultado del SP sp_Estados_List           |
| `Models/TicketsEfRepo.cs`                  | Repositorio   | Abstrae el acceso a datos de Tickets       |
| `Pages/Tickets/Index.cshtml(.cs)`          | Página        | Lista y elimina tickets                    |
| `Pages/Tickets/Create.cshtml(.cs)`         | Página        | Crea un nuevo ticket                       |
| `Pages/Tickets/Edit.cshtml(.cs)`           | Página        | Modifica un ticket existente               |
| `Pages/Tickets/Delete.cshtml.cs`           | PageModel     | Lógica de eliminación (sin vista propia)   |
| `Pages/Clientes/Index.cshtml(.cs)`         | Página        | Lista y elimina clientes                   |
| `Pages/Clientes/CreateCliente.cshtml(.cs)` | Página        | Crea un nuevo cliente                      |
| `Pages/Clientes/EditCliente.cshtml(.cs)`   | Página        | Modifica un cliente                        |
| `Pages/Estados/Index.cshtml(.cs)`          | Página        | Reporte de estados + generación PDF        |
| `Pages/Shared/_Layout.cshtml`              | Layout        | Plantilla compartida (header, nav, footer) |
| `wwwroot/css/styles.css`                   | CSS           | Estilos visuales de la app                 |
| `wwwroot/lib/bootstrap/`                   | Librería      | Framework CSS/JS de Bootstrap              |

---

_Joshua Hernandez — UTT, Ingeniería en Entornos Virtuales y Negocios Digitales_
_Proyecto: SoporteDeTickets · 2025_
_Stack: ASP.NET Core 9.0 · Entity Framework Core 9.0 · SQL Server_
