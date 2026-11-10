# AtomUI.Themes.PrimeNG

Compiled Aura, Lara and Nora presets for AtomUI desktop controls. Register the package once; the default remains `DaybreakBlue`.

```csharp
using AtomUI.Desktop.Controls;
using AtomUI.Themes.PrimeNG;
using AtomUI.Theme;

appBuilder.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.Theme.UsePrimeNGThemes();
    builder.Theme.WithInitialTheme("PrimeNGAura");
});
```

The package references `AtomUI.Desktop.Controls` because its generated theme program uses typed control identities and Own Token keys. XML v1 files are compiled at build time; application startup does not parse XML or scan assemblies. Registering this package does not enable desktop controls automatically.

## Explicit appearance and density profiles

Each of the three preset families has four compiled IDs:

| Family | Light | Dark | Light compact | Dark compact |
|---|---|---|---|---|
| Aura | `PrimeNGAura` | `PrimeNGAura.Dark` | `PrimeNGAura.Compact` | `PrimeNGAura.DarkCompact` |
| Lara | `PrimeNGLara` | `PrimeNGLara.Dark` | `PrimeNGLara.Compact` | `PrimeNGLara.DarkCompact` |
| Nora | `PrimeNGNora` | `PrimeNGNora.Dark` | `PrimeNGNora.Compact` | `PrimeNGNora.DarkCompact` |

Select a profile ID with `WithInitialTheme` or `ApplyThemeAsync(new ThemeRequest("PrimeNGAura.DarkCompact", null, ThemeTransitionReason.UserRequest))`. The catalog contains twelve definitions in three families, plus Core's default. This package uses the general `.Dark`, `.Compact`, `.DarkCompact` suffix convention; no automatic runtime variant dispatch is installed.

Dark profiles use zinc-950 (`#09090B`) field backgrounds, zinc-900 (`#18181B`) Card/elevated backgrounds, white text and zinc-400 muted text. Aura/Lara use emerald-400/300/200 primary states, zinc-600 field borders and zinc-700 content borders. Nora uses emerald-500/400/300 states and zinc-500 field borders. Dark masks use 60% black; tooltip backgrounds use zinc-700 with white text. These values follow the official presets' `colorScheme.dark` table. Aura/Lara color-mix highlights and Lara outline colors are represented as static alpha colors. Nora intentionally uses a solid emerald highlight with zinc-950 foreground. Its two Dark profiles set the existing Steps `FinishIconColor` Own Token to that contrasting foreground, so the completed check remains readable on the solid highlight; primary button/process-number contrast and other presets keep their existing values.

Compact profiles retain the normal 40px/44px height Seed and run the existing Compact algorithm: the resulting normal height is 36px/40px. Explicit SM/LG aliases are 75%/125% of that result (Aura/Nora 27/45; Lara 30/50), matching the existing height calculation; horizontal content padding becomes 8px. Select option padding is `8,6` (Aura/Nora) or `8,8` (Lara); Lara compact popup padding is `4` rather than its full-density `0,8`. Checkbox indicator and Card/Dialog padding remain preset-specific fixed values.

## Mapping and sources

