# Theme algorithm strategy migration guide (unreleased)

[简体中文](unreleased-theme-algorithms-api-changes.zh-CN.md)

This guide covers the theme algorithm changes on top of 6.2.14. The theme algorithm is now a pluggable strategy:
algorithm identities are open `(Catalog, Id)` pairs instead of a closed enum, theme packages and applications can
register their own algorithms, and an algorithm can compute Alias tokens and per-control tokens as well as Map
tokens. `AtomUI.Themes.PrimeNG` is the first package built this way. The release version is not decided yet; the
release owner folds this file into `docs/releases/<version>-api-changes.md`. See the [Changelog](../../CHANGELOG.md)
for release highlights and [Writing theme algorithms](../guides/theming/writing-theme-algorithms.md) (Chinese) for
the author's guide.

## Quick reference

| Area | 6.2.14 | Now / migration |
| --- | --- | --- |
| `ThemeAlgorithm` | `enum` (`Default = 0`, `Dark = 1`, `Compact = 2`) | `readonly struct` identity `(Catalog, Id)`; `ThemeAlgorithm.Default` / `Dark` / `Compact` keep their names. Binary incompatible; `switch`/`case`, `(int)` casts, `const`, `Enum.*` and `default(ThemeAlgorithm)`-as-Default no longer work; compare with `==` |
| `ThemeAlgorithmAttribute` | `(ThemeAlgorithm algorithm, int revision, ThemeAppearanceEffect appearanceEffect)` | `(string catalog, string id, int revision, ThemeAppearanceEffect appearanceEffect)` |
| `ThemeAlgorithmDescriptor` | constructor and `Algorithm` typed as the enum | same shape, typed as the struct; rejects `default(ThemeAlgorithm)` |
| `DefaultThemeVariantCalculator.Algorithm`, `DarkThemeVariantCalculator.Algorithm`, `CompactThemeVariantCalculator.Algorithm` | `public const ThemeAlgorithm` | removed; use `ThemeAlgorithm.Default` / `Dark` / `Compact` |
| `ThemeAlgorithmTypeConverter` (AXAML) | `DefaultAlgorithm`, `DarkAlgorithm`, `CompactAlgorithm` | `Default`, `Dark`, `Compact` or `Catalog:Id`; the `...Algorithm` spellings are rejected |
| `IThemeManagerBuilder` | — | new `AddThemeAlgorithms`, `AddControlTokenAlgorithms`; custom implementations of the interface must add them |
| New algorithm API | — | `IThemeAliasAlgorithm`, `IControlTokenAlgorithm<TToken>`, `ControlTokenAlgorithmAttribute`, `ThemeAlgorithmContext`, `ThemeAlgorithmContractException`, `ThemeAlgorithmContractViolationKind`, `IControlToken`, `ControlTokenAlgorithmDescriptor`, `ThemeBuildAlgorithmAttribute`; no action |
| Generated `<Catalog>Algorithms` | — | one public static class per algorithm catalog in a package (e.g. `PrimeNGAlgorithms.AuraDark`); no action |
| Control token classes of `AtomUI.Controls`, `AtomUI.Desktop.Controls`, ColorPicker, DataGrid | 148 `internal`, 2 `public` | all 150 `public partial` (117 terminal, 33 abstract); token properties are partial properties. Larger public surface; abstract layers can be inherited and their `protected` helpers are public API |
| Third-party control token classes | ordinary class, auto-properties, `public override CalculateTokenValues` | `partial` class, `partial` properties (`ATOMUIGEN049` otherwise), `protected override CalculateTokenValues` (CS0507 otherwise) |
| `DesignToken` global token properties (255) | auto-properties | partial properties; no change for callers |
| `AbstractControlDesignToken.CalculateTokenValues` | `public virtual` | `protected virtual`; cannot be called from outside |
| `AbstractDesignToken` | — | new `protected void RecordWrite(string)` called by the generated setters; do not call it yourself |
| Theme XML `<Algorithm>` | `Id` is `Default`, `Dark` or `Compact` | optional `Catalog` (default `AtomUI`); `Id` accepts any identifier. Existing files stay valid |
| Explicit global Alias values | written after the Alias formulas; derived Alias tokens ignored them | drive the Alias tokens derived from them (a fix). Themes that set Alias tokens explicitly can compile differently |
| Compiled theme input (build-tool intermediate) | format version 4, algorithms as integers | version 5, algorithms as `Catalog`/`Id`; stale intermediates fail with "Compiled theme input format is out of date; rebuild the project." |
| PrimeNG presets | XML value tables with Ant-derived fill-ins | produced by six PrimeNG algorithms; visible colour changes (see below) |

