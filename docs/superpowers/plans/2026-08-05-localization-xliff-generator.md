# Localization XLIFF Generator Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate strongly typed Catalog descriptors, compiled Translation Bundles, existing-shape XAML markup extensions, module registration, and Application bootstrap code from `[LanguageCatalog]` enums and XLIFF 2.1 build inputs.

**Architecture:** A dedicated incremental generator joins Roslyn Catalog symbols with normalized XLIFF build-time models and explicit AdditionalFiles metadata. It reports deterministic `ATOMUILOC` diagnostics for invalid contracts, then emits only direct constructor calls and static tables consumed by `AtomUI.Localization`; XLIFF, XML, package discovery, enum reflection, and assembly scanning never reach runtime.

**Tech Stack:** C# incremental generators, Roslyn symbols and AdditionalText, XLIFF 2.1 parsed with build-time `System.Xml`, Avalonia MarkupExtension, xUnit v3, Shouldly, .NET 10/.NET 8.

## Global Constraints

- Follow `docs/modules/localization/catalog-and-xliff.md`, `generation-and-build.md`, `public-api.md`, and `diagnostics-and-testing.md`.
- XLIFF 2.1 is the only translation source; runtime code contains compiled strings and never parses XML.
- Catalog source language is always `en-US`; every Catalog requires a complete source bundle.
- Catalog members use explicit, unique, positive integer IDs; `[Flags]`, aliases, zero, negative values, and implicit values are errors.
- Catalog ID is `{LanguageModuleId}:{FullyQualifiedMetadataName}`; module identity comes from `PackageId`, then `AssemblyName`.
- Generated code does not call `Enum.GetNames`, `Type.GetFields`, `Assembly.GetTypes`, attributes through reflection, or `Activator.CreateInstance`.
- XAML remains `{login:LoginLangResource Title}` and `{atom:DatePickerLangResource Today}` through a generated `XxxLangResourceExtension`.
- Translation precedence is `ApplicationOverride`, `StaticLanguagePack`, `ModuleBuiltIn`, then `en-US` source; registration order never changes precedence.
- No dynamic language loading, `.atomlang`, runtime XLIFF, per-language startup extensions, or locale enum.
- Every behavior follows red-green-refactor and every test command includes `-p:IsTestProject=true`.

---

### Task 1: Add the runtime base for generated language markup extensions

**Files:**
- Create: `src/AtomUI.Localization/Resources/LanguageResourceExtension.cs`
- Create: `tests/AtomUI.Core.Tests/Localization/LanguageResourceExtensionTests.cs`

**Interfaces:**
- Produces `LanguageResourceExtension<TResourceKind> : MarkupExtension where TResourceKind : struct, Enum`.
- Generated extensions pass enum values directly to Avalonia `DynamicResourceExtension`; no string resource path exists.

- [ ] Write RED tests that constructor assignment preserves the enum key, `ProvideValue()` returns a dynamic resource for `StyledElement` targets, and static non-Avalonia targets resolve the current Application resource by the enum key.
- [ ] Run `dotnet test tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj --framework net10.0 --no-restore -p:IsTestProject=true --filter FullyQualifiedName~LanguageResourceExtensionTests` and confirm the base type is missing.
- [ ] Implement the public abstract base with `Kind`, parameterless/key constructors, native `DynamicResourceExtension` handling for Avalonia targets, and a clear exception when no Application exists for a static target.
- [ ] Keep static lookup on `Application.TryGetResource(Kind, themeVariant, out value)`; do not reference the old `LanguageResourceBinder`, private DynamicResource anchor reflection, or ThemeManager.
- [ ] Rerun the focused and complete Localization tests to GREEN.
- [ ] Commit `feat(Localization): add generated resource extension base`.

---

### Task 2: Define localization generator diagnostics and Catalog symbol model

