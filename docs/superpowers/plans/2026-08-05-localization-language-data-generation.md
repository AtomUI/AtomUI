# AtomUI Localization Language Data Generation Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate the public `LanguageTags` constants and standard `LanguageDefinition` metadata from version-pinned language data without introducing a closed locale enum or runtime culture enumeration.

**Architecture:** `AtomUI.Generator` consumes one explicitly marked TSV `AdditionalFile` only while compiling `AtomUI.Localization`. It validates and canonicalizes the pinned records, emits the public `LanguageTags` surface plus an internal switch-based language definition table, and produces no output in unrelated compilations. The generated runtime code uses direct `LanguageTag` values and static switch branches, so consumers do not scan cultures, enums, assemblies, or attributes at runtime.

**Tech Stack:** Roslyn Incremental Generator, repository-pinned Unicode/IANA-derived TSV data, C# generated sources, xUnit v3, Shouldly, .NET 10/.NET 8.

## Global Constraints

- Follow `docs/modules/localization/public-api.md`, `generation-and-build.md`, `runtime-architecture.md`, and `diagnostics-and-testing.md`.
- `LanguageTag` remains the open identity. `LanguageTags` is a generated convenience surface, never an enum and never a statement that a translation bundle is installed.
- The data source must record its schema and upstream snapshot versions in comments and remain deterministic under reordered file enumeration.
- The generator runs only for an `AdditionalFile` with `build_metadata.AdditionalFiles.AtomUILanguageTagsData=true`.
- Do not use `CultureInfo.GetCultures`, assembly/type/enum reflection, runtime file I/O, regex-based runtime parsing, or hand-maintained C# properties.
- Generated code must use fully-qualified type names and stable ordinal ordering by identifier.
- Generator diagnostics use the registered `ATOMUILOCNNN` domain and point to the offending data line.
- Every behavior follows red-green-refactor; test commands include `-p:IsTestProject=true`.

---

## File Structure

```text
src/AtomUI.Localization/
├── LanguageData/
│   └── language-tags.tsv
├── StandardLanguageDefinitions.cs
└── AtomUI.Localization.csproj

src/AtomUI.Generator/Localization/
├── LanguageDataEntry.cs
├── LanguageDataFileParser.cs
├── LanguageTagsGenerator.cs
└── LanguageTagsSourceWriter.cs

tests/AtomUI.Generator.Tests/Localization/
└── LanguageTagsGeneratorTests.cs

tests/AtomUI.Localization.Tests/
└── LanguageTagsTests.cs
```

The generator emits into the `AtomUI.Localization` assembly:

```text
LanguageTags.g.cs
GeneratedStandardLanguageDefinitions.g.cs
```

---

### Task 1: Register localization diagnostics and isolate the legacy generator

**Files:**
- Modify: `docs/engineering/compiler-diagnostics-guidelines.md`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticIds.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticCategories.cs`
- Modify: `src/AtomUI.Generator/Diagnostics/AtomUIDiagnosticDescriptors.cs`
- Modify: `src/AtomUI.Generator/LanguageGenerator.cs`
- Modify: `tests/AtomUI.Generator.Tests/Language/LanguageProviderConstructorGeneratorTests.cs`

**Interfaces:**
- Produces: `ATOMUILOC001` for malformed language data and `ATOMUILOC002` for duplicate identifier/tag records.
- Preserves: all existing provider generation when at least one legacy provider exists.

- [ ] **Step 1: Write a failing no-provider isolation test**

Extend the existing no-provider test to assert that none of these files exist:

```csharp
var generatedPaths = outputCompilation.SyntaxTrees.Select(static tree => tree.FilePath).ToArray();
generatedPaths.ShouldNotContain(static path => path.EndsWith("LanguageResourceConst.g.cs"));
generatedPaths.ShouldNotContain(static path => path.EndsWith("LanguageProviderPool.g.cs"));
generatedPaths.ShouldNotContain(static path => path.EndsWith("LanguageProviderConstructors.g.cs"));
```

- [ ] **Step 2: Run the existing generator test and confirm RED**

Run:

```bash
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true \
  --filter FullyQualifiedName~LanguageProviderConstructorGeneratorTests.DoesNotEmitConstructorFileWhenProjectHasNoLanguageProviders
