# AtomUI Theme System Progress

This file tracks implementation progress for
[the theme system implementation plan](../plans/2026-07-16-theme-system-implementation.md).
The normative design remains in `docs/architecture/systems/theming/runtime.md` and
`docs/reference/theming/theme-definition-xml-v1.md`.

## Current State

| Item | Status | Evidence |
|---|---|---|
| Formal architecture | Complete | `docs/architecture/systems/theming/runtime.md` |
| XML v1 specification and XSD | Complete | XSD validated by `xmllint` and .NET `XmlSchemaSet` |
| Baseline Core tests | Complete | 141 passed, 0 failed on 2026-07-16 |
| Task 1: XML syntax and algorithm state | Complete | 15 focused tests; secure XSD reader implemented |
| Task 2: Schema registry and Binder | Complete | Deterministic registry and typed immutable Binder; 11 registry and 9 Binder tests |
| Task 3: Generated descriptors | Complete | 20 generator tests and 27 focused Core schema/Binder/parser tests; direct delegates cover all built-in value types |
| Task 3A: Source ownership and generated identity | Complete | Old directories removed; Theme root types use `AtomUI.Theme`; owner-specific generated pools compile across Gallery and Desktop; `BaseControlTheme` exposes only strongly typed TemplateParent bindings |
| Task 4: Initial config normalization and merge | Complete | 10 focused behavior tests; typed capture, canonical formatter, stable fingerprint and deterministic change set implemented |
| Task 4B: Final immutable config and algorithm contracts | Complete | Deeply immutable Config/builders, revisioned independent algorithms, deterministic schema revision and explicit appearance folding; Core, generator and Gallery gates pass |
| Task 5: Dense snapshot compiler | Complete | Dense immutable global/Control slot tables, exact global and Control derivation order, final Motion/Wave normalization, immutable resources/palettes and single-Control structural sharing |
| Task 6: Bounded caches | Complete | Structured keys, dual-limit LRU, single-flight and Control compilation reuse; focused, Core and Gallery gates pass |
| Task 7: ThemeManager transactions | Complete | Five-stage generation queue, atomic CommitCore, non-rollback Publish and isolated result events; Core and Gallery gates pass |
| Task 8: Scope graph | Complete | Stable Context/Provider per scope, graph stamps, detach/reparent invalidation and WeakReference coverage |
| Task 9: Resources and resolver | Complete | Generated asset identity, unified SharedTokenResource, 0 B hot lookup, resolver lifecycle and TopLevel bridge |
| Task 10: Manager/startup cutover | Complete | Single ThemeManager owner, first-frame snapshot, explicit Light/Dark and FollowSystem coverage |
| Task 11: Legacy removal | Complete | User-approved physical deletion; AXAML and imperative consumers migrated without compatibility shells |
| Task 12: Acceptance | Complete | Cross-module tests, Release multi-target build, NativeAOT publish and static scans completed on 2026-07-19 |

## Verification Log

