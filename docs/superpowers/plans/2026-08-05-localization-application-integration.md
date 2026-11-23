# Localization Application Integration Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate the new localization runtime into the root `Application.UseAtomUI(...)` startup path with a root builder, generated application bootstrap hook, stable Application resource-provider mount, DI-neutral service access, and atomic root FlowDirection projection.

**Architecture:** `AtomUIBuilder` owns independent `ThemeManagerBuilder` and `LocalizationBuilder` instances. Startup invokes the generated Application language bootstrap, applies user configuration, builds both frozen runtimes before first frame, mounts one stable language provider at Application scope, and publishes one application-owned runtime service record. Theme and language states remain independent even while the old Catalog migration is still in progress.

**Tech Stack:** .NET 10/.NET 8, Avalonia 12 `Application`/`ResourceProvider`/`Style`, immutable localization snapshots, source-generated bootstrap contract, xUnit v3, Shouldly.

## Global Constraints

- Follow `docs/modules/localization/public-api.md`, `runtime-architecture.md`, `generation-and-build.md`, and `migration.md`.
- Use red-green-refactor for every behavior; test commands include `-p:IsTestProject=true`.
- `UseAtomUI()` accepts `Action<IAtomUIBuilder>`; language and theme builders are sibling properties.
- `UseLanguages()` accepts `LanguageTag` values and never package names, locale enums, or per-language extension methods.
- Application bootstrap is an explicit generated interface call, never assembly scanning.
- Mount one `LanguageResourceProvider` for the Application lifetime; language changes never swap dictionaries.
- FlowDirection is projected by the same resource revision notification as localized text.
- Do not bridge the new manager to `ThemeManager`, old `LanguageProvider`, or process-global Culture.
- Do not parse XLIFF or inspect package files at runtime.

---

### Task 1: Add Application bootstrap and FlowDirection runtime contracts

**Files:**
- Create: `src/AtomUI.Localization/Runtime/IGeneratedApplicationLanguageBootstrap.cs`
- Create: `src/AtomUI.Localization/Runtime/LanguageRuntimeResourceKeys.cs`
- Modify: `src/AtomUI.Localization/Runtime/LanguageResourceProvider.cs`
- Modify: `src/AtomUI.Localization/AtomUI.Localization.csproj`
- Modify: `tests/AtomUI.Localization.Tests/LanguageResourceProviderTests.cs`

**Interfaces:**
- Produces `IGeneratedApplicationLanguageBootstrap.RegisterApplicationLanguages(ILocalizationBuilder builder)` for generated Application partial types.
- Produces an internal stable FlowDirection resource key consumed only by AtomUI.Core startup styles.
- Grants `AtomUI.Core` access to internal runtime composition types without making them public.

- [ ] Write a failing provider test that resolves LTR initially, switches to an RTL language, and resolves RTL from the same current revision.
- [ ] Run the provider test and confirm RED because the runtime resource key does not exist.
- [ ] Add the generated bootstrap interface with `EditorBrowsableState.Never` and no reflection path.
- [ ] Add the stable internal key and map `LanguageTextDirection` to Avalonia `FlowDirection` in `LanguageResourceProvider` before Catalog lookup.
- [ ] Add `InternalsVisibleTo` for `AtomUI.Core` and rerun Localization tests to GREEN.
- [ ] Commit `feat(Localization): expose application bootstrap contracts`.

---

### Task 2: Introduce the root AtomUI builder

**Files:**
- Create: `src/AtomUI.Core/IAtomUIBuilder.cs`
- Create: `src/AtomUI.Core/AtomUIBuilder.cs`
- Create: `src/AtomUI.Core/Localization/AtomUIBuilderLocalizationExtensions.cs`
- Create: `src/AtomUI.Core/Theme/AtomUIBuilderThemeExtensions.cs`
- Create: `tests/AtomUI.Core.Tests/Localization/AtomUIBuilderTests.cs`

**Interfaces:**

```csharp
public interface IAtomUIBuilder
{
    IThemeManagerBuilder Theme { get; }
    ILocalizationBuilder Localization { get; }
}

public static IAtomUIBuilder UseLanguages(
    this IAtomUIBuilder builder,
    LanguageTag defaultLanguage,
    IEnumerable<LanguageTag> supportedLanguages);
```

- [ ] Write RED tests for the two sibling builders, `UseLanguages()` forwarding, defensive input copying, default membership validation at Build, and fluent return identity.
- [ ] Run tests and confirm missing root types RED.
- [ ] Implement `AtomUIBuilder` as the only owner of concrete sub-builders.
- [ ] Add root theme forwarding extensions for existing common startup calls so user configuration no longer depends on the theme sub-builder type.
- [ ] Run Core and Localization tests to GREEN.
- [ ] Commit `feat(Core): add root AtomUI builder`.

---

### Task 3: Own Application-scoped services and resource lifetime

