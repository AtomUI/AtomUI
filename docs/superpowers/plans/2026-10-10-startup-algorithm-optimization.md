# AtomUI Startup Algorithm Optimization Implementation Plan

> Execute the plan task by task. No commits are authorized.

**Goal:** Reduce real empty-window startup cost by removing evidenced redundant work while preserving every registration, theme, localization, resource and lifecycle contract.

**Architecture:** Optimize existing algorithms and generated immutable facts first. Retain synchronous complete initial theme/language state. Resource-loading and native-window candidates require evidence before changing their behavior.

**Tech Stack:** .NET 8/10, Avalonia 12.1.3, C#, Roslyn generators, AXAML, Windows CoreCLR and applicable NativeAOT checks.

**Spec:** The user-approved source audit in this conversation (2026-10-10): all ten finding groups, including secondary and conditional paths, must receive an implemented, measured or evidence-backed rejected disposition. Existing contracts in docs/architecture/foundations/control-registration-contracts.md and docs/architecture/systems/localization/runtime.md bind this work.

## Global Constraints

- Preserve public APIs, serialized/fingerprint formats, full CLR identities, stale-schema rejection, cross-package conflicts, resource precedence and AOT trimming behavior.
- Preserve mutable Token isolation, custom accessors/evaluators, complete startup localization coverage, fallback ordering, sparse overrides and diagnostic identity.
- No post-first-frame patch-up, arbitrary delay, removed UI feature, reflection discovery, warning suppression, dependency upgrade or manual generated-file edit.
- No commit, amend, rebase, full regression or affected-wide run. Preserve the existing Desktop csproj diff.
- Search existing tests first. New reproduction/cost/differential tests belong to owning test project's Temporary/StartupAlgorithms or system temp; prove RED/GREEN, remove temporary files, run final exact module tests.
- Use one implementation agent at a time; independent read-only work may overlap. Builds and timing runs must not overlap. Every task receives an independent scope review; final review covers the full change.
- Cold samples mean fresh processes, not an empty OS cache. Baseline/candidate use identical probe, platform options, workloads and sampling order. Report Opened/render callback separately; neither proves display presentation.
- The 2026-10-10 macOS result document records rejected localization candidate indexing for small early-hit catalogs. Include that adverse case before accepting a new candidate algorithm.

## Review Focus

- Old generated fingerprint evidence must still fail against changed runtime globals; optimize encoding without altering bytes.
- Custom Token accessors and mutable evaluator inputs must remain isolated and honor overrides.
- One-unit localization catalog with immediate application override and many unrelated bundles must not regress due to eager candidate indexing.
- Image first enqueue racing Dispose/cancellation must terminate without starting leaked workers or losing completion.
- Resource owner/detach, lexical StaticResource and deferred loading must survive wrapper changes; hidden visual trees must not be replaced with manual C# lifecycle machinery.

### Task 1: Baseline and acceptance instrumentation

**Files:** system temp startup probe; .superpowers/sdd/2026-10-10-startup-algorithm-optimization/progress.md.
**Interfaces:** independent executable scenarios plain Avalonia, AtomUI initialization + Avalonia Window, AtomUI Window; immutable baseline output used by Task 10.
- [ ] Build a temporary source-referencing probe before product edits; retain isolated baseline binaries.
- [ ] Record process-to-Initialize/Opened/first Render callback, Initialize duration, allocated bytes and scenario/config identity for >=10 fresh processes per scenario.
- [ ] Record scoped baseline tests and any existing failures; never label an environmental failure as a valid RED.

### Task 2: Registration and schema algorithms

**Files:** Core Registration/ControlPackageRegistrationBuilder.cs; Theme/Schema/{ThemeSchemaRegistry,ControlThemeAssetDescriptor,ControlThemeAssetManifest,ControlTokenIdentityCanonicalizer}.cs; Theme/ControlPackageRegistration.cs; Theme/SemanticParts/SemanticPartDescriptor.cs.
**Interfaces:** preserve all existing signatures and fingerprint bytes. No generated API change.
- [ ] Establish independent old-encoder equivalence/cost evidence, empty-bindings allocation, builder failure poisoning and canonical owner replacement behavior.
- [ ] Fold zero suffix FNV operations; cache type/assembly identity strings within a fingerprint calculation/registry lifetime.
- [ ] Remove empty-bindings second canonicalization, avoid capture/nested Mutate allocation while preserving poison-on-failure, consolidate identical freeze work without removing validation boundaries.
- [ ] Replace SemanticPart Split validation with allocation-free scans where equivalent; fix dictionary key-owner replacement linear scan using an internal representation that preserves actual canonical keys.
- [ ] Run exact schema/registration/semantic/configuration tests, measure costs, remove temporary tests, write report and review diff.

### Task 3: Localization initialization

**Files:** Generator/Localization/{LanguageTagsSourceWriter,LanguageModuleSourceWriter}.cs; Localization/{LanguageTag,LanguageSnapshotBuilder}.cs; Localization/Catalog and Services only where proven useful.
**Interfaces:** same generated public LanguageTags properties and strict public Parse. Internal trusted construction only.
- [ ] Prove all generated tag values equal public parser, first-tag cold cost, snapshot differential fallback/source/coverage behavior and early-hit adverse case.
- [ ] Generate canonical value properties without all-tag static initialization; reuse generated tag constants in bundle registrations where available.
- [ ] Optimize repeated candidate matching only if both representative Gallery and small early-hit costs improve; preserve early return and sparse overrides.
- [ ] Evaluate registry duplicate scans/copies; retain defensive ownership, remove only proven redundant work.
- [ ] Execute exact runtime/generator localization tests, clean temporary tests, report measurements and review.

