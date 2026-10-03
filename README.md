- [DotNet Template Kit](#dotnet-template-kit)
  - [(1) Available templates](#1-available-templates)
    - [Single-Project Solution](#single-project-solution)
    - [Multi-Project Solution](#multi-project-solution)
  - [(2) Branching](#2-branching)
  - [(3) Requirements](#3-requirements)
  - [(4) Installation](#4-installation)
  - [(5) Quick start](#5-quick-start)
  - [(6) Template options](#6-template-options)
  - [(7) Documentation](#7-documentation)
  - [(8) Project status](#8-project-status)
  - [(9) Contributing](#9-contributing)
  - [(10) License](#10-license)

---

![DotNet Template Kit banner](./docs/img/banner.png)

---

![Under Construction](./docs/img/under-construction.jpg)

# DotNet Template Kit

<https://github.com/carloswm85/dotnet-template-kit>

- DotNet Template Kit is a collection of opinionated .NET 10 solution templates for building web applications with architectures of increasing complexity.
- Choose:
    - **Lightweight monolith** for smaller applications,
    - **N-layer** solution for clear separation of concerns, or
    - **Clean Architecture** template for domain-rich systems with strict dependency boundaries.
- The previous N-Layer Angular client implementation is preserved in the [`archive/dtk-nlayer-with-angular`](https://github.com/carloswm85/dotnet-template-kit/tree/archive/dtk-nlayer-with-angular) branch.

---

## (1) Available templates

- `✅ Ready - 🚧 In progress - 📋 Planned - ⛔ Blocked`
- Increasing complexity, top to bottom.

### Single-Project Solution

| Code  | Architecture    | Status | Recommended for                             | Template command                 | Documentation                                        |
| ----- | --------------- | ------ | ------------------------------------------- | -------------------------------- | ---------------------------------------------------- |
| `SMA` | Simple Monolith | ✅     | CRUD applications, internal tools, and MVPs | `dotnet new dtk-simple-monolith` | [docs](./templates/SimpleMonolithTemplate/README.md) |

### Multi-Project Solution

| Code  | Architecture       | Status | Recommended for                                                                    | Template command                    | Documentation                                           |
| ----- | ------------------ | ------ | ---------------------------------------------------------------------------------- | ----------------------------------- | ------------------------------------------------------- |
| `NLA` | N-Layer            | ✅     | Enterprise applications, large teams, and long-term maintenance                    | `dotnet new dtk-nlayer`             | [docs](./templates/NLayerTemplate/README.md)            |
| `VSA` | Vertical Slice     | 📋     | Independent API features, microservices, and modular monoliths                     | Not available yet                   | [docs](./templates/VerticalSliceTemplate/README.md)     |
| `CNA` | Clean Architecture | ✅     | Clean Architecture applications requiring implementing `ASP.NET Core Identity API` | `dotnet new dtk-clean-architecture` | [docs](./templates/CleanArchitectureTemplate/README.md) |

## (2) Branching

```powershell
`main` # Latest version of the project, this is the working branch
   | # Working versions are migrated to latest available version
   |-> `dtk-netcore10-identity` # Current and latest (2026)
   |-> `dtk-netcore08-lightweight` # Archived, working but not receiving changes
```

## (3) Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A development environment such as Visual Studio, Visual Studio Code, or JetBrains Rider
- Docker Desktop for templates that provide container orchestration
- Node.js and Angular tooling when using a template that includes an Angular client

Run the following command to verify the installed .NET SDKs:

```powershell
dotnet --list-sdks
```

---

## (4) Installation

Clone the repository and install its templates from the repository root:

```powershell
git clone https://github.com/carloswm85/dotnet-template-kit.git
Set-Location ./dotnet-template-kit
dotnet new install .
```

List the installed DTK templates:

```powershell
dotnet new list dtk
```

To identify the registration name before uninstalling the templates, run:

```powershell
dotnet new uninstall
```

Then pass the listed package or directory identifier to `dotnet new uninstall`.

---

## (5) Quick start

Create a Basic N-Layer solution named `Contoso.BackOffice`:

```powershell
dotnet new <template-name> --name ContosoBackOffice
Set-Location ./ContosoBackOffice
dotnet restore
```

Use another command from the [available templates](#1-available-templates) table to select a different architecture.

---

## (6) Template options

The available templates target .NET 10 and support the following shared options:

| Option                   | Type     | Default           | Description                                                  |
| ------------------------ | -------- | ----------------- | ------------------------------------------------------------ |
| `--name`                 | `string` | Template-specific | Sets the generated solution and project name                 |
| `--IncludeDocumentation` | `bool`   | `false`           | Includes explanatory documentation in the generated solution |

Inspect every option supported by a template before creating a solution:

```powershell
dotnet new <template-name> --help
```

Example with documentation included:

```powershell
dotnet new <template-name> `
    --name ContosoBackOffice `
    --IncludeDocumentation true
```

---

## (7) Documentation

- [Project documentation](./docs/README.md)
- [Microsoft .NET application architecture](https://learn.microsoft.com/dotnet/architecture/)
- [Azure Architecture Center](https://learn.microsoft.com/azure/architecture/)
- [.NET architecture guides](https://github.com/dotnet-architecture/eBooks)
- Clean Architecture examples:
    - [Ardalis CleanArchitecture](https://github.com/ardalis/CleanArchitecture)
    - [Jason Taylor CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)

---

## (8) Project status

- The `main` branch contains the latest supported templates and documentation.
- Templates still under development are identified in the [available templates](#1-available-templates) table.

---

## (9) Contributing

Issues and pull requests are welcome. Before proposing a change:

- Verify that it targets a supported template.
- Keep architecture-specific changes inside the corresponding template directory.
- Update the relevant template documentation.
- Confirm that the template installs and creates a new solution successfully.

---

## (10) License

DotNet Template Kit is available under the terms of the [MIT License](./LICENSE.txt).
