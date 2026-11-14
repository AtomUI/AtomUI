# AtomUI.Themes.PrimeNG

Status: implemented and validated locally; not published to NuGet.

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

Dark profiles use zinc-950 (`#09090B`) field backgrounds, zinc-900 (`#18181B`) Card/elevated backgrounds, white text and zinc-400 muted text. Aura/Lara use emerald-400/300/200 primary states, zinc-600 field borders and zinc-700 content borders. Nora uses emerald-500/400/300 states and zinc-500 field borders. Dark masks use 60% black; tooltip backgrounds use zinc-700 with white text. These values follow the official presets' `colorScheme.dark` table. Aura/Lara color-mix highlights and Lara outline colors are represented as static alpha colors. Nora intentionally uses a solid emerald highlight with zinc-950 foreground. Its dark algorithm's Steps control stage sets the existing Steps `FinishIconColor` Own Token to that contrasting foreground, so the completed check remains readable on the solid highlight; primary button/process-number contrast and other presets keep their existing values.

Compact profiles retain the normal 40px/44px height Seed and run the existing Compact algorithm: the resulting normal height is 36px/40px. SM/LG heights are 75%/125% of that result (Aura/Nora 27/45; Lara 30/50), matching the existing height calculation; horizontal content padding becomes 8px. Select option padding is `8,6` (Aura/Nora) or `8,8` (Lara); Lara compact popup padding is `4` rather than its full-density `0,8`. Checkbox indicator and Card/Dialog padding remain preset-specific fixed values. The Aura compact profiles are the base profiles with `AtomUI:Compact` appended and nothing else (see [Aura profiles](#aura-profiles)); the Lara compact profiles add one Select value to that (see [Lara profiles](#lara-profiles)); the Nora compact profiles are again the base profiles with `AtomUI:Compact` appended (see [Nora profiles](#nora-profiles)).

## Mapping and sources

The value baseline is the approved PrimeNG v22 mapping (14px compatibility typography), rather than a promise of identical web/native rendering. Sources: [PrimeNG styled mode](https://primeng.dev/theming), official [Aura](https://github.com/primefaces/primeuix/tree/main/packages/themes/src/presets/aura), [Lara](https://github.com/primefaces/primeuix/tree/main/packages/themes/src/presets/lara), and [Nora](https://github.com/primefaces/primeuix/tree/main/packages/themes/src/presets/nora) preset sources in `@primeuix/themes`. All values are produced by the `PrimeNG:Aura` / `PrimeNG:Aura.Dark`, `PrimeNG:Lara` / `PrimeNG:Lara.Dark` and `PrimeNG:Nora` / `PrimeNG:Nora.Dark` algorithms ([Preset algorithms](#preset-algorithms)); the XML files hold only the chain and the seeds.

| PrimeNG semantic | AtomUI global mapping (Aura Light) |
|---|---|
| Primary / hover / active | `ColorPrimary #10B981`, `ColorPrimaryHover #059669`, `ColorPrimaryActive #047857` |
| Contrast / highlight | `ColorTextLightSolid #FFFFFF`, `ColorPrimaryBg #ECFDF5`, `ColorPrimaryText #047857` |
| Text / muted | `ColorText #334155`, `ColorTextSecondary` and `ColorTextDescription #64748B` |
| Content / hover / border | `ColorBgContainer #FFFFFF`, `ColorFillTertiary #F1F5F9`, `ColorBorderSecondary #E2E8F0` |
| Form border / placeholder / invalid | `ColorBorder #CBD5E1`, `ColorTextPlaceholder #64748B`, `ColorError #F87171` (red-400 from the red-500 seed `#EF4444`) |
| Disabled background | `ColorBgContainerDisabled #E2E8F0` (separate from the hover fill) |
| Radius xs / sm / md / lg | `BorderRadiusXS 2`, `BorderRadiusSM 4`, `BorderRadius 6`, `BorderRadiusLG 8` |
| Field density / typography | `ControlHeight 40`, SM `36`, LG `48`; `PaddingContentHorizontal 12`, `FontSizeSM 12`, LG `16` |
| Focus / motion | `FocusVisualBorderThickness 1`, `ColorFocusBorder #10B981`, `ControlOutlineWidth 0`, `MotionDurationMid 200ms` |
| Select/popover / modal / mask | `BoxShadowsSecondary`, `BoxShadows`, `ColorBgMask #66000000` |

| Control | Mapping (control stages; Aura values shown) |
|---|---|
| Button | `FontWeight 500`, `BorderRadius 6` |
| CheckBox | `CheckIndicatorSize 20`, `CheckedMarkSize 14`, local `BorderRadiusSM 4` |
| Select | `OptionPadding 12,8`, `PopupContentPadding 4` |
| Card | `BodyPadding 20`, local `BorderRadiusLG 12`, `RestingShadows` with the two PrimeNG static shadow layers |
| Dialog | local `BorderRadiusLG 12`; `HeaderPadding`, `ContentPadding`, `WindowContentPadding`, `FooterPadding 20`; `HeaderFontSize 20`, local `FontWeightStrong 600` |
| ToolTip | `ToolTipBackground #334155`, `ToolTipCornerRadius 6`, `ContentPadding 12,8` |

Lara uses 44px normal controls, 24px checkbox indicators, 600 button weight, 16px button horizontal padding, full-density Select option padding `16,10` and popup padding `0,8` (the official Lara list values), a 3px emerald-200 (`#A7F3D0`) outline and the shallower overlay shadow ([Lara rules](#lara-rules)). Nora uses 2px radii, 700 button weight, slate-900 text (`#0F172A`), slate-500 borders (`#64748B`), emerald-600/700/800 primary states, transparent secondary borders, 2px focus borders, Select popup padding `0,2` and `EnableMotion=false` ([Nora rules](#nora-rules)).

## Preset algorithms

`UsePrimeNGThemes()` also registers six theme algorithms and their control stages (all through the generated `GeneratedThemeAlgorithmRegistration.Register`); the generated identities are in `PrimeNGAlgorithms`:

| Identity | Appearance | Class |
|---|---|---|
| `PrimeNG:Aura` | Light | `PrimeNGAuraAlgorithm` |
| `PrimeNG:Aura.Dark` | Dark | `PrimeNGAuraDarkAlgorithm` |
| `PrimeNG:Lara` | Light | `PrimeNGLaraAlgorithm` |
| `PrimeNG:Lara.Dark` | Dark | `PrimeNGLaraDarkAlgorithm` |
| `PrimeNG:Nora` | Light | `PrimeNGNoraAlgorithm` |
| `PrimeNG:Nora.Dark` | Dark | `PrimeNGNoraDarkAlgorithm` |

The twelve profiles run them. All of them share one implementation; each preset brings a rule table (`PrimeNGAuraPreset`, `PrimeNGLaraPreset`, `PrimeNGNoraPreset`), and the control stages find the table of the last PrimeNG algorithm in the chain (an unknown PrimeNG identity fails instead of falling back to another preset). Each algorithm can start a chain on its own; `AtomUI:Compact` appended gives the compact density. A dark algorithm that follows another algorithm only recomputes the colours and keeps the previous Map's sizes. A light PrimeNG algorithm placed after another algorithm discards that algorithm's whole Map, because its Ant Design base pass ignores `previousMap`; the dark one keeps the previous sizes.

**Seeds.** `ColorPrimary` is the 500 stop of the primary family for both appearances (Aura and Lara map it to 500 in light and 400 in dark, Nora to 600 and 500). `ColorSuccess`, `ColorWarning`, `ColorError` and `ColorInfo` are the 500 stops of their families; `ColorLink` defaults to the primary seed. The Aura profiles set `ColorPrimary #10B981` (emerald-500) and `ColorError #EF4444` (red-500) and leave the status seeds at the Ant Design defaults, which are not Tailwind 500 stops and therefore go through the formula. The neutral scale is a constant of the preset: slate in Light, zinc in Dark. The size ladder is relative to the `BorderRadius` and `ControlHeight` seeds.

**Colour scale.** A seed that equals the 500 stop of one of the 22 Tailwind scales PrimeNG ships resolves to that hand-tuned table. Any other seed goes through the PrimeUIX `palette` formula: stops up to 500 mix in white (`19 × (5 − i)` %), stops above 500 mix in black (`15 × (i − 5)` %), per sRGB channel, rounded half up (`Math.round`). The seed's alpha is kept on every stop.

**Sources.** The Tailwind tables are transcribed verbatim from the `primitive` export of [`@primeuix/themes`](https://www.npmjs.com/package/@primeuix/themes) **3.1.1** (`dist/aura/base/index.mjs`; Lara and Nora carry the identical table). The formula is checked against the `palette`/`mix` implementation of [`@primeuix/styled`](https://www.npmjs.com/package/@primeuix/styled) **1.2.0**. The Aura rules below are checked against the Aura preset of `@primeuix/themes` 3.1.1 (`aura/base`, `aura/button`, `aura/message`, `aura/tooltip`).

### Aura Map rules

`P` is the primary family, `S` a status family (success, warning, info), `E` the error family, `N` the neutral scale. `@n%` is an opacity. Column "Source" says where a row comes from: **XML** – the value today's profiles set explicitly (acceptance baseline); **PrimeUIX** – the Aura preset token named; **AtomUI** – a proposal with no direct PrimeUIX counterpart (the token has no equivalent there).

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorPrimary` / `Hover` / `Active` | P500 / P600 / P700 | P400 / P300 / P200 | XML; PrimeUIX `primary.color` / `hoverColor` / `activeColor` |
| `ColorPrimaryBg` | P50 | P400@16% | XML; PrimeUIX `highlight.background` |
| `ColorPrimaryBgHover` | P100 | P400@24% | PrimeUIX `highlight.focusBackground` |
| `ColorPrimaryText` | P700 | white@87% | XML; PrimeUIX `highlight.color` |
| `ColorPrimaryTextActive` | P800 | white@87% | PrimeUIX `highlight.focusColor` |
| `ColorPrimaryTextHover` | P600 | P300 | AtomUI (spec proposal; Light matches `floatLabelFocusColor`) |
| `ColorPrimaryBorder` / `BorderHover` | P200 / P300 | P700@36% / P600@36% | AtomUI (Light = spec proposal; Dark follows the severity border `message.*.borderColor`) |
| `ColorLink` / `Hover` / `Active` | P500 / P600 / P700 | P400 / P300 / P200 | AtomUI. PrimeUIX `button.link` keeps `primary.color` in every state and underlines on hover; AtomUI has no underline state, so the states follow the primary ones |
| `SelectionBackground` / `Foreground` | P500 / white | P400 / N900 | PrimeUIX `primary.color` / `primary.contrastColor` |
| Status `Color*` / `Hover` / `Active` | S500 / S600 / S700 | S400 / S300 / S200 | PrimeUIX `button.{success,info,warn}` |
| Status `Bg` / `BgHover` | S50 / S100 | S500@16% / S500@24% | PrimeUIX `message.*.background` (Light 50@95% there, opaque here) / `closeButton.hoverBackground` (Light); Dark hover is AtomUI |
| Status `Border` / `BorderHover` | S200 / S300 | S700@36% / S600@36% | PrimeUIX `message.*.borderColor`; hover is AtomUI |
| Status `Text` / `TextHover` / `TextActive` | S600 / S500 / S700 | S500 / S400 / S300 | PrimeUIX `message.*.color` (`Text`); hover and active are AtomUI |
| `ColorError` | E400 | E300 | XML; PrimeUIX `formField.invalidBorderColor` |
| `ColorErrorHover` / `Active` | E500 / E600 | E200 / E100 | AtomUI (one step on from the base, as the primary states) |
| Other `ColorError*` | as status | as status | PrimeUIX `message.error` / AtomUI as for status; `BgActive` is E200 / E500@32% (AtomUI) |
| `ColorText` / `Secondary` | N700 / N500 | white / N400 | XML; PrimeUIX `text.color` / `text.mutedColor` |
| `ColorTextTertiary` / `Quaternary` | N400 / N300 | N500 / N600 | AtomUI |
| `ColorBgContainer` | white | N950 | XML; PrimeUIX `formField.background` |
| `ColorBgElevated` | white | N900 | Dark XML; PrimeUIX `overlay.*.background`, `content.background` |
| `ColorBgLayout` / `Spotlight` | N50 / N700 | N950 / N700 | AtomUI / PrimeUIX `tooltip.background` |
| `ColorFill` / `Secondary` / `Tertiary` / `Quaternary` | N300 / N200 / N100 / N50 | N600 / N700 / N800 / N900 | `Tertiary`: XML, PrimeUIX `content.hoverBackground`; the others AtomUI |
| `ColorBgSolid` / `Hover` / `Active` | N900 / N800 / N950 | white / N300 / N100 | AtomUI |
| `ColorBgBlur` | transparent | white@4% | AtomUI (the Ant Design values) |
| `ColorBorder` / `Secondary` | N300 / N200 | N600 / N700 | XML; PrimeUIX `formField.borderColor` / `content.borderColor` |
| `ColorBgMask` | black@40% | black@60% | XML; PrimeUIX `mask.background` |
| `ColorBgBase` / `ColorTextBase` (when the seed is unset) | white / black | N950 / white | Dark XML; Light keeps the Ant Design values |
| `BorderRadiusXS` / `SM` / `LG` | radius − 4 / − 2 / + 2 | same | XML (2/4/8 around 6); PrimeUIX `borderRadius.xs/sm/md/lg` |
| `ControlHeightSM` / `LG` | height − 4 / + 8 | same | XML (36/48 around 40; Lara 40/52 around 44) |
| Font sizes, sizes, motion, line widths, Ant preset palettes | Ant Design formulas | Ant Design Dark | The Ant ladder already gives PrimeUIX's 12/14/16 |

The spec 4.7 row "按暗色对称取级" for the dark primary border and text states is replaced by the values above, which follow PrimeUIX where it has a counterpart. Every "现值" row of spec 4.7 is reproduced exactly (covered by `PrimeNGAlgorithmContractTests.Aura_Map_Matches_The_Baseline_Colours`).

### Aura Alias rules

Written by the Alias stage of both Aura algorithms; the appearance comes from the resolved chain (`context.Appearance`), so a mixed chain such as `[PrimeNG:Aura, AtomUI:Dark]` gets the dark column. A value the theme sets explicitly still wins.

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorFocusBorder` | `ColorPrimary` | `ColorPrimary` | PrimeUIX `focus.ring.color` |
| `FocusVisualBorderThickness` | 1 × `LineWidth` | same | PrimeUIX `focus.ring.width` |
| `ControlOutlineWidth` | 0 × `LineWidth` | same | Aura has no field focus glow; `InputActiveShadow` follows |
| `ColorTextPlaceholder`, `ColorTextDescription` | `ColorTextSecondary` | same | PrimeUIX `form.field.placeholder.color`, `text.muted.color` |
| `ColorBgContainerDisabled` | N200 | N700 | PrimeUIX `form.field.disabled.background` |
| `ColorTextDisabled` | N400 | N500 | AtomUI. PrimeUIX `form.field.disabled.color` is N500 / N400, the muted-text stop, and PrimeNG separates it with a component opacity AtomUI does not apply; one stop further from the text keeps it apart from `ColorTextSecondary`. Contrast on `ColorBgContainerDisabled`: 2.08 light, 2.16 dark (the Ant formula gave 1.20 / 1.35, the old profiles 1.79 / 2.06) |
| `ColorTextLightSolid` | white | N900 | PrimeUIX `primary.contrast.color` |
| `PaddingContentHorizontal` | `SizeSM` (12; compact 8) | same | The Ant formula (`SizeMS`) gives 16 / 12 |
| `BoxShadows` / `BoxShadowsSecondary` | modal / popover shadow | same | PrimeUIX `overlay.modal.shadow` / `overlay.popover.shadow` |

### Aura control stages

One class per control under `Algorithms/Controls`, each declared for both `PrimeNG:Aura` and `PrimeNG:Aura.Dark` (revision 1). Values that follow from other tokens are computed. The fixed sizes are the values of the pre-algorithm XML profiles (**XML (baseline)**); the compact profiles have always kept them, so they do not follow `AtomUI:Compact`. They equal the `aura-compat` preset of `@primeuix/themes` 3.1.1 at a 16px root; the standard `aura` preset is denser (card and modal padding 18, tooltip padding 6/10, option padding 4/10, checkbox 18 / 12).

| Control | Values | Source |
|---|---|---|
| Button | `FontWeight 500` | PrimeUIX `button.label.font.weight`; the radius is the global `BorderRadius` already |
| CheckBox | `CheckIndicatorSize 20`, `CheckedMarkSize 14` | XML (baseline) (`aura-compat` `checkbox.width`, `checkbox.icon.size`); the radius is the global `BorderRadiusSM` |
| Select | `OptionPadding` = (`UniformlyPaddingSM`, `UniformlyPaddingSM / 2 + 2`): 12,8, compact 8,6; `PopupContentPadding` = `UniformlyPaddingXXS` (4) | Option padding: AtomUI, a fit to the XML (baseline) values 12,8 / 8,6, not a PrimeUIX derivation; popup padding: PrimeUIX `list.padding` |
| Card | for the card only: `ColorBgContainer` = `ColorBgElevated`, `BorderRadiusLG` = radius + 6 (12); `BodyPadding 20`; `RestingShadows` static two-layer shadow; dark `HeaderBg` = the card surface | PrimeUIX `card.background` (`content.background`), `border.radius.xl`, `card.shadow`; body padding XML (baseline) (`aura-compat` `card.body.padding`); dark header XML (baseline) |
| Dialog | for the dialog only: `BorderRadiusLG` = radius + 6 (12); header, content, window content and footer padding 20; `HeaderFontSize` = `FontSizeXL` (20) | PrimeUIX `overlay.modal.border.radius`; padding and title size XML (baseline) (`aura-compat` `overlay.modal.padding`, `dialog.title.font.size`); weight 600 is the global `FontWeightStrong` |
| ToolTip | `ToolTipBackground` N700, `ToolTipColor` white, `ToolTipCornerRadius` = `BorderRadius`, `ContentPadding 12,8` | PrimeUIX `tooltip.background`, `tooltip.color` (`surface.0`), `overlay.popover.border.radius`; padding XML (baseline) (`aura-compat` `tooltip.padding`) |

### Aura profiles

The four Aura XML files hold only the chain and the seeds `ColorPrimary #10B981`, `ColorError #EF4444`, `BorderRadius 6`, `ControlHeight 40`; light and dark differ only in the chain. Consequences:

- `PrimeNGAura` with the chain `[PrimeNG:Aura.Dark]` compiles slot by slot to `PrimeNGAura.Dark`.
- **不等价项** (slots where `PrimeNGAura` + `AtomUI:Compact` differs from `PrimeNGAura.Compact`): none. The compact files are the base files with `AtomUI:Compact` appended; every value the old compact XML wrote by hand (SM/LG heights 27/45, `PaddingContentHorizontal 8`, Select `OptionPadding 8,6`) now follows from the rules. `PrimeNGPresetMigrationTests.Appending_Compact_Equals_The_Compact_Profile_Except_Listed_Slots("Aura")` asserts the list.
- Changing the primary seed moves the hover, active, focus ring and selected backgrounds with it.

**Change in the dark Aura profiles.** `PrimeNGAura.Dark` and `PrimeNGAura.DarkCompact` now compile `ColorPrimary` to `#34D399` (was `#2FB685`) and `ColorError` to `#FCA5A5` (was `#D98F8F`), the values their XML always wrote: the old `AtomUI:Dark` chain re-derived those seeds in its Map stage. Everything derived from them follows (selection background, the primary fills of tabs, steps, switches and radios, error text and badges). Other differences against the old profiles come from the Aura rules filling tokens the old XML left to Ant Design (status, link, fill, solid, spotlight and tertiary text colours).

**Known limitation.** A control in `ControlAlgorithmMode.Global` or `Custom` re-runs the chain with the already-compiled global tokens as seeds; a derived colour is not a Tailwind 500 stop, so that control's colours shift one stop. The Ant Design dark algorithm has the same drift; it is not fixed here.

## Lara rules

The Lara rule table (`PrimeNGLaraPreset`) is transcribed from the four pre-algorithm Lara XML profiles, which are the acceptance baseline. Only the rows that differ from Aura are listed; every other Map, Alias and control row is the Aura row above.

**Map.** All colour rows equal the Aura Map rules. The old Lara XML wrote exactly the Aura colours (**XML (baseline)**), and the semantic colour scheme of the `lara` / `lara-compat` presets of `@primeuix/themes` 3.1.1 (`primary`, `highlight`, `text`, `content`, `formField` background / border / disabled, `mask`, `surface` slate / zinc) equals the `aura` one. The size ladder is the Aura ladder around the `ControlHeight 44` seed: SM 40, LG 52 (XML (baseline)).

**Alias.**

| Token | Light | Dark | Source |
|---|---|---|---|
| `FocusVisualBorderThickness` | 0 × `LineWidth` | same | XML (baseline); PrimeUIX `lara-compat` `focus.ring.width` 0 |
| `ControlOutlineWidth` | 3 × `LineWidth` | same | XML (baseline); PrimeUIX `focus.ring.shadow` is a 0.2rem (3.2px) spread |
| `ColorControlOutline` | `ColorPrimaryBorder` (P200) | `ColorPrimary` @20% (P400@20%) | XML (baseline); PrimeUIX `lara-compat` `focus.ring.shadow` (`primary.200` / `primary.color` with 80% transparency) |
| `BoxShadowsSecondary` | `0 2 12 0 #1A000000` | same | XML (baseline); PrimeUIX `lara-compat` `overlay.select.shadow` |
| `BoxShadows` | the Aura modal shadow | same | XML (baseline); PrimeUIX `lara` `overlay.modal.shadow` is `0 1px 3px rgba(0,0,0,0.3)`, not used |
| `ColorTextDisabled` | N400 | N500 | AtomUI, as for Aura (same reasoning, same contrast: 2.08 light, 2.16 dark on `ColorBgContainerDisabled`; the old Lara profiles had 1.79 / 2.06) |

`InputActiveShadow` (and the error / warning variants) follow `ControlOutlineWidth` and `ColorControlOutline`, so the glow follows the primary seed.

**Control stages.**

| Control | Values | Source |
|---|---|---|
| Button | `FontWeight 600`; for the button only: `ControlPaddingHorizontal 16` | PrimeUIX `lara-compat` `button.label.font.weight`, `button.padding.x` (1rem); kept for compact too (XML (baseline)) |
| CheckBox | `CheckIndicatorSize 24`, `CheckedMarkSize 14` | XML (baseline); PrimeUIX `lara-compat` `checkbox.width` is 24, its `checkbox.icon.size` 16 |
| Select | `OptionPadding` = (`UniformlyPadding`, `UniformlyPaddingSM / 2 + 4`): 16,10, compact 8,8; `PopupContentPadding` = (0, `UniformlyPaddingXS`): 0,8 | Full density: PrimeUIX `lara-compat` `list.option.padding` (0.625rem 1rem), `list.padding` (0.5rem 0). The compact option padding is AtomUI, a fit to the XML (baseline) value 8,8 |
| Card, Dialog, ToolTip | as Aura (body padding 20, radius + 6, the two-layer resting shadow; dialog padding and title 20; tooltip N700, white, 12,8) | XML (baseline): the old Lara XML wrote the Aura values. PrimeUIX `lara-compat` differs (card padding 24 and radius `border.radius.lg`, modal padding 24, tooltip padding 12,10) |

### Lara profiles

The four Lara XML files hold the chain and the seeds `ColorPrimary #10B981`, `ColorError #EF4444`, `BorderRadius 6`, `ControlHeight 44`; light and dark differ only in the chain, and the compact files add `AtomUI:Compact` and one control value.

- `PrimeNGLara` with the chain `[PrimeNG:Lara.Dark]` compiles slot by slot to `PrimeNGLara.Dark` (and the compact pair likewise).
- **不等价项** (slots where `PrimeNGLara` + `AtomUI:Compact` differs from `PrimeNGLara.Compact`): `Select.PopupContentPadding`. The old compact profiles wrote `4` where the full-density profiles have `0,8`; no size token gives `0` at full density and `4` in compact, so the two compact XML files keep `PopupContentPadding 4` (`PrimeNGLara` + `AtomUI:Compact` gives `0,4`). Everything else the old compact XML wrote (SM/LG heights 30/50, `PaddingContentHorizontal 8`, Select `OptionPadding 8,8`) follows from the rules. `PrimeNGPresetMigrationTests.Appending_Compact_Equals_The_Compact_Profile_Except_Listed_Slots("Lara")` asserts the list.
- **Change in the dark Lara profiles**, the same as for Aura: `PrimeNGLara.Dark` and `PrimeNGLara.DarkCompact` now compile `ColorPrimary` to `#34D399` (was `#2FB685`) and `ColorError` to `#FCA5A5` (was `#D98F8F`), the values their XML always wrote.

## Nora rules

The Nora rule table (`PrimeNGNoraPreset`) is transcribed from the four pre-algorithm Nora XML profiles, which are the acceptance baseline. Only the rows that differ from Aura are listed; every other Map, Alias and control row is the Aura row above. PrimeUIX is named only for values checked in the `nora-compat` preset of `@primeuix/themes` 3.1.1 (`nora-compat/base`, `button`, `steps`, `card`, `dialog`, `checkbox`, `tooltip`, `tag`).

**Map.**

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorPrimary` / `Hover` / `Active` | P600 / P700 / P800 | P500 / P400 / P300 | XML (baseline); PrimeUIX `primary.color` / `hoverColor` / `activeColor` |
| `ColorPrimaryBg` / `BgHover` | P50 / P100 | P500 / P700 (opaque) | `Bg`: XML (baseline); dark `Bg`: PrimeUIX `highlight.background`. Light `highlight.background` is P600 in PrimeUIX; the XML keeps P50. Dark `BgHover` departs from PrimeUIX `highlight.focusBackground` (P400): PrimeUIX puts dark text on it, but AtomUI controls draw `ColorText` (white) on `ControlItemBgActiveHover` (time cells, DataGrid selected rows, Splitter); P700 gives white 5.48 (P400 would give 1.92, the old profile 7.74) |
| `ColorPrimaryText` | P800 | N950 | XML (baseline); dark: PrimeUIX `highlight.color` (`surface.950`) |
| `ColorPrimaryTextHover` / `TextActive` | P700 / P900 | N950 / N950 | AtomUI (light: one stop on, as Aura); dark: PrimeUIX `highlight.focusColor` |
| `ColorPrimaryBorder` / `BorderHover` | P200 / P300 | P700@36% / P600@36% | AtomUI (the Aura rows) |
| `ColorLink` / `Hover` / `Active`, `SelectionBackground` | follow the primary rows | same | as Aura |
| `SelectionForeground`, `ColorTextLightSolid` | white | N950 | XML (baseline); PrimeUIX `primary.contrastColor` |
| Status `Color*` / `Hover` / `Active` | S600 / S700 / S800 | S500 / S400 / S300 | AtomUI decision: the primary rule, as spec 4.7 proposes for the status families; PrimeUIX `button.{success,info,warn}` and `tag.*` use these stops. The other status rows are the Aura rows |
| `ColorError` / `Hover` / `Active` | E400 / E500 / E600 | E400 / E300 / E200 | XML (baseline) for the base (`#F87171` in both); the states one stop on, as Aura. PrimeUIX `formField.invalidBorderColor` is E500 / E400 |
| `ColorText` | N900 | white | XML (baseline); PrimeUIX `text.color` |
| `ColorTextTertiary` / `Quaternary` | N500 / N400 | N500 / N600 (as Aura) | AtomUI. Light is one stop deeper than Aura so tertiary text and `ColorIcon` keep 3:1 on white (4.76; N400 would give 2.56, the old profile 3.32); quaternary 2.56 (old 1.82). `ColorTextDisabled` N400 equals the quaternary stop, as in Ant Design where both are 25% text. PrimeUIX `nora` has a single muted level, N600 / N400 |
| `ColorTextSecondary` | N500 | N400 | XML (baseline); PrimeUIX `text.mutedColor` is N600 / N400 |
| `ColorBorder` | N500 | N500 | XML (baseline); PrimeUIX `formField.borderColor` |
| `ColorBorderSecondary` | transparent | transparent | XML (baseline) |
| Radius ladder | XS, SM, LG = `BorderRadius` (2) | same | XML (baseline); PrimeUIX `formField.borderRadius`, `content.borderRadius` and the overlays all use `border.radius.xs` |

**Alias.**

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorFocusBorder` | `ColorPrimary` | `ColorBorder` (N500) | XML (baseline). Light: PrimeUIX `focus.ring.color`. Dark: PrimeUIX `form.field.focus.border.color` (`surface.500`); the PrimeUIX focus ring is the primary colour, which the XML did not use |
| `FocusVisualBorderThickness` | 2 × `LineWidth` | same | XML (baseline); PrimeUIX `focus.ring.width` 2px |
| `ColorTextDisabled` | N400 | N500 | AtomUI, as for Aura (one stop from the secondary text towards the background). Contrast on `ColorBgContainerDisabled`: 2.08 light, 2.16 dark (old Nora profiles 1.79 / 2.06) |

**Control stages.**

| Control | Values | Source |
|---|---|---|
| Button | `FontWeight 700`; dark, for the button only: `ColorPrimaryBg` = `ColorPrimary`@16%, `ColorPrimaryBgHover` = `ColorPrimary`@24% | Weight: XML (baseline); PrimeUIX `button.label.fontWeight`. The scoped backgrounds are AtomUI (the Aura shape): the Filled variant draws `ColorPrimary` on them, and the global ones are the solid primary / P700 (1.00 and 2.16; now 6.39 and 5.46). They also make the Text variant's hover background translucent (hover text 1.32 → 8.43). `DropdownButton`, `ResetButton` and `SubmitButton` are separate identities and keep the global values, see [Unsupported](#unsupported-and-known-differences) |
| CheckBox | `CheckIndicatorSize 20`, `CheckedMarkSize 14` | XML (baseline); PrimeUIX `checkbox.width` 1.25rem |
| Select | `OptionPadding` as Aura (12,8, compact 8,6); `PopupContentPadding` 0,2 in both densities | XML (baseline); PrimeUIX `list.padding` (0.125rem 0) |
| Card | as Aura, radius + 0 (2); dark: for the card only `ColorBorderSecondary` N700 | XML (baseline); the dark border equals PrimeUIX `overlay.*.borderColor` (transparent / `surface.700`). PrimeUIX `card.borderRadius` is `border.radius.sm` and its shadow differs; the XML values are kept |
| Dialog | as Aura, radius + 0 (2) | XML (baseline); PrimeUIX `overlay.modal.padding` 1.25rem |
| ToolTip | as Aura (N700, white, `BorderRadius`, 12,8) | XML (baseline); PrimeUIX `tooltip.padding` 0.5rem 0.75rem. PrimeUIX `tooltip.background` is N900 / `surface.0`; the XML keeps N700 / white |
| NavMenu (`PrimeNG:Nora.Dark` only) | dark: `ItemSelectedBg` = `ColorPrimary`@16%; nothing under a light appearance | AtomUI. Ant Design takes it from `ControlItemBgActive`, the solid primary here, and draws `ItemSelectedColor` (the primary) on it (1.00; now 6.39). `ItemSelectedColor` cannot become the contrast colour, because it also colours the selected-path headers, which sit on the menu background |
| Steps (`PrimeNG:Nora.Dark` only) | dark: `FinishIconColor` = `ColorTextLightSolid` (N950); nothing under a light appearance | XML (baseline); PrimeUIX `steps.itemNumber.activeColor` (`primary.contrast.color`) |

### Nora profiles

The four Nora XML files hold the chain and the seeds `ColorPrimary #10B981`, `ColorError #EF4444`, `BorderRadius 2`, `ControlHeight 40`, plus the constant `EnableMotion false` (PrimeUIX `transitionDuration` 0s); light and dark differ only in the chain.

- `PrimeNGNora` with the chain `[PrimeNG:Nora.Dark]` compiles slot by slot to `PrimeNGNora.Dark` (and the compact pair likewise).
- **不等价项** (slots where `PrimeNGNora` + `AtomUI:Compact` differs from `PrimeNGNora.Compact`): none. Everything the old compact XML wrote (SM/LG heights 27/45, `PaddingContentHorizontal 8`, Select `OptionPadding 8,6`) follows from the rules. `PrimeNGPresetMigrationTests.Appending_Compact_Equals_The_Compact_Profile_Except_Listed_Slots("Nora")` asserts the list.
- **Change in the dark Nora profiles**, as for Aura: `PrimeNGNora.Dark` and `PrimeNGNora.DarkCompact` now compile `ColorPrimary` to `#10B981` (was `#11A071`) and `ColorError` to `#F87171` (was `#D66363`), the values their XML always wrote.
- **Change in all four Nora profiles:** the success, warning and info base colours take the 600 stop (light) and 500 stop (dark) of their families, so they are darker than the old Ant Design values in light (`ColorSuccess #46A716`, was `#52C41A`).

## Unsupported and known differences

- Ripple, floating labels, `disabledOpacity 0.6`, Filled field appearance and rem root-font scaling are not reproduced. Disabled colors follow AtomUI's state model.
- Nora's inset field-focus geometry is not reproduced; the preset maps the available 2px focus-border token.
- Nora dark: the Filled primary variant of `DropdownButton`, `ResetButton` and `SubmitButton` still draws `ColorPrimary` on the global `ColorPrimaryBg`, which is the solid primary (text contrast 1.00); only `Button` has the scoped translucent background. Elsewhere, selected backgrounds are solid primary with `ColorText` on them where the control does not switch the text colour (white on `#10B981`: 2.54, unchanged from the old profiles).
- Input hover borders use the primary color in AtomUI; PrimeNG Aura uses slate 400. No hover token or control template is added by this package.
- Small LineEdit text follows `InputFontSize` by AtomUI contract; setting global `FontSizeSM` does not shrink its small text.
- Select option item radius and the 2px item gap are not mapped: the existing option theme has no Token consumer for those values. Closed-field geometry and option padding remain separate contracts.
- Card keeps AtomUI's header divider, host typography and border. The static PrimeNG shadow maps to `RestingShadows`; `CardShadows` remains the hover shadow.
- XML overrides keep their normal precedence over algorithms. No profile has explicit colours left, so requesting the base ID with `[PrimeNG:Aura.Dark]`, `[PrimeNG:Lara.Dark]` or `[PrimeNG:Nora.Dark]` gives the dark profile. Overriding a profile algorithm does not select a different profile. This preserves the existing compiler contract and avoids a package-specific runtime dispatcher.

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
