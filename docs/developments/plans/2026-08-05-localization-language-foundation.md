# AtomUI Localization Language Foundation Implementation Plan

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create the independent `AtomUI.Localization` runtime package and implement its open BCP 47 `LanguageTag` plus immutable language metadata/state/change contracts without coupling them to ThemeManager.

**Architecture:** `AtomUI.Localization` is a lower-level runtime package referenced by `AtomUI.Core`. The first deliverable contains only language identity and state contracts; it deliberately does not add temporary `LanguageTags` constants, Catalog registries, XLIFF parsing, Avalonia resource providers, or adapters to the legacy LanguageProvider system. BCP 47 parsing is implemented as an internal deterministic parser over pinned alias data so normal operation does not depend on runtime assembly scanning or platform-specific culture enumeration.

**Tech Stack:** .NET 10/.NET 8 multi-targeted SDK projects, C# record structs/records, `System.Globalization`, xUnit v3, Shouldly, existing central package management.

## Global Constraints

- Follow `docs/modules/localization/overview.md`, `public-api.md`, `runtime-architecture.md`, and `migration.md` exactly.
- `LanguageTag` is an open canonical BCP 47 value type; do not introduce a locale enum.
- `default(LanguageTag)` is invalid and no API may interpret it as system culture or `en-US`.
- Language identity does not imply a translation bundle is installed.
- Do not add runtime reflection, `Assembly.GetTypes()`, `Enum.GetNames()`, XLIFF parsing, dynamic language loading, `.atomlang`, or ThemeManager integration.
- Do not hand-write the final `LanguageTags` common-locale surface; it will be generated from pinned CLDR/IANA inputs in a separate plan.
- Every production behavior follows red-green-refactor and every command must pass `-p:IsTestProject=true` because the current repository `IsTestProject` property contains XML whitespace.
- Do not modify or stage files outside this isolated worktree and the exact paths listed below.

---

## File Structure

```text
src/AtomUI.Localization/
├── AtomUI.Localization.csproj
├── LanguageTag.cs                    # Public open BCP 47 value type
├── Bcp47LanguageTagParser.cs         # Internal validation/canonicalization parser
├── Bcp47LanguageTagAliases.cs        # Pinned grandfathered/deprecated aliases used by parser
├── LanguageDefinition.cs             # Tag + formatting culture + native name + text direction
├── LanguageTextDirection.cs          # LTR/RTL domain enum
├── LanguageState.cs                  # Immutable committed language state
├── LanguageChangeResult.cs           # Committed/NoOp result and old/new state
├── LanguageChangedEventArgs.cs       # Event payload
└── LanguageExceptions.cs             # Public typed exceptions

tests/AtomUI.Localization.Tests/
├── AtomUI.Localization.Tests.csproj
├── LanguageTagTests.cs
└── LanguageContractsTests.cs
```

Project graph changes:

```text
AtomUI.Core -> AtomUI.Localization -> Avalonia
AtomUI.Localization.Tests -> AtomUI.Localization
```

`AtomUI.slnx` includes both new projects. No existing runtime class consumes the new package in this plan.

---

### Task 1: Add the independent runtime and test projects

**Files:**
- Create: `src/AtomUI.Localization/AtomUI.Localization.csproj`
- Create: `tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj`
- Create: `tests/AtomUI.Localization.Tests/LanguageTagTests.cs`
- Modify: `src/AtomUI.Core/AtomUI.Core.csproj`
- Modify: `AtomUI.slnx`

**Interfaces:**
- Consumes: repository `Directory.Build.props`, central package versions, `$(AtomUITargetFrameworks)`.
- Produces: buildable `AtomUI.Localization` package project, test project, and a deliberately failing reference to `LanguageTag.Parse`.

- [ ] **Step 1: Create the test project and first failing test**

Create the test project with the same xUnit/Shouldly references as `AtomUI.Core.Tests`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <RootNamespace>AtomUI.Localization.Tests</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Shouldly" />
    <PackageReference Include="xunit.v3" />
    <PackageReference Include="xunit.runner.visualstudio" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../src/AtomUI.Localization/AtomUI.Localization.csproj" />
  </ItemGroup>
</Project>
```

Add the first test before creating `LanguageTag`:

```csharp
using Shouldly;
using Xunit;

namespace AtomUI.Localization.Tests;