### Task 4: Image service lifecycle

**Files:** Controls.Shared/ImageLoading/ImageRequestScheduler.cs and ImageFileCache.cs only if conditional cleanup evidence justifies change.
**Interfaces:** unchanged scheduling API, priority/aging/cancellation and disposal behavior.
- [ ] RED: no requests should start zero workers; verify first-use concurrent enqueue, cancellation, disposal before/after activation.
- [ ] Activate each pool on demand under synchronization; keep cancellation/completion and teardown race safe.
- [ ] Measure persistent-cache startup cleanup separately; if changed, preserve layout/version and concurrent read safety. Otherwise record why deferral would alter required integrity behavior.
- [ ] Exact image tests; remove temporary tests; review.

### Task 5: Theme compilation copies and projections

**Files:** Core Theme/Compilation/{ThemeCompiler,ThemeCacheKey,ControlCompilationCache,BoundedLruCache}.cs; Theme/DesignTokens/Definitions/DesignToken.cs; Theme/Algorithms/{ColorMap,DefaultThemeVariantCalculator}.cs; Generator schema writer as required.
**Interfaces:** internal generated global copy helper; public DesignToken constructor and arbitrary custom descriptor fallback unchanged.
- [ ] RED allocation evidence and differential snapshots for default/dark/compact/config overrides/custom descriptors/mutable own evaluators.
- [ ] Generate typed global copying with safe recognition of canonical generated descriptor table; avoid default seed reinitialization and palette temporary arrays; preserve isolated mutable state.
- [ ] Remove duplicate projection clones/content fingerprint computation where ownership permits; evaluate synchronous cache overhead without weakening coalescing.
- [ ] Reuse raw palette calculations with keys including all algorithm inputs; retain subclass customization and color-role mapping.
- [ ] Exact compiler/cache/algorithm/generator tests, cleanup and review.

### Task 6: Static theme definition startup

**Files:** Core Theme/Definitions/{CoreThemeDefinitionResolver,CompiledThemeCatalog,ThemeDocumentReader}.cs; generator/build integration if warranted; Core project resource metadata.
**Interfaces:** internal built-in compiled source; external source contract and XML diagnostics unchanged.
- [ ] RED first-use XSD/XML cost plus differential built-in documents/digests/bound values.
- [ ] Move fixed built-in documents to validated build-time representation, retain actual registry binding, source identity/revision and duplicate/invalid source handling. Avoid hardcoded drifting theme values.
- [ ] Reduce bounded-read buffer overhead for small external documents without changing limits/stream semantics.
- [ ] Reuse normalized initial request when identity/config semantics match; exact theme/catalog/config tests, clean and review.

### Task 7: Resource factory and mounting costs

**Files:** Toolchain/Tasks/GenerateThemeAssetWrappersTask.cs; Generator/Registration writers; Core/ThemeManager.cs only as required.
**Interfaces:** factory, asset identity, export keys, deferred resources, lexical scope and precedence unchanged.
- [ ] Measure actual wrapper/include/dictionary/theme construction on current Avalonia; prove whether a redundant wrapper can be eliminated using compiled direct resource creation.
- [ ] Build a narrow prototype with StaticResource/deferred/owner/order failures as RED evidence. Accept only demonstrated simplification and measurable gain; otherwise retain existing correct architecture with explicit evidence-backed disposition.
- [ ] Validate generator/toolchain/registration tests plus actual trimmed/native consumer; clean and review.

### Task 8: Window and shared icon initialization

**Files:** Desktop.Controls/Window/Window.cs; WindowTitleBar/WindowTitleBar.cs; Window/MediaBreakPointThemeBootstrapper.cs; Controls/Icon/IconPresenter.cs; clip/native code only upon repeat-count evidence.
**Interfaces:** unchanged CSD, titlebar, icon, media breakpoint and reattach contracts.
- [ ] Establish unnecessary reflection and duplicate-bind counts with visible/hidden titlebar, icon replace, detach/reattach cases.
- [ ] Remove work behind unreachable branches and avoid recreating still-live bindings; preserve initialization values and release paths.
- [ ] Measure template/layout/frame geometry/clip candidates; no speculative template-to-code conversion or loss of first-frame border behavior.
- [ ] Exact Window/Icon tests, representative live probe, cleanup and review.

### Task 9: Gallery configuration duplication

**Files:** controlgallery/AtomUIGallery/AtomUIGalleryModule.cs and desktop/browser setup only if needed; GalleryBase configuration unchanged externally.
**Interfaces:** same route factories/navigation/configuration and app-specific lifetime.
- [ ] RED repeated construction count and routes/config immutability equivalence.
- [ ] Share one application configuration between ViewLocator and UseGalleryBase without globally retaining app-owned controls or changing localization timing.
- [ ] Exact Gallery configuration/routing tests, cleanup and review.

### Task 10: Integrated verification, performance and disposition

**Files:** relevant permanent architecture/module docs if behavior implementation changes; concise results document.
- [ ] Resolve every audit finding as optimized, already protected, or rejected by concrete correctness/performance evidence; do not silently drop minor/conditional findings.
- [ ] Build Release net8/net10 affected production projects and actual NativeAOT/trim consumer matching risk; run final cleaned module tests only.
- [ ] Alternate baseline/candidate >=10 fresh-process samples per scenario with no concurrent build; compare startup, allocation, first render callback and schema/theme output equivalence.
- [ ] Independent whole-diff review; fix real findings, reverify exact affected modules; git diff --check.
- [ ] Stop owned processes and delete temporary tests/probes/artifacts; leave product changes uncommitted and report measured results and limitations.
