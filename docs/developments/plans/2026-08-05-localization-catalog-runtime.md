# AtomUI Localization Catalog Runtime Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the reflection-free Catalog, Translation Bundle, fallback, immutable Snapshot, Localizer, LanguageManager, and stable Avalonia resource-provider runtime defined by the approved localization architecture.

**Architecture:** Generated code registers strongly typed Catalog descriptors and compiled translation arrays through `ILocalizationBuilder`. Build freezes deterministic catalog slots, validates bundle sources and coverage, and precomputes one immutable Snapshot per supported language. Manager, Localizer, and ResourceProvider share one atomically published runtime revision so a query never mixes language state.

**Tech Stack:** .NET 10/.NET 8, Avalonia `ResourceProvider`, immutable arrays/read-only collections, `CompositeFormat`, generated enum switch delegates, xUnit v3, Shouldly.

## Global Constraints

- Follow every invariant in `docs/modules/localization/overview.md`, `public-api.md`, `catalog-and-xliff.md`, `runtime-architecture.md`, and `diagnostics-and-testing.md`.
- Do not parse XLIFF, inspect packages/files, scan assemblies, enumerate enum fields/names, or construct providers through reflection at runtime.
- Catalog identity is `{LanguageModuleId}:{FullyQualifiedMetadataName}`; every Catalog has complete `en-US` source values.
- Source priority is `ApplicationOverride > StaticLanguagePack > ModuleBuiltIn`; within a source priority, fallback candidates are exact-to-parent.
- Same-priority duplicate values for the same Catalog/language/unit are errors, never registration-order overrides.
- A non-English supported language must resolve every unit before final `en-US`; final English fallback alone is not coverage.
- Language switching is synchronous, UI-thread-only, no-op aware, revisioned, and independent from ThemeManager.
- Provider identity is stable. Switching replaces an atomic revision and raises one hosted-resource notification; it never swaps merged dictionaries.
- `ILocalizer.Get/Format` reads one revision exactly once and uses generated enum-to-slot mapping, never `Enum.ToString()`.
- Every behavior follows red-green-refactor and all test commands use `-p:IsTestProject=true`.

---

## Runtime File Structure

```text
src/AtomUI.Localization/
├── Catalog/
│   ├── LanguageCatalogAttribute.cs
│   ├── LanguageCatalogUnitDescriptor.cs
│   ├── LanguageCatalogDescriptor.cs
│   ├── TranslationBundleDescriptor.cs
│   ├── TranslationSourceKind.cs
│   ├── ILanguageCatalogRegistry.cs
│   └── LanguageCatalogRegistry.cs
├── Runtime/
│   ├── ILocalizationBuilder.cs
│   ├── LocalizationBuilder.cs
│   ├── LanguageFallbackResolver.cs
│   ├── LanguageSnapshot.cs
│   ├── LanguageSnapshotBuilder.cs
│   ├── LanguageRuntimeContext.cs
│   ├── LocalizationRuntime.cs
│   ├── ILanguageManager.cs
│   ├── ILocalizer.cs
│   ├── Localizer.cs
│   ├── LanguageManager.cs
│   ├── LanguageResourceProvider.cs
│   └── LocalizationRuntimeLogger.cs
```

Tests are split by the same behavior boundaries under `tests/AtomUI.Localization.Tests/`.

---

### Task 1: Define Catalog and compiled bundle contracts

**Files:**
- Create runtime files under `src/AtomUI.Localization/Catalog/` through `TranslationBundleDescriptor.cs`.
- Create: `tests/AtomUI.Localization.Tests/LanguageCatalogDescriptorTests.cs`

**Interfaces:**

```csharp
[LanguageCatalog(ContractVersion = 1)]
public enum LoginLangResourceKind { Title = 1 }

public sealed class LanguageCatalogDescriptor<TResourceKind> : LanguageCatalogDescriptor
    where TResourceKind : struct, Enum
{
    public LanguageCatalogDescriptor(
        string catalogId,
        int contractVersion,
        IReadOnlyList<LanguageCatalogUnitDescriptor> units,
        Func<TResourceKind, int> unitSlotResolver);
}

public sealed class TranslationBundleDescriptor
{
    public TranslationBundleDescriptor(
        string catalogId,
        int contractVersion,
        LanguageTag language,
        TranslationSourceKind sourceKind,
        string sourceIdentity,
        IReadOnlyList<string?> values);
}
```