public class LanguageTagTests
{
    [Fact]
    public void Parse_Canonicalizes_Language_And_Region_Casing()
    {
        var language = LanguageTag.Parse("EN-us");

        language.Value.ShouldBe("en-US");
        language.ToString().ShouldBe("en-US");
    }
}
```

- [ ] **Step 2: Add project scaffolding without adding production behavior**

Create `AtomUI.Localization.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>$(AtomUITargetFrameworks)</TargetFrameworks>
    <RootNamespace>AtomUI.Localization</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" />
  </ItemGroup>
  <ItemGroup>
    <InternalsVisibleTo Include="AtomUI.Localization.Tests" />
  </ItemGroup>
</Project>
```

Add it as a normal `ProjectReference` from `AtomUI.Core`, and add both new projects to `AtomUI.slnx` in the existing lexical grouping.

- [ ] **Step 3: Restore and verify the first test fails for the missing API**

Run:

```bash
dotnet restore tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj
dotnet test tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
```

Expected: compilation fails with `CS0103`/`CS0246` because `LanguageTag` does not exist. A project file or restore error is not an acceptable RED result.

- [ ] **Step 4: Keep the confirmed failing test uncommitted for the GREEN step**

Do not commit a branch state that cannot build. Task 2 implements the minimal production API, reruns the tests to GREEN, and
commits the project scaffolding, specification test and implementation together.

---

### Task 2: Implement deterministic BCP 47 parsing and canonicalization

**Files:**
- Modify: `tests/AtomUI.Localization.Tests/LanguageTagTests.cs`
- Create: `src/AtomUI.Localization/LanguageTag.cs`
- Create: `src/AtomUI.Localization/Bcp47LanguageTagParser.cs`
- Create: `src/AtomUI.Localization/Bcp47LanguageTagAliases.cs`

**Interfaces:**
- Consumes: raw string or `CultureInfo` only at explicit boundaries.
- Produces: `LanguageTag.Parse(string)`, `TryParse(string?, out LanguageTag)`, `FromCultureInfo(CultureInfo)`, canonical `Value`, ordinal equality/hash, and invalid-default guards.

- [ ] **Step 1: Expand failing tests for the public contract**

Add one focused `[Theory]` for canonical values:

```csharp
[Theory]
[InlineData("en", "en")]
[InlineData("EN-us", "en-US")]
[InlineData("zh-hant-hk", "zh-Hant-HK")]
[InlineData("sr-latn-rs", "sr-Latn-RS")]
[InlineData("en-US-u-ca-gregory", "en-US-u-ca-gregory")]
[InlineData("en-x-ACME", "en-x-acme")]
[InlineData("iw-IL", "he-IL")]
[InlineData("i-klingon", "tlh")]
public void Parse_Returns_Canonical_Bcp47_Tag(string source, string expected)
{
    LanguageTag.Parse(source).Value.ShouldBe(expected);
}
```

Add invalid input tests for `null`, empty/whitespace, underscore, empty subtag, one-character language, incomplete extension/private-use, duplicate extension singleton, and non-ASCII characters. Assert `TryParse` returns `false`; assert `Parse(null!)` throws `ArgumentNullException`, while other invalid strings throw `FormatException` containing the original input.

Add equality/default/CultureInfo tests:

```csharp
[Fact]
public void Canonical_Tags_Have_Ordinal_Value_Equality()
{
    LanguageTag.Parse("EN-us").ShouldBe(LanguageTag.Parse("en-US"));
}

[Fact]
public void Default_Tag_Cannot_Expose_A_Value()
{
    Should.Throw<InvalidOperationException>(() => default(LanguageTag).Value);
}

