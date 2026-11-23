# AtomUI.DesignLanguages.PrimeNG

Status: implemented and validated locally (token values equal the pre-refactor PrimeNG profiles for all 12 variants and
the brand seed matrix); not published to NuGet.

The PrimeNG design language: the three standard presets of [`@primeuix/themes`](https://www.npmjs.com/package/@primeuix/themes)
3.1.1 at a 16px root (what primeng 22.1.5 on [primeng.dev](https://primeng.dev) renders) as the styles **Aura**
(default), **Lara** and **Nora**. The default primary is primeng.dev's **noir** (seed slate-950 `#020617`), see
[Noir](#noir-the-default-primary).

Applications do not reference this package directly: it comes with the value packages of the control packages they use.

```csharp
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.PrimeNG;
using AtomUI.Theme;
using AtomUI.Theme.Languages;

appBuilder.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.UseDesktopDataGrid();
    builder.UsePrimeNG(language => language.AddDesktopControls().AddDesktopDataGrid());
    builder.WithInitialTheme(ThemeSelection.Of<PrimeNGLanguage>(PrimeNGLanguage.Lara)
                                           .WithAppearance(ThemeAppearance.Dark)
                                           .WithDensity(DesignDensity.Compact));
});
```

## Axes

| Axis | Values | Notes |
|---|---|---|
| Style | `Aura` (default), `Lara`, `Nora` | `PrimeNGLanguage.Aura` / `Lara` / `Nora` |
| Appearance | Light, Dark | The presets' `colorScheme.light` / `colorScheme.dark` |
| Density | `Default`, `Compact` | **Compact is an AtomUI extension**: PrimeUIX has no compact density (see [Compact density](#compact-density)) |
| Idiom | `Desktop` | PrimeNG has no mobile specification; `Mobile` is not declared |

No named theme is shipped: style, appearance and density come from the selection. The twelve v1 profiles of the old
`AtomUI.Themes.PrimeNG` package (`PrimeNGAura`, `PrimeNGAura.Dark`, … `PrimeNGNora.DarkCompact`) are replaced by
`ThemeSelection.Of("PrimeNG", style)` with the appearance and density.

## Seeds

`DefineSeeds` gives the seeds the old profiles wrote; the appearance and density change no seed.

| Seed | Aura | Lara | Nora | Source |
|---|---|---|---|---|
| `ColorPrimary` | `#020617` | `#020617` | `#020617` | primeng.dev noir (slate 950) |
| `ColorError` / `ColorSuccess` / `ColorWarning` / `ColorInfo` | `#EF4444` / `#22C55E` / `#F97316` / `#0EA5E9` | same | same | Tailwind red / green / orange / sky 500 (PrimeUIX `button`, `tag`, `badge` severities) |
| `EnableWaveSpirit` | false | false | false | PrimeNG ships `ripple: false` |
| `BorderRadius` | 6 | 6 | 2 | `border.radius.md` (Nora `border.radius.xs`) |
| `ControlHeight` | 35 | 39 | 35 | the standard (16px) presets' field height |

**Adjustable seed:** `ColorPrimary` (old `BrandColor="Seed"`). Its declared derivations (`SeedDerivations`) are the ten
primary-family Map tokens, `SelectionBackground`, `ColorLinkHover` and `ColorLinkActive`; a theme that writes one of them
without locking the seed gets the stale-value warning. PrimeNG has no neutral seed.

## Language tokens

`PrimeNGToken` holds the PrimeUIX `surface` scale of the style and appearance: `Surface50` … `Surface950` (light slate,
dark zinc in all three presets; `surface.0` is white and has no token). Every rule of the global and control
derivations resolves its surface stops against these tokens, so a theme that overrides a stop (`<LanguageTokens>`)
changes every value derived from it. The rest of the preset vocabulary (the rule tables below) is the language's
internal implementation; it follows the official presets and may be rewritten when they are upgraded.

## Component recipes

Value packages can read these roles from `context.Language` during a control derivation. Pass the control's effective
`view.Global`, and pass `context.Variant` when the recipe resolves preset or appearance rules.

| API | Roles |
| --- | --- |
| `GetTableRecipe(global)` | Header, row, hover, selection, border, cell padding by size and container radius follow the desktop DataGrid mapping. Table consumers can also opt into `GetStripedRowBackground(variant)`. |
| `GetListItemRecipe(global)` | PrimeUIX `list.option.focus` and `highlight` backgrounds and foregrounds used by lists and selectors. |
| `GetMenuOverlayRecipe(global, variant)`, `GetSelectOverlayRecipe(global, variant)`, `GetPopoverOverlayRecipe(global, variant)`, `GetDialogOverlayRecipe(global, variant)` | Panel background, border, shadow, radius, padding and modal mask. The menu uses the content border. The select panel and popover use the overlay border. |
| `GetFieldRecipe(global, variant)` | PrimeUIX `form.field` and `inputgroup.addon` roles, including hover and focus border, focus ring, placeholder, disabled colours and add-on background. |

Recipes are read-only views for the current derivation. Override the source language or global token to change a value.
`GetStripedRowBackground(variant)` maps PrimeUIX DataTable's `row.stripedBackground` to the effective
`Surface50` in light appearance and `Surface950` in dark appearance. Aura, Lara and Nora use the same rule in
`dist/{aura,lara,nora}/datatable/index.mjs` from the [`@primeuix/themes` 3.1.1 package](https://registry.npmjs.org/@primeuix/themes/-/themes-3.1.1.tgz).

Other PrimeUIX component roles, such as card borders and toggle switch colours, are resolved by
`GetVisualRecipes(global, variant)`. Tags, buttons and feedback controls can read the stops for their severity role
through `GetStatusScale` and `GetFeedbackScale`. The desktop value package uses these public contracts.

## Derivation

`DeriveGlobal` reproduces the previous `[PrimeNG preset, (AtomUI:Compact)]` algorithm chain exactly:

1. **Map.** The Ant Design base (a copy owned by this language: the default calculator, or the dark one for the dark
   appearance) gives every Map token a value; then the preset writes its colours from the seeds (`WriteColors`, with
   the noir rows for a noir seed), its `transition.duration` (Nora 0s) and its size ladder around `BorderRadius` and
   `ControlHeight`; for the compact density the compact size and height ladders run last.
2. **Alias.** The Ant Design alias formulas (copied), where the three tokens they write and read again
   (`LineWidthFocus`, `ControlOutlineWidth`, `ColorControlOutline`) take the preset's values in place so the input
   shadows follow them; then the preset's alias rules (focus, placeholder, disabled, contrast, highlight, field
   padding, shadows). This equals the previous staged evaluation (formulas, preset step, formulas again with the
   preset's values kept, preset step again).

The control derivations live in the value packages (one per control token class): the preset's control-global
overrides first (written on the control's view, seeds included), then the control's formulas, then the preset's
control values; where a formula read a value the preset replaces, it runs again after it, as the previous second
evaluation round did.

## Compact density

AtomUI extension. The compact density applies the Ant Design compact ladders after the preset's Map: control height
− 4 (Aura / Nora 31, Lara 35), SM / LG heights 75% / 125% of it, the compact size ladder (`SizeSM` 8), so field padding
becomes 6 (SM 4, LG 8), horizontal content padding 7 and Select option padding `6,4` (Aura / Nora) or `10,4` (Lara).
Checkbox indicator, Card / Dialog padding and tooltip padding keep their full-density values. Lara's Select panel
padding (`list.padding` 0.5rem 0, `0,8`) has no compact counterpart on the size ladder: the compact Lara density keeps
the `4` of the old compact profiles (the one value that lived only in their XML, now in the Select derivation).

## 映射与来源 (mapping and sources)

> **Sources.** In the rule tables, "XML (baseline)" names the value the pre-algorithm v1 profiles of the previous
> `AtomUI.Themes.PrimeNG` package wrote; those values are the acceptance baseline this language reproduces.
> "AtomUI" marks a deliberate AtomUI value or proposal where PrimeUIX has none or AtomUI deviates.

The authority is the latest release, primeng 22.1.5 (what primeng.dev runs) with `@primeuix/themes` 3.1.1 and `@primeuix/styled` 1.2.0 (all latest on npm on 2026-11-15). The primary and highlight rows of the three tables were diffed against `dist/{aura,lara,nora}/base/index.mjs` of 3.1.1 on that date: Aura and Lara match; Nora light's highlight was the old pale P50 / P100 and now follows 3.1.1 (see Nora rules). The value baseline is the standard (16px) presets of `@primeuix/themes` 3.1.1, as specified by [PrimeNG Styled Mode → Base Font Size](https://primeng.dev/theming/styled): PrimeNG sizes components in rem against a 16px document root, and AtomUI maps 1rem to 16px, so it uses `aura`, `lara` and `nora` (typography `0.875rem` = 14px). The `aura-compat`, `lara-compat` and `nora-compat` variants are calibrated for a legacy 14px root (maintained until June 2027); reading their values against a 16px root enlarges every size by 16/14, which is why earlier AtomUI profiles were about 14% too large (control height 40 instead of 35). The result is checked against what primeng.dev (PrimeNG 22) renders, rather than promising identical web/native rendering. Sources: that page, [PrimeNG styled mode](https://primeng.dev/theming) and the published presets under `dist/aura`, `dist/lara` and `dist/nora` of [`@primeuix/themes`](https://www.npmjs.com/package/@primeuix/themes); PrimeNG's own `packages/themes/src/presets` re-exports them.

**Source packages and versions.**

| What | Package | Version | Where |
|---|---|---|---|
| Tailwind colour tables (22 scales, 50–950) | [`@primeuix/themes`](https://www.npmjs.com/package/@primeuix/themes) | 3.1.1 | the `primitive` export, `dist/aura/base/index.mjs`; Lara and Nora carry the identical table |
| Colour-scale formula (`palette`, `mix`) | [`@primeuix/styled`](https://www.npmjs.com/package/@primeuix/styled) | 1.2.0 | the `palette` / `mix` implementation |
| Aura rules | `@primeuix/themes` | 3.1.1 | `aura/base`, `aura/button`, `aura/checkbox`, `aura/card`, `aura/dialog`, `aura/message`, `aura/tooltip`; sizes from the standard preset |
| Lara rules | `@primeuix/themes` | 3.1.1 | `lara` (sizes); colour and focus rules also checked against `lara-compat` |
| Nora rules | `@primeuix/themes` | 3.1.1 | `nora` (sizes; `primary` and `highlight` checked against `dist/nora/base/index.mjs`); other colour rules from `nora-compat/base`, `button`, `steps`, `card`, `dialog`, `tag` |
| Default primary (noir) | `primeng` (primeng.dev) | 22.1.5 | the site configurator's noir preset extension, see [Noir](#noir-the-default-primary) |

**Colour scale.** Stops are 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950 (index `i` = 0 to 10). A seed that equals the 500 stop of one of the 22 Tailwind scales PrimeNG ships resolves to that hand-tuned table. Any other seed goes through the PrimeUIX `palette` formula: stops up to 500 mix in white (`19 × (5 − i)` %, so 50 is 95% white and 500 is the seed), stops above 500 mix in black (`15 × (i − 5)` %, so 950 is 75% black), linearly per sRGB channel, rounded half up (`Math.round`). The seed's alpha is kept on every stop.

### Noir (the default primary)

primeng.dev starts every preset on the primary "noir" (primeng 22.1.5: the configurator state `{preset: 'Aura', primary: 'noir', surface: null}`, verified on 2026-11-15; Aura, Lara and Nora alike). Noir is not a palette of `@primeuix/themes`: the site extends the preset with `primary` = the surface palette (`{surface.50}` … `{surface.950}`) and these colour-scheme rows, identical for all presets:

| Site token | Light | Dark |
|---|---|---|
| `primary.color` / `hoverColor` / `activeColor` | `primary.950` / `800` / `700` | `primary.50` / `200` / `300` |
| `primary.contrastColor` | `#ffffff` | `primary.950` |
| `highlight.background` / `focusBackground` | `primary.950` / `700` | `primary.50` / `300` |
| `highlight.color` / `focusColor` | `#ffffff` | `primary.950` |

**Trigger.** A `ColorPrimary` seed whose RGB is the 950 stop of a Tailwind neutral family (slate `#020617`, gray `#030712`, zinc `#09090B`, neutral `#0A0A0A`, stone `#0C0A09`) is noir. The primary palette is then the surface scale of the appearance being computed (slate in light, zinc in dark), so one seed gives noir in both appearances (the Gallery remembers one brand seed per design language). `ColorLink` follows its own seed (the primary by default): a noir link seed takes the noir link row, any other keeps the preset's link row. Any other primary seed keeps the preset's rules. The twelve profiles pin `ColorPrimary #020617`. The rows are one definition per appearance (`PrimeNGNoir`), shared by the three presets; the Alias and control derivations recognise noir from the computed primary (light N950, dark N50).

| Token | Light (slate) | Dark (zinc) | Source / reason |
|---|---|---|---|
| `ColorPrimary` / `Hover` / `Active` | N950 / N800 / N700 (`#020617` / `#1E293B` / `#334155`) | N50 / N200 / N300 (`#FAFAFA` / `#E4E4E7` / `#D4D4D8`) | site `primary.color` / `hoverColor` / `activeColor` |
| `SelectionBackground` | N950 | N50 | site `primary.color` |
| `SelectionForeground`, `ColorTextLightSolid` | white | N950 (`#09090B`) | site `primary.contrastColor` |
| `ControlItemBgActive` / `Hover` (selected item) | N950 / N700 | N50 / N300 | site `highlight.background` / `focusBackground` (Alias `Highlight` row: `ColorPrimary` / `ColorPrimaryActive`) |
| `ControlItemColorActive` (selected text: list, tree, DataGrid rows, time / range cells) | white | N950 | site `highlight.color` (`HighlightColor` row: `ColorTextLightSolid`), written once by the alias derivation: light 20.17 / 10.35 on the background / focus background, dark 19.06 / 13.46 |
| `ColorPrimaryBg` / `BgHover` (filled buttons, tags, step panels) | N50 / N100 | N50@16% / N50@24% | PrimeUIX `button.text` hover / active background (`primary.50` / `primary.100`; dark the Aura shape `primary.color` at 16% / 24%) |
| `ColorPrimaryBorder` / `BorderHover` | N200 / N300 | N600 / N500 | Light: the Aura row (Lara's focus glow `ColorPrimaryBorder` is `primary.200` = N200, as on the site). Dark: opaque, because the primary end of this scale is the light one and the Aura `P700@36%` would be a dark grey that vanishes on N950 |
| `ColorPrimaryText` / `TextHover` / `TextActive` | N950 / N800 / N700 | N50 / N200 / N300 | AtomUI: the primary states. The site's `highlight.color` is white / N950, but AtomUI draws these tokens on the page background, where white (light) and N950 (dark) would vanish |
| `ColorLink` / `Hover` / `Active` (noir link seed) | N950 / N800 / N700 | N50 / N200 / N300 | PrimeUIX `button.link` is `primary.color`; the states follow the primary states: 20.17 / 14.63 / 10.35 on white, 19.06 / 15.68 / 13.46 on N950 |
| Button (Filled), Steps, Calendar / DateViewer content cells | | | The selected background is solid, so the solid-highlight stages apply as in Nora: Steps `FinishIconColor` = `ColorTextLightSolid`, Calendar / DateViewer `ItemActiveColor` / `CellActiveColor` = `ControlItemColorActive` (see [Highlight](#highlight-selected-items)). The Filled button draws on the pale global `ColorPrimaryBg` and needs nothing |
| ToggleSwitch `HandleCheckedBg` | the preset row: Aura / Lara white, Nora white | Aura / Lara N900, Nora N950 | PrimeUIX `toggleswitch.handle.checked.background` (Nora: `primary.contrast.color`); visible on the N50 dark track |
| CheckBox `ColorWhite` (check mark), RadioButton `RadioColor` (dot) | `ColorTextLightSolid` (white) | `ColorTextLightSolid` (N950) | PrimeUIX `checkbox.icon.checked.color` / `radiobutton.icon.checked.color` = `primary.contrast.color`; a white mark would vanish on the dark N50 primary. Control stages `PrimeNGCheckBoxTokens`, `PrimeNGRadioButtonTokens`; a coloured primary keeps the white mark |
| `ColorFocusBorder`, `ColorControlOutline` | the preset's rule | the preset's rule | They follow `ColorPrimary` / `ColorPrimaryBorder` (Nora dark: the field border) |

The design language contract (`PrimeNGContractTests`) checks the paired roles of every variant; the previous package checked these pairs for all twelve profiles (text 4.5:1, the Steps check icon 3:1).

**Reading the rule tables.** `P` is the primary family, `S` a status family (success, warning, info), `E` the error family, `N` the neutral scale. `@n%` is an opacity. The "Source" column says where a row comes from: **XML (baseline)** – the value the pre-algorithm XML profiles set explicitly, which were the acceptance baseline of the rebuild; **PrimeUIX** – the preset token named; **AtomUI** – a decision with no direct PrimeUIX counterpart.

### Highlight (selected items)

PrimeUIX `highlight` is the selected item's background, focus background (selected and hovered) and text. AtomUI maps it in two places, so the pale primary of filled buttons, tags and step panels (`ColorPrimaryBg`) stays separate from the selected item:

| Preset | `ControlItemBgActive` / `Hover` (Alias `Highlight` row) | Selected text `ControlItemColorActive` (Alias, `HighlightColor` row) |
|---|---|---|
| Aura, Lara | the Ant formula: `ColorPrimaryBg` / `BgHover` = P50 / P100, dark P400@16% / @24% | `ColorPrimaryText`: P700, dark white@87% |
| Nora | `ColorPrimary` / `ColorPrimaryHover`: P600 / P700, dark P500 / P400 | `ColorTextLightSolid`: white, dark N950 |
| Noir (any preset) | `ColorPrimary` / `ColorPrimaryActive`: N950 / N700, dark N50 / N300 | `ColorTextLightSolid`: white, dark N950 |

The alias derivation writes `highlight.color` once, to the global `ControlItemColorActive`; the selected-text tokens default to it: Select / Cascader `OptionSelectedColor`, ComboBox / ListBox / ListView / TransferListView `ItemSelectedColor`, TreeView / TransferTreeView / TreeSelect `NodeSelectedColor` and DataGrid `RowSelectedColor` (the selected row). The selected time cell of the pickers and the range-middle date cells read it directly, so the pickers follow PrimeUIX `datepicker` exactly: the selected date is `primary.color` with `primary.contrast.color` (the Ant compact cell already is), the range middle `highlight.background` with `highlight.color`. PrimeUIX `highlight.focusColor` equals `highlight.color` except in Aura / Lara light (P800); AtomUI has no selected-hover text slot. The content presentation of Calendar (fullscreen) and DateViewer draws today's and the selected cell in the primary on `ControlItemBgActive`; under a solid highlight `PrimeNGActiveCellTokens` sets their `ItemActiveColor` / `CellActiveColor` to `ControlItemColorActive` The LunarCalendar draws its selected cells' text in the contrast colour (`ColorTextLightSolid`), on the primary for a date and on `ItemActiveBg` for a month of the fullscreen year view, so `PrimeNGLunarCalendarTokens` (all six algorithms) sets its `ItemActiveBg` to `ColorPrimary` and `ItemActiveColor` to `ColorTextLightSolid`: PrimeUIX `datepicker` `date.selected` / `month.selected` (`primary.color` with `primary.contrast.color`) in every preset. Under a solid highlight that is the value it had; under the pale highlight of Aura / Lara with a coloured seed a selected month was white on P50 (Ant Design has the same white-on-pale month and is unchanged). Steps compensates as listed under the Nora control derivations. NavMenu does not use the highlight at all: it follows PrimeUIX `navigation` (see [Navigation](#navigation-navmenu)). `PrimeNGThemePackageTests.Highlight_Follows_PrimeUIX` asserts the table; `PrimeNGDataGridHighlightTests` (Rendering tests) the DataGrid row.

### Navigation (NavMenu)

PrimeUIX navigation components (`menu`, `tieredmenu`, `menubar`, `panelmenu`; `@primeuix/themes` 3.1.1 `dist/{aura,lara,nora}/base` `navigation.item`, checked on 2026-11-15) never use the highlight. An active item (the open submenu's path) has a neutral background, and Aura / Lara focus (hover) on the same pair. PrimeNG menus have no persistent selection, so `PrimeNGNavMenuTokens` (all six algorithms) gives the NavMenu's selected item the active pair; the primary seed (noir or a colour) does not change it:

| Preset | Selected / hover / keyboard-active background | Selected / hover text | Source |
|---|---|---|---|
| Aura, Lara | N100, dark N800 | N800, dark white | `navigation.item.active.background` = `focus.background` (`surface.100` / `surface.800`); `active.color` = `focus.color` = `text.hover.color` |
| Nora | N200, dark N700 | N950, dark white | `navigation.item.active.background` (`surface.200` / `surface.700`), `active.color` (`text.hover.color` / `text.color`) |

Tokens: `ItemSelectedBg`, `ItemHoverBg`, `ItemActiveBg`, `HorizontalItemSelectedBg`, `HorizontalItemHoverBg` = the background; `ItemSelectedColor`, `ItemHoverColor`, `HorizontalItemSelectedColor` = the text; the horizontal menu (PrimeUIX `menubar`) has no indicator bar (`ActiveBarHeight` 0) and takes the content radius (`HorizontalItemBorderRadius` = `BorderRadius`, `menubar.baseItem.borderRadius`). The selected-path headers keep the selected text colour on the menu background (Aura N800 on white 14.6:1). Departure: Nora focuses on the solid primary with the contrast colour (`navigation.item.focus.background` = `primary.color`), but AtomUI keeps a hovered selected-path header in the selected text colour, which would vanish on it (noir light N950 on N950); Nora hovers on its `content.hover.background` / `content.hover.color` (the same neutral pair) instead. Not mapped: the item radius (`navigation.item.borderRadius`, Aura `border.radius.sm`, Lara / Nora 0) and paddings. `PrimeNGThemePackageTests.NavMenu_Follows_PrimeUIX_Navigation` asserts the table.

### Overlays (popovers, select panels, menus)

PrimeUIX gives every overlay a 1px border, a radius and a shadow (`overlay.select`, `overlay.popover`, `overlay.navigation`; `@primeuix/themes` 3.1.1 `dist/{aura,lara,nora}/base`, checked on 2026-11-15 together with `popover`, `select`, `menu`, `tieredmenu`, `contextmenu` and `tooltip`). Measured on [primeng.dev/popover](https://primeng.dev/popover) (Aura light): `.p-popover` border `1px solid #E2E8F0`, radius 6, padding 10, shadow `0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1)`, 10px from the anchor, the border continuing around the arrow. The overlay stages (`Algorithms/Controls/PrimeNGOverlayTokens.cs`, all six algorithms) map them:

| What | Aura | Lara | Nora | Written to | Source |
|---|---|---|---|---|---|
| Border colour, light / dark | N200 / N700 (`#E2E8F0` / `#3F3F46`) | as Aura | transparent / N700 | `OverlayBorder` row: `ArrowDecoratedBoxToken.ContentBorderColor`; the `0 0 0 1` ring of `PopupToken` shadows | `overlay.{select,popover}.border.color` |
| Border width | `LineWidth` (1) | same | same | `ArrowDecoratedBoxToken.ContentBorderThickness` | CSS `1px solid` |
| Radius | `BorderRadius` (6) | 6 | 2 | arrow box `BorderRadius` (global already), `PopupToken.PopupCornerRadius` | `overlay.{select,popover}.border.radius` (md / xs) |
| Arrow flyout padding | 10 | 14 | 10 | `ArrowDecoratedBoxToken.ContentPadding` | `overlay.popover.padding` |
| Arrow flyout minimum height | 0 | 0 | 0 | `ArrowDecoratedBoxToken.ContentMinHeight` (Ant: `ControlHeight`) | PrimeNG has none |
| Arrow, gap to the anchor | `ArrowSize 19`, `FlyoutHostToken.MarginToAnchor 0`: the box 10 from the anchor, the tip 0.75 before it | same | same | see `PrimeNGPopoverArrow` | `popover.gutter` 10px (a 20×10 border triangle whose tip ends 1px before the anchor) |
| Arrow flyout shadow | `0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000` | `0 1 3 0 #1A000000` | as Aura | `FlyoutHostToken.PopupRootShadow` / `OverlayHostShadow` (no ring: the arrow box draws the border) | `overlay.popover.shadow` |
| Select panel / menu shadow | `0 0 0 1 <border>` ring + `BoxShadowsSecondary` (`0 4 6 -1, 0 2 4 -2`) | ring + `0 2 12 0 #1A000000` | `BoxShadowsSecondary` only in light (transparent border), ring N700 in dark | `PopupToken.PopupRootShadow` / `OverlayHostShadow` | `overlay.select.shadow` = `overlay.navigation.shadow` |
| Gap to the anchor (no arrow) | 2 | 2 | 0 | `PopupToken.MarginToAnchor` | `anchor.gutter` |

Arrow flyouts are the AtomUI controls whose surface is an `ArrowDecoratedBox`: InfoFlyout (Popover), PopupConfirm, the date and time picker panels, dropdown menus (`MenuFlyoutPresenter`), tree flyouts, Tour, the DataGrid filter flyouts and the ColorPicker panel. With a border the arrow box draws one outline around the box and the arrow (no line across the arrow's base, as PrimeNG's `::before` / `::after` triangles), and a frame ring of the hosting popup is not painted, so a date picker panel inside a `PopupToken` popup keeps a single border. The ToolTip shares the arrow box but keeps no border: PrimeUIX `tooltip` has none. Its arrow is its own (`ToolTipToken.ArrowSize` 8, `ArrowTipRadius` and `ArrowBaseRadius` 0, `ArrowStrokeThickness` 0, `MarginToAnchor` 0.5): PrimeUIX draws the tooltip arrow as a plain `tooltip.gutter` (4px) border triangle, 8 wide and 4 high, in the gutter between the anchor and the box, smaller than the popover arrow and without its border-coloured edge. The popover arrows are sharp border triangles too (`ArrowSize` `2 × popover.gutter`, both radii 0). Panels without an arrow (Select, Cascader, ComboBox, AutoComplete, Mentions, context and sub menus) have no border of their own, so the border is the ring ahead of the shadow, as the Shadcn package does.

### Progress

PrimeUIX 3.1.1 `progressbar` (`dist/{aura,lara,nora}/progressbar`; measured on [primeng.dev/progressbar](https://primeng.dev/progressbar), Aura noir light: value `#020617`, track `#E2E8F0`, label `#FFFFFF`, height 18, radius 6, label 10px / 600). `PrimeNGProgressTokens` (all six algorithms) writes, for ProgressBar, StepsProgressBar, CircleProgress and DashboardProgress:

| Token | Value | Source |
|---|---|---|
| `DefaultColor` | `ColorPrimary` (noir light `#020617`, dark `#FAFAFA`; Nora indigo light `#4F46E5`) | `progressbar.value.background` = `{primary.color}` |
| `RemainingColor` | Aura / Lara N200 / N700 (`#E2E8F0` / `#3F3F46`), Nora N300 / N600 (`#CBD5E1` / `#52525B`) (`ProgressTrack` row) | `progressbar.background` (Aura / Lara `content.border.color`; Nora `surface.300` / `surface.600`) |
| ProgressBar `LineBorderRadius` | `BorderRadius` (6, Nora 2) instead of the capsule | `progressbar.border.radius` = `content.border.radius` |
| ProgressBar `InnerLabelColor` | `ColorTextLightSolid` (white, dark N950): the inner percentage is always the contrast colour | `progressbar.label.color` = `primary.contrast.color` |

The line bar is PrimeUIX's fixed height (1.125rem, Lara 1.25rem; small and large 12 / 24, Lara 14 / 26) so the label sits inside it. Not mapped: the label font (10px / 600, Lara 12px, Nora 700), which has no token; the circle text keeps `ColorText` (PrimeNG has no circle progress). PrimeUIX has no success or exception state; those keep the preset's status colours. An inner label at 0% sits on the track in AtomUI (PrimeNG clips it with the value bar), so it is the contrast colour on the track there.

### Aura at the emerald seed

The rule tables below give stops; at the emerald seed (`ColorPrimary #10B981`, the raw `@primeuix/themes` preset value; the seeds default to [noir](#noir-the-default-primary)) they produce these values (Aura Light):

| PrimeNG semantic | AtomUI global token |
|---|---|
| Primary / hover / active | `ColorPrimary #10B981`, `ColorPrimaryHover #059669`, `ColorPrimaryActive #047857` |
| Contrast / highlight | `ColorTextLightSolid #FFFFFF`, `ColorPrimaryBg #ECFDF5`, `ColorPrimaryText #047857` |
| Text / muted | `ColorText #334155`, `ColorTextSecondary` and `ColorTextDescription #64748B` |
| Content / hover / border | `ColorBgContainer #FFFFFF`, `ColorFillTertiary #F1F5F9`, `ColorBorderSecondary #E2E8F0` |
| Form border / placeholder / invalid | `ColorBorder #CBD5E1`, `ColorTextPlaceholder #64748B`, `ColorError #F87171` (red-400 from the red-500 seed `#EF4444`) |
| Disabled background | `ColorBgContainerDisabled #E2E8F0` (separate from the hover fill) |
| Radius xs / sm / md / lg | `BorderRadiusXS 2`, `BorderRadiusSM 4`, `BorderRadius 6`, `BorderRadiusLG 8` |
| Field density / typography | `ControlHeight 35`, SM `28`, LG `42`; field padding X 10 / SM 8 / LG 12 (`InputPadding*`); `PaddingContentHorizontal 11` (content inset: field padding plus border), `FontSizeSM 12`, LG `16` |
| Focus / motion | `FocusVisualBorderThickness 1`, `ColorFocusBorder #10B981`, `ControlOutlineWidth 0`, `MotionDurationMid 200ms` |
| Select/popover / modal / mask | `BoxShadowsSecondary`, `BoxShadows`, `ColorBgMask #66000000` |

| Control | Values (control derivations) |
|---|---|
| Button | `FontWeight 500`; radius is the global `BorderRadius 6` |
| CheckBox | `CheckIndicatorSize 20`, `CheckedMarkSize 14`; radius is the global `BorderRadiusSM 4` |
| Select | `OptionPadding 10,4`, `PopupContentPadding 4` |
| Card | `BodyPadding 18`, card-scoped `BorderRadiusLG 12`, `RestingShadows` with the two PrimeNG static shadow layers |
| Dialog | dialog-scoped `BorderRadiusLG 12`; `HeaderPadding`, `ContentPadding`, `WindowContentPadding`, `FooterPadding 18`; `HeaderFontSize 18`; title weight is the global `FontWeightStrong 600` |
| ToolTip | `ToolTipBackground #334155`, `ToolTipColor #FFFFFF`, `ToolTipCornerRadius 6`, `ContentPadding 10,6` |

### Aura Map rules

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorPrimary` / `Hover` / `Active` | P500 / P600 / P700 | P400 / P300 / P200 | XML (baseline); PrimeUIX `primary.color` / `hoverColor` / `activeColor` |
| `ColorPrimaryBg` | P50 | P400@16% | XML (baseline); PrimeUIX `highlight.background` (Aura and Lara: also the selected background `ControlItemBgActive`, the Ant formula) |
| `ColorPrimaryBgHover` | P100 | P400@24% | PrimeUIX `highlight.focusBackground` |
| `ColorPrimaryText` | P700 | white@87% | XML (baseline); PrimeUIX `highlight.color` |
| `ColorPrimaryTextActive` | P800 | white@87% | PrimeUIX `highlight.focusColor` |
| `ColorPrimaryTextHover` | P600 | P300 | AtomUI (spec proposal; Light matches `floatLabelFocusColor`) |
| `ColorPrimaryBorder` / `BorderHover` | P200 / P300 | P700@36% / P600@36% | AtomUI (Light = spec proposal; Dark follows the severity border `message.*.borderColor`) |
| `ColorLink` / `Hover` / `Active` | P700 / P800 / P900 | P400 / P300 / P200 | AtomUI. PrimeUIX `button.link` keeps `primary.color` in every state and underlines on hover (the PrimeNG button recipe underlines link buttons on hover too); the global link states are stops of the primary scale (the link seed's scale when `ColorLink` is set). In light the links take a deeper stop than the primary fill so link text stays readable on white, and hover and active go deeper still, as the other light state families do: P700 / P800 / P900 give 5.48 / 7.68 / 9.72 (P500 would give 2.54, the pre-algorithm profiles' Ant blue 4.10). Dark keeps the primary states (10.35 / 13.05 / 15.51 on N950) |
| `SelectionBackground` / `Foreground` | P500 / white | P400 / N900 | PrimeUIX `primary.color` / `primary.contrastColor` |
| Status `Color*` / `Hover` / `Active` | S500 / S600 / S700 | S400 / S300 / S200 | PrimeUIX `button.{success,info,warn}` |
| Status `Bg` / `BgHover` | S50 / S100 | S500@16% / S500@24% | PrimeUIX `message.*.background` (Light 50@95% there, opaque here) / `closeButton.hoverBackground` (Light); Dark hover is AtomUI |
| Status `Border` / `BorderHover` | S200 / S300 | S700@36% / S600@36% | PrimeUIX `message.*.borderColor`; hover is AtomUI |
| Status `Text` / `TextHover` / `TextActive` | S600 / S500 / S700 | S500 / S400 / S300 | PrimeUIX `message.*.color` (`Text`); hover and active are AtomUI |
| `ColorError` | E400 | E300 | XML (baseline); PrimeUIX `formField.invalidBorderColor` |
| `ColorErrorHover` / `Active` | E500 / E600 | E200 / E100 | AtomUI (one step on from the base, as the primary states) |
| Other `ColorError*` | as status | as status | PrimeUIX `message.error` / AtomUI as for status; `BgActive` is E200 / E500@32% (AtomUI) |
| `ColorText` / `Secondary` | N700 / N500 | white / N400 | XML (baseline); PrimeUIX `text.color` / `text.mutedColor` |
| `ColorTextTertiary` / `Quaternary` | N500 / N400 | N500 / N600 | AtomUI proposal; PrimeUIX has a single muted level (Aura's `text.mutedColor`, N500 / N400). Light keeps tertiary text and `ColorIcon` at 3:1 on white: 4.76 (N400 would give 2.56, the pre-algorithm profiles 3.32); quaternary 2.56 (N300 would give 1.48, the pre-algorithm profiles 1.82). `ColorTextDisabled` N400 equals the quaternary stop, as in Ant Design where both are 25% text. Lara and Nora use the same stops |
| `ColorBgContainer` | white | N950 | XML (baseline); PrimeUIX `formField.background` |
| `ColorBgElevated` | white | N900 | Dark XML (baseline); PrimeUIX `overlay.*.background`, `content.background` |
| `ColorBgLayout` / `Spotlight` | N50 / N700 | N950 / N700 | AtomUI / PrimeUIX `tooltip.background` |
| `ColorFill` / `Secondary` / `Tertiary` / `Quaternary` | N300 / N200 / N100 / N50 | N600 / N700 / N800 / N900 | `Tertiary`: XML (baseline), PrimeUIX `content.hoverBackground`; the others AtomUI |
| `ColorBgSolid` / `Hover` / `Active` | N900 / N800 / N950 | white / N300 / N100 | AtomUI |
| `ColorBgBlur` | transparent | white@4% | AtomUI (the Ant Design values) |
| `ColorBorder` / `Secondary` | N300 / N200 | N600 / N700 | XML (baseline); PrimeUIX `formField.borderColor` / `content.borderColor` |
| `ColorBgMask` | black@40% | black@60% | XML (baseline); PrimeUIX `mask.background` |
| `ColorBgBase` / `ColorTextBase` (when the seed is unset) | white / black | N950 / white | Dark XML (baseline); Light keeps the Ant Design values |
| `BorderRadiusXS` / `SM` / `LG` | radius − 4 / − 2 / + 2 | same | XML (baseline) (2/4/8 around 6); PrimeUIX `borderRadius.xs/sm/md/lg` |
| `ControlHeightSM` / `LG` | height − 7 / + 7 | same | PrimeUIX 3.1.1 `form.field.sm` / `lg` paddings at 12px / 16px type (28/42 around 35; Lara 32/46 around 39) |
| Font sizes, sizes, motion, line widths, Ant preset palettes | Ant Design formulas | Ant Design Dark | The Ant ladder already gives PrimeUIX's 12/14/16; Nora sets `MotionDurationFast` / `Mid` / `Slow` to 0 (see [Motion](#motion)) |

The spec 4.7 row "按暗色对称取级" for the dark primary border and text states is replaced by the values above, which follow PrimeUIX where it has a counterpart. Every "现值" row of spec 4.7 is reproduced exactly (`PrimeNGAlgorithmContractTests.Aura_Map_Matches_The_Spec_Colours`).

### Aura Alias rules

Written by the alias derivation for the variant's appearance. A value the theme sets explicitly still wins.

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorFocusBorder` | `ColorPrimary` | `ColorPrimary` | PrimeUIX `focus.ring.color` |
| `FocusVisualBorderThickness` | 1 × `LineWidth` | same | PrimeUIX `focus.ring.width` |
| `ControlOutlineWidth` | 0 × `LineWidth` | same | Aura has no field focus glow; `InputActiveShadow` follows |
| `ColorTextPlaceholder`, `ColorTextDescription` | `ColorTextSecondary` | same | PrimeUIX `form.field.placeholder.color`, `text.muted.color` |
| `ColorBgContainerDisabled` | N200 | N700 | PrimeUIX `form.field.disabled.background` |
| `ColorTextDisabled` | N400 | N500 | AtomUI. PrimeUIX `form.field.disabled.color` is N500 / N400, the muted-text stop, and PrimeNG separates it with a component opacity AtomUI does not apply; one stop further from the text keeps it apart from `ColorTextSecondary`. Contrast on `ColorBgContainerDisabled`: 2.08 light, 2.16 dark (the Ant formula gave 1.20 / 1.35, the pre-algorithm profiles 1.79 / 2.06) |
| `ColorTextLightSolid` | white | N900 | PrimeUIX `primary.contrast.color` |
| `PaddingContentHorizontal` | `SizeSM` (12; compact 8) | same | The Ant formula (`SizeMS`) gives 16 / 12 |
| `BoxShadows` / `BoxShadowsSecondary` | modal / select shadow | same | PrimeUIX `overlay.modal.shadow` / `overlay.select.shadow` (= `overlay.navigation.shadow`; the Aura popover shadow is the same value). Arrow flyouts take `overlay.popover.shadow` from the overlay stages, see [Overlays](#overlays-popovers-select-panels-menus) |

### Aura control values

One class per control under `Algorithms/Controls`, each declared for both `PrimeNG:Aura` and `PrimeNG:Aura.Dark` (revision 1). Values that follow from other tokens are computed. The fixed sizes are the standard `aura` preset of `@primeuix/themes` 3.1.1 at a 16px root (what primeng.dev renders); they do not follow `AtomUI:Compact`. Paddings that sit on the size ladder (field, button, Select option) do.

| Control | Values | Source |
|---|---|---|
| Button | `FontWeight 500`; `ContentPadding` / `SM` / `LG` horizontal = `button.padding.x` + border (11 / 9 / 13, the label inset primeng.dev renders); Link variant: for the Button only `ColorLink` / `Hover` / `Active` = `ColorPrimary` / `Hover` / `Active` (all presets) | PrimeUIX `button.label.font.weight`, `button.padding.x` (`{form.field.padding.x}`, sm / lg likewise); the radius is the global `BorderRadius` already; `button.link.color` / `hoverColor` / `activeColor` are all `{primary.color}` (PrimeNG underlines the label on hover; the PrimeNG button recipe sets `UnderlineLinkOnHover`, and hover and active take the primary states). The global link colours (hyperlinks, link text) keep the deeper rows below. Under noir nothing changes (the noir link rows equal the primary rows); with a coloured seed the link button is the primary fill colour (emerald light `#10B981` / `#059669` / `#047857` instead of `#047857` / `#065F46` / `#064E3B`) |
| CheckBox | `CheckIndicatorSize 18`, `CheckedMarkSize 12` | PrimeUIX `checkbox.width` (1.125rem), `checkbox.icon.size` (0.75rem); the radius is the global `BorderRadiusSM` |
| Select | `OptionPadding` = (`UniformlyPaddingSM − 2`, `UniformlyPaddingXXS`): 10,4, compact 6,4; `PopupContentPadding` = `UniformlyPaddingXXS` (4) | PrimeUIX `list.option.padding` (0.25rem 0.625rem), `list.padding` (0.25rem) |
| Card | for the card only: `ColorBgContainer` = `ColorBgElevated`, `BorderRadiusLG` = radius + 6 (12); `BodyPadding 18`; `RestingShadows` static two-layer shadow; dark `HeaderBg` = the card surface | PrimeUIX `card.background` (`content.background`), `border.radius.xl`, `card.shadow`; `card.body.padding` (1.125rem); dark header XML (baseline) |
| Dialog | for the dialog only: `BorderRadiusLG` = radius + 6 (12); header, content, window content and footer padding 18; `HeaderFontSize 18` | PrimeUIX `overlay.modal.border.radius`, `overlay.modal.padding` (1.125rem), `dialog.title.font.size` (1.125rem); weight 600 is the global `FontWeightStrong` |
| ToolTip | `ToolTipBackground` N700, `ToolTipColor` white, `ToolTipCornerRadius` = `BorderRadius`, `ContentPadding 10,6` | PrimeUIX `tooltip.background`, `tooltip.color` (`surface.0`), `overlay.popover.border.radius`, `tooltip.padding` (0.375rem 0.625rem) |
| FloatButton | `FloatButtonShadows` empty: the floating button family (FloatButton, BackTopFloatButton, the group and host containers) rests without a shadow; the Ant baseline keeps the global `BoxShadowsSecondary` through the token's default. `DefaultBg` / `DefaultHoverBg` / `DefaultActiveBg` / `DefaultIconColor` = `button.secondary` `surface.100` / `200` / `300` / `600` (dark `800` / `700` / `600` / `300`); the Ant baseline keeps `ColorBgElevated` / `ColorBgTextHover` / transparent / `ColorText`. Declared for all six algorithms | PrimeUIX 3.1.1 `button`: every severity has no resting shadow (only the raised variant adds `raisedShadow`), and `speeddial` — the floating action button — has no shadow tokens; shadows belong to the overlays and the card. PrimeNG has no white shadowless button, so the Ant white default would be invisible: a neutral floating button takes `button.secondary` |
| MessageCard | Aura: `Information` / `Success` / `Warning` / `Error` shadows = `0 4 8 0` in blue / green / yellow / red 500 at 4% (alpha `0x0A`) behind a 1px 200 ring; Lara and Nora the neutral value. Severity `*Bg` / `*IconColor` / `*Color` from the `message` surfaces (tint or Nora fill). `Loading` (AtomUI-only, no PrimeUIX counterpart) takes the neutral value. No severity-less slot: every `MessageType` has one | PrimeUIX 3.1.1 `message` / `toast` severity wireless: `shadow: '0px 4px 8px 0px color-mix(in srgb, {blue|green|yellow|red}.500, transparent 96%)'`, identical in both colour schemes |
| NotificationCard | `RestingShadows` = `overlay.popover.shadow` (Aura / Nora the two-layer md, Lara `0 1 3 0`); the four severity shadows as MessageCard; `NotificationBorderThickness` `0,0,0,6` under Lara / Nora with `NotificationBorderColor` = `content.border.color`; severity `*Bg`, `*BorderColor`, `*IconColor`, `*TitleColor`, `*DescriptionColor`, `*CloseIconColor`, `*CloseIconHoverColor` from the `toast` severity rows | PrimeUIX 3.1.1 `toast`: `normal.shadow: '{overlay.popover.shadow}'`, the severities as above; `secondary` / `contrast` (surface.500 / 950) have no AtomUI `NotificationType` and are not mapped |
| ComboBox | Drop-down row `ItemHeight 29` (0.875rem line × 1.5 + 2 × 0.25rem) and `ItemPadding 10,0`; the Ant baseline keeps `ControlHeight` / `UniformlyPaddingSM` | PrimeUIX 3.1.1 `list.option` (`padding: 0.25rem 0.625rem`, no min height) with `list.padding: 0.25rem`, which the popup frame carries as its content padding; `anchor.gutter: 2px` already matched |
| SplitButton | `SolidSeparatorColor` = `ColorPrimary`, `DangerSeparatorColor` = `ColorError`: the seam between the two filled halves takes the fill colour and disappears. The Ant baseline keeps `ColorPrimaryHover` / `ColorErrorHover`; the outline form draws no seam | `@primeuix/styles` 3.1.1 `.p-splitbutton-button`: `border-inline-end: 0 none` (also on hover and active) and the dropdown supplies the edge through `button.<severity>.borderColor`, which for a filled severity equals its fill — PrimeNG paints no seam |
| ButtonSpinner / NumericUpDown | Handle: rest transparent, hover `surface.100` / `800`, pressed `surface.200` / `700`, icon `surface.400` / `500`-`300` / `600`-`200`, no divider, icon 10.5 and a column of the icon plus `InputPadding.Left` on each side. Written for both token identities (`ButtonSpinnerToken`, `NumericUpDownToken`); the Ant baseline keeps `ColorBgContainer` / transparent / `ColorFillAlter` / `ColorIcon` / `ColorPrimary` / `ColorPrimaryActive` / `ColorBorder` and `ControlHeightSM` / `fontSize / 2` | PrimeUIX 3.1.1 `inputnumber.button` (`background`, `hover.background`, `active.background`, `color`, `hover.color`, `active.color`, `width: 2.25rem`); `@primeuix/styles` `.p-inputnumber-stacked .p-inputnumber-button { border: 0 none }`, `button-group { inset-block-start: 1px; inset-inline-end: 1px; height: calc(100% - 2px) }` and `.p-inputnumber-button .p-icon { font-size/width/height: form.field.font.size }` (0.875rem = 14; AtomUI renders it one quarter step smaller at 10.5, and narrows the 2.25rem column so the icon sits as far from the right edge as the text from the left one, by product decision) |

### Control values of all presets

Every rule below is part of the PrimeNG control derivations and reads the style (`PrimeNGPresets.KindOf`). Values are PrimeUIX 3.1.1 at a 16px root, the noir default primary.

| Family | Stages | PrimeUIX source |
|---|---|---|
| Form fields | `PrimeNGInputStage` for LineEdit, TextArea, AutoComplete, Mentions, Select, Cascader, TreeSelect, NumericUpDown, DatePicker, RangeDatePicker, TimePicker, RangeTimePicker, OTP: hover border, icon colour, add-on background, disabled text, radii, Nora's 1px primary focus ring | `form.field.*`, `inputtext`, `inputgroup` |
| Buttons | `PrimeNGButtonFamilyTokens<T>` for Button, DropdownButton, SubmitButton, ResetButton: recipe (no variants recipe, `DisabledOpacity 0.6`, no press movement, link underline on hover), danger stops, contrast, padding, small font, icon gap, no shadows, outlined / text secondary | `button` |
| Choice | Slider (rail, range, handle ring and fill), Rate, ToggleSwitch, RadioButton, CheckBox (sizes, hover border), ComboBox, ListBox, OTP | `slider`, `rating`, `toggleswitch`, `radiobutton`, `checkbox`, `listbox`, `inputotp` |
| Date and time | `PrimeNGDatePanel`: 28px (Lara 36) round cells, row height, panel width, round today fill, header title 500, week-day row and weight; picker popups without an arrow, panel and button-bar paddings, range gap | `datepicker` |
| Data display | Tag, CountBadge, DotBadge (weight 700, radius, padding), Avatar, Skeleton, Separator, Timeline, ListView, TreeView, Carousel, Descriptions, QRCode, Pagination (36px round items, solid selected page, 4px gap) | `tag`, `badge`, `avatar`, `skeleton`, `divider`, `timeline`, `listbox`, `tree`, `paginator` |
| Containers and navigation | Tabs (muted items, title weight 600 / 700, padding, ink bar), Collapse / Expander (accordion header weight, paddings, colours), GroupBox (legend weight), Breadcrumb, Splitter, Menu / ContextMenu / MenuFlyout (item rows, group title weight), NavMenu, Steps (32px numbers, title weight) | `tabs`, `accordion`, `fieldset`, `breadcrumb`, `splitter`, `menu`, `panelmenu`, `steps` |
| Feedback and overlays | Alert (message palettes, severity text colour, weight 500 / 700), PopupConfirm, Tour, Upload, Dialog / MessageBox / Drawer, ToolTip, MessageCard / NotificationCard metrics and shadows | `message`, `confirmpopup`, `fileupload`, `dialog`, `drawer`, `tooltip`, `toast` |

Control capabilities these stages use were added to the controls as Own tokens whose defaults reproduce the previous look, so Ant Design and Shadcn render unchanged: `ButtonThemeRecipe` switches (`UsesRecipeVariants`, `DisabledOpacity`, `PressMoves`, `UnderlineLinkOnHover`); Pagination item colours, weight and radius; Collapse / Expander header colour and weight; DateViewer cell radius, today fill, header title weight, week-day row height and weight; picker `PopupArrowVisible`; Tag, CountBadge, Tabs, GroupBox, Steps and menu group title font weights; Alert severity text colours and message weight; CheckBox / RadioButton `IndicatorHoverBorderColor`; Slider `ThumbBg` and `ThumbHoverOutlineThickness` (focus ring only, none on hover); Tag `FilledStatusSolid` (Lara / Nora solid status tags); ToggleSwitch `UncheckedContentColor`; Mentions / AutoComplete `OptionColor` / `OptionSelectedColor` (candidate text, readable on the solid highlight); Pagination and SimplePagination item colours passed as plain properties of each paginator.

### Lara rules

The Lara rule table (`PrimeNGLaraPreset`) is transcribed from the pre-algorithm Lara XML profiles. Only the rows that differ from Aura are listed; every other Map, Alias and control row is the Aura row above.

**Map.** All colour rows equal the Aura Map rules. The old Lara XML wrote exactly the Aura colours (XML (baseline)), and the semantic colour scheme of the `lara` / `lara-compat` presets of `@primeuix/themes` 3.1.1 (`primary`, `highlight`, `text`, `content`, `formField` background / border / disabled, `mask`, `surface` slate / zinc) equals the `aura` one. The size ladder is the Aura ladder around the `ControlHeight 44` seed: SM 40, LG 52 (XML (baseline)).

**Alias.**

| Token | Light | Dark | Source |
|---|---|---|---|
| `FocusVisualBorderThickness` | 0 × `LineWidth` | same | XML (baseline); PrimeUIX `lara-compat` `focus.ring.width` 0 |
| `ControlOutlineWidth` | 3 × `LineWidth` | same | XML (baseline); PrimeUIX `focus.ring.shadow` is a 0.2rem (3.2px) spread |
| `ColorControlOutline` | `ColorPrimaryBorder` (P200) | `ColorPrimary` @20% (P400@20%) | XML (baseline); PrimeUIX `lara-compat` `focus.ring.shadow` (`primary.200` / `primary.color` with 80% transparency) |
| `BoxShadowsSecondary` | `0 2 12 0 #1A000000` | same | XML (baseline); PrimeUIX `lara-compat` `overlay.select.shadow` |
| `BoxShadows` | the Aura modal shadow | same | XML (baseline); PrimeUIX `lara` `overlay.modal.shadow` is `0 1px 3px rgba(0,0,0,0.3)`, not used |
| `ColorTextDisabled` | N400 | N500 | AtomUI, as for Aura (same reasoning, same contrast: 2.08 light, 2.16 dark on `ColorBgContainerDisabled`; the pre-algorithm Lara profiles had 1.79 / 2.06) |

`InputActiveShadow` (and the error / warning variants) follow `ControlOutlineWidth` and `ColorControlOutline`, so the glow follows the primary seed.

**Control values.**

| Control | Values | Source |
|---|---|---|
| Button | `FontWeight 600`; `ContentPadding` horizontal = `SizeSM + 2` + border (15, compact 11); SM / LG as Aura (9 / 13) | PrimeUIX `button.label.font.weight`, `button.padding.x` (0.875rem; sm / lg are the field paddings) |
| CheckBox | `CheckIndicatorSize 20`, `CheckedMarkSize 14` | PrimeUIX `checkbox.width` (1.25rem), `checkbox.icon.size` (0.875rem) |
| Select | `OptionPadding` = (`UniformlyPaddingSM + 2`, `UniformlyPaddingXS`): 14,8, compact 10,4; `PopupContentPadding` = (0, `UniformlyPaddingXS`): 0,8 | PrimeUIX `list.option.padding` (0.5rem 0.875rem), `list.padding` (0.5rem 0); compact panel padding: see 不等价项 |
| Card, Dialog, ToolTip | as Aura, except body and dialog padding 20 and tooltip padding 10,8 | PrimeUIX `card.body.padding` and `overlay.modal.padding` (1.25rem), `tooltip.padding` (0.5rem 0.625rem); dialog title 1.125rem as Aura |

### Nora rules

The Nora rule table (`PrimeNGNoraPreset`) is transcribed from the pre-algorithm Nora XML profiles; its light highlight follows `@primeuix/themes` 3.1.1 `nora`. Only the rows that differ from Aura are listed; every other Map, Alias and control row is the Aura row above. PrimeUIX is named only for values checked in the `nora-compat` preset of `@primeuix/themes` 3.1.1.

**Map.**

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorPrimary` / `Hover` / `Active` | P600 / P700 / P800 | P500 / P400 / P300 | XML (baseline); PrimeUIX `primary.color` / `hoverColor` / `activeColor` |
| `ColorPrimaryBg` / `BgHover` | P50 / P100 | P500@16% / P500@24% | PrimeUIX `button.text` hover / active background (light `primary.50` / `primary.100`, dark `primary.color` mixed with 84% / 76% transparent): filled buttons, tags and step panels keep a pale primary. The solid highlight is `ControlItemBgActive`, see [Highlight](#highlight-selected-items) |
| `ColorPrimaryText` | P800 | P400 | Light: XML (baseline). AtomUI draws `ColorPrimaryText*` on the page background, so both appearances use page-readable stops instead of PrimeUIX `highlight.color` (white / `surface.950`, which goes to the selected-text tokens): dark P400 on N950 is 10.35 for emerald (N950 would vanish) |
| `ColorPrimaryTextHover` / `TextActive` | P700 / P900 | P300 / P200 | AtomUI: one stop on, as the Aura rows |
| `ColorPrimaryBorder` / `BorderHover` | P200 / P300 | P700@36% / P600@36% | AtomUI (the Aura rows) |
| `ColorLink` / `Hover` / `Active` | P700 / P800 / P900 | P500 / P400 / P300 | light: the Aura row (5.48 / 7.68 / 9.72 on white; the primary P600 would give 3.77); dark: the primary row (7.84 / 10.35 / 13.05 on N950) |
| `SelectionBackground` | follows the primary row | same | as Aura |
| `SelectionForeground`, `ColorTextLightSolid` | white | N950 | XML (baseline); PrimeUIX `primary.contrastColor` |
| Status `Color*` / `Hover` / `Active` | S600 / S700 / S800 | S500 / S400 / S300 | AtomUI decision: the primary rule, as spec 4.7 proposes for the status families; PrimeUIX `button.{success,info,warn}` and `tag.*` use these stops. The other status rows are the Aura rows |
| `ColorError` / `Hover` / `Active` | E400 / E500 / E600 | E400 / E300 / E200 | XML (baseline) for the base (`#F87171` in both); the states one stop on, as Aura. PrimeUIX `formField.invalidBorderColor` is E500 / E400 |
| `ColorText` | N900 | white | XML (baseline); PrimeUIX `text.color` |
| `ColorTextSecondary` | N500 | N400 | XML (baseline); PrimeUIX `text.mutedColor` is N600 / N400 |
| `ColorBorder` | N500 | N500 | XML (baseline); PrimeUIX `formField.borderColor` |
| `ColorBorderSecondary` | transparent | transparent | XML (baseline) |
| Radius ladder | XS, SM, LG = `BorderRadius` (2) | same | XML (baseline); PrimeUIX `formField.borderRadius`, `content.borderRadius` and the overlays all use `border.radius.xs` |

**Alias.**

| Token | Light | Dark | Source |
|---|---|---|---|
| `ColorFocusBorder` | `ColorPrimary` | `ColorBorder` (N500) | XML (baseline). Light: PrimeUIX `focus.ring.color`. Dark: PrimeUIX `form.field.focus.border.color` (`surface.500`); the PrimeUIX focus ring is the primary colour, which the XML did not use |
| `FocusVisualBorderThickness` | 2 × `LineWidth` | same | XML (baseline); PrimeUIX `focus.ring.width` 2px |
| `ColorTextDisabled` | N400 | N500 | AtomUI, as for Aura (one stop from the secondary text towards the background). Contrast on `ColorBgContainerDisabled`: 2.08 light, 2.16 dark (pre-algorithm Nora profiles 1.79 / 2.06) |

**Control values.**

| Control | Values | Source |
|---|---|---|
| Button | `FontWeight 700` | XML (baseline); PrimeUIX `button.label.fontWeight`. The Filled variant draws `ColorPrimary` on the global `ColorPrimaryBg`, which is the pale primary (the solid highlight is `ControlItemBgActive` only), so no scoped background is needed |
| CheckBox | as Aura (18 / 12) | PrimeUIX `checkbox.width` (1.125rem), `checkbox.icon.size` (0.75rem) |
| Select | `OptionPadding` as Aura (10,4, compact 6,4); `PopupContentPadding` 0,2 in both densities | PrimeUIX `list.option.padding` (0.25rem 0.625rem), `list.padding` (0.125rem 0) |
| Card | as Aura, radius + 0 (2); dark: for the card only `ColorBorderSecondary` N700 | XML (baseline); the dark border equals PrimeUIX `overlay.*.borderColor` (transparent / `surface.700`). PrimeUIX `card.borderRadius` is `border.radius.sm` and its shadow differs; the XML values are kept |
| Dialog | as Aura, radius + 0 (2) | PrimeUIX `overlay.modal.padding` (1.125rem) |
| ToolTip | as Aura (N700, white, `BorderRadius`, 10,6) | PrimeUIX `tooltip.padding` (0.375rem 0.625rem). PrimeUIX `tooltip.background` is N900 / `surface.0`; the XML kept N700 / white |
| Steps (`PrimeNGStepsTokens`, all styles) | `FinishIconColor` = `ColorTextLightSolid` (light white, dark N950), only while `ControlItemBgActive` is the solid primary; nothing under the pale highlight | XML (baseline); PrimeUIX `steps.itemNumber.activeColor` (`primary.contrast.color`) |

## Unsupported and known differences

**Behaviour of the derivations.**

- **Status seeds.** The language seeds `ColorSuccess #22C55E`, `ColorWarning #F97316` and `ColorInfo #0EA5E9` (green / orange / sky 500, the PrimeUIX `button`, `tag` and `badge` severity families). Alert, MessageCard and NotificationCard use the PrimeUIX `message` / `toast` palettes instead (info blue, warn yellow), scoped in their own stages (`PrimeNGMessageSeverity`).
- **Solid highlight contrast.** Under Nora the selected text is the PrimeUIX contrast colour on the solid primary: light white on P600 (emerald 3.77, blue 5.17, indigo 6.29), dark N950 on P500 (emerald 7.84, blue 5.41, indigo 4.45). The emerald light pair is below 4.5 in PrimeUIX itself.
- **Selected-hover text.** AtomUI has one selected-text colour for the selected and the selected-hover background; PrimeUIX Aura / Lara light use `primary.800` on the focus background (`primary.700` is used for both).
- **Overlay borders of menus.** PrimeUIX menus take `content.border.color`, select panels `overlay.select.border.color`. They are equal in Aura and Lara; in Nora the menu border is N400 / N500 while the select border is transparent / N700. AtomUI menus and select panels share `PopupToken` (and dropdown menus the arrow box), so Nora menus get the select border.
- **Arrow shape.** The AtomUI arrow has a slightly rounded tip and is 19 wide at the box (PrimeNG: a sharp 20×10 triangle); its position along the edge follows AtomUI's placement (`ArrowPosition`), not PrimeUIX `popover.arrowOffset` (1.125rem from the box edge).
- **Switch unchecked handle.** Only the checked handle follows PrimeUIX (`HandleCheckedBg`); the unchecked handle stays the Ant Design white because the unchecked track is Ant's `ColorTextQuaternary`, not PrimeUIX `toggleswitch.background` (Aura / Lara `surface.300` / `surface.700`, Nora the field background with a border).

**History: changes the previous package made against its pre-algorithm profiles.** In the old dark profiles the chain `[AtomUI:Dark]` re-derived the primary and error seeds the XML had already written as final values. The dark profiles now compile to the values their XML wrote:

| Profiles | `ColorPrimary` before → now | `ColorError` before → now |
|---|---|---|
| `PrimeNGAura.Dark`, `PrimeNGAura.DarkCompact` | `#2FB685` → `#34D399` | `#D98F8F` → `#FCA5A5` |
| `PrimeNGLara.Dark`, `PrimeNGLara.DarkCompact` | `#2FB685` → `#34D399` | `#D98F8F` → `#FCA5A5` |
| `PrimeNGNora.Dark`, `PrimeNGNora.DarkCompact` | `#11A071` → `#10B981` | `#D66363` → `#F87171` |

Everything derived from them follows (selection background, the primary fills of tabs, steps, switches and radios, error text and badges). In all twelve profiles, tokens the old XML left to Ant Design (status, link, fill, solid, spotlight, tertiary and disabled text colours) now follow the PrimeNG rules above; no size, padding, radius, font, line width or motion value changed.

**Not reproduced.**

- The wave is off in every style (`EnableWaveSpirit` false), as PrimeNG ships with `ripple: false`. Floating labels, the Filled field appearance and rem root-font scaling are not reproduced. Buttons dim to `disabled.opacity` 0.6 (the PrimeNG button recipe); other disabled controls follow AtomUI's state model.
- Nora's inset field-focus geometry is not reproduced; the preset maps the available 2px focus-border token.
- Input hover borders follow `form.field.hover.border.color` (Aura surface 400 / dark 500, Lara primary, Nora the border colour) through `PrimeNGInputStage`; the resting field shadow of Aura (`0 1 2 0` at 5%) is not drawn.
- Small LineEdit text follows `InputFontSize` by AtomUI contract; setting global `FontSizeSM` does not shrink its small text.
- `InputPaddingSM` is derived for the `FontHeight` text line that every AtomUI Small field uses (plus `InputContentMargin` and the border), so each Small field (LineEdit, NumericUpDown, ComboBox, Select, pickers) is exactly `ControlHeightSM`.
- Small fields use PrimeUIX `form.field.sm.font.size` (0.75rem = `FontSizeSM`) through each field's `InputFontSizeSM` Own token: LineEdit, SearchEdit, NumericUpDown, Select, Cascader and TreeSelect (ComboBox and the date and time pickers already read `FontSizeSM`). SearchEdit now has its own token class and takes the same form field stage as LineEdit.
- Select option item radius and the 2px item gap are not mapped: the existing option theme has no Token consumer for those values. Closed-field geometry and option padding remain separate contracts.
- Card keeps AtomUI's header divider, host typography and border. The static PrimeNG shadow maps to `RestingShadows`; `CardShadows` remains the hover shadow.
- Explicit values keep their precedence (language derivation < theme value < value of an inner scope); a control derivation's overrides of the control view still apply over a scope's explicit global value (spec 6.4).

## Motion

PrimeNG has two kinds of motion, and only one of them belongs to the preset:

- **State transitions** (hover, focus, checked colours) are CSS `transition`s timed by the preset's `transition.duration`
  (Aura and Lara 0.2s, Nora **0s**). AtomUI times them with the global `MotionDurationFast` / `Mid` / `Slow`: Aura and
  Lara keep the Ant Design ladder (Mid 0.2s), Nora sets all three to 0 in its map derivation (`PrimeNGPreset.TransitionDuration`),
  so its colours switch instantly.
- **Component animations** are fixed in `@primeuix/styles` 3.1.1 / primeng 22.1.5 and the same in every preset, Nora
  included: anchored overlays (select, popover, menu, confirm popup) and dialogs fade and scale in over 300ms
  (`cubic-bezier(.19,1,.22,1)`), the drawer slides over 0.5s, collapsible content (accordion, panel, tree) expands
  over 0.2s, the toast moves over `toast.transition.duration` 0.3s, the tabs ink bar slides over 250ms, the carousel
  over 500ms, the tooltip fades in over 250ms, the progress value over 1s; the toggle switch knob slides over its own
  `slide.duration` (Aura / Lara 0.2s, Nora 0.1s). Spinners, skeletons and the indeterminate progress bar always run.
  PrimeNG skips them only for the operating system's reduced-motion preference.

So Nora keeps `EnableMotion` on (it used to be `false` in the profiles, which also froze every popup, drawer, collapse
and spinner), and its control derivations restore the duration each animation reads in its own scope
(`PrimeNGMotion`): Popup, FlyoutHost, PopupConfirm, ContextMenu `MotionDurationMid` 300ms; Dialog and MessageBox Mid /
Slow 300ms; Drawer Slow 500ms; Collapse, Expander, TreeView Slow 200ms and NavMenu Mid 200ms; TabControl / TabStrip
Slow 250ms; Carousel Slow 500ms; NotificationCard and MessageCard Mid 300ms; ToggleSwitch Mid 100ms; ToolTip
`MotionDuration` 250ms; Spin `IndicatorDuration` 1.2s (the Ant cycle). Each owner restores only the token its
animation reads, so its own hover colours stay instant (Collapse: Slow restored, Mid stays 0). An explicit
`EnableMotion false` still turns every animation off. Aura and Lara keep the Ant Design durations.

### 2026-11-21 live checks on primeng.dev

Measured with computed styles on primeng.dev (16px root, Aura unless noted) and `@primeuix/themes` 3.1.1 sources on unpkg:

- ProgressBar: 18 high (1.125rem), radius 6, label 10px / 600. The default (Large) AtomUI preset takes it; Middle / Small 12 / 8 (Lara 20 / 14 / 10).
- ConfirmPopup: 1px surface 200 border, radius 6, `0 4px 6px -1px` + `0 2px 4px -2px` at 10%, box 10px from the anchor, arrow 20 × 10 (outline) / 16 × 8 (fill). Nora: transparent border, radius 2, same shadow, so its light edge is faint by design (our overlay rendering matches it within a few levels; primeng.dev only reads better because its page is surface 50). **AtomUI deviation:** Nora light overlays (select panels, popovers, menus, dialogs, and the plain Notification / Message cards without a severity) take a 1px surface 200 border instead of the transparent one, the light counterpart of Nora dark's surface 700 border, so they separate from white surfaces.
- Toast: Aura 1px border (`content.border.color`, severity `{color}.200`) and `overlay.popover.shadow` (severity: `0 4px 8px` at 4%); Lara and Nora draw a 6px start bar (`border.width: 0 0 0 6px`). Severity surfaces: Aura and Lara tint (`{color}.50` at 95%, dark `{color}.500` at 16%) with 600 (dark 500) summary, icon and close icon and a surface 700 (dark surface 0) detail; Nora fills (800, warn 600; dark 200) with 50 (dark 950) text. AtomUI's NotificationCard follows all of it (the Aura border as a ring ahead of the shadow; the start bar as a 6px inset shadow along the leading edge with the content padding taking the 6px, Lara `{color}.500` / dark `{color}.700` at 36%, Nora the fill: a 6,0,0,0 frame border would make Avalonia's Border drop the card's box shadows). The plain card (and the Loading message) gets a 1px `content.border.color` ring in every preset: Aura's own toast border, an AtomUI legibility addition under Lara and Nora. MessageCard takes the PrimeUIX `message` surfaces (the same tint or fill, text and icon in the severity colour; Aura's 200 border as a ring).
- Select panel: list padding 4, gap 2, option padding 4 10 and radius 4 in Aura; Lara and Nora square options without a gap. Cascader and AutoComplete follow the same list (Aura: 4px corners, a 1px row margin and the list padding less 1; AutoComplete rows take the height the Select's option renders, `OptionHeight`); TreeSelect already has the tree's 2px `tree.gap` and `content.border.radius` nodes, with its panel padding 4 4 4 2.

**AtomUI legibility deviations** (requested after side-by-side review; each keeps the preset's look but fixes a place where the official value is hard to read or hit on a desktop):

- `anchor.gutter` (Aura / Lara 2, Nora 0, where the panel border stacked on the field border): every Select-family panel (Select, ComboBox, Cascader, TreeSelect, AutoComplete, Mentions, the pickers) opens as far from its field as the DropdownButton menu, `MarginXXS` (4) between the visible edges in all three presets (`Popup.MarginToAnchor` = `MarginXXS` + the 1px ring drawn outside the panel frame).
- `form.field.placeholder.color` (the muted text: surface 500, Nora 600; dark surface 400) → surface 400 in light and surface 500 in dark in every preset, so a placeholder reads clearly lighter than a typed value; `ColorTextSecondary` keeps the muted text.
- Splitter: Aura and Nora's `handle.background` is transparent; every preset draws Lara's 24px grip (surface 400 / dark 600), Nora two stops past its surface 400 gutter (600 / dark 300).
- OptionButton (outline style): Nora fills the checked option like its `togglebutton` (`highlight.background`, `highlight.color` text) instead of only recolouring its border.
- Transfer: a 2px gap between list rows in every preset (Aura's `list.gap`), so adjacent selected rows stay separate.
- Notification / Message: the plain card is outlined in every preset (see Toast above).
- List check marks: PrimeUIX `listbox.checkmark.color` is `list.option.color`, which vanishes on a solid (noir, Nora) highlight; ListView, ListBox and Select draw the selected item's check mark in `highlight.color` (`ItemSelectedIndicatorColor` / `OptionSelectedIndicatorColor`).
- Collapse: the panel header takes the Expander's padding (6px 16px, Lara 4px 16px) instead of `accordion.header.padding` 16 (Nora 14px 16px), so the two collapsible headers have the same height.
