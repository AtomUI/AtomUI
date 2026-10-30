# Restore Debug/Release Target Frameworks Plan

> **For agentic workers:** Use superpowers:executing-plans for this focused correction.

**Goal:** Debug products target net10.0; Release products target net8.0 and net10.0.

**Architecture:** Restore conditional framework selection in ProjectDefaults.props. Keep .NET 10 registration and trimming output unchanged, and make only the compatibility changes required to build and consume the .NET 8 product assemblies. Validate real package contents independently of the evaluated project framework property.

**Tech Stack:** MSBuild, .NET 10 SDK, .NET 8 runtime, Roslyn generators, PowerShell NuGet packaging.

**Spec:** User instruction: Debug supports .NET 10, Release supports .NET 8 and .NET 10, everything else unchanged.

## Global Constraints

- Keep version 6.2.3, Avalonia/dependency versions and control behavior unchanged.
- Do not commit, retag or republish; existing v6.2.3 remains untouched.
- Temporary validation lives in /tmp/atomui-test-dual-framework.feRxBh and is deleted after validation.
- Run only affected module tests and relevant build/pack/publish checks, not full regression.

## Review Focus

- Debug must not unexpectedly acquire a second product target.
- Release product packages must contain both lib/net8.0 and lib/net10.0.
- Generator/language/build-tool target frameworks must retain their existing targets.
- .NET 10 TypeMap and trimmed/AOT behavior must remain unchanged.
- .NET 8 assemblies must actually compile and register controls in a real consumer.

## Tasks

- [x] Reproduce current Release selection and package-content failures with temporary executable checks; probe .NET 8 compilation before edits.
- [x] Restore conditional target selection and fix only proven framework compatibility failures.
- [x] Verify Release package inventory and framework payloads; verify net8/net10 consumers and current net10 registration behavior.
- [x] Synchronize current framework documentation without rewriting historical released-package facts.
- [x] Delete temporary tests/helpers/projects, run final focused generator/build/registration verification and diff checks, and request independent review.

## Ledger

- Starting HEAD: 74a5e2ee4; clean checkout. Root cause: 3cc6cd4cb removed Debug/Release framework selection in the TypeMap migration.
- Ruling: Proceed inline within the explicit correction scope; prepare reviewable changes before any approval request. No commit or publication is authorized for this correction.

- RED: temporary MSBuild contract test failed because Release lacked net8.0; original 13 product packages also failed the independent dual-framework payload check. Explicit net8 build first failed on FeatureSwitchDefinition, then on 38 TypeMap references.
- GREEN: Debug/Release framework evaluation passes; net8 Desktop build passes with zero warnings/errors; both real build/pack flows produced 20 packages and all 13 product packages match the configuration contract. Product framework gate accepts correct packages, rejects Release missing net8 and Debug containing net8.
- Ruling: .NET 8 reuses existing generated static fragments; .NET 10 keeps TypeMap metadata/accessor and publish assets. Keep tools/language TFMs, version and dependencies unchanged. Non-Release configurations retain the previous net10 default.

- Cold local NuGet consumers: net8 and net10 both registered desktop/DataGrid/ColorPicker/Extras/GalleryBase and materialized Button, Calendar and DateViewer templates. net10 osx-arm64 Gallery NativeAOT publish and output validation passed; existing ReactiveUI and local dylib warnings remain.
- Final review: one important shared-property spill into four executable tools, fixed by pinning their existing development framework; temporary MSBuild check failed for all four before correction and passes after.
- Final minor (deferred): framework gate identifies current runtime packages by their existing shared-framework project declaration; future hardcoded project target changes require revisiting identification. Current 13 runtime package contracts are all validated.
- Temporary checks, consumer projects, isolated cache and package/publish artifacts removed after validation; concise evidence logs retained outside repo. Final focused tests still pending.

- Final cleaned-tree tests: dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore → 452/452 passed, 33s. dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Registration → 42/42 passed, 703ms. PowerShell parse and git diff --check pass. No permanent tests added and no commit/tag/publish performed.