The value baseline is the approved PrimeNG v22 mapping (14px compatibility typography), rather than a promise of identical web/native rendering. Sources: [PrimeNG styled mode](https://primeng.dev/theming), official [Aura](https://github.com/primefaces/primeuix/tree/main/packages/themes/src/presets/aura), [Lara](https://github.com/primefaces/primeuix/tree/main/packages/themes/src/presets/lara), and [Nora](https://github.com/primefaces/primeuix/tree/main/packages/themes/src/presets/nora) preset sources in `@primeuix/themes`. The XML files are the complete, inspectable value tables.

| PrimeNG semantic | AtomUI global mapping (Aura Light) |
|---|---|
| Primary / hover / active | `ColorPrimary #10B981`, `ColorPrimaryHover #059669`, `ColorPrimaryActive #047857` |
| Contrast / highlight | `ColorTextLightSolid #FFFFFF`, `ColorPrimaryBg #ECFDF5`, `ColorPrimaryText #047857` |
| Text / muted | `ColorText #334155`, `ColorTextSecondary` and `ColorTextDescription #64748B` |
| Content / hover / border | `ColorBgContainer #FFFFFF`, `ColorFillTertiary #F1F5F9`, `ColorBorderSecondary #E2E8F0` |
| Form border / placeholder / invalid | `ColorBorder #CBD5E1`, `ColorTextPlaceholder #64748B`, `ColorError #F87171` |
| Disabled background | `ColorBgContainerDisabled #E2E8F0` (separate from the hover fill) |
| Radius xs / sm / md / lg | `BorderRadiusXS 2`, `BorderRadiusSM 4`, `BorderRadius 6`, `BorderRadiusLG 8` |
| Field density / typography | `ControlHeight 40`, SM `36`, LG `48`; `PaddingContentHorizontal 12`, `FontSizeSM 12`, LG `16` |
| Focus / motion | `FocusVisualBorderThickness 1`, `ColorFocusBorder #10B981`, `ControlOutlineWidth 0`, `MotionDurationMid 200ms` |
| Select/popover / modal / mask | `BoxShadowsSecondary`, `BoxShadows`, `ColorBgMask #66000000` |

| Control | Explicit mapping |
|---|---|
| Button | `FontWeight 500`, `BorderRadius 6` |
| CheckBox | `CheckIndicatorSize 20`, `CheckedMarkSize 14`, local `BorderRadiusSM 4` |
| Select | `OptionPadding 12,8`, `PopupContentPadding 4` |
| Card | `BodyPadding 20`, local `BorderRadiusLG 12`, `RestingShadows` with the two PrimeNG static shadow layers |
| Dialog | local `BorderRadiusLG 12`; `HeaderPadding`, `ContentPadding`, `WindowContentPadding`, `FooterPadding 20`; `HeaderFontSize 20`, local `FontWeightStrong 600` |
| ToolTip | `ToolTipBackground #334155`, `ToolTipCornerRadius 6`, `ContentPadding 12,8` |

Lara uses 44px normal controls, 24px checkbox indicators, 600 button weight, 16px button horizontal padding, full-density Select option padding `16,10` and popup padding `0,8` (the official Lara list values), a 3px emerald-200 (`#A7F3D0`) outline and the shallower overlay shadow. Nora uses 2px radii, 700 button weight, slate-900 text (`#0F172A`), slate-500 borders (`#64748B`), emerald-600/700/800 primary states, transparent secondary borders, 2px focus borders and `EnableMotion=false`.

## Unsupported and known differences

- Ripple, floating labels, `disabledOpacity 0.6`, Filled field appearance and rem root-font scaling are not reproduced. Disabled colors follow AtomUI's state model.
- Nora's inset field-focus geometry is not reproduced; the preset maps the available 2px focus-border token.
- Input hover borders use the primary color in AtomUI; PrimeNG Aura uses slate 400. No hover token or control template is added by this package.
- Small LineEdit text follows `InputFontSize` by AtomUI contract; setting global `FontSizeSM` does not shrink its small text.
- Select option item radius and the 2px item gap are not mapped: the existing option theme has no Token consumer for those values. Closed-field geometry and option padding remain separate contracts.
- Card keeps AtomUI's header divider, host typography and border. The static PrimeNG shadow maps to `RestingShadows`; `CardShadows` remains the hover shadow.
- XML overrides keep their normal precedence over algorithms. Requesting the base ID with a Dark algorithm override retains its explicit Light colors; choose the `.Dark` / `.DarkCompact` profile instead. Overriding a profile algorithm does not select a different profile. This preserves the existing compiler contract and avoids a package-specific runtime dispatcher.

## Actual Aura visual comparison

On 2026-11-10, the five controls were rendered with the current compiled XML using the existing Skia/headless rendering fixture, with real desktop themes and a 1x, white 520×550 window. A screenshot was inspected against the official v22 [Button](https://primeng.dev/button), [InputText](https://primeng.dev/inputtext), [Checkbox](https://primeng.dev/checkbox), [Select](https://primeng.dev/select), and [Card](https://primeng.dev/card) demonstrations, configured as Aura / emerald / slate / Light through the site's own saved configuration. The web application reported Angular 22.2.3. Screenshots and the temporary probe were removed after review.

| Region | Observed AtomUI | Observed official demo | Assessment |
|---|---|---|---|
| Button | 40px tall, radius 6, emerald fill | 35px tall, radius 6, emerald fill | Radius and primary family agree; package intentionally retains the pinned 40px density |
| LineEdit / InputText | 40px tall, white field, slate-300 border, radius 6 | 35px tall, white field, `#CBD5E1` border, radius 6 | Border/radius agree; density differs |
| CheckBox indicator | 20px, radius 4, emerald selected fill | 18px, radius 4 | Radius agrees; size follows pinned 20px table |
| Select closed field | 40px tall, white field, slate-300 border, radius 6 | 35px tall, white field, `#CBD5E1` border, radius 6 | Border/radius agree; density differs |
| Card | Radius 12, light static shadow, 20px body padding, visible header divider/border | Radius 12, light shadow, no matching header divider | Silhouette/shadow are close; original AtomUI structure remains visible |

This is a bounded visual comparison of radius, normal height, primary fill and border regions, not pixel equivalence. Current web demonstration density differs from the pinned mapping and the canonical non-compat preset values; the package does not silently change its value table to follow the live site. Native OS rendering, all interaction states and the full theme/algorithm matrix were not visually compared.