**Files:**
- Create: `src/AtomUI.Core/ApplicationScope.cs`
- Create: `src/AtomUI.Core/ApplicationScopeRegistry.cs`
- Create: `tests/AtomUI.Core.Tests/Localization/ApplicationScopeTests.cs`

**Interfaces:**
- `ApplicationScope` owns the concrete ThemeManager and LocalizationHost.
- The scope mounts/removes the stable provider in `Application.Resources.MergedDictionaries`.
- The scope mounts/removes one `TopLevel` style whose `Visual.FlowDirectionProperty` uses the internal dynamic language resource key.
- The registry uses Application identity, rejects duplicate registration, supports exact unregistration, and does not use AvaloniaLocator for localization.

- [ ] Write RED tests for one provider mount, stable provider identity, service identity, duplicate attach rejection, FlowDirection dynamic update, and disposal cleanup.
- [ ] Run tests and confirm missing scope/registry RED.
- [ ] Implement the application-owned scope and weak identity registry.
- [ ] Ensure disposal removes provider/style and disposes Manager subscriptions without retaining Window or ViewModel objects.
- [ ] Run Core tests to GREEN.
- [ ] Commit `feat(Core): own application localization scope`.

---

### Task 4: Replace the UseAtomUI root startup entry

**Files:**
- Modify: `src/AtomUI.Core/ApplicationExtensions.cs`
- Create: `tests/AtomUI.Core.Tests/Localization/ApplicationLocalizationStartupTests.cs`

**Interfaces:**

```csharp
public static Application UseAtomUI(
    this Application application,
    Action<IAtomUIBuilder>? configure = null);

public static ILanguageManager? GetLanguageManager(this Application application);
public static ILocalizer? GetLocalizer(this Application application);
public static IThemeManager? GetThemeManager(this Application application);
```

- [ ] Write RED headless tests for default `en-US`, configured default language, generated bootstrap invocation before user configuration, shared service instances, Application dynamic enum resources, committed resource refresh, root RTL/LTR update, and duplicate `UseAtomUI()` rejection.
- [ ] Run tests and confirm old callback type/service path RED.
- [ ] Build the root builder, invoke `IGeneratedApplicationLanguageBootstrap`, invoke user configuration, freeze Localization, build ThemeManager, initialize the application-owned runtime, then run theme initializers.
- [ ] Preserve the existing ThemeManager AvaloniaLocator binding only for the still-current theme internals; localization services come exclusively from the application runtime store.
- [ ] Dispose both partial runtimes if initialization fails before attachment.
- [ ] Run Core tests to GREEN.
- [ ] Commit `feat(Core): initialize application localization services`.

---

### Task 5: Move package startup extensions to IAtomUIBuilder

**Files:**
- Modify package builder extensions under `src/AtomUI.Controls`, `src/AtomUI.Desktop.Controls*`, `src/AtomUI.Fonts.*`, `src/AtomUI.Toolkits.GalleryBase`, and `controlgallery/AtomUIGallery`.
- Modify relevant tests that implement or assert the old root callback surface.

**Interfaces:**
- Package entry points accept and return `IAtomUIBuilder`.
- Existing generated theme registration receives `builder.Theme` during the migration window.
- Language module registration will be added beside theme registration by the Catalog generator plan; no adapter wraps old providers into new bundles.

- [ ] Compile affected projects to obtain the exact RED call sites after the root callback change.
- [ ] Change package extension signatures and forwarding calls without altering package registration order.
- [ ] Keep old LanguageProvider calls confined to the old theme sub-builder until each Catalog is migrated; do not mount them into the new localization runtime.
- [ ] Update Gallery startup to `UseLanguages()` only when its Catalogs have moved to the new runtime; do not create dual state adapters.
- [ ] Run affected Core, Desktop, GalleryBase, and Gallery tests to GREEN.
- [ ] Commit `refactor(Core): route package setup through root builder`.

---

### Task 6: Verify startup boundaries

- [ ] Build `AtomUI.Core` for net10.0 and net8.0 Release with zero warnings.
- [ ] Run Localization, Core, Generator, Desktop Controls, GalleryBase, and Gallery targeted tests.
- [ ] Audit that root localization startup contains no `ThemeManager` language access, assembly scan, enum reflection, XLIFF/XML/file read, `.atomlang`, or global Culture mutation.
- [ ] Verify the provider is mounted once, FlowDirection uses the same notification, and `GetLanguageManager()`/`GetLocalizer()` return the exact runtime instances.
- [ ] Run `git diff --check`; commit only necessary verification fixes.

## Follow-on Plans

1. XLIFF 2.1 Catalog generator, generated Markup Extensions, Language Module registration, and Application bootstrap generation.
2. `AtomUI.Build.Tasks`, buildTransitive localization assets, manifest/template export and merge, and static I18n package template.
3. Built-in Catalog migration, old LanguageProvider/ThemeManager language deletion, Gallery language migration, and NativeAOT publish validation.
