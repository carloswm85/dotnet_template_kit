<!-- [cb1] -->

- [TODO](#todo)
  - [(s1) Completed: host selection](#s1-completed-host-selection)
    - [(s1.1) Acceptance criteria](#s11-acceptance-criteria)
    - [(s1.2) Verification](#s12-verification)
  - [(s2) Clean Architecture test report](#s2-clean-architecture-test-report)
    - [(s2.1) Test results](#s21-test-results)
    - [(s2.2) Findings](#s22-findings)

---

# TODO

> Track the next template enhancement and findings from the latest Clean
> Architecture template installation and user-level smoke test.

---

## (s1) Completed: host selection

✅ Done in commit `b4d4079` (template package version `2.1`).

Add a `dotnet new dtk-clean-architecture` option that lets the user generate
only the API project, only the Web project, or both projects.

The option should describe host selection during project generation, not
template-package installation. Use an explicit choice parameter such as
`--Hosts API`, `--Hosts Web`, or `--Hosts Both`, with `Both` as the default to
preserve the current behavior.

### (s1.1) Acceptance criteria

_[L1]_ The selective-generation feature must satisfy these requirements:

- [x] Define the option in the Clean Architecture `template.json` file.
- [x] Support `API`, `Web`, and `Both` values with clear help text.
- [x] Keep `Both` as the default for backward compatibility.
- [x] Exclude unselected project files from generated output.
- [x] Exclude unselected projects from the generated solution.
- [x] Remove or conditionally configure dependencies on an excluded host.
- [x] Ensure the Web-only selection has a documented external API
  configuration.
- [x] Ensure restore, build, and applicable tests pass for every selection.
- [x] Add smoke tests for all three generation modes.
- [x] Update the root and Clean Architecture template documentation with
  examples.

### (s1.2) Verification

`scripts/Test-CleanArchitectureTemplate.ps1` passed for every mode, and a
manual test of the generated solutions confirmed the results.

**[t3]**

| # | `--Hosts` | Build | Unit | Integration | Status |
| --- | --- | --- | --- | --- | --- |
| `r12` | `API` | 0 errors | 2 of 2 | 20 of 20 | ✅ |
| `r13` | `Web` | 0 errors | 2 of 2 | 38 of 38 | ✅ |
| `r14` | `Both` | 0 errors | 2 of 2 | 47 of 47 | ✅ |

Each Release build reports one warning, CS8602, in
`ApplicationCore/Extensions/ServiceExtensions.cs:19`.

## (s2) Clean Architecture test report

The latest local template package version `2.0`, from commit `ec6dd10`, was
installed successfully. It generated the following solution:

`C:\Users\dds_mineria\source\repos\practice\CleanArchitectureProject\CleanArchitectureProject.slnx`

The template repository remained clean and unchanged. The API and Web runtime
processes used for smoke testing were stopped after validation.

### (s2.1) Test results

**[t1]**

| # | Test | Status | Result |
| --- | --- | --- | --- |
| `r1` | Template installation | ✅ | Replaced the existing installation successfully |
| `r2` | Project generation and restore | ✅ | All six projects restored |
| `r3` | Solution build | ✅ | Zero warnings and zero errors |
| `r4` | Unit tests | ✅ | 2 of 2 passed |
| `r5` | Integration tests | ✅ | 47 of 47 passed |
| `r6` | API smoke test | ✅ | Health, readiness, Swagger UI, and OpenAPI returned HTTP 200 |
| `r7` | Web smoke test | ✅ | Main pages, health endpoints, and assets returned HTTP 200 |

### (s2.2) Findings

**[t2]**

| # | Status | Finding |
| --- | --- | --- |
| `r8` | ✅ | Fixed: the Web weather client now calls the API weather route. |
| `r9` | ✅ | Fixed: `dotnet test` now discovers and runs the tests. |
| `r10` | 🤔 | Integration tests pass but log concurrent database-creation errors. |
| `r11` | 🤔 | Fluent Assertions 8.8.0 emits a commercial-use licensing warning. |

_[L2]_ Follow-up details for the findings:

- The Web client requests `api/WeatherForecasts/data`, while the API exposes
  `api/v{version}/WeatherForecasts/weatherList`. The actual API route returned
  five items, but the Web page displayed none.
  - ✅ Resolution: `WeatherForecastService` calls
    `api/v1/WeatherForecasts/weatherList`. The controller comment and the API
    `.http` file now include the `v1` segment. Verified live: the API returned
    five items, the old route returned 404, and the Web page listed five
    forecasts.
- The xUnit v3 projects are executables. Running them with `dotnet run` executed
  all 49 tests, while `dotnet test` returned success without executing tests.
  This behavior can create false-positive CI results.
  - ✅ Resolution: both test projects reference `Microsoft.NET.Test.Sdk`
    18.12.0 and `xunit.runner.visualstudio` 4.0.1 (central versions). The
    smoke script runs `dotnet test` with
    `RunConfiguration.TreatNoTestsAsError=true`, and every mode reported the
    same totals as `dotnet run` (API 2 + 20, Web 2 + 38, Both 2 + 47).
- Multiple integration-test fixtures use the same LocalDB database name. This
  causes database-already-exists errors in the logs even though all tests pass.
- Review the Fluent Assertions license before using version 8.8.0 in a
  commercial or enterprise project.

_[L3]_ Recommended implementation order:

1. ✅ Correct the weather API route mismatch.
1. ✅ Configure the test runner so zero executed tests cannot report success.
1. ✅ Implement and test the API, Web, and Both generation modes.
1. Isolate integration-test databases or disable conflicting parallel setup.
1. Decide whether to retain or replace Fluent Assertions 8.8.0.