- [ ] Write failing tests for attribute usage, positive explicit unit IDs, unique unit IDs/names, positive ContractVersion, exact generic resource type, generated resolver behavior, invalid enum key, defensive array copies, valid source kind, valid tag, source identity, and nullable partial bundle slots.
- [ ] Run tests and confirm missing-type RED.
- [ ] Implement validated immutable descriptors. The base descriptor exposes `CatalogId`, `ContractVersion`, `ResourceKindType`, and read-only `Units`; only generated generic code supplies the direct enum-to-slot delegate.
- [ ] Run tests to GREEN and commit `feat(Localization): define catalog bundle contracts`.

---

### Task 2: Freeze deterministic Registry and reject conflicts

**Files:**
- Create: `src/AtomUI.Localization/Runtime/ILocalizationBuilder.cs`
- Create: `src/AtomUI.Localization/Runtime/LocalizationBuilder.cs`
- Create: `src/AtomUI.Localization/Catalog/LanguageCatalogRegistry.cs`
- Create: `tests/AtomUI.Localization.Tests/LanguageCatalogRegistryTests.cs`

**Interfaces:**

```csharp
public interface ILocalizationBuilder
{
    void AddCatalog(LanguageCatalogDescriptor descriptor);
    void AddTranslationBundle(TranslationBundleDescriptor descriptor);
    void AddLanguageDefinition(LanguageDefinition definition);
    void ConfigureLanguages(
        LanguageTag defaultLanguage,
        IEnumerable<LanguageTag> supportedLanguages);
}
```

- [ ] Write RED tests for deterministic Catalog slots sorted by ordinal Catalog ID, duplicate Catalog ID/type, unknown bundle Catalog, contract mismatch, value-count mismatch, incomplete `en-US`, same-priority unit conflict, non-overlapping partial values, and registration-order independence.
- [ ] Implement collect-only builder inputs and a freeze step. Clone every caller collection; do not expose mutable dictionaries or allow registration after Build.
- [ ] Associate bundles by generated Catalog ID and source metadata. Validate conflicts per unit, not merely per file.
- [ ] Run Registry tests to GREEN and commit `feat(Localization): freeze catalog registry inputs`.

---

### Task 3: Resolve fallback and build complete immutable Snapshots

**Files:**
- Create: `src/AtomUI.Localization/Runtime/LanguageFallbackResolver.cs`
- Create: `src/AtomUI.Localization/Runtime/LanguageSnapshot.cs`
- Create: `src/AtomUI.Localization/Runtime/LanguageSnapshotBuilder.cs`
- Create: `tests/AtomUI.Localization.Tests/LanguageFallbackResolverTests.cs`
- Create: `tests/AtomUI.Localization.Tests/LanguageSnapshotTests.cs`

**Interfaces:**

```text
zh-Hant-HK -> zh-Hant -> zh -> en-US
zh-CN      -> zh-Hans -> zh -> en-US
zh-TW      -> zh-Hant -> zh -> en-US
fr-CA      -> fr -> en-US
```

- [ ] Write failing fallback tests for the exact chains above, explicit script/region, neutral language, English, extensions/private use, duplicate suppression, and default rejection. Standard tags use the generated FormattingCulture mapping to obtain CLDR/.NET script parents; no `Split('-')` guessing is allowed.
- [ ] Implement a span/Culture-parent resolver that returns a frozen ordered list and appends `en-US` exactly once.
- [ ] Write failing Snapshot tests for source-priority ordering, per-unit fallback, resolved-source-language tracking, application partial overrides, static pack over built-in, exact over parent within a priority, missing English source, and non-English coverage failure.
- [ ] Build `string[][]` plus resolved-language arrays by deterministic catalog/unit slot. Preparse formatted values into `CompositeFormat` at Snapshot construction and wrap invariant failures with Catalog/unit/source context.
- [ ] Run tests to GREEN and commit `feat(Localization): build immutable language snapshots`.

---

### Task 4: Add the strongly typed Localizer hot path

**Files:**
- Create: `src/AtomUI.Localization/Runtime/ILocalizer.cs`
- Create: `src/AtomUI.Localization/Runtime/LanguageRuntimeContext.cs`
- Create: `src/AtomUI.Localization/Runtime/Localizer.cs`
- Create: `tests/AtomUI.Localization.Tests/LocalizerTests.cs`

**Interfaces:**

```csharp
public interface ILocalizer
{
    string Get<TResourceKind>(TResourceKind key)
        where TResourceKind : struct, Enum;

    string Format<TResourceKind>(TResourceKind key, params object?[] arguments)
        where TResourceKind : struct, Enum;
}
```