## `ThemeAlgorithm` becomes an identity struct

`ThemeAlgorithm` is now:

```csharp
public readonly struct ThemeAlgorithm : IEquatable<ThemeAlgorithm>
{
    public ThemeAlgorithm(string catalog, string id); // both validated as theme schema identifiers
    public string Catalog { get; }
    public string Id { get; }
    public static ThemeAlgorithm Default { get; } // AtomUI:Default
    public static ThemeAlgorithm Dark { get; }    // AtomUI:Dark
    public static ThemeAlgorithm Compact { get; } // AtomUI:Compact
}
```

Equality is ordinal on `Catalog` and `Id`; `ToString()` returns `Catalog:Id` (`AtomUI:Dark`, previously `Dark`).

The most common call sites compile unchanged:

```csharp
new ThemeConfigBuilder()
    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark)
    .Build();
```

Rewrite code that relied on the enum:

```csharp
// Before
switch (algorithm)
{
    case ThemeAlgorithm.Dark:
        ...
}
var code = (int)algorithm;
const ThemeAlgorithm fallback = ThemeAlgorithm.Default;

// After
if (algorithm == ThemeAlgorithm.Dark)
{
    ...
}
var key = algorithm.ToString();                       // "AtomUI:Dark"
static readonly ThemeAlgorithm Fallback = ThemeAlgorithm.Default;
```

`default(ThemeAlgorithm)` no longer means `Default`: it has a null `Catalog`. The configuration normalizer reports
it as `ATMTHM4004` ("Algorithm identity is uninitialized."), and the descriptor constructors throw
`ArgumentException` ("Theme algorithm identity is uninitialized."). Persisted integer values cannot be converted
back; persist `Catalog` and `Id` instead.

Assemblies compiled against 6.2.14 that use `ThemeAlgorithm` (including generated compiled theme programs, which
used to emit `(ThemeAlgorithm)1`) must be rebuilt.

## Declaring an algorithm

```csharp
// Before
[ThemeAlgorithm(ThemeAlgorithm.Dark, 1, ThemeAppearanceEffect.Dark)]

// After
[ThemeAlgorithm("Acme", "Brand.Dark", 1, ThemeAppearanceEffect.Dark)]
```

Use your own catalog; `AtomUI` is the catalog of the three built-in algorithms. Hand-built descriptors keep the
same constructor shape with the struct: `new ThemeAlgorithmDescriptor(new ThemeAlgorithm("Acme", "Brand"), 1,
ThemeAppearanceEffect.Light, static () => new AcmeBrandAlgorithm())`.

The `public const ThemeAlgorithm Algorithm` fields of `DefaultThemeVariantCalculator`, `DarkThemeVariantCalculator`
and `CompactThemeVariantCalculator` are removed. Replace `DefaultThemeVariantCalculator.Algorithm` with
`ThemeAlgorithm.Default` (likewise `Dark`, `Compact`).

## AXAML type converter spellings

`ThemeAlgorithmTypeConverter` now accepts `Default`, `Dark`, `Compact` and `Catalog:Id`. Replace
`DefaultAlgorithm`, `DarkAlgorithm` and `CompactAlgorithm` in AXAML with `Default`, `Dark` and `Compact`.

## `IThemeManagerBuilder` gains two members

```csharp
IThemeManagerBuilder AddThemeAlgorithms(IEnumerable<ThemeAlgorithmDescriptor> algorithms);
IThemeManagerBuilder AddControlTokenAlgorithms(IEnumerable<ControlTokenAlgorithmDescriptor> algorithms);
```

Generated `GeneratedThemeAlgorithmRegistration.Register(builder)` calls them. Only code that implements
`IThemeManagerBuilder` itself (for example a test double) needs to add the members.

## New algorithm stages

An algorithm now has three stages. The Map stage (`IThemeAlgorithm.Evaluate`) is unchanged. Two optional stages are
new:

| Type | Purpose |
| --- | --- |
| `IThemeAliasAlgorithm : IThemeAlgorithm` | `EvaluateAlias(in ThemeAlgorithmContext, DesignToken)` writes Alias-stage global tokens, on the same class as the Map stage |
| `IControlTokenAlgorithm<TToken>` | `EvaluateGlobal` writes control-scoped global tokens, `EvaluateOwn` writes the control's own tokens; one class per control |
| `ControlTokenAlgorithmAttribute(catalog, id, revision)` | links a control stage class to an algorithm; repeatable |
| `ThemeAlgorithmContext` | `Appearance` of the whole chain and the chain's `Algorithms` |
| `ThemeAlgorithmContractException`, `ThemeAlgorithmContractViolationKind` | thrown when a stage writes outside its stage, does not stabilize, or throws |
| `IControlToken`, `ControlTokenAlgorithmDescriptor` | generated control identity of a token class; reflection-free control stage descriptor |
| `ThemeBuildAlgorithmAttribute` | build contract that carries algorithm identities to referencing assemblies |

Priority is fixed: Ant default formulas < algorithm < explicit theme values < explicit values of an inner scope.
Built-in themes compile to the same values as before. Nothing needs to change unless you write algorithms; see
[Writing theme algorithms](../guides/theming/writing-theme-algorithms.md).

## Control token classes become public partial classes

In `AtomUI.Controls`, `AtomUI.Desktop.Controls`, `AtomUI.Desktop.Controls.ColorPicker` and
`AtomUI.Desktop.Controls.DataGrid`, 148 control token classes (116 terminal classes and 32 abstract layers) became
public; with the two that were already public (`ColorPickerToken` and `AbstractColorPickerToken`), all 150 (117
terminal, 33 abstract) are now `public partial`, so theme packages can write them from control stages.
`AtomUI.Toolkits.GalleryBase` keeps its 8 token classes `internal`. The token names and types were already public
through `XxxTokenKind` and the `ThemeBuildToken` attributes; what is new is the types themselves.

The public abstract layers (for example `AbstractButtonGeometryToken`) can now be inherited by other assemblies.
Their `protected` members, such as `CalculateContentPadding`, are part of the public API from now on.

Third-party token classes must follow the new shape:

```csharp
// Before
[ControlDesignToken]
internal sealed class RatingToken : AbstractControlDesignToken
{
    public double StarSize { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        StarSize = EffectiveGlobalToken.ControlHeight;
    }
}

// After
[ControlDesignToken]
public sealed partial class RatingToken : AbstractControlDesignToken
{
    public partial double StarSize { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        StarSize = EffectiveGlobalToken.ControlHeight;
    }
}
```

- The generator emits the other half of every partial token property (`get => field;` and a setter that calls
  `RecordWrite`). A token class or property that is not partial reports `ATOMUIGEN049` (Error) and gets no
  implementation. Property initializers can stay on the declaration.
- `AbstractControlDesignToken.CalculateTokenValues` is now `protected virtual`. `public override` reports CS0507.
  Code outside the token class can no longer call it.
- The class may stay `internal`; then only its own assembly can declare control stages for it.

The 255 global token properties of `DesignToken` are partial properties too; callers are unaffected.

## Explicit global Alias values now drive derived Alias tokens

Before, explicit Alias values were written after `CalculateAliasTokenValues`, so Alias tokens derived from them kept
the formula values. Now explicit Alias values are written and marked before the formulas and again afterwards. The
affected derivations:

| Explicit token | Now drives |
| --- | --- |
| `ControlOutlineWidth` | `InputActiveShadow`, `InputErrorActiveShadow`, `InputWarningActiveShadow` (spread) |
| `ColorControlOutline` | `InputActiveShadow` (colour) |
| `ColorErrorOutline` | `InputErrorActiveShadow` (colour) |
| `ColorWarningOutline` | `InputWarningActiveShadow` (colour) |
| `LineWidthFocus` | `FocusVisualBorderThickness` |
| `ControlPaddingHorizontal`, `ControlPaddingHorizontalSM`, `UniformlyPaddingSM` | `InputPaddingLG`, `InputPaddingSM`, `InputPadding` |