```

Expected: the resource/pool assertions fail because the legacy generator emits empty-compilation output.

- [ ] **Step 3: Stop all legacy language output when provider collection is empty**

Add one early return in `LanguageGenerator`'s source-output callback before constructing any writer:

```csharp
if (languageProviders.IsEmpty)
{
    return;
}
```

- [ ] **Step 4: Register the localization diagnostic domain**

Add category `Localization`, IDs `ATOMUILOC001`/`ATOMUILOC002`, and Error descriptors with messages containing the data path, line, and actionable reason. Update the global diagnostic registry with the `LOC` domain and both IDs before using them.

- [ ] **Step 5: Run the targeted and full generator tests**

Expected: the targeted test and all current 52 generator tests pass.

- [ ] **Step 6: Commit**

```bash
git add docs/engineering/compiler-diagnostics-guidelines.md \
  src/AtomUI.Generator/Diagnostics src/AtomUI.Generator/LanguageGenerator.cs \
  tests/AtomUI.Generator.Tests/Language/LanguageProviderConstructorGeneratorTests.cs
git commit -m "fix(Generator): isolate legacy language generation"
```

---

### Task 2: Parse and validate pinned language data

**Files:**
- Create: `src/AtomUI.Generator/Localization/LanguageDataEntry.cs`
- Create: `src/AtomUI.Generator/Localization/LanguageDataFileParser.cs`
- Create: `tests/AtomUI.Generator.Tests/Localization/LanguageTagsGeneratorTests.cs`

**Interfaces:**
- Consumes TSV records: `Identifier<TAB>Tag<TAB>CultureName<TAB>NativeName<TAB>Direction`.
- Produces immutable entries ordered by `Identifier`, or `ATOMUILOC001/002` diagnostics.

- [ ] **Step 1: Write failing parser-through-generator tests**

Use an in-memory AdditionalText and per-file options provider returning:

```text
build_metadata.AdditionalFiles.AtomUILanguageTagsData=true
```

Cover valid comments/header/records, invalid column count, invalid C# identifier, invalid or non-canonical BCP 47 tag, invalid direction, duplicate identifier, and duplicate canonical tag. Assert exact diagnostic ID, Error severity, line location, and a key message fragment.

- [ ] **Step 2: Run the new tests and confirm RED**

Expected: compilation fails because `LanguageTagsGenerator` does not exist.

- [ ] **Step 3: Implement the immutable parser model**

Use this internal shape:

```csharp
internal sealed record LanguageDataEntry(
    string Identifier,
    string Tag,
    string CultureName,
    string NativeName,
    bool IsRightToLeft,
    Location Location);
```

The parser ignores empty/comment lines, requires the exact schema marker, validates identifiers with Roslyn `SyntaxFacts`, validates canonical BCP 47 casing with a build-time parser, rejects empty metadata, and reports rather than throws on user/data errors.

- [ ] **Step 4: Run parser tests to GREEN**

Expected: all diagnostic cases pass and no source is emitted for invalid input.

- [ ] **Step 5: Commit**

```bash
git add src/AtomUI.Generator/Localization tests/AtomUI.Generator.Tests/Localization
git commit -m "feat(Generator): validate pinned localization data"
```

---

### Task 3: Generate LanguageTags and standard metadata

**Files:**
- Modify: `src/AtomUI.Generator/Localization/LanguageTagsGenerator.cs`
- Create: `src/AtomUI.Generator/Localization/LanguageTagsSourceWriter.cs`
- Modify: `tests/AtomUI.Generator.Tests/Localization/LanguageTagsGeneratorTests.cs`

**Interfaces:**
- Produces: `public static class AtomUI.Localization.LanguageTags`.
- Produces: internal `GeneratedStandardLanguageDefinitions.TryCreate(LanguageTag, out LanguageDefinition?)`.

- [ ] **Step 1: Write failing source-shape and determinism tests**

Given records for `EnUS`, `ZhCN`, `ZhTW`, and `ArSA`, assert generated source contains:

```csharp
public static global::AtomUI.Localization.LanguageTag EnUS { get; } =
    global::AtomUI.Localization.LanguageTag.Parse("en-US");
