- [Troubleshooting](#troubleshooting)
  - [NET Core](#net-core)
    - [HTTPS Developer ASP.NET Certificate](#https-developer-aspnet-certificate)
    - [API](#api)

---

# Troubleshooting

---

## NET Core

### HTTPS Developer ASP.NET Certificate

```powershell
> dotnet dev-certs https --clean
> dotnet dev-certs https --trust
> dotnet dev-certs https --check
```

### API

- The API project does not work in Visual Studio 2022.
- Targeting .NET 10.0 or higher in Visual Studio 2022 17.14 is not supported, use Visual Studio 2026 instead.
