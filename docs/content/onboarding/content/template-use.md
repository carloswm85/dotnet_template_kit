- [Template Installation And Use](#template-installation-and-use)
    - [From Nuget](#from-nuget)
    - [From Local Repository](#from-local-repository)
    - [Using The Template](#using-the-template)

---

# Template Installation And Use

## From Nuget

// TODO

## From Local Repository

1. Clone or download repository.
1. Install from local folder.
1. At root level `dotnet_template_kit` run:

   ```powershell
   # dotnet_template_kit\.
   dotnet new install .
   ```

   Or:

   ```powershell
   # dotnet_template_kit\.
   dotnet new uninstall .
   ```

1. Use any template from anywhere (`dtk-simple-monolith`, `dtk-nlayer`, or
   `dtk-clean-architecture`):

```powershell
dotnet new dtk-nlayer -o "NLayerExample"
```

## Using The Template

```powershell
# Custom solution name (and path, if included in the string)
dotnet new dtk-nlayer -o "../../MyFolder/NLayerExample2"
```

```powershell
# Force file generation, and override existing files if any
dotnet new dtk-nlayer -o "NLayerExample3" --force
```

```powershell
# Clean Architecture with only the API host (API, Web, or Both)
dotnet new dtk-clean-architecture -o "CleanApiExample" --Hosts API
```