| Date | Command | Result |
|---|---|---|
| 2026-07-16 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` | 141 passed, 0 failed |
| 2026-07-16 | `xmllint --schema docs/modules/core/schemas/atomui-theme-v1.xsd docs/modules/core/examples/daybreak-blue.theme.xml` | Standard example valid |
| 2026-07-16 | .NET `XmlSchemaSet` validation harness | Standard example valid; 8 invalid structural cases rejected |
| 2026-07-16 | `dotnet test ... --filter FullyQualifiedName~ThemeDocumentReaderTests` | 15 passed, 0 failed |
| 2026-07-16 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` | 156 passed, 0 failed after Task 1 |
| 2026-07-16 | `dotnet test ... --filter FullyQualifiedName~ThemeSchemaRegistryTests` | 11 passed, 0 failed |
| 2026-07-16 | `dotnet test ... --filter FullyQualifiedName~ThemeDefinitionBinderTests` | 9 passed, 0 failed |
| 2026-07-16 | Full Core test with detailed hang diagnostics | One non-deterministic Avalonia Dispatcher host stall isolated to `ThemeConfigProviderTests`; focused and predecessor-combination reproductions passed |
| 2026-07-16 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` (run 1) | 176 passed, 0 failed |
| 2026-07-16 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` (run 2) | 176 passed, 0 failed |
| 2026-07-16 | `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore` | 20 passed, 0 failed after owner-specific generated schema output |
| 2026-07-16 | Focused Core registry, Binder, generated descriptor and parser suites | 27 passed, 0 failed |
| 2026-07-16 | `dotnet test ... --filter FullyQualifiedName~ButtonThemeScopeTests` | 4 passed, 0 failed; generated FQN collision removed |
| 2026-07-16 | `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --framework net10.0 --no-restore` | Build succeeded, 0 warnings, 0 errors after directory reorganization |
| 2026-07-16 | `git diff --check` | Passed after Task 3/3A |
| 2026-07-16 | Focused Task 4 Normalizer and Merger suites | 10 passed, 0 failed |
| 2026-07-16 | Task 4 plus schema, Binder and generated descriptor suites | 35 passed, 0 failed |
| 2026-07-16 | Parser, formatter and Task 4 suites after performance review | 13 passed, 0 failed |
| 2026-07-16 | Full Core with `--blame-hang --blame-hang-timeout 30s` | 146 tests completed with 0 failures before the known `ThemeConfigProviderTests.Content_Replacement_Clears_Old_Scope_And_Publishes_New_Scope` host stall; hang dumps captured |
| 2026-07-16 | Isolated previously stalled ThemeConfigProvider test | 1 passed, 0 failed in 129 ms |
| 2026-07-16 | Final Generator verification after writer rename | 20 passed, 0 failed |
| 2026-07-16 | Final Gallery build after Task 4 and generated formatter rollout | Build succeeded, 0 warnings, 0 errors |
| 2026-07-16 | Final Desktop Button scope regression | 4 passed, 0 failed |
| 2026-07-16 | `dotnet test ... --filter FullyQualifiedName~BaseControlThemeTests` | 1 passed, 0 failed; string-path TemplateParent overloads are absent |
| 2026-07-16 | `dotnet test ... --filter FullyQualifiedName~ThemeCoordinatorTests` | 8 passed, 0 failed after obsolete Theme API removal |
| 2026-07-16 | Gallery build after obsolete Theme API removal | Build succeeded, 0 warnings, 0 errors |
| 2026-07-16 | Theme root namespace, coordinator, provider and snapshot cache suites | 32 passed, 0 failed |
| 2026-07-16 | Desktop Button scope regression after root namespace consolidation | 4 passed, 0 failed |
| 2026-07-16 | Gallery build after root namespace consolidation | Build succeeded, 0 warnings, 0 errors |
| 2026-07-18 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Theme'` | 196 passed, 0 failed |
| 2026-07-18 | Focused generator Theme/TokenResource tests | 6 passed, 0 failed |
| 2026-07-18 | Final architecture-to-plan audit | Tasks 4B-12 rewritten for immutable Config, exact algorithms, five-stage transactions, graph stamps, asset manifest and TopLevel bridge |
| 2026-07-18 | Task 4B immutable Config and algorithm contract RED/GREEN cycle | Builder absence and algorithm-revision fingerprint assertions failed before implementation; focused suites passed after replacement |
| 2026-07-18 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` | 202 passed, 0 failed after Task 4B |
| 2026-07-18 | Focused generator Theme/TokenResource tests after Task 4B | 6 passed, 0 failed |
| 2026-07-18 | `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --framework net10.0 --no-restore` | Build succeeded, 0 warnings, 0 errors after Task 4B |
| 2026-07-18 | `git diff --check` | Passed after Task 4B |
| 2026-07-18 | Task 5 dense snapshot/compiler RED cycle | Missing `ThemeCompileInput`, dense tables and Control slot APIs failed first; mutable snapshot exposure and mutable brush projection then failed as targeted |
| 2026-07-18 | Focused dense compiler and snapshot suites | 11 passed, 0 failed; covers algorithm chaining, Seed/Map/Alias order, Motion/Wave, immutable values, Control structural sharing and 0 B dense slot reads after warmup |
| 2026-07-18 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` | 212 passed, 0 failed after Task 5 |
| 2026-07-18 | `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --framework net10.0 --no-restore` | Build succeeded, 0 warnings, 0 errors after Task 5 |
| 2026-07-18 | `rg -n 'DesignTokenClone' src/AtomUI.Core/Theme --type cs` | Only `DesignTokenClone.cs` remains; no runtime caller reads clone compatibility objects |
| 2026-07-18 | `git diff --check` | Passed after Task 5 |
| 2026-07-18 | Task 6 bounded-cache RED cycle | Missing `BoundedLruCache`, typed snapshot cache API and Control compilation cache failed before implementation |
| 2026-07-18 | Task 6 cache/compiler focused suites | 40 passed, 0 failed; covers structured identity, LRU order, entry/byte limits, collision rejection, cancellation, single-flight and Control reuse |
| 2026-07-18 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --blame-hang --blame-hang-timeout 30s` | 225 passed, 0 failed after Task 6 |
| 2026-07-18 | `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --framework net10.0 --no-restore` | Build succeeded, 0 warnings, 0 errors after Task 6 |
| 2026-07-18 | `git diff --check` | Passed after Task 6 |
| 2026-07-18 | Task 7 transaction RED cycle | Missing transaction/result models and Manager API failed first; latest-active/pending and stale-failure ordering regressions also failed before scheduler fixes |
| 2026-07-18 | `dotnet test ... --filter 'FullyQualifiedName~ThemeManagerTransactionTests|FullyQualifiedName~ThemeManagerTests'` | 17 passed, 0 failed; covers atomic commit, NoOp, single-flight, caller cancellation, supersede/coalescing, reentrancy, publish ordering and observer isolation |
| 2026-07-18 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Theme --blame-hang --blame-hang-timeout 30s` | 237 passed, 0 failed after Task 7; filter matches the complete Core test namespace |
| 2026-07-18 | `dotnet build controlgallery/AtomUIGallery.Desktop/AtomUIGallery.Desktop.csproj --framework net10.0 --no-restore` | Build succeeded, 0 warnings, 0 errors after Task 7 |
| 2026-07-18 | `git diff --check` | Passed after Task 7 |
| 2026-07-19 | Task 8-12 focused acceptance suites | Covers 10 nested scopes, 100 update coalescing, stale topology rejection, cache limits/reuse, FollowSystem, zero-allocation lookup, lease/bridge and GalleryCodeViewer lifecycle |
| 2026-07-19 | `dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore` | 167 passed, 0 failed after final legacy Token API removal |
| 2026-07-19 | `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore` | 1450 passed, 0 failed including Popup/Flyout local Context inheritance |
| 2026-07-19 | `dotnet test tests/AtomUI.Desktop.Controls.DataGrid.Tests/AtomUI.Desktop.Controls.DataGrid.Tests.csproj --framework net10.0 --no-restore` | 88 passed, 0 failed |
| 2026-07-19 | `dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore` | 42 passed, 0 failed |
| 2026-07-19 | `dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore` | 452 passed, 0 failed after Masonry AOT binding migration |
| 2026-07-19 | `dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore` | 25 passed, 0 failed |
| 2026-07-19 | `dotnet test tests/AtomUI.Controls.Shared.Tests/AtomUI.Controls.Shared.Tests.csproj --framework net10.0 --no-restore` | 27 passed, 0 failed; 7 pre-existing xUnit cancellation analyzer warnings |
| 2026-07-19 | `dotnet build AtomUI.slnx --configuration Release -p:GalleryPublishAot=false --no-restore` | net10.0, net8.0, Desktop and Browser succeeded with 0 warnings, 0 errors |
| 2026-07-19 | Direct Gallery NativeAOT publish | Succeeded with 0 errors; only 4 ReactiveUI third-party IL2104/IL3053 summary warnings |
| 2026-07-19 | `PublishToLocal.ps1 ... -publishAot true` outside the foreground command time limit | NativeAOT output validation, installer asset copy and version substitution passed |
| 2026-07-19 | Deleted API, reflection/Activator and old shared-extension searches plus `git diff --check` | Production searches empty; only explicit absence assertions remain; diff check passed |

