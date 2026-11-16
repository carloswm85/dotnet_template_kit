- [CleanArchitectureTemplate Solution](#cleanarchitecturetemplate-solution)
  - [(s1) First-run setup](#s1-first-run-setup)

---

# CleanArchitectureTemplate Solution

Based on Basic N-Layer Architecture Template: <https://github.com/carloswm85/dotnet-template-kit/>

Find documentation reference for this template project at: [./docs/onboarding/README.md](./docs/onboarding/README.md)

## (s1) First-run setup

Local configuration files are not committed. Each one has a committed
`*.example` copy with `<my-...>` placeholders.

| File to create                                         | Copy from                          | Replace           |
| ------------------------------------------------------ | ---------------------------------- | ----------------- |
| `.env`                                                 | `.env.example`                     | `<my-password>`   |
| `solution/CleanArchitectureTemplate.Web/appsettings.Development.json` | `appsettings.Development.example.json` | Nothing required |
| `solution/CleanArchitectureTemplate.API/appsettings.Development.json` | `appsettings.Development.example.json` | `<my-secret>`     |

1. Copy the files from the solution root:

   ```powershell
   # [cb8]
   Copy-Item .env.example .env
   $projects = "Web", "API"
   foreach ($p in $projects) {
       $dir = "solution/CleanArchitectureTemplate.$p"
       Copy-Item "$dir/appsettings.Development.example.json" `
           "$dir/appsettings.Development.json"
   }
   ```

1. Generate a JWT secret and paste it as `JwtSettings:SecretKey` in the API
   file:

   ```powershell
   # [cb9]
   [Convert]::ToBase64String(
       [Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
   ```

1. In `.env`, set the same SA password in `MSSQL_SA_PASSWORD` and in
   `DOCKER_SQLSERVER_CONNECTION_STRING`. It is needed only for
   `docker compose up`.

`.env` and `appsettings.Development.json` are ignored by Git and excluded
from the template package.