Themes that set none of these compile exactly as before (`Default`, `Dark`, `Compact` and DaybreakBlue are
unchanged). Among the preset packages, Bootstrap and the PrimeNG profiles changed: for example the PrimeNG Aura focus
shadow now has a spread of 0 because its `ControlOutlineWidth` is 0, and Bootstrap's input focus shadow is
`0 0 0 3 #6fc8ff` instead of `0 0 0 2 #190591ff` (the Bootstrap preset also moved to Bootstrap 6.0 values; Dark is
`0 0 0 3 #0064ba`). If you relied on the old result, set the derived token
explicitly too. The analyzers `ATOMUIGEN040` and `ATOMUIGEN048` now also check `DesignToken.CalculateAliasTokenValues`.

## Theme XML: `Catalog` on `<Algorithm>`

```xml
<Algorithms>
  <Algorithm Catalog="PrimeNG" Id="Aura.Dark" />
  <Algorithm Id="Compact" />
</Algorithms>
```

`Catalog` is optional and defaults to `AtomUI`; existing files are unchanged. The XSD type of `Id` is the union of
the three built-in values and `Identifier`. An invalid identifier fails in the reader; a valid but unregistered
identity fails in the binder (`ATMTHM2001`) or, for compiled theme packages, at build time (`ATOMUITOKEN002`,
"Unknown algorithm 'Catalog:Id'."). A repeated identity in one chain reports `ATMTHM1002`.

## Compiled theme input format version 5

The neutral intermediate file that the Toolchain task passes to the generator moves from format version 4 to 5:
algorithm entries carry `Catalog`, `Id`, line and column instead of an integer. It is a private build-tool protocol;
the generated runtime ABI stays v1. An intermediate left over from an older build fails with
"Compiled theme input format is out of date; rebuild the project." Rebuild (or clean) the project.

## New and extended diagnostics

| Id / exception | Change |
| --- | --- |
| `ATOMUIGEN049` (Error) | New: a `[ControlDesignToken]` or `[GlobalDesignToken]` token property or its class is not partial |
| `ATOMUIGEN040`, `ATOMUIGEN048` | Scope extended to `DesignToken.CalculateAliasTokenValues` (Alias properties, `IsExplicit(SharedTokenKind.X)` guards); messages now name the Kind enum and the method |
| `ATOMUITOKEN002` | New triggers: unknown algorithm in theme XML, invalid or duplicate algorithm build contracts, colliding `<Catalog>Algorithms` member names, invalid `[ControlTokenAlgorithm]` classes, invalid `[ThemeAlgorithm]` declarations (not a non-abstract, non-generic `IThemeAlgorithm` class; no public parameterless constructor; invalid catalog, id, revision or appearance effect), which used to be skipped silently |
| `ATMTHM1002` | New reader messages: "Theme algorithm 'Catalog:Id' is duplicated." (duplicate detection moved from the XSD to the reader) and, only when the XSD has not already reported the element, "Theme algorithm identity 'Catalog:Id' is not a valid identifier." |
| `ATMTHM4004` | New message: "Algorithm identity is uninitialized." |
| `ThemeSchemaException` | "Duplicate algorithm identity 'Catalog:Id'." and "Duplicate control algorithm '{algorithm}' for control '{control}'." |
| `ThemeLoadException` | Runtime package append: "Package append cannot add a control algorithm for the already registered algorithm '{algorithm}' and control '{control}'." |
| `ThemeAlgorithmContractException` | `StageBoundary`, `Unstable` or `Failed` violation of an optional algorithm stage; also `Failed` when an algorithm's Map stage throws (built-in algorithms included). A control-stage violation is thrown when that control is first read, not at compile time |

## PrimeNG presets are produced by algorithms

`AtomUI.Themes.PrimeNG` (not yet published to NuGet) now registers six algorithms, `PrimeNG:Aura`,
`PrimeNG:Aura.Dark`, `PrimeNG:Lara`, `PrimeNG:Lara.Dark`, `PrimeNG:Nora` and `PrimeNG:Nora.Dark`, with Alias and
control stages. The twelve profile IDs are kept; each XML file now holds the chain and Seed tokens (primary, error,
radius, control height; Nora also `EnableMotion`), and only the two Lara compact profiles keep one Select override. Visible
changes:

- `ColorPrimary`, `ColorError` and the other status colours in a PrimeNG theme are seeds (the 500 stop of their
  scale); the algorithm maps them to the displayed stop. Changing the primary seed now also moves hover, active,
  focus ring and selected backgrounds.
- Dark primary and error colours change, because the old dark profiles ran `AtomUI:Dark` over values that were
  already final:

  | Profiles | `ColorPrimary` | `ColorError` |
  | --- | --- | --- |
  | `PrimeNGAura.Dark`, `PrimeNGAura.DarkCompact` | `#2FB685` → `#34D399` | `#D98F8F` → `#FCA5A5` |
  | `PrimeNGLara.Dark`, `PrimeNGLara.DarkCompact` | `#2FB685` → `#34D399` | `#D98F8F` → `#FCA5A5` |
  | `PrimeNGNora.Dark`, `PrimeNGNora.DarkCompact` | `#11A071` → `#10B981` | `#D66363` → `#F87171` |

- Colours that used to be derived by the Ant formulas now follow the PrimeNG scales: the hover, active, background,
  border and text states of the success, warning, info and error families; link colours (stops of the primary
  seed's scale; in light the links take deeper stops than the primary fill, 700 / 800 / 900, so link text stays
  readable on white); neutral fills, solid and spotlight backgrounds; tertiary, quaternary and disabled text; the contrast
  foreground on solid primary (dark: neutral 900, Nora dark: neutral 950). Nora also moves its success, warning and
  info base colours to the 600 stop (dark 500). Fills that were translucent are now opaque scale colours.
- Beyond the Alias-derivation fix above, the algorithm migration changed no size, padding, radius, font, line width,
  motion or shadow geometry value.
- `[PrimeNG:Aura.Dark]` equals the `PrimeNGAura.Dark` profile, and appending `AtomUI:Compact` equals the
  `.Compact` profile (Lara compact keeps one listed difference). Appending `AtomUI:Dark` to a light PrimeNG chain is
  a hybrid chain and is not the PrimeNG dark theme.

The package README lists every rule and its source.

## Performance

Token write recording adds a null check to every generated token setter and 8 bytes to every token instance. It was
measured against the tree before the change (2026-11-13, default Ant theme): cold theme compilation is 2.3%–2.7%
slower by the gated statistic (25th percentile, about 0.018 ms) and 3.4%–3.7% slower by median. The startup check
failed its literal rule (most out-of-range values were on the faster side; on the slower side the plain-window
prepare phase exceeded the baseline maximum by 0.005 ms and the splash commit phase by 0.17 ms). Theme switching,
allocations, assembly and NativeAOT sizes and compiled values passed. This misses the design's 2% threshold; the
project owner accepted the result as an explicit relaxation. Rendering and resource lookup do not go through token
properties and are unaffected.

Everything added after that decision (the Alias and control stages) was re-measured on 2026-11-14 against commit
`3a87bba25`, the tree at which the recording cost was accepted. For the built-in Default, Dark and Compact chains,
cold compilation (−0.87% / −1.24% / −1.21%, 25th percentile) and theme switching are within noise, and allocation per
cold compile (1,138,032 / 1,143,224 / 1,138,416 bytes) and compiled values are unchanged; the machine resolved cold
compilation only to 8%–9% that day, so a 2% difference could not be detected. About 1 ms of extra startup time, caused
by jitting the control-stage registry even when it was empty, was found and fixed before release; startup now jits the
same methods as the baseline and is unchanged within the machine's resolution of about 1 ms (applications that register
control stages were not measured). `AtomUI.Core.dll` grew by 2.05% (1,125,888 → 1,148,928 bytes), which is above the
plan's 1% assembly-size gate; the gate was not met, and the project owner accepted this growth on 2026-11-14 as an
explicit relaxation limited to this assembly. Other gated assemblies grew by at most 0.10%, and the NativeAOT
Gallery executable is 0.09% smaller. A theme that uses the stages pays for them: `PrimeNGAura` compiles in about
0.88 ms versus about 0.74 ms for the built-in default theme (+18%) and allocates 1,249,256 versus 1,138,032 bytes
(+9.8%); `PrimeNGAura.Dark` takes about 0.89 ms (+20%) and 1,254,568 bytes (+10.2%).