```

and a `tag.Value switch`/switch statement that creates read-only `LanguageDefinition` metadata with the exact culture, native name, and RTL value. Run the same data in two record orders and assert byte-identical generated source. Assert generated code contains no culture enumeration or reflection calls.

- [ ] **Step 2: Run the focused tests and confirm RED**

Expected: data parses, but expected `LanguageTags.g.cs` and metadata output are absent.

- [ ] **Step 3: Implement deterministic source output**

Sort by identifier using `StringComparer.Ordinal`, emit robust C# string literals through Roslyn, use fully-qualified names, and add source only when the marked file has no diagnostics. Generate switch branches, not runtime dictionaries populated through discovery.

- [ ] **Step 4: Run generator tests to GREEN**

Expected: output-shape, compile, and determinism tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/AtomUI.Generator/Localization tests/AtomUI.Generator.Tests/Localization
git commit -m "feat(Generator): generate common language tags"
```

---

### Task 4: Pin the common-language dataset and wire AtomUI.Localization

**Files:**
- Create: `src/AtomUI.Localization/LanguageData/language-tags.tsv`
- Create: `src/AtomUI.Localization/StandardLanguageDefinitions.cs`
- Modify: `src/AtomUI.Localization/AtomUI.Localization.csproj`
- Create: `tests/AtomUI.Localization.Tests/LanguageTagsTests.cs`

**Interfaces:**
- Public examples required: `EnUS`, `EnGB`, `ZhCN`, `ZhHans`, `ZhTW`, `ZhHant`, `JaJP`, `KoKR`, `ArSA`, `SrLatnRS`.
- Internal: `StandardLanguageDefinitions.TryCreate(LanguageTag, out LanguageDefinition?)` delegates only to generated metadata.

- [ ] **Step 1: Write failing runtime API tests**

Assert required properties return canonical values, are distinct where expected, contain no enum surface, and standard definitions return correct Culture/native name/direction. Assert a private tag such as `en-x-acme` has no generated definition.

- [ ] **Step 2: Run Localization tests and confirm RED**

Expected: `LanguageTags` and `StandardLanguageDefinitions` are missing.

- [ ] **Step 3: Add the pinned data file**

The file header records schema version, IANA language-subtag registry snapshot, and CLDR snapshot. Include neutral/script tags plus commonly deployed regional locales across all major language families, with identifiers produced by the documented PascalCase BCP 47 transformation. Keep records sorted by identifier.

- [ ] **Step 4: Wire the analyzer only into AtomUI.Localization**

Add the analyzer ProjectReference with `OutputItemType="Analyzer"`, `ReferenceOutputAssembly="false"`, and `PrivateAssets="all"`. Add the TSV as `AdditionalFiles` with `AtomUILanguageTagsData="true"`, and expose that metadata through `CompilerVisibleItemMetadata`.

- [ ] **Step 5: Add the internal wrapper and run tests**

`StandardLanguageDefinitions` must contain no hand-authored tag switch; it delegates to generated code. Expected: runtime tests and generator tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/AtomUI.Localization tests/AtomUI.Localization.Tests/LanguageTagsTests.cs
git commit -m "feat(Localization): add generated common language metadata"
```

---

### Task 5: Verify generation boundaries

**Files:**
- Modify only if validation exposes a defect in files from Tasks 1-4.

- [ ] **Step 1: Run all affected tests and multi-target build**

```bash
dotnet test tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj --framework net10.0 --no-restore -p:IsTestProject=true
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore -p:IsTestProject=true
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore -p:IsTestProject=true
dotnet restore src/AtomUI.Localization/AtomUI.Localization.csproj -p:Configuration=Release --force
dotnet build src/AtomUI.Localization/AtomUI.Localization.csproj --configuration Release --no-restore
git diff --check
```

- [ ] **Step 2: Audit generated source and forbidden behavior**

Confirm the generated files exist under compiler output, the public type is in `AtomUI.Localization.dll`, and there are no matches for `CultureInfo.GetCultures`, `Assembly.GetTypes`, `Enum.GetNames`, `GetFields`, or runtime file reads in the new path.

- [ ] **Step 3: Review package dependency direction**

Confirm `AtomUI.Localization` consumes `AtomUI.Generator` only as an analyzer and the generator has no runtime ProjectReference back to Localization.

- [ ] **Step 4: Commit validation fixes only when required**

Do not create an empty commit.

---

## Follow-on Plan

After this batch, create and execute the Catalog/Snapshot runtime plan using the generated standard definitions for `UseLanguages()` metadata resolution. That plan owns descriptors, bundle priority/conflict rules, CLDR fallback candidates, Registry, immutable Snapshot, `ILocalizer`, `ILanguageManager`, stable Avalonia `LanguageResourceProvider`, and root builder integration.
