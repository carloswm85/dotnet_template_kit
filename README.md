# Archived DTK N-Layer with Angular template

- [(1) Purpose](#1-purpose)
- [(2) Retained solution](#2-retained-solution)
- [(3) Technology stack](#3-technology-stack)
- [(4) Requirements](#4-requirements)
- [(5) Generate a solution](#5-generate-a-solution)
- [(6) Run the solution](#6-run-the-solution)
- [(7) Branch status](#7-branch-status)

> [!IMPORTANT]
> This archived branch preserves the DTK N-Layer solution template with its Angular client.
> It is retained as a historical and reusable reference rather than as the active multi-template branch.

## (1) Purpose

This branch contains one .NET 10 solution template built around a traditional N-Layer architecture.
It preserves both ASP\.NET Core presentation options and the Angular single-page application.

Template command: `dotnet new dtk-nlayer`

## (2) Retained solution

| # | Project | Responsibility |
|---:|---|---|
| 1 | `NLayerTemplate.Data` | Entities, EF Core context, migrations, and database initialization |
| 2 | `NLayerTemplate.Repository` | Repositories, Unit of Work, and stored-procedure access |
| 3 | `NLayerTemplate.Service` | Application services, DTOs, and mapping |
| 4 | `NLayerTemplate.Web.API` | Versioned ASP\.NET Core Web API and OpenAPI documentation |
| 5 | `NLayerTemplate.Web.MVC` | Server-rendered ASP\.NET Core MVC application |
| 6 | `NLayerTemplate.Web.Angular` | Angular single-page application |
| 7 | `NLayerTemplate.Testing.Unit` | Service-layer unit tests |
| 8 | `NLayerTemplate.Testing.Integration` | MVC integration tests |

## (3) Technology stack

### (3.1) Solution overview

| # | Layer | Main elements |
|---:|---|---|
| 1 | Data | EF Core, SQL Server, configuration |
| 2 | Repository | Generic Repository, Unit of Work, stored procedures |
| 3 | Service | Application services, DTOs, Mapster |
| 4 | Web API | ASP\.NET Core API, versioning, Swagger, SPA proxy |
| 5 | Web MVC | ASP\.NET Core MVC, Mapster, Bootstrap, jQuery |
| 6 | Angular | Angular, TypeScript, RxJS, Jasmine/Karma |
| 7 | Testing | xUnit, FluentAssertions, Moq, MVC Testing |
| 8 | Infrastructure | Docker, SQL Server container, CSharpier |

### (3.2) Data layer

| # | Main element | Version |
|---:|---|---|
| 1 | .NET | `net10.0` |
| 2 | Entity Framework Core | `10.0.1` |
| 3 | SQL Server provider | `10.0.1` |
| 4 | Configuration packages | `10.0.1` |

### (3.3) Repository layer

| # | Main element | Version |
|---:|---|---|
| 1 | .NET | `net10.0` |
| 2 | Generic Repository | Internal |
| 3 | Unit of Work | Internal |
| 4 | Stored Procedure Repository | Internal |

### (3.4) Service layer

| # | Main element | Version |
|---:|---|---|
| 1 | .NET | `net10.0` |
| 2 | Application services and DTOs | Internal |
| 3 | Mapster | `10.0.7` |
| 4 | ASP\.NET HTTP Features | `5.0.17` |

### (3.5) Web API layer

| # | Main element | Version |
|---:|---|---|
| 1 | ASP\.NET Core Web API | `net10.0` |
| 2 | API Versioning | `8.1.1` |
| 3 | Swashbuckle | `10.1.0` |
| 4 | Microsoft OpenAPI | `2.11.0` |
| 5 | SPA Proxy | `10.0.1` |

### (3.6) Web MVC layer

| # | Main element | Version |
|---:|---|---|
| 1 | ASP\.NET Core MVC | `net10.0` |
| 2 | Mapster | `10.0.7` |
| 3 | Bootstrap | `5.3.8` |
| 4 | jQuery | `3.7.1` |
| 5 | Library Manager | `3.0.71` |

### (3.7) Angular layer

| # | Main element | Locked version |
|---:|---|---|
| 1 | Angular | `20.3.17` |
| 2 | Angular CLI | `20.3.18` |
| 3 | TypeScript | `5.9.3` |
| 4 | RxJS | `7.8.2` |
| 5 | Jasmine / Karma | `5.6.0` / `6.4.4` |

### (3.8) Testing layer

| # | Main element | Version |
|---:|---|---|
| 1 | xUnit v3 | `3.2.2` |
| 2 | FluentAssertions | `8.8.0` |
| 3 | Moq | `4.20.72` |
| 4 | ASP\.NET Core MVC Testing | `10.0.5` |

### (3.9) Infrastructure

| # | Main element | Version |
|---:|---|---|
| 1 | Docker Compose | - |
| 2 | .NET containers | `10.0` |
| 3 | SQL Server container | `2022-latest` |
| 4 | CSharpier | `1.2.6` |

## (4) Requirements

- .NET 10 SDK
- A compatible Node.js and npm installation
- Visual Studio JavaScript SDK support when building the `.esproj` through MSBuild
- Docker Desktop when running the containerized MVC and SQL Server services

Verify the installed .NET SDKs:

```powershell
dotnet --list-sdks
```

## (5) Generate a solution

Install the template from the repository root:

```powershell
dotnet new install .
```

Generate a solution named `Contoso.BackOffice`:

```powershell
dotnet new dtk-nlayer --name Contoso.BackOffice
Set-Location ./Contoso.BackOffice
```

## (6) Run the solution

Start the API:

```powershell
dotnet run --project ./solution/Contoso.BackOffice.Web.API
```

Start the MVC application:

```powershell
dotnet run --project ./solution/Contoso.BackOffice.Web.MVC
```

Install and start the Angular client:

```powershell
Set-Location ./solution/Contoso.BackOffice.Web.Angular
npm install
npm start
```

Alternatively, start the MVC application and SQL Server with Docker Compose:

```powershell
docker compose up --build
```

## (7) Branch status

- Branch: `legacy/dtk-nlayer-with-angular`
- Status: archived
- Scope: N-Layer template with Angular, Web API, and MVC presentation projects
- Active development continues on the repository's main branch: <https://github.com/carloswm85/dotnet-template-kit>
