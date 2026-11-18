- [CleanArchitectureTemplate Solution](#cleanarchitecturetemplate-solution)
  - [(s1) First-run setup](#s1-first-run-setup)
    - [(s1.1) Host selection](#s11-host-selection)
      - [(s1.1.1) Web-only: external API configuration](#s111-web-only-external-api-configuration)
  - [(s2) Configuration reference](#s2-configuration-reference)
  - [(s3) Operations](#s3-operations)
  - [(s4) Development tooling](#s4-development-tooling)
  - [(s5) Upgrade notes](#s5-upgrade-notes)

---

# CleanArchitectureTemplate Solution

Based on Basic N-Layer Architecture Template: <https://github.com/carloswm85/dotnet-template-kit/>

Find documentation reference for this template project at: [./docs/onboarding/README.md](./docs/onboarding/README.md)

## (s1) First-run setup

Local configuration files are not committed. Each one has a committed
`*.example` copy with `<my-...>` placeholders.

| File to create                                         | Copy from                          | Replace           |
| ------------------------------------------------------ | ---------------------------------- | ----------------- |
| `.env`                                                 | `.env.example`                     | `<my-password>`, `<my-secret>` |
| `solution/CleanArchitectureTemplate.Web/appsettings.Development.json` | `appsettings.Development.example.json` | Nothing required |
| `solution/CleanArchitectureTemplate.API/appsettings.Development.json` | `appsettings.Development.example.json` | `<my-secret>`     |

1. Copy the files from the solution root:

   ```powershell
   # [cb8]
   Copy-Item .env.example .env
   $projects = "Web", "API"
   foreach ($p in $projects) {
       $dir = "solution/CleanArchitectureTemplate.$p"
       if (-not (Test-Path $dir)) { continue } # host not generated
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
   `DOCKER_SQLSERVER_CONNECTION_STRING`, and a JWT secret in
   `JWT_SECRET_KEY`. They are needed only for `docker compose up`.

`.env` and `appsettings.Development.json` are ignored by Git and excluded
from the template package.

### (s1.1) Host selection

The `--Hosts` option chooses which host projects are generated.
ApplicationCore, Infrastructure, and the test projects are always included.

| `--Hosts` | Projects            | Docker Compose services   | Integration tests              |
| --------- | ------------------- | ------------------------- | ------------------------------ |
| `Both`    | API + Web (default) | Web, API, SQL Server      | All                            |
| `API`     | API                 | API, SQL Server           | API and shared service tests   |
| `Web`     | Web                 | Web, SQL Server           | Web and shared service tests   |

```powershell
# [cb10]
dotnet new dtk-clean-architecture --name Contoso.Api --Hosts API
dotnet new dtk-clean-architecture --name Contoso.Portal --Hosts Web
```

Rows in this README marked for a host that was not generated do not apply.
Each host creates and seeds the database on startup, so either one works
alone.

#### (s1.1.1) Web-only: external API configuration

The Web app calls an API through the named `HttpClient` `ApiClient`, which
reads `ApiBaseUrl`. The app starts without it, but every API call throws
`InvalidOperationException` while it is empty.

| Where it runs    | Set                                                                 |
| ---------------- | ------------------------------------------------------------------- |
| Local (`dotnet run`) | `ApiBaseUrl` in `solution/CleanArchitectureTemplate.Web/appsettings.Development.json` |
| Docker Compose   | `API_BASE_URL` in `.env` (mapped to `ApiBaseUrl`)                    |
| Other hosts      | Environment variable `ApiBaseUrl`                                    |

The external API must expose the routes the Web services call, such as the
weather forecast example, and accept the Web app's origin if it enforces
CORS. Use an address the Web process can reach, for example
`http://host.docker.internal:5018` from a container to an API on the host.

## (s2) Configuration reference

Settings live in `appsettings.json` (defaults), `appsettings.{Environment}.json`
and environment variables (`Section__Key`).

| Setting | Host | Default | Purpose |
| --- | --- | --- | --- |
| `DataProtection:ApplicationName` | Web, API | `CleanArchitectureTemplate` | Key ring name; keys persist to `/app/DataProtection-Keys` in containers. |
| `Hosting:PathBase` | Web, API | empty | Sub-path behind a reverse proxy, e.g. `/apps/web`. |
| `Hosting:ForwardedHeaders:Enabled` | Web, API | `false` | Trust `X-Forwarded-For`/`-Proto` from a proxy. |
| `Hosting:ForwardedHeaders:KnownProxies` | Web, API | `[]` | Proxy IPs to trust. Empty trusts any source. |
| `RateLimiting:Authentication:PermitLimit` | Web, API | `10` | Login/register/reset/refresh requests per window and IP. |
| `RateLimiting:Authentication:WindowSeconds` | Web, API | `60` | Rate-limit window. |
| `Localization:DefaultCulture` | Web | `en-US` | Default request culture. |
| `Localization:SupportedCultures` | Web | `["en-US"]` | Accepted cultures. |
| `Uploads:RootPath` | Web | `wwwroot/uploads` | Folder for uploaded images, served at `/uploads`. |
| `ApiBaseUrl` | Web | none | Base URL of the API used by `ApiClient`. Required for API calls; see (s1.1.1). |
| `JwtSettings:SecretKey` | API | none | At least 32 bytes. Required. |
| `JwtSettings:Issuer`, `JwtSettings:Audience` | API | none | Required. |
| `JwtSettings:ExpirationMinutes` | API | `60` | Access-token lifetime. |
| `JwtSettings:RefreshTokenLifetimeMinutes` | API | `10080` | Refresh-token lifetime (7 days). |
| `Swagger:Enabled` | API | Development only | Swagger UI and JSON. |

Runtime settings that change without a redeploy live in the `AppSetting`
table:

| Name | Default | Purpose |
| --- | --- | --- |
| `IsAppOnline` | `true` | `false` puts Web and API in maintenance mode. SuperAdmin users still get in to the Web app. |
| `AnnouncementMessage` | empty | Text for a banner at the top of every page. Empty hides it. |

## (s3) Operations

| Endpoint | Host | Purpose |
| --- | --- | --- |
| `/health/live` | Web, API | The process responds. No database check. |
| `/health/ready` | Web, API | The database is reachable (503 when it is not). |
| `/health/database-unavailable-test` | Web, API | Shows the database-outage page without needing an outage. |
| `/out-of-service` | Web | Maintenance page shown while `IsAppOnline` is `false`. |

- Outside Development, migrations do not run at startup: apply them as a
  deployment step (`dotnet ef database update` or a migration bundle).
- User and role management (`/Identity`) requires the `SuperAdmin` role
  (policy `IdentityAdministration`).

## (s4) Development tooling

1. Enable the Git hooks once per clone (they reject staged files with a
   UTF-8 BOM or CRLF line endings; see `.githooks/README.md`):

   ```powershell
   # [cb10]
   ./scripts/Install-GitHooks.ps1
   ```

1. Package versions are central in `Directory.Packages.props`; shared build
   settings, including NuGet vulnerability audit as errors, are in
   `Directory.Build.props`.
1. Integration tests use LocalDB on Windows and a SQL Server container
   (Testcontainers, needs Docker) elsewhere. Set
   `INTEGRATIONTESTS_CONNECTIONSTRING` to use another server.
1. `.gitlab-ci.example.yml` builds, tests, and publishes the Web and API
   images. Copy it to `.gitlab-ci.yml` to use it.

## (s5) Upgrade notes

- API access tokens now carry an `iat_ms` claim. Tokens issued before this
  change are rejected, so API clients must log in again once.
- The admin controller and views moved from `Admin` to `Identity`. All user
  and role pages are under `/Identity/...`, e.g. `/Identity/UserEdit/{id}`.
