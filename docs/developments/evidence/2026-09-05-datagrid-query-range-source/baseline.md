# DataGrid Immutable Query + Range Source Baseline

Captured on 2026-09-05 before runtime refactoring.

## Identity and Environment

| Item | Value |
| --- | --- |
| Git revision | `e8f97e38c0a33c48d8e525c4dea34c3bab456d8c` |
| Branch/worktree | `codex/datagrid-query-range-source`; isolated worktree |
| Host | macOS 26.6.2 (25G83), arm64 |
| .NET SDK | 10.0.300 |
| .NET runtime | 10.0.8, osx-arm64 |
| Configuration | Release for performance; net10.0 for tests and performance |

The worktree contained only the approved DataGrid design-document changes and
the new baseline harness/matrix when this evidence was captured. No DataGrid
runtime or test source had been changed. `git diff --check` passed.

## Functional Baseline

Command:

~~~bash
dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore /m:1 /nr:false --nologo -v:minimal
~~~

Result: **147 passed, 0 failed, 0 skipped**; reported test duration 17 seconds.

## Performance Harness Boundary

The repository-wide `tools/performances/AtomUI.Performance` project does not
build at the baseline revision. It has 16 pre-existing stale-API compilation
errors across unrelated Calendar, Avatar, Steps, and DataGrid-filter code. The
Gallery performance project also has 63 pre-existing view-model namespace/API
errors. These failures were reproduced before runtime edits and were not
weakened or repaired as part of this DataGrid refactor.

To produce comparable evidence without changing unrelated suites, the baseline
adds `tools/performances/AtomUI.DataGridPerformance`. It measures only current
public DataGrid behavior in Avalonia Headless: control creation, first layout,
and close. The same executable and arguments must be used for the final
comparison.

Build result: Release/net10.0, 0 warnings, 0 errors.

## Repeated In-Process Control Baseline

Command:

~~~bash
dotnet run --project tools/performances/AtomUI.DataGridPerformance/AtomUI.DataGridPerformance.csproj -c Release --framework net10.0 --no-build -- --count 60 --warmup 5 --markdown /tmp/datagrid-query-range-control-baseline.md
~~~

Operation: create + first layout + close. Values are repeated same-process
control evidence and are not cold-navigation claims.

| Scenario | Mean ms | Median ms | P95 ms | Mean allocated bytes | Median allocated bytes | P95 allocated bytes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| DataGrid.Basic.8x4 | 23.340 | 24.395 | 38.496 | 3,130,234 | 3,160,816 | 3,162,752 |
| DataGrid.Virtualized.1000x8 | 18.554 | 16.832 | 29.804 | 4,513,089 | 4,576,808 | 4,580,456 |
| DataGrid.RowDetails.1000 | 10.189 | 9.512 | 14.383 | 2,866,554 | 2,937,560 | 2,941,688 |
| DataGrid.GalleryShape | 42.431 | 44.321 | 59.857 | 10,696,787 | 10,695,768 | 10,703,104 |

## Independent Cold Process Samples

Each sample starts a fresh benchmark process with `--count 1 --warmup 0` for
`DataGrid.GalleryShape`. Process launch time itself is outside the measurement;
Avalonia and AtomUI initialization inside the process remain cold.

| Sample | Time ms | Allocated bytes |
| ---: | ---: | ---: |
| 1 | 477.043 | 12,126,168 |
| 2 | 444.157 | 12,112,432 |
| 3 | 503.376 | 12,112,432 |
| 4 | 549.790 | 12,113,808 |
| 5 | 540.682 | 12,112,432 |
| 6 | 646.966 | 12,113,808 |
| 7 | 438.071 | 12,112,432 |
| 8 | 455.436 | 12,112,432 |
| 9 | 441.546 | 12,112,432 |
| 10 | 436.722 | 12,112,432 |

| Statistic | Time ms | Allocated bytes |
| --- | ---: | ---: |
| Mean | 493.379 | 12,114,081 |
| Median | 466.240 | 12,112,432 |
| P95 (nearest-rank) | 646.966 | 12,126,168 |

Cold timing variance is substantial; the final report must compare all samples
and must not infer an improvement from a single run.

## Acceptance Reference

The exact functional, lifecycle, visual, virtualization, and performance
contract is frozen in
`tools/performances/AtomUI.Performance/Suites/DataGrid/Regression.md`. Items may
only be checked after the corresponding automated or recorded evidence exists.
