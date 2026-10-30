# Desktop Test Startup Performance Design

## Purpose and evidence

AtomUI Desktop's full test project uses Avalonia Headless xUnit with `PerTest` application isolation and disabled in-assembly parallelization. The test-value cleanup reduced discovered cases from 7,318 to 6,900, but a count reduction alone cannot eliminate the repeated application setup cost.

On 2026-09-30, a warm 29-case `WindowingPlatformDetectionTests` run had a median duration of 6 seconds across three runs. Temporary instrumentation around `TestApplication.Initialize` recorded 30 initialization events in one 29-case run (one extra startup event), with a median of about 231 ms each. The builder callback consumed about 130 ms, and later scope/theme initialization about 101 ms. Inside the builder callback, `UseDesktopControls()` accounted for about 131 ms median, ColorPicker about 6 ms, and Extras, image-loading and language registration were negligible. Removing Extras or the explicit language registration in isolated A/B runs did not improve the 29-case median. All instrumentation was removed; the original source and deterministic build outputs were restored.

Historical full-run logs show the unchanged 4,589-case Desktop project taking 19:45, 20:16, and 43:53 on this host. That variation rules out a reliable full-run speedup claim from a single local benchmark. Test enumeration/build added roughly 2 minutes to those runs; Desktop execution remained the dominant cost.

## Constraints

- Preserve `AvaloniaTestIsolationLevel.PerTest` and serial execution of the UI assembly. Do not hide bugs by sharing Application, resource hosts, theme managers, event subscriptions, or other mutable state between tests.
- Preserve ordinary and trimmed/AOT registration behavior, TypeMap retention, resource ownership, diagnostics, duplicate detection and failure ordering.
- A static cache may contain immutable generated metadata only. It must not retain an Application, provider, ThemeManager, ResourceDictionary instance, registration builder or ApplicationScope.
- Generated metadata used by a trimmed build must stay behind the selected per-fragment/asset reference boundary. Do not introduce all-control static roots or runtime assembly scans.
- Do not run a local full regression without the user's explicit request or a release.
- Other uncommitted release work in the primary checkout, including `scripts/run-full-regression.sh`, remains outside this implementation.

## Considered approaches

1. **Cache immutable generated descriptors per registration fragment (recommended).** Audit `ControlTokenDescriptor`, `ControlSemanticDescriptor`, `ControlThemeAssetDescriptor`, and `ControlThemeResourceRegistration` for mutability and Application ownership. Where safe, emit a lazy/static descriptor field inside its own generated fragment or asset holder and reuse it across fresh Application builders. Keep fresh provider/resource dictionary instances and the per-Application registration builder. A fragment or asset that is not selected in a trimmed build must not initialize or root another fragment. The first registration still performs constructor validation; every new builder still performs package-level validation and freezes its own schema. This targets the measured 131 ms registration stage without changing test isolation. It changes source-generator output and needs AOT/publish evidence.
2. **Separate UI-free tests into a plain xUnit project.** A conservative source scan found up to 387 cases in 65 candidate files, though some still need Avalonia resources. Even if all migrated safely, eliminating roughly 0.2 seconds of setup per case yields only an upper bound around 1–1.5 minutes, with new test-project and `InternalsVisibleTo` maintenance. This can be a later cleanup, not the main answer to the one-hour run.
3. **Parallel UI shards.** Independent testhost processes could reduce wall time while retaining per-process `PerTest` isolation, but they would relax the current serial UI test constraint and require shared-output/fixture collision analysis. Do not implement this path under the current project instructions without explicit user authorization to change that constraint.

## Proposed implementation boundary

The first implementation step is a feasibility audit of the four descriptor types and generated fragment output. Record whether each descriptor can be reused as a value without an Application-specific reference or mutated state. If any descriptor fails that audit, exclude it rather than add a general cache.

Modify the ordinary generator's registration writer to emit selected-fragment-local caches only for safe metadata. The emitted `Add(builder)` still runs for each Application and populates a fresh `ControlPackageRegistrationBuilder`. Resource factories are invoked per Application as before. No new public API is planned; if a public change proves necessary, stop and present it for explicit approval under the repository's API rule.

Do not cache a built `ControlPackageRegistration`: it includes an `IControlThemesProvider` and has app-specific commit state. Do not cache `ThemeManager` or `CompiledThemeCatalog` in this step. The remaining roughly 101 ms of initialization needs separate evidence before a second design.

## Validation and stop conditions

- Add focused generator/runtime tests that prove descriptor identity reuse across two builders while each builder gets a distinct provider, registration state, theme manager and resource instances. Include simultaneous builder creation and failure/duplicate evidence.
- Run affected generator, Core, Desktop and GalleryBase tests with explicit `--scope iterate --path` paths. Verify semantic part, token/resource registration and lifecycle suites selected by the change.
- Perform the relevant trimmed/NativeAOT registration and publish checks required by the verification plan. A normal test pass is not AOT evidence.
- Compare identical warm 29-case test runs before and after on the same machine, alternating order at least three times, using medians for `UseDesktopControls()` and total `Initialize`, plus case count and failures. Keep the temporary instrumentation out of the delivered source.
- Keep the optimization only if it reduces median registration time materially (initial target: at least 30%) without increasing total test duration, memory retention, or AOT size. Otherwise revert the optimization and use the measurements to choose the next target.
- Do not claim a change to full-regression runtime without an authorized full run. Report measured focused improvement separately from projected all-case savings.

## Implementation evidence and tradeoff

The prototype emits a cache per generated fragment or asset. It publishes a successful immutable descriptor with `Volatile.Read` and `Interlocked.CompareExchange`; a racing first caller may construct and discard a duplicate descriptor, but both builders receive the same published object. Factory exceptions keep their original type. The package registration builder, provider, ThemeManager and resource instances remain per Application. A generated stale-fingerprint test first failed because a static initializer wrapped `ThemeSchemaException`, then passed after the cache changed.

On the same host, three alternating baseline/optimized runs of the unchanged 29-case `WindowingPlatformDetectionTests` class passed in both builds. The test execution durations were 6/7/7 seconds at the base commit and 3/3/3 seconds with the cache. End-to-end warm command times were 11.07/11.71/12.27 seconds and 7.71/7.91/9.64 seconds respectively (median 11.71 to 7.91 seconds). This is a class-level result, not a measured full-suite result.

Focused registration verification passed 230 cases in five projects. The verification CLI remains `pending` because it reports a separate NativeAOT obligation; a real `osx-arm64` Gallery publish passed, and its exact published executable reached and rendered the main window in a temporary `.app` bundle. A clean base-commit NativeAOT executable was 82,463,976 bytes; the optimized executable was 82,630,168 bytes, an increase of 166,192 bytes (0.20%). Therefore the strict no-growth size condition above is not met. The optimization is left uncommitted for review with this explicit tradeoff. No full regression was run.