[Fact]
public void FromCultureInfo_Uses_Ietf_Language_Tag()
{
    LanguageTag.FromCultureInfo(CultureInfo.GetCultureInfo("zh-Hant-TW"))
               .Value.ShouldBe("zh-Hant-TW");
}
```

- [ ] **Step 2: Run tests and confirm RED is caused by the missing parser**

Run the targeted project command. Expected: the tests fail to compile because the new API is absent, or behavioral assertions fail after the first minimal type is introduced. Do not proceed on unrelated restore/test-host failures.

- [ ] **Step 3: Implement `LanguageTag` as an immutable canonical value**

Use a nullable backing field so `default(LanguageTag)` remains detectably invalid:

```csharp
public readonly record struct LanguageTag
{
    private readonly string? _value;

    private LanguageTag(string canonicalValue) => _value = canonicalValue;

    public string Value => _value ?? throw new InvalidOperationException(
        "The default LanguageTag value is invalid.");

    public static LanguageTag Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!Bcp47LanguageTagParser.TryParse(value, out var canonical))
        {
            throw new FormatException($"'{value}' is not a valid BCP 47 language tag.");
        }
        return new LanguageTag(canonical);
    }

    public static bool TryParse(string? value, out LanguageTag language)
    {
        if (value is not null && Bcp47LanguageTagParser.TryParse(value, out var canonical))
        {
            language = new LanguageTag(canonical);
            return true;
        }
        language = default;
        return false;
    }

    public static LanguageTag FromCultureInfo(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return Parse(culture.IetfLanguageTag);
    }

    public override string ToString() => _value ?? string.Empty;
}
```

- [ ] **Step 4: Implement the internal BCP 47 parser**

The parser must tokenize once, reject `_`, whitespace and non-ASCII, and validate this ordered grammar:

```text
language: 2-3 alpha with optional extlang, or 4 alpha, or 5-8 alpha
script: exactly 4 alpha
region: exactly 2 alpha or 3 digits
variant: 5-8 alnum, or digit followed by 3 alnum
extension: unique singleton except x, followed by one or more 2-8 alnum subtags
private-use: x followed by one or more 1-8 alnum subtags
```

Canonicalize language/extlang/variant/extension/private-use to lowercase, script to title case, and region to uppercase.
Check complete grandfathered tags before normal grammar. Apply pinned preferred-value aliases after structural parsing; the first table must cover `iw -> he`, `in -> id`, `ji -> yi`, and `i-klingon -> tlh`, with the table isolated in `Bcp47LanguageTagAliases.cs` so the later IANA data generator can replace it without changing `LanguageTag`.

Use spans/indexes and a `StringBuilder` only for the canonical output. Do not call `string.Split`, `CultureInfo.GetCultures`, regex, or reflection.

- [ ] **Step 5: Run the targeted tests and refactor while green**

Run:

```bash
dotnet test tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
```

Expected: all LanguageTag tests pass with zero failures and no nullable warnings.

- [ ] **Step 6: Commit the parser**

```bash
git add src/AtomUI.Localization/LanguageTag.cs \
  src/AtomUI.Localization/Bcp47LanguageTagParser.cs \
  src/AtomUI.Localization/Bcp47LanguageTagAliases.cs \
  tests/AtomUI.Localization.Tests/LanguageTagTests.cs \
  tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj \
  src/AtomUI.Localization/AtomUI.Localization.csproj \
  src/AtomUI.Core/AtomUI.Core.csproj AtomUI.slnx
git commit -m "feat(Localization): add canonical BCP 47 language tags"
```

---

### Task 3: Add immutable language metadata and state contracts

**Files:**
- Create: `tests/AtomUI.Localization.Tests/LanguageContractsTests.cs`
- Create: `src/AtomUI.Localization/LanguageTextDirection.cs`
- Create: `src/AtomUI.Localization/LanguageDefinition.cs`
- Create: `src/AtomUI.Localization/LanguageState.cs`
- Create: `src/AtomUI.Localization/LanguageChangeResult.cs`
- Create: `src/AtomUI.Localization/LanguageChangedEventArgs.cs`
- Create: `src/AtomUI.Localization/LanguageExceptions.cs`

**Interfaces:**
- Consumes: valid `LanguageTag`, `CultureInfo`, native display name.
- Produces: immutable definitions/state, `Committed`/`NoOp` result model, event payload, and typed public exceptions used by later Manager/Registry plans.

- [ ] **Step 1: Write failing constructor and immutability tests**

Cover these behaviors with separate facts/theories:

```csharp
var definition = new LanguageDefinition(
    LanguageTag.Parse("ar-SA"),
    CultureInfo.GetCultureInfo("ar-SA"),
    "العربية",
    LanguageTextDirection.RightToLeft);

definition.Tag.Value.ShouldBe("ar-SA");
definition.FormattingCulture.Name.ShouldBe("ar-SA");
definition.NativeName.ShouldBe("العربية");
definition.TextDirection.ShouldBe(LanguageTextDirection.RightToLeft);
```

Assert `LanguageDefinition` rejects default tag, null culture, empty/whitespace native name, and undefined direction. Assert it clones the supplied `CultureInfo` as read-only so later caller mutation cannot change committed metadata.

Assert `LanguageState` rejects default tag, null culture, undefined direction and negative revision. Assert `LanguageChangeResult.Committed(oldState, newState)` has `Committed`, while `NoOp(state)` uses the same old/new reference and `NoOp`.

- [ ] **Step 2: Run tests and verify RED**

Expected: compilation fails because the contract types do not exist.

- [ ] **Step 3: Implement validated immutable contracts**

Use a closed byte enum:

```csharp
public enum LanguageTextDirection : byte
{
    LeftToRight = 0,
    RightToLeft = 1
}
```

Implement `LanguageDefinition` and `LanguageState` as sealed records with explicit constructors, validated properties, and
`CultureInfo.ReadOnly((CultureInfo)culture.Clone())`. Do not expose mutable constructor collections.

Define:

```csharp
public enum LanguageChangeStatus : byte
{
    Committed,
    NoOp
}

