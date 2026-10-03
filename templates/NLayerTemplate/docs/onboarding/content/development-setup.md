- [Development Set-Up](#development-set-up)
  - [NET Core Development](#net-core-development)

---

# Development Set-Up

## NET Core Development

- <https://dotnet.microsoft.com/en-us/download/dotnet>

Specific NET Core version installation:

- In Windows:

  ```powershell
  winget install Microsoft.DotNet.SDK.10
  ```

For running EF Core migrations correctly, get the correct tool versions:

```powershell
> dotnet tool uninstall --global dotnet-ef
> dotnet tool install --global dotnet-ef
```

Add correct packages:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```
