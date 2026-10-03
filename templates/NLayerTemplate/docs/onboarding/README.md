- [NLayer Template](#nlayer-template)
  - [Version compatibility](#version-compatibility)
    - [Table: .NET Core](#table-net-core)
    - [Table: Identity API](#table-identity-api)
    - [Table: MVC](#table-mvc)
  - [Content](#content)
    - [`Contoso University` Tutorial Example](#contoso-university-tutorial-example)

---

| STATUS                                                                |
| --------------------------------------------------------------------- |
| .NET Core 10 solution with API and MVC presentation projects.          |
| You can find previous functional versions in the repository branches. |

---

# NLayer Template

---

## Version compatibility

### Table: .NET Core

| Current | .NET Core | .NET Core release type        | EF Core  | Status            |
| ------- | --------- | ----------------------------- | -------- | ----------------- |
| ✅      | `10`      | LTS (ends: November 14, 2028) | -        | Under development |
|         | `9`       | STS (ends: November 10, 2026) | -        | Skipped           |
|         | `8.0.100` | LTS (ends: November 10, 2026) | `8.0.22` | Fully functional  |

[.NET and .NET Core Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) ↗

### Table: Identity API

| Current | Identity API (compatible with this EF Core) | Status      |
| ------- | ------------------------------------------- | ----------- |
| ❌      | `8.0.21`                                    | Unsupported |

[Official Documentation on Identity API](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=visual-studio) ↗

### Table: MVC

| Current | Bootstrap | jQuery  | jQuery Validate | Jquery Validation Unobtrusive | Status           |
| ------- | --------- | ------- | --------------- | ----------------------------- | ---------------- |
| ✅      | `5.3.8`   | `3.7.1` | `1.21.0`        | `4.0.0`                       | Fully functional |

- Used `libman.json` for client side libraries.
- Bootstrap `+5.x` does not depend on `jQuery`.

## Content

- Section 1:
  - [Solution Artchitecture](./content/architecture.md)
  - [Template Installation and Use](./content/template-use.md)
  - [Troubleshooting](./content/troubleshooting.md)
- Section 2:
  - [Development Set-Up](./content/development-setup.md)
  - Dependencies:
    - [NET Core](./content/dependencies-net-core.md)
      - [Identity API](./content/identity-api.md)
- Section 3:
  - [Education](./content/education.md)

---

### `Contoso University` Tutorial Example

- This template has a built in example case for use and testing of database, EF Core, API endpoints and MVC frontend.
- The example is partially completed.
- For additional information on this example, see the official tutorial: <https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/?view=aspnetcore-8.0>

![alt text](./img/contoso-db-diagram.png)
