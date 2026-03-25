# Soporte de Tickets — Zynapse

Sistema web de gestión de tickets de soporte técnico desarrollado con ASP.NET Core y SQL Server.

## Funcionalidades

- **Tickets** — crear, listar, modificar y eliminar tickets de soporte
- **Clientes** — gestión completa de clientes
- **Estados** — reporte de tickets por estado con exportación a PDF

## Stack

- ASP.NET Core 9.0 (Razor Pages)
- Entity Framework Core 9.0
- SQL Server (Stored Procedures)
- Bootstrap 5

## Requisitos

- .NET 9 SDK
- SQL Server (local o remoto)
- Visual Studio 2022 o VS Code con extensión C#

## Configuración

**1. Crear la base de datos**

Ejecuta el script SQL incluido en `DOCUMENTACION.md` (Parte 2) en SQL Server Management Studio para crear las tablas y los stored procedures necesarios.

**2. Connection string**

Edita `SoporteDeTickets/appsettings.json` con los datos de tu servidor:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=SoporteTickets;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**3. Ejecutar**

```bash
cd SoporteDeTickets
dotnet run
```

La app estará disponible en `https://localhost:7232`.

## Estructura del proyecto

```
SoporteDeTickets/
├── Data/               → DbContext (EF Core)
├── Models/             → Entidades y repositorio
├── Pages/
│   ├── Tickets/        → CRUD de tickets
│   ├── Clientes/       → Gestión de clientes
│   └── Estados/        → Reporte de estados
└── wwwroot/            → Archivos estáticos (CSS, JS)
```

## Documentación

Ver [DOCUMENTACION.md](DOCUMENTACION.md) para la explicación completa de la arquitectura, base de datos, flujo de datos y notas de mejora.

---

Joshua Hernandez — UTT, Ingeniería en Entornos Virtuales y Negocios Digitales
