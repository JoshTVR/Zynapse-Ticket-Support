<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:512BD4,50:3b82f6,100:0ea5e9&height=180&section=header&text=Zynapse%20Ticket%20Support&fontSize=44&fontAlignY=38&animation=fadeIn&desc=ASP.NET%20Core%209%20·%20Razor%20Pages%20·%20SQL%20Server&descAlignY=60&descSize=15&fontColor=ffffff"/>

<br/>

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://learn.microsoft.com/en-us/sql/sql-server)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp)
[![License](https://img.shields.io/badge/License-MIT-7c3aed?style=for-the-badge)](LICENSE)

</div>

---

## About

**Zynapse Ticket Support** is a web-based technical support ticket management system built with ASP.NET Core 9 and Razor Pages.

Designed to handle the full lifecycle of a support request — from creation and assignment to resolution and PDF reporting — with a clean multi-client architecture.

---

## Features

- **Ticket CRUD** — create, view, update, and close support tickets
- **Client Management** — manage clients tied to ticket requests
- **Status Tracking** — open, in-progress, and resolved states
- **PDF Export** — generate printable reports per ticket or by status
- **Razor Pages UI** — server-rendered, fast, and straightforward

---

## Tech Stack

<div align="center">

[![C#](https://skillicons.dev/icons?i=cs)](https://learn.microsoft.com/en-us/dotnet/csharp)
[![.NET](https://skillicons.dev/icons?i=dotnet)](https://dotnet.microsoft.com)
[![SQL Server](https://skillicons.dev/icons?i=sqlserver)](https://learn.microsoft.com/en-us/sql/sql-server)
[![Visual Studio](https://skillicons.dev/icons?i=visualstudio)](https://visualstudio.microsoft.com)

</div>

---

## Project Structure

```
SoporteDeTickets/
├── Pages/              # Razor Pages (UI + page models)
├── Models/             # Domain entities
├── Data/               # EF Core DbContext + migrations
└── wwwroot/            # Static assets

SoporteDeTickets.sln    # Visual Studio solution
DOCUMENTACION.md        # Full Spanish technical documentation
```

---

## Local Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or Express)

### Steps

```bash
git clone https://github.com/JoshTVR/Zynapse-Ticket-Support.git
cd Zynapse-Ticket-Support
```

Update the connection string in `appsettings.json`, then:

```bash
dotnet ef database update
dotnet run --project SoporteDeTickets
```

---

<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0ea5e9,50:3b82f6,100:512BD4&height=100&section=footer&animation=fadeIn"/>

</div>
