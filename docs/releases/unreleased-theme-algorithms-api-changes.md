# Design language migration guide (unreleased)

[简体中文](unreleased-theme-algorithms-api-changes.zh-CN.md)

This guide covers upgrading from 6.2.15. The theme system is now organised around **design languages**: a language
package (for example `AtomUI.DesignLanguages.AntDesign`) owns the seeds and the global derivation, control packages
only declare their tokens, and a value package per control package and language (for example
`AtomUI.Desktop.Controls.Themes.AntDesign`) writes the control values. Themes are selected by language, style,
appearance and density instead of by a chain of algorithms. The release version is not decided yet; the release owner
folds this file into `docs/releases/<version>-api-changes.md`. See [Writing design languages](../guides/theming/writing-design-languages.md)
(Chinese), [Theme definition XML v2](../reference/theming/theme-definition-xml-v2.md) (Chinese) and
[Compiled theme ABI 4](../reference/theming/compiled-theme-abi-v4.md) (Chinese) for the details.

Ant Design renders exactly as in 6.2.15 apart from the deliberate corrections listed under
[Token additions and corrections](#token-additions-and-corrections).

## Quick reference

| Area | 6.2.15 | Now / migration |
| --- | --- | --- |
| Startup | `UseDesktopControls()` (and ColorPicker / DataGrid) brought the Ant Design themes with it | Enable a language explicitly: `UseAntDesign(language => language.AddDesktopControls())`; without any language startup throws "No design language is enabled; enable one (for example builder.UseAntDesign(...))." |
| Packages | Ant Design formulas inside `AtomUI.Core` and each control token class | New `AtomUI.DesignLanguages.AntDesign`, `AtomUI.Controls.Themes.AntDesign`, `AtomUI.Desktop.Controls.Themes.AntDesign`, `AtomUI.Desktop.Controls.ColorPicker.Themes.AntDesign`, `AtomUI.Desktop.Controls.DataGrid.Themes.AntDesign` |
| `ThemeAlgorithm` and the algorithm chain | `enum` `Default` / `Dark` / `Compact`, `ThemeConfigBuilder.WithAlgorithms`, `<Algorithms>` in theme XML | Removed. Appearance (`ThemeAppearance`), density (`DesignDensity`), style (`DesignStyle`) and idiom (`DesignIdiom`) are independent parameters |
| Calculators | `DefaultThemeVariantCalculator`, `DarkThemeVariantCalculator`, `CompactThemeVariantCalculator`, `AbstractThemeVariantCalculator`, `IThemeVariantCalculator`, `IThemeVariantCalculatorFactory`, `IThemeAlgorithm`, `ThemeAlgorithmAttribute`, `ThemeAlgorithmDescriptor`, `ThemeAppearanceEffect`, `ThemeAlgorithmTypeConverter` | Removed; the Ant Design formulas live in the Ant Design language package |
| Colour utilities | `AtomUI.Theme.Algorithms` (`PaletteGenerator`, `PresetPalettes`, `PresetPrimaryColor`, `PresetColorType`, `PaletteInfo`, `PaletteGenerateOption`, `ColorMap`) | Same types in `AtomUI.Theme.Derivation`; change the `using`. New public `CalculatorUtils`, `FontSizeInfo`, `RadiusInfo` there too |
| `IThemeManager` | `AvailableThemes` | `Themes` (also lists the implicit `$<Language>` default entry of every enabled language); new `Languages`, `CurrentSelection`, `ApplyAsync(ThemeSelection)` |
| `ThemeCatalogReloadResult` | `AvailableThemes` | `Themes` |
| `ThemeInfo` | `(Id, Name, Appearance, IsDefault)` plus `AccentColor` | `(Id, Name, Language)` with `init` properties `Style`, `Appearance` (nullable), `Density`, `LockedSeeds`, `SeedValues` (the theme's own adjustable seed values), `AccentColor`, `IsLanguageDefault`; `IsDefault` is gone |
| `ThemeConfig` / `ThemeConfigBuilder` | `Algorithms`, `WithAlgorithms(...)` | `Language`, `Style`, `Appearance`, `Density`, `Idiom`, `LanguageTokens`; `WithLanguage`, `WithStyle`, `WithAppearance`, `WithDensity`, `WithIdiom`, `WithLanguageToken` |
| `ControlAlgorithmMode` | four control algorithm modes in control configuration | Removed; a control-level value replaces only that value, a control-level seed or axis makes the control its own evaluation unit |
| Theme definition XML | v1 | v2 only; convert once with `dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory> [-p:AtomUIThemeDefinitionOutput=<directory>]` |
| Compiled theme packages | ABI 1 | ABI 4; rebuild packages with the current generator |
| Control token classes | mostly `internal`, values computed in `CalculateTokenValues` | `public partial` declarations only; the values come from the language's value package |
| `SharedTokenResourceValue` | public | removed |

## Enabling a design language

```csharp
// Before (6.2.15)
builder.UseAtomUI(root => root
    .UseDesktopControls()
    .UseDesktopColorPicker()
    .UseDesktopDataGrid());

// After
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls.Themes.AntDesign;   // AddDesktopControls, AddDesktopColorPicker, AddDesktopDataGrid

builder.UseAtomUI(root => root
    .UseDesktopControls()
    .UseDesktopColorPicker()
    .UseDesktopDataGrid()
    .UseAntDesign(language => language
        .AddDesktopControls()
        .AddDesktopColorPicker()
        .AddDesktopDataGrid()));
```

- `UseDesktopControls()`, `UseDesktopColorPicker()` and `UseDesktopDataGrid()` enable the control packages, as before.
- `UseAntDesign(...)` enables the language and its official theme (Daybreak Blue). The `Add*()` methods register the
  value packages only; they do not enable the control packages.
- Every enabled control package needs a value package for every enabled language. A missing one fails at startup with
  a message that names the missing value package.

## Selecting a theme

```csharp
// Before
await themeManager.ApplyThemeAsync(new ThemeRequest("DaybreakBlue", config, ThemeTransitionReason.UserRequest));

// After: by language and variant
await themeManager.ApplyAsync(
    ThemeSelection.Of("AntDesign")
        .WithAppearance(ThemeAppearance.Dark)
        .WithDensity(DesignDensity.Compact)
        .WithSeed("ColorPrimary", "#1677FF"));
```

- `ThemeSelection` carries `Language`, `Style`, an optional named `ThemeId`, `Appearance` (or `FollowSystemAppearance()`),
  `Density`, global `Seeds` and `LanguageSeeds`. `ThemeSelection.Of<TLanguage>()` takes the language type instead of the id.
- `IThemeManager.Languages` lists each enabled language as a `DesignLanguageInfo` (`Id`, `Styles`, `DefaultStyle`,
  `Densities`, `Idioms`, `AdjustableSeeds`); build language, style, density and brand-colour menus from it.
- Every enabled language has an implicit catalog entry `$<Language>` (`ThemeInfo.LanguageDefaultId`, flagged
  `IsLanguageDefault`), so a language is selectable without a theme file.
- An unknown language, theme or axis value throws `ThemeSelectionException`.
- `ApplyThemeAsync(ThemeRequest)` applies a theme request with a full runtime configuration (control-level values
  included). `IThemeManager.DEFAULT_THEME_ID` is removed: core names no language's theme; use
  `AntDesignThemes.DaybreakBlue` from the Ant Design language package.
- `IThemeManagerBuilder.WithInitialTheme(ThemeSelection)` sets the startup selection.

## Theme configuration

`ThemeConfig.Algorithms` and `ThemeConfigBuilder.WithAlgorithms` are removed. Write the variant directly:

```csharp
// Before
new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark, ThemeAlgorithm.Compact).Build();

// After
new ThemeConfigBuilder()
    .WithAppearance(ThemeAppearance.Dark)
    .WithDensity(DesignDensity.Compact)
    .Build();
```

Each axis inherits independently from the outer scope; an inner scope no longer replaces the whole chain. Explicit
values are fixed before any derivation runs, so a value a theme or configuration supplies is never overwritten and the
values derived from it follow it.

## Theme definition XML v2

Theme files use format v2. The four files of one language and style (light, dark, compact, dark compact) become one
file with `<When>` sections. `<Algorithms>`, `IsDefault`, `DesignLanguage`, `BrandColor` and `NeutralColor` are gone:
`Language` and `Style` on `<Theme>` group the theme, and `Locked` on a token says what users may not replace. v1 files
are not read; a v1 file in a user theme directory is reported as `ATMTHM1004` with the conversion command, and the
directory does not take effect until it is converted. Convert once:

```bash
dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory>
```

By default the command rewrites in place and deletes the merged variant files; pass
`-p:AtomUIThemeDefinitionOutput=<directory>` to write elsewhere. See [Theme definition XML v2](../reference/theming/theme-definition-xml-v2.md)
section 14.

## Compiled theme packages

The generator emits compiled theme ABI 4, which records the language a package requires (manifest scope `language`).
Packages built for ABI 1 do not load; rebuild them.

## Control token classes and third-party control packages

All control token classes of `AtomUI.Controls`, `AtomUI.Desktop.Controls`, ColorPicker and DataGrid are `public
partial` and contain declarations only. `CalculateTokenValues` and value-setting token constructors are gone; the Ant
Design values moved, unchanged, into `[ControlDerivation]` classes in the Ant Design value packages.

A third-party control package declares its tokens and ships one value package per language it supports:

```csharp
// Control package: declarations only
[ControlDesignToken]
public sealed partial class RatingToken : AbstractControlDesignToken
{
    public partial double StarSize { get; set; }
}

// Value package (for example Acme.Controls.Themes.AntDesign)
[ControlDerivation]
internal sealed class AntDesignRatingDerivation : IControlDerivation<AntDesignToken, RatingToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<RatingToken> view)
    {
        view.Token.StarSize = view.Global.ControlHeight * 0.625;
    }
}
```

- A token class or token property that is not `partial` reports `ATOMUIGEN049`.
- A derivation must write every token of its class (`ATOMUIGEN050`) and may not read a token before writing it
  (`ATOMUIGEN051`). Writes to `view.Global` are control-scoped overrides of Map and Alias tokens;
  a seed write throws.
- The control package gets `[assembly: TokenSchemaDigest]` and the value package `[assembly: CoversTokenSchema]` from the
  generator. At startup a value package built against a different version of the control package's tokens is
  rejected with both package names and both digests.
- See [Writing design languages](../guides/theming/writing-design-languages.md) section 4 and
  [Third-party control packages](../guides/theming/third-party-control-packages.md).

## Writing a design language

A language is a class derived from `DesignLanguage<TLanguageToken>` with `[DesignLanguage]`, declaring its `Axes`
(`DesignLanguageAxes`: styles, appearances, densities, idioms, adjustable seeds) and implementing `DefineSeeds`,
`DeriveLanguage` and `DeriveGlobal`. Its language tokens derive from `LanguageDesignToken` with
`[LanguageDesignToken]`. Value packages implement `IControlDerivation<TLanguageToken, TToken>`; the lookup walks the
language token's inheritance chain, so a language derived from another language inherits its control values and
overrides only what differs. The contract test library `AtomUI.Theme.ContractTests` (`DesignLanguageContract.Verify`)
checks identity, completeness, purity, determinism and readability for every variant.

New public types: `DesignLanguage`, `DesignLanguage<TLanguageToken>`, `DesignLanguageAttribute`, `DesignLanguageAxes`,
`DesignLanguageBuilder<TLanguage>`, `DesignStyle`, `DesignDensity`, `DesignIdiom`, `DesignVariant`,
`DerivationContext<TLanguageToken>`, `ControlView<TToken>`, `IControlDerivation<TLanguageToken, TToken>`,
`ControlDerivationAttribute`, `LanguageDesignToken`, `LanguageDesignTokenAttribute`, `ReservedTokenAttribute`,
`IControlToken`, `DesignLanguageDescriptor`, `ControlDerivationDescriptor`, `GlobalTokenSchema`,
`TokenSchemaDigestAttribute`, `CoversTokenSchemaAttribute`, `ThemeBuildLanguageAttribute`, `ThemeDerivationException`,
`DesignLanguageInfo`, `ThemeSelection`, `ThemeSelectionResult`, `ThemeSelectionResolver`, `ThemeSelectionException`.
`IThemeManagerBuilder` gains `AddDesignLanguage` and `AddControlDerivations` (called by generated registration code;
only custom implementations of the interface need them) and `WithInitialTheme`.

## Shadcn and PrimeNG

`AtomUI.Themes.Shadcn` and `AtomUI.Themes.PrimeNG` (never published to NuGet) are replaced by the design languages
`Shadcn` and `PrimeNG`, built the same way as Ant Design:

| Language | Packages | Styles | Adjustable seeds | Entry point |
| --- | --- | --- | --- | --- |
| `Shadcn` | `AtomUI.DesignLanguages.Shadcn`, `AtomUI.Controls.Themes.Shadcn`, `AtomUI.Desktop.Controls.Themes.Shadcn`, `AtomUI.Desktop.Controls.ColorPicker.Themes.Shadcn`, `AtomUI.Desktop.Controls.DataGrid.Themes.Shadcn` | `Vega`, `Nova` (default), `Maia`, `Lyra`, `Mira`, `Luma`, `Sera`, `Rhea` | `ColorPrimary`, language seed `BaseColor` | `UseShadcn(language => language.AddDesktopControls())` |
| `PrimeNG` | `AtomUI.DesignLanguages.PrimeNG`, `AtomUI.Controls.Themes.PrimeNG`, `AtomUI.Desktop.Controls.Themes.PrimeNG`, `AtomUI.Desktop.Controls.ColorPicker.Themes.PrimeNG`, `AtomUI.Desktop.Controls.DataGrid.Themes.PrimeNG` | `Aura` (default), `Lara`, `Nora` | `ColorPrimary` | `UsePrimeNG(language => language.AddDesktopControls())` |

The languages ship no named themes: the former profile ids (`ShadcnNova`, `ShadcnNova.Dark`,
`PrimeNGAura.DarkCompact`, …) are gone. Select `ThemeSelection.Of("Shadcn", new DesignStyle("Nova"))
.WithAppearance(ThemeAppearance.Dark)` and so on; the language's own entry is `$Shadcn` / `$PrimeNG`. Every variant and
seed case compiles to the same token values as the former profiles. The package READMEs list the rules, their sources
and what is not supported.

## Token additions and corrections

New tokens; their defaults keep the 6.2.15 Ant Design rendering unless stated otherwise:

| Token | Meaning |
| --- | --- |
| `DesignToken.BorderRadiusMD` (Map) | the medium radius (default size), equal to the seed `BorderRadius`; controls read it instead of the seed |
| `DesignToken.InputActiveBorderColor` (Alias) | border of a focused input; defaults to `ColorPrimary` |
| `AbstractButtonVariantToken.LinkColor`, `DangerAccentColor` | the link button's resting colour (default `ColorLink`) and the colour the danger variants are built from (default `ColorError`) |
| `AlertToken.SuccessIconColor`, `InfoIconColor`, `WarningIconColor`, `ErrorIconColor`; `MessageCardToken.LoadingIconColor` | status icon colours (defaults: the status seeds; the info and loading icons `ColorPrimary`) |
| `TagToken.SuccessColor`, `InfoColor`, `WarningColor`, `ErrorColor` | main colour of a status tag (defaults: the status seeds) |
| `ToolTipToken.ToolTipFontSize`, `ToolTipMinHeight` | tooltip text size and minimum height (defaults `FontSize`, `ControlHeight`) |
| `DesignToken.ControlItemColorActive` (Alias) | text colour on `ControlItemBgActive`; defaults to `ColorText`. The selected-text tokens of Select, Cascader, ComboBox, ListBox, ListView, TransferListView, TreeView, TransferTreeView, TreeSelect and the new `DataGridToken.RowSelectedColor` default to it |
| `AbstractCalendarToken.ItemActiveColor`, `AbstractDateViewerToken.CellActiveColor` | primary text on the selected cell; default `ColorPrimary` |
| `ArrowDecoratedBoxToken.ContentBorderColor`, `ContentBorderThickness`, `ContentMinHeight` | border and minimum height of arrow boxes; a non-zero border now draws one outline around box and arrow instead of hiding the arrow |
| `ArrowDecoratedBoxToken.ArrowTipRadius`, `ArrowBaseRadius` (also on `ToolTipToken`) | rounded arrow geometry |
| `ProgressBarToken.InnerLabelColor` | dark candidate of the inner percentage label; default `ColorTextLabel` |
| `ToolTipToken.ToolTipShadows`, `DrawerToken.EdgeBorderColor`, `EdgeBorderThickness` | shadow and edge border |
| `AbstractButtonGeometryToken.RecipeVariants`, `OutlineOnlyDefaultShadow`, `DisabledByOpacity`, `DisabledOpacity`, `PressMoves`, `UnderlineLinkOnHover`, `UnderlineLinkAtRest`; `SplitButtonToken.InsetSeparator`; `TransparentOutlineBorder`, `BottomOutlineBorder` on `LineEditToken` / `TextAreaToken` / `SelectToken`; `SelectToken.MutedSelectIndicator`, `OptionBackgroundInset`, `OptionBorderRadius`; `FloatButtonToken.HasOutline`, `OutlineOnlyShadow`, `CapsuleDescription`, `UppercaseDescription`, `DescriptionWeight`, `OutlineDescriptionLineHeight`; `OtpLineEditToken.JoinCells`; `AbstractTimePickerToken.CenterItemContent`, `ItemBackgroundInset` | behaviour switches that languages set; the defaults are the 6.2.15 behaviour |

Deliberate visual corrections in Ant Design:

- `ArrowIndicator` follows the antd rounded-arrow construction (a right-angled isosceles triangle with an arc tip of
  `ArrowTipRadius` and concave base arcs of `ArrowBaseRadius`); `ArrowDecoratedBoxToken.ArrowSize` defaults to
  `SizePopupArrow` instead of `SizePopupArrow / 1.3`. The arrow shape changes slightly.
- The ToolTip theme sets its arrow box's `BorderThickness` to 0, and PopupConfirm reads its gap to the anchor from
  `FlyoutHostToken.MarginToAnchor` (same default).
- An explicit global Alias value now drives the Alias tokens derived from it. In 6.2.15 explicit Alias values were
  written after the Alias formulas, so for example an explicit `ControlOutlineWidth` did not reach `InputActiveShadow`.
  Themes that set none of `ControlOutlineWidth`, `ColorControlOutline`, `ColorErrorOutline`, `ColorWarningOutline`,
  `LineWidthFocus`, `ControlPaddingHorizontal`, `ControlPaddingHorizontalSM` or `UniformlyPaddingSM` compile exactly as
  before. A control-level Alias value replaces only that value and does not re-derive other tokens of the control.

## Diagnostics

| Id | Change |
| --- | --- |
| `ATOMUIGEN049` | New (Error): a token class or token property is not `partial` |
| `ATOMUIGEN050` | New (Error): a control derivation leaves a token unwritten |
| `ATOMUIGEN051` | New (Error): a control derivation reads a token before writing it |
| `ATOMUIGEN052` | New (Error): a value package misses the derivation of a control token class it covers |
| `ATOMUIGEN053` | New (Error): package dependency direction violated (a language package references a control package, a control package references the language side, or a value package declares controls) |
| `ATOMUIGEN024` | Removed with `CalculateTokenValues` |
| `ATMTHM1004` | A v1 theme file; the message names the conversion command |

The theme XML v2 reader and binder diagnostics are listed in [Theme definition XML v2](../reference/theming/theme-definition-xml-v2.md)
section 10.

## Performance

Measured on 2026-11-22 against the same tree without the change (default Ant Design theme, Release, 30 samples, two
alternating rounds): a cold theme compilation allocates 712,792 instead of 1,202,392 bytes (-41%), and its median time
drops from 2.44 to 1.92 ms. The controls of one scope now share the global view their derivations start from unless a
derivation overrides a global token, and a local scope that overrides only Map or Alias tokens reuses the parent's
result for every control whose derivation did not read them. Compiled values are identical; light/dark switching is
unchanged within the machine's resolution.