public sealed record LanguageChangeResult
{
    public LanguageChangeStatus Status { get; }
    public LanguageState OldState { get; }
    public LanguageState NewState { get; }

    public static LanguageChangeResult Committed(LanguageState oldState, LanguageState newState);
    public static LanguageChangeResult NoOp(LanguageState state);
}
```

`LanguageChangedEventArgs` exposes the exact committed result and rejects a `NoOp` result because no event is published for no-op transitions.

Add public exception types with context-bearing constructors:

```text
LanguageConfigurationException : InvalidOperationException
LanguageCatalogException : InvalidOperationException
LanguageCoverageException : InvalidOperationException
LanguageNotSupportedException : ArgumentException
```

Do not add `ILanguageManager` yet; its behavior depends on Snapshot and Provider contracts from the next runtime plan.

- [ ] **Step 4: Run contract tests and adjacent baseline tests**

```bash
dotnet test tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
```

Expected: all new tests pass; existing Core remains 272 passed.

- [ ] **Step 5: Commit the contracts**

```bash
git add src/AtomUI.Localization/LanguageTextDirection.cs \
  src/AtomUI.Localization/LanguageDefinition.cs \
  src/AtomUI.Localization/LanguageState.cs \
  src/AtomUI.Localization/LanguageChangeResult.cs \
  src/AtomUI.Localization/LanguageChangedEventArgs.cs \
  src/AtomUI.Localization/LanguageExceptions.cs \
  tests/AtomUI.Localization.Tests/LanguageContractsTests.cs
git commit -m "feat(Localization): define immutable language state contracts"
```

---

### Task 4: Verify package boundaries and close the foundation batch

**Files:**
- Modify only if verification exposes a documented project-boundary defect: `src/AtomUI.Localization/AtomUI.Localization.csproj`, `src/AtomUI.Core/AtomUI.Core.csproj`, `AtomUI.slnx`

**Interfaces:**
- Consumes: completed Tasks 1-3.
- Produces: a clean, independently buildable package foundation ready for the LanguageTags generator and Catalog/Snapshot plans.

- [ ] **Step 1: Run full targeted verification**

```bash
dotnet test tests/AtomUI.Localization.Tests/AtomUI.Localization.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj \
  --framework net10.0 --no-restore -p:IsTestProject=true
dotnet build src/AtomUI.Localization/AtomUI.Localization.csproj \
  --configuration Release --no-restore
git diff --check
```

Expected: all tests pass, Release builds `net10.0` and `net8.0`, and the diff check is empty.

- [ ] **Step 2: Audit forbidden dependencies and legacy coupling**

```bash
rg -n "ThemeManager|LanguageProvider|Assembly\.GetTypes|GetFields|Activator\.CreateInstance|\.atomlang|XLIFF" \
  src/AtomUI.Localization tests/AtomUI.Localization.Tests
```

Expected: no matches except test names/messages explicitly asserting prohibited input. The new package must not reference `AtomUI.Core`.

- [ ] **Step 3: Review the branch diff**

Confirm:

- `AtomUI.Localization` owns only identity/state foundations.
- `AtomUI.Core` has one-way project dependency and no behavior change.
- No manual `LanguageTags` constants, Catalog placeholders, Provider adapters, dynamic loading, or global Culture mutation were added.
- No unrelated user/worktree files are staged.

- [ ] **Step 4: Commit verification-only fixes if any**

If Step 1 or 2 required a real project-boundary correction, commit only that correction:

```bash
git add AtomUI.slnx src/AtomUI.Localization/AtomUI.Localization.csproj src/AtomUI.Core/AtomUI.Core.csproj
git commit -m "build(Localization): finalize runtime package boundary"
```

Do not create an empty commit when no fix was necessary.

---

## Follow-on Plans

This plan intentionally stops at a complete language identity/state foundation. Continue with separate implementation plans in this order:

1. Generated `LanguageTags` and pinned CLDR/IANA data.
2. Catalog descriptors, Translation Bundles, fallback resolver, Registry, Snapshot, Localizer and LanguageManager.
3. Stable Avalonia LanguageResourceProvider, Markup Extension compatibility and root `IAtomUIBuilder` integration.
4. XLIFF 2.1 Generator/diagnostics and `AtomUI.Build.Tasks`.
5. Common/Controls/Desktop/optional package Catalog migration.
6. Gallery/application migration, static I18n packaging/template and NativeAOT release validation.
