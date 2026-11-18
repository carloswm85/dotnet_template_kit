<!-- [cb1] -->

- [TODO](#todo)
  - [(s1) Next task](#s1-next-task)
    - [(s1.1) Acceptance criteria](#s11-acceptance-criteria)
  - [(s2) Clean Architecture test report](#s2-clean-architecture-test-report)
    - [(s2.1) Test results](#s21-test-results)
    - [(s2.2) Findings](#s22-findings)

---

# TODO

> Track the next template enhancement and findings from the latest Clean
> Architecture template installation and user-level smoke test.

---

## (s1) Next task

Add a `dotnet new dtk-clean-architecture` option that lets the user generate
only the API project, only the Web project, or both projects.

The option should describe host selection during project generation, not
template-package installation. Use an explicit choice parameter such as
`--Hosts API`, `--Hosts Web`, or `--Hosts Both`, with `Both` as the default to
preserve the current behavior.

### (s1.1) Acceptance criteria

_[L1]_ The selective-generation feature must satisfy these requirements:

- Define the option in the Clean Architecture `template.json` file.
- Support `API`, `Web`, and `Both` values with clear help text.
- Keep `Both` as the default for backward compatibility.
- Exclude unselected project files from generated output.
- Exclude unselected projects from the generated solution.
- Remove or conditionally configure dependencies on an excluded host.
- Ensure the Web-only selection has a documented external API configuration.
- Ensure restore, build, and applicable tests pass for every selection.
- Add smoke tests for all three generation modes.
- Update the root and Clean Architecture template documentation with examples.

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
| `r8` | ❌ | The Web weather client and API weather route do not match. |
| `r9` | ❌ | `dotnet test` exits successfully without executing tests. |
| `r10` | 🤔 | Integration tests pass but log concurrent database-creation errors. |
| `r11` | 🤔 | Fluent Assertions 8.8.0 emits a commercial-use licensing warning. |

_[L2]_ Follow-up details for the findings:

- The Web client requests `api/WeatherForecasts/data`, while the API exposes
  `api/v{version}/WeatherForecasts/weatherList`. The actual API route returned
  five items, but the Web page displayed none.
- The xUnit v3 projects are executables. Running them with `dotnet run` executed
  all 49 tests, while `dotnet test` returned success without executing tests.
  This behavior can create false-positive CI results.
- Multiple integration-test fixtures use the same LocalDB database name. This
  causes database-already-exists errors in the logs even though all tests pass.
- Review the Fluent Assertions license before using version 8.8.0 in a
  commercial or enterprise project.

_[L3]_ Recommended implementation order:

1. Correct the weather API route mismatch.
1. Configure the test runner so zero executed tests cannot report success.
1. Implement and test the API, Web, and Both generation modes.
1. Isolate integration-test databases or disable conflicting parallel setup.
1. Decide whether to retain or replace Fluent Assertions 8.8.0.