**Files:**
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticCategories.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticDescriptors.cs`
- Create: `src/AtomUI.Generator/Localization/Catalog/LanguageCatalogInfo.cs`
- Create: `src/AtomUI.Generator/Localization/Catalog/LanguageCatalogSymbolParser.cs`
- Create: `tests/AtomUI.Generator.Tests/Localization/LanguageCatalogGeneratorTests.cs`
- Modify: `docs/engineering/compiler-diagnostics-guidelines.md`

**Interfaces:**
- Produces immutable `LanguageCatalogInfo(ModuleId, MetadataName, Namespace, TypeName, ContractVersion, Units, Location)` and `LanguageCatalogUnitInfo(Id, Name, IsFormatted, Location)` records.
- Registers `ATOMUILOC003` invalid Catalog declaration, `ATOMUILOC004` invalid Catalog unit, `ATOMUILOC005` invalid XLIFF document, `ATOMUILOC006` Catalog/XLIFF contract mismatch, `ATOMUILOC007` invalid translation message, and `ATOMUILOC008` invalid Application bootstrap host; all are Error diagnostics.

- [ ] Write RED generator tests for a valid public enum and for non-enum, generic, `[Flags]`, implicit, zero, negative, duplicate ID, duplicate name/alias, and non-positive `ContractVersion` inputs.
- [ ] Assert each error test checks diagnostic ID, severity, minimal source location, and message fields; add a correct-input test that reports none.
- [ ] Run the focused tests and confirm the localization generator/parser is absent.
- [ ] Add the six diagnostic IDs/descriptors and register their stable meanings in the compiler diagnostic table.
- [ ] Parse enum constants through Roslyn `IFieldSymbol.ConstantValue`, detect explicit initializer syntax, sort units by numeric ID, and derive CLR metadata names without source-string parsing.
- [ ] Rerun the focused tests to GREEN and commit `feat(Generator): validate language Catalog symbols`.

---

### Task 3: Parse and normalize XLIFF 2.1 AdditionalFiles

**Files:**
- Create: `src/AtomUI.Localization.Build.Shared/Xliff/XliffDocumentModel.cs`
- Create: `src/AtomUI.Localization.Build.Shared/Xliff/Xliff21Parser.cs`
- Create: `src/AtomUI.Localization.Build.Shared/Xliff/CompositeFormatContractParser.cs`
- Create: `src/AtomUI.Localization.Build.Shared/Language/Bcp47LanguageTagParser.cs`
- Create: `src/AtomUI.Generator/Localization/Xliff/AdditionalLanguageFile.cs`
- Create: `src/AtomUI.Generator/Localization/Xliff/LanguageGeneratorOptions.cs`
- Modify: `src/AtomUI.Generator/AtomUI.Generator.csproj`
- Create: `tests/AtomUI.Generator.Tests/Localization/Xliff21ParserTests.cs`
- Create: `tests/AtomUI.Generator.Tests/Localization/LocalizationGeneratorTestHost.cs`

**Interfaces:**
- Consumes AdditionalFiles with `build_metadata.AdditionalFiles.AtomUILanguage=true`.
- Consumes `AtomUILanguageSourceKind`, `AtomUILanguageSourceIdentity`, and optional `AtomUILanguageModuleId`; consumes global `build_property.PackageId`, `build_property.AssemblyName`, and `build_property.RootNamespace`.
- Produces one immutable model per XLIFF file containing normalized `srcLang`, optional `trgLang`, `file id`, ordered units, source/target text, target state, placeholder signature, source kind, identity, and Roslyn `Location` values.

- [ ] Write RED tests for the canonical XLIFF 2.1 namespace/version, entity-safe text, `en-US` source, normalized BCP 47 target, numeric unit IDs, names, target state, notes, escaped braces, and indexed CompositeFormat placeholders.
- [ ] Add RED cases for DTD/entity expansion, malformed XML, wrong namespace/version, wrong `srcLang`, missing/invalid `trgLang`, multiple files/segments, duplicate units, invalid state, invalid format syntax, and source/target placeholder mismatch.
- [ ] Run the parser tests and confirm the parser types are missing.
- [ ] Implement XML parsing from `AdditionalText.GetText()` only, disable DTD/external resolution, retain XML line information, and create AdditionalText diagnostics without `File.ReadAllText`.
- [ ] Compile `src/AtomUI.Localization.Build.Shared/**/*.cs` into the Generator through linked source items; this same source is later compiled into `AtomUI.Build.Tasks` without introducing a shared runtime DLL.
- [ ] Reuse `LanguageTag` normalization rules through `Bcp47LanguageTagParser` with equivalent test vectors; do not add a runtime project reference from the analyzer.
- [ ] Make model equality/order deterministic by catalog metadata name, language tag, source priority, source identity, and unit ID.
- [ ] Run the focused tests to GREEN and commit `feat(Generator): parse XLIFF 2.1 language inputs`.

---

### Task 4: Join Catalogs to XLIFF and validate translation contracts

**Files:**
- Create: `src/AtomUI.Generator/Localization/Catalog/LanguageCatalogCompiler.cs`
- Create: `src/AtomUI.Generator/Localization/Catalog/CompiledLanguageCatalog.cs`
- Create: `src/AtomUI.Generator/Localization/Catalog/ReferencedLanguageCatalogResolver.cs`
- Modify: `tests/AtomUI.Generator.Tests/Localization/LanguageCatalogGeneratorTests.cs`

**Interfaces:**
- Produces `CompiledLanguageCatalog` with stable Catalog ID, contract version, numeric-ID-to-slot mapping, unit descriptors, and ordered compiled bundles.
- A bundle includes `LanguageTag`, `TranslationSourceKind`, `SourceIdentity`, and a slot-aligned `string?[]`.
- StaticLanguagePack and ApplicationOverride inputs resolve their `file id` through `Compilation.GetTypeByMetadataName`; referenced enum constants and `LanguageCatalogAttribute.ContractVersion` are read through Roslyn metadata symbols, never runtime reflection.

- [ ] Write RED tests joining a Catalog to complete `en-US`, `zh-CN`, and `zh-TW` files and asserting slots follow numeric ID rather than declaration/file order.
- [ ] Add RED diagnostics for missing/duplicate `en-US`, wrong file ID, wrong/missing unit ID or name, unknown unit, incomplete publishable target, duplicate same-priority source, and source text mismatch across language files.
- [ ] Add RED tests for a static pack targeting a Catalog enum from a metadata reference, a missing referenced Catalog, a module-ID mismatch, and an incompatible ContractVersion.
- [ ] Add a valid rename case where numeric ID is stable and XLIFF name must match the current enum member before generation.
- [ ] Run focused tests and confirm no compiler/join stage exists.
- [ ] Implement a deterministic join that derives source-unit `IsFormatted`, validates target placeholder sets, maps source kinds to runtime enum names, and never selects a winner by file enumeration order.
- [ ] Rerun focused tests to GREEN and commit `feat(Generator): compile language Catalog contracts`.

---

### Task 5: Generate descriptors, compiled bundles, and existing-shape markup extensions

**Files:**
- Create: `src/AtomUI.Generator/Localization/LanguageCatalogSourceWriter.cs`
- Create: `src/AtomUI.Generator/Localization/LanguageModuleSourceWriter.cs`
- Create: `src/AtomUI.Generator/Localization/LocalizationGenerator.cs`
- Modify: `tests/AtomUI.Generator.Tests/Localization/LanguageCatalogGeneratorTests.cs`

**Interfaces:**
- Produces public `LoginLangResourceExtension` beside `LoginLangResourceKind` by replacing a terminal `Kind` suffix with `Extension`; other valid enum names receive a terminal `Extension` without imposing a hidden naming restriction.
- Produces `AtomUI.Generated.<AssemblyIdentifier>.GeneratedLanguageModuleRegistration.Register(ILocalizationBuilder builder)` for module-owned Catalogs and `ModuleBuiltIn` bundles.

- [ ] Write RED golden-output tests for `LoginLangResourceExtension` constructors, `LanguageCatalogDescriptor<LoginLangResourceKind>`, explicit enum switch-to-slot mapping, unit descriptors, and compiled string arrays for three languages.
- [ ] Assert generated code contains direct `builder.AddCatalog(...)` and `builder.AddTranslationBundle(...)` calls and contains no reflection, XML, file IO, `Enum.GetNames`, `ToString()` key path, or `Activator`.
- [ ] Compile the generated output against minimal AtomUI.Localization/Avalonia stubs and assert zero C# errors.
- [ ] Implement the incremental pipeline by combining Catalog symbols, marked AdditionalFiles, and analyzer options; do not merge it into the legacy `LanguageGenerator`.
- [ ] Generate escaped C# string literals with Roslyn syntax APIs and stable hint names based on metadata identity.
- [ ] Rerun focused tests to GREEN and commit `feat(Generator): emit compiled language modules`.

---

### Task 6: Generate the explicit Application bootstrap

**Files:**
- Create: `src/AtomUI.Generator/Localization/ApplicationLanguageBootstrapWriter.cs`
- Modify: `src/AtomUI.Generator/Localization/LocalizationGenerator.cs`
- Create: `tests/AtomUI.Generator.Tests/Localization/ApplicationLanguageBootstrapGeneratorTests.cs`

**Interfaces:**
- Produces an explicit partial implementation of `IGeneratedApplicationLanguageBootstrap.RegisterApplicationLanguages(ILocalizationBuilder builder)` on the one concrete top-level non-generic partial Avalonia `Application` type.
- The method invokes module registration and directly adds StaticLanguagePack/ApplicationOverride bundles compiled in the final application.

- [ ] Write RED tests for a valid partial Application, application-owned Catalogs, language-pack inputs, overrides, a project with no localization input, non-partial Application, multiple concrete Applications, nested/generic Applications, and a class library without Application.
- [ ] Assert invalid hosts report `ATOMUILOC008` at the Application identifier and never fall back to assembly scanning.
- [ ] Assert a class library still emits module registration but no Application partial; an application bootstrap calls module registration exactly once.
- [ ] Implement semantic inheritance checks against `Avalonia.Application`, partial/top-level/non-generic validation, and explicit interface implementation.
- [ ] Compile generated output and rerun focused tests to GREEN.
- [ ] Commit `feat(Generator): generate application language bootstrap`.

---

### Task 7: Verify determinism, incremental isolation, and runtime integration

**Files:**
- Create: `tests/AtomUI.Generator.Tests/Localization/LocalizationGeneratorDeterminismTests.cs`
- Create: `tests/AtomUI.Core.Tests/Localization/GeneratedLocalizationIntegrationTests.cs`
- Modify: `docs/modules/localization/generation-and-build.md`

**Interfaces:**
- Verifies the generator output can initialize the real `Application.UseAtomUI()` runtime and serve XAML/C# enum keys from the same Snapshot.

- [ ] Add determinism tests that reorder enum declarations, XLIFF files, units, and AdditionalFiles while preserving IDs and assert byte-identical generated sources.
- [ ] Add an incremental tracking test showing one XLIFF change only invalidates the affected Catalog output and application bootstrap aggregate.
- [ ] Add a real runtime integration fixture with an application Catalog, generated bootstrap, `en-US`/`zh-CN` bundles, `ILocalizer.Get/Format`, and the generated markup extension key.
- [ ] Run all Generator, Localization, and Core tests to GREEN.
- [ ] Build `AtomUI.Generator`, `AtomUI.Localization`, and `AtomUI.Core` in Release for their supported target frameworks with zero warnings.
- [ ] Audit generated output and binaries for runtime XLIFF/XML parsing, reflection discovery, `.atomlang`, and process Culture mutation.
- [ ] Run `git diff --check` and commit `test(Localization): verify generated Catalog integration`.

## Completion Criteria

- `[LanguageCatalog]` enums plus marked XLIFF 2.1 inputs generate deterministic, compiling, strongly typed runtime registration.
- Module packages can call one generated registration method; applications receive one explicit generated bootstrap implementation.
- `{login:LoginLangResource Title}` resolves through the new stable `LanguageResourceProvider` without a string key.
- Invalid Catalogs, XLIFF, translations, or Application hosts fail with documented `ATOMUILOC003` through `ATOMUILOC008` diagnostics.
- Runtime code and publish inputs contain compiled tables only; no XLIFF parser, XML document, assembly scan, or language-pack loader is reachable at runtime.