- [ ] Write failing tests for typed Get, culture-aware Format, unknown enum type, unknown enum value, insufficient format arguments with preserved inner exception/context, and concurrent revision replacement observing only whole snapshots.
- [ ] Implement generic Catalog lookup by exact `typeof(TResourceKind)` and generated resolver delegate. Read one `LanguageRuntimeRevision` reference per call.
- [ ] Run tests to GREEN and commit `feat(Localization): add strongly typed localizer`.

---

### Task 5: Add synchronous LanguageManager and stable ResourceProvider

**Files:**
- Create: `src/AtomUI.Localization/Runtime/ILanguageManager.cs`
- Create: `src/AtomUI.Localization/Runtime/LanguageManager.cs`
- Create: `src/AtomUI.Localization/Runtime/LanguageResourceProvider.cs`
- Create: `src/AtomUI.Localization/Runtime/LocalizationRuntimeLogger.cs`
- Create: `tests/AtomUI.Localization.Tests/LanguageManagerTests.cs`
- Create: `tests/AtomUI.Localization.Tests/LanguageResourceProviderTests.cs`

**Interfaces:**

```csharp
public interface ILanguageManager
{
    LanguageState Current { get; }
    IReadOnlyList<LanguageDefinition> SupportedLanguages { get; }
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
    LanguageChangeResult ChangeLanguage(LanguageTag language);
}
```

- [ ] Write RED tests for initial state revision 0, supported-language order, `NoOp` identity/no event/no notification, committed revision increment, unsupported tag, default tag, wrong thread, disposal, RTL/Culture state, event order, isolated throwing subscribers, and stable provider identity.
- [ ] Implement one internal atomic revision object shared by Manager/Localizer/Provider. Commit the reference before resource notification and event publication.
- [ ] Subclass Avalonia `ResourceProvider`; resolve exact boxed enum types/values through Registry and current Snapshot. Raise hosted resources changed exactly once on committed changes.
- [ ] Dispatch each language event subscriber independently; log failures through Avalonia Logging and never roll back or suppress later subscribers.
- [ ] Run tests to GREEN and commit `feat(Localization): commit atomic language changes`.

---

### Task 6: Build the complete runtime from configuration

**Files:**
- Create: `src/AtomUI.Localization/Runtime/LocalizationRuntime.cs`
- Complete: `src/AtomUI.Localization/Runtime/LocalizationBuilder.cs`
- Create: `tests/AtomUI.Localization.Tests/LocalizationBuilderTests.cs`
- Modify: `src/AtomUI.Localization/AtomUI.Localization.csproj` for `InternalsVisibleTo` only if AtomUI.Core construction needs it in the follow-on integration.

- [ ] Write RED tests for default `en-US` configuration, configured default membership, defensive de-duplication, generated standard definitions, required explicit private-tag definition, duplicate/conflicting definitions, empty supported set, full snapshot prebuild, and freeze-after-Build.
- [ ] Build standard definitions through `StandardLanguageDefinitions`, never Culture enumeration. Require explicit definitions when generated metadata does not know the tag.
- [ ] Return one internal `LocalizationRuntime` owning Registry, Snapshot map, Manager, Localizer, Provider, and disposal.
- [ ] Run Localization tests to GREEN and commit `feat(Localization): compose application localization runtime`.

---

### Task 7: Verify runtime and package boundaries

- [ ] Run Localization, Core, and Generator tests; Release-build `AtomUI.Localization` for net10.0/net8.0.
- [ ] Audit for `ThemeManager`, old `LanguageProvider`, `Assembly.GetTypes`, `GetFields`, `Enum.GetNames`, `Activator.CreateInstance`, XLIFF/XML/file reads, `.atomlang`, and global Culture mutation.
- [ ] Verify Registry/Snapshot collections cannot be mutated, provider is one stable object, and the new package still has no reference to AtomUI.Core.
- [ ] Run `git diff --check`; commit only real verification fixes.

---

## Follow-on Plans

1. Root `IAtomUIBuilder`, `UseLanguages`, Application service exposure, and stable Provider mounting.
2. XLIFF 2.1 Catalog Generator, diagnostics, generated Markup Extensions, module/application bootstrap.
3. `AtomUI.Build.Tasks`, buildTransitive assets, manifests, template export/merge, static I18n package template.
4. Built-in Catalog migration, old Provider deletion, Gallery/application migration, and NativeAOT release validation.