## Decisions Locked

- Central transaction owner and only snapshot publisher: internal `ThemeManager : Styles`, also exposed through `IThemeManager`; per-request phase state remains in `ThemeTransaction`.
- Backend algorithm state: `ControlAlgorithmMode` enum.
- XML explicit values: `Disabled` and `Global`; custom mode uses `<Algorithms>`.
- Runtime truth source: immutable `ThemeSnapshot` only.
- Shared Token AXAML contract: always `{atom:SharedTokenResource TokenName}`; Control-specific shared overrides use ambient
  Control token scope and internal composite keys, never per-Control shared-token MarkupExtension names.
- Compatibility with the old theme API: intentionally not implemented.
- Unsupported old Theme APIs are deleted; the new system does not retain `[Obsolete]` throwing shells.
- The Task 11 deletion inventory was explicitly approved by the user; dead Token query/clone/resource projection methods,
  `ControlTokenConfigInfo`, `TokenConfigBuckets` and `GetTokenKindType` overrides were removed with no compatibility layer.
- Execution: inline only, no subagents.
- Theme source ownership: manager/transaction/context/scope runtime entries live in the `Theme` root; there is no separate `ThemeEngine`; `Definitions` absorbs Catalog; `Algorithms` absorbs palette calculation; `Resources` absorbs Control theme loading; `Styling`, `Engine`, `ControlThemes` and `TokenSystem` directories are removed, with compile-only Token code living in `Tokens`.
- Theme root source files use `AtomUI.Theme`; only responsibility subdirectories introduce child namespaces.
- Generated schema ownership: `AtomUI.Generated.<OwnerAssemblyIdentifier>.GeneratedThemeSchema`; no shared generated full type name and no namespace nesting beneath a possibly same-named Control type.
- Token descriptor canonicalization: generated descriptors own direct parser and formatter delegates; normalized fingerprints never use raw input strings or process-random string hashes.
