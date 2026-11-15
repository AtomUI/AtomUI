# AtomUI.Themes.Shadcn

Status: implemented and validated locally; not published to NuGet.

Compiled Zinc, Slate and Stone presets that approximate shadcn/ui through AtomUI's existing global tokens. Registration preserves the `DaybreakBlue` default.

```csharp
using AtomUI.Desktop.Controls;
using AtomUI.Themes.Shadcn;
using AtomUI.Theme;

appBuilder.UseAtomUI(builder =>
{
    builder.UseDesktopControls();
    builder.Theme.UseShadcnThemes();
    builder.Theme.WithInitialTheme("ShadcnZinc");
});
```

The package references Core and Desktop.Controls. Dark profiles use typed CheckBox and RadioButton identities to override CheckBox's local `ColorWhite` and RadioButton's existing Own `RadioColor`: their selected check/dot colors otherwise derive from white rather than `ColorTextLightSolid`. This keeps selected marks dark against the light dark-mode primary fill without changing global white or control templates. The generator compiles XML v1 into typed registrations at build time; startup does not read XML, scan assemblies or register controls. Consumers register their own control packages.

## Appearance and density profiles

| Family | Light | Dark | Light compact | Dark compact |
|---|---|---|---|---|
| Zinc | `ShadcnZinc` | `ShadcnZinc.Dark` | `ShadcnZinc.Compact` | `ShadcnZinc.DarkCompact` |
| Slate | `ShadcnSlate` | `ShadcnSlate.Dark` | `ShadcnSlate.Compact` | `ShadcnSlate.DarkCompact` |
| Stone | `ShadcnStone` | `ShadcnStone.Dark` | `ShadcnStone.Compact` | `ShadcnStone.DarkCompact` |

Select the exact ID with `WithInitialTheme` or `ApplyThemeAsync(new ThemeRequest("ShadcnSlate.DarkCompact", null, ThemeTransitionReason.UserRequest))`. Gallery groups these through the existing generic profile suffix convention and compiled package origin.

No automatic runtime profile dispatch is installed. Applying `Dark` or `Compact` algorithms to a base ID retains its explicit Map/Alias pins; that request is not equivalent to selecting the corresponding profile ID. Dark profiles reverse the neutral surfaces and text, use a light primary seed with dark solid text, and let the Dark algorithm derive its actual primary fill. Compact profiles use the existing Compact algorithm to reduce normal height from 36 to 32, explicitly reduce SM/LG from 32/40 to 28/36, and horizontal content padding from 12 to 8. Radius, line width, shadows and disabled wave remain unchanged.

## Mapping and sources

Base values are pinned to the approved preset table: primary zinc-900 `#18181B`, slate-900 `#0F172A`, stone-900 `#1C1917`; solid primary text `#FAFAFA`; radius 6; heights 36/32/40; line width 1; wave disabled. Shadows follow shadcn/ui's Tailwind levels: `BoxShadowsSecondary` (popups, menus, dropdowns and select panels) uses `shadow-md` `0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000`, the level of shadcn dropdown, popover and select content; `BoxShadows` (dialogs, drawers and notifications) uses `shadow-lg` `0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000`. `shadow-sm` is not used for either level, since it leaves popups without a visible edge. shadcn popups also carry a 1px `border`, and `shadow-md` casts mostly downward, so without it a popup's top edge disappears on a page of the same colour. Each profile therefore overrides `Controls[Popup]` `PopupRootShadow` and `OverlayHostShadow` (menus, Select, Cascader, ComboBox, AutoComplete, Mentions) with a `0 0 0 1` ring in that profile's `ColorBorder` ahead of `shadow-md`. Arrow flyouts and other `BoxShadowsSecondary` consumers keep the plain shadow, because a ring would cross the arrow's base.

The neutral hex palettes come from the official [Tailwind CSS v3.4.17 color reference](https://v3.tailwindcss.com/docs/customizing-colors), checked on 2026-11-10. The palette version is deliberate: current shadcn/ui defaults can evolve, while the approved hex seeds remain fixed. [shadcn/ui theming](https://ui.shadcn.com/docs/theming) supplies the semantic foreground/background/primary and dark override model; AtomUI token selection is an approximation.

| Role | Light palette step | Dark palette step |
|---|---|---|
| Layout | 50 | 900 |
| Container / elevated | white / white | 900 / 800 |
| Text / secondary / tertiary / quaternary | 900 / 500 / 400 / 300 | `#FAFAFA` / 400 / 500 / 600 |
| Border / secondary border | 200 / 100 | 800 / 700 |
| Disabled container | 100 | 800 |
| Fill / secondary / tertiary / quaternary | 200 / 100 / 100 / 50 | 700 / 800 / 800 / 900 |
| Primary hover / active | 800 / 700 | 100 / 200 |

Description follows secondary text; placeholder uses step 400 in Light and 500 in Dark. The palette covers the corresponding family's 50–900 neutrals. Semantic success, warning, error and info continue to use AtomUI's algorithms.

## 不支持 / Unsupported

- shadcn/ui focus-ring shape and offset: AtomUI keeps its existing focus mechanism.
- Command-style popups, command palette structure and keyboard semantics: existing AtomUI popup/control structures remain in use.
- Exact web component geometry, all web interaction states or pixel equivalence. These are token presets, not a port of shadcn/ui controls.
